using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;


namespace DialogueSystem.Windows
{
    using Elements;
    using Utilities;
    using Data.Error;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        private SerializableDictionary<string, DialogueSystemNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DialogueSystemGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>> groupedNodes;

        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemditorWindow)
        {
            editorWindow = dialogueSystemditorWindow;
            ungroupedNodes = new SerializableDictionary<string, DialogueSystemNodeErrorData>();
            groups = new SerializableDictionary<string, DialogueSystemGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>>();

            AddManipulators();
            AddSearchWindow();
            AddGridBackground();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();

            AddStyles();
        }

        #region Overrided Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port) return;
                if (startPort.node == port.node) return;
                if (startPort.direction == port.direction) return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DialogueSystemNode.DialogueTypes.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DialogueSystemNode.DialogueTypes.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueSystemNode.DialogueTypes dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }
        #endregion

        #region Elements Creation
        public DialogueSystemNode CreateNode(DialogueSystemNode.DialogueTypes dialogueType, Vector2 nodePosition)
        {
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DialogueSystem{dialogueType}Node");
            DialogueSystemNode node = (DialogueSystemNode)Activator.CreateInstance(nodeType);

            node.Initialise(this, nodePosition);
            node.Draw();

            AddUngroupedNode(node);

            return node;
        }

        public DialogueSystemGroup CreateGroup(string title, Vector2 groupPosition)
        {
            DialogueSystemGroup group = new DialogueSystemGroup(title, groupPosition);
            AddGroup(group);
            AddElement(group);

            foreach(GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DialogueSystemNode)) continue;

                DialogueSystemNode node = (DialogueSystemNode)selectedElement;

                group.AddElement(node);
            }

            return group;
        }
        #endregion

        #region Callbacks
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                List<Edge> edgesToDelete = new List<Edge>();
                List<DialogueSystemNode> nodesToDelete = new List<DialogueSystemNode>();
                List<DialogueSystemGroup> groupsToDelete = new List<DialogueSystemGroup>();

                foreach(GraphElement element in selection)
                {
                    if(element is DialogueSystemNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (element.GetType() == typeof(Edge))
                    {
                        edgesToDelete.Add((Edge)element);
                        continue;
                    }

                    if (element.GetType() != typeof(DialogueSystemGroup)) { continue; }

                    groupsToDelete.Add((DialogueSystemGroup)element);
                }

                foreach (DialogueSystemGroup group in groupsToDelete)
                {
                    List<DialogueSystemNode> groupNodes = new List<DialogueSystemNode>();

                    foreach(GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is DialogueSystemNode)) continue;
                        groupNodes.Add((DialogueSystemNode)groupElement);
                    }

                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);
                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach (DialogueSystemNode node in nodesToDelete)
                {
                    // Remove the node from the group, This calls the 'ElementsRemovedFromGroup' callback automatically
                    if (node.Group != null) node.Group.RemoveElement(node);

                    RemoveUngroupedNode(node);
                    node.DisconnetAllPorts();
                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach(GraphElement element in elements)
                {
                    if (!(element is DialogueSystemNode)) { continue; }

                    DialogueSystemGroup nodeGroup = (DialogueSystemGroup)group;
                    DialogueSystemNode node = (DialogueSystemNode)element;
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueSystemNode)) { continue; }

                    DialogueSystemGroup nodeGroup = (DialogueSystemGroup)group;
                    DialogueSystemNode node = (DialogueSystemNode)element;

                    RemoveGroupedNode(node, nodeGroup);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DialogueSystemGroup dsGroup = (DialogueSystemGroup)group;
                RemoveGroup(dsGroup);
                dsGroup.oldTitle = newTitle;
                AddGroup(dsGroup);
            };
        }
        #endregion

        #region Repeated Elements
        public void AddUngroupedNode(DialogueSystemNode node)
        {
            // Reference the name once here
            string nodeName = node.DialogueName;

            // If ungrouped nodes does not contain a node with this name
            if(!ungroupedNodes.ContainsKey(nodeName))
            {
                // Generate a new random colour used to represent duplicates so it can be referenced when a duplicate is made
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();                
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            // Refernce the Node List once here
            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            // Add the node
            ungroupedNodesList.Add(node);

            // Reference that duplicated Node's error colour and apply it
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // If this is the second node then set the first node to have the error colour, subsequent duplicates won't need to set this as number 2 will already have done it
            if(ungroupedNodesList.Count == 2)
            {
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName;

            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            // Remove the node from the dictionary
            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            // Only one Node left with this name, so reset the error style
            if (ungroupedNodesList.Count == 1)
            {
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            // No Nodes left with this name so remove it's key from the dictionary
            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        public void AddGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            // Reference the name once here
            string nodeName = node.DialogueName;

            // Set the Group refernece in the Node
            node.Group = group;

            // If the Group dictionary does not contain this group
            if (!groupedNodes.ContainsKey(group))
            {
                // It's a new group so add it
                groupedNodes.Add(group, new SerializableDictionary<string, DialogueSystemNodeErrorData>());
            }

            // If the group does not contain a node with the same name
            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                // Generate the error colour for use later
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();
                // Add the node
                nodeErrorData.Nodes.Add(node);
                groupedNodes[group].Add(nodeName, nodeErrorData);
                return;
            }

            // Reference the list once here
            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            // Add the node
            groupedNodesList.Add(node);

            // Reference that duplicated Node's error colour and apply it
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // If this is the second node then set the first node to have the error colour, subsequent duplicates won't need to set this as number 2 will already have done it
            if (groupedNodesList.Count == 2)
            {
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            // Reference these variables once here
            string nodeName = node.DialogueName;
            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            // Remove the Group reference from the Node
            node.Group = null;

            // Remove Node from Group
            groupedNodesList.Remove(node);
            // Reset the error status (it will be check again when the node is added to ungrouped nodes)
            node.ResetStyle();

            // If there only remains one node of that name in the group reset it's error status
            if (groupedNodesList.Count == 1)
            {
                groupedNodesList[0].ResetStyle();
                return;
            }

            // If there is no remaining Nodes of that name in tha t Group, remove the Key
            if(groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                // If removing that node results in the group being empty, remove the Group entirly
                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        public void AddGroup(DialogueSystemGroup group)
        {
            if(!groups.ContainsKey(group.title))
            {
                DialogueSystemGroupErrorData groupErrorData = new DialogueSystemGroupErrorData();
                groupErrorData.Groups.Add(group);
                groups.Add(group.title, groupErrorData);
                return;
            }

            List<DialogueSystemGroup> groupList = groups[group.title].Groups;
            groupList.Add(group);
            Color errorColor = groups[group.title].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (groupList.Count == 2)
            {
                groupList[0].SetErrorStyle(errorColor);
            }
        }
        
        public void RemoveGroup(DialogueSystemGroup group)
        {
            string oldGroupName = group.oldTitle;

            List<DialogueSystemGroup> groupList = groups[oldGroupName].Groups;
            groupList.Remove(group);
            group.ResetStlye();

            if(groupList.Count == 1)
            {
                groupList[0].ResetStlye();
                return;
            }

            if (groupList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
        #endregion

        #region Elements Addition
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DialogueSystemSearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "Dialogue System/DialogueSystemGraphViewStyles.uss", 
                "Dialogue System/DialogueSystemNodeStyles.uss"
                );
        }
        #endregion

        #region Utility
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
        #endregion
    }
}