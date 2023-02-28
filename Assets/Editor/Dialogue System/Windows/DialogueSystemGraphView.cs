using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Collections.Generic;


namespace DialogueSystem.Windows
{
    using Data.Error;
    using Data.Save;
    using Utilities;
    using Elements;
    using Types;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        private MiniMap miniMap;

        // Dictionary that tracks all the groups on the graph
        private SerializableDictionary<string, DialogueSystemGroupErrorData> groups;
        // Dictionary that tracks all the ungrouped Nodes on the graph
        private SerializableDictionary<string, DialogueSystemNodeErrorData> ungroupedNodes;
        // Dictionary that tracks all the grouped nodes on the graph, using the group as a key
        private SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>> groupedNodes;

        // Tracks how many errors are in the graph
        private int nameErrorsAmount = 0;

        // Accessors to the private nameErrorsAmount
        public int NameErrorsAmount
        {
            get
            {
                return nameErrorsAmount;
            }
            set
            {
                nameErrorsAmount = value;
                if (nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                }
                else
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        /// <summary>
        /// Graph View Constructor, builds the graph by adding manipulators, search window, minimap, and grid background.
        /// </summary>
        /// <param name="dialogueSystemditorWindow"></param>
        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemditorWindow)
        {
            // Initialise varialbes and Dictionaries
            editorWindow = dialogueSystemditorWindow;
            ungroupedNodes = new SerializableDictionary<string, DialogueSystemNodeErrorData>();
            groups = new SerializableDictionary<string, DialogueSystemGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>>();

            // Add Visual Elements to the graph
            AddManipulators();
            AddSearchWindow();
            AddMiniMap();
            AddGridBackground();

            // Register the callbacks
            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            // Add the styles
            AddStyles();
            AddMiniMapStyles();
        }

        #region Overrided Methods
        /// <summary>
        /// Overrides the GetCompatiblePorts Method and only returns ports of a different node in a different direction.
        /// </summary>
        /// <returns> Returns a List of compatible Ports.</returns>
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
        /// <summary>
        /// Adds the ContentZoomer, ContentDragger, SelectionDragger, and the RectangleSelector manipulators as well as the contextual menu.
        /// </summary>
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateDialogueNodeContextualMenu("Add Dialogue Node (Single Choice)", DialogueTypes.SingleChoice));
            this.AddManipulator(CreateDialogueNodeContextualMenu("Add Dialogue Node (Multiple Choice)", DialogueTypes.MultipleChoice));
            this.AddManipulator(CreateEdgeNodeContextualMenu("Add Edge Node"));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateDialogueNodeContextualMenu(string actionTitle, DialogueTypes dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateDialogueNode("NodeName", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }

        private IManipulator CreateEdgeNodeContextualMenu(string actionTitle)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateEdgeNode(GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }
        #endregion

        #region Elements Creation
        /// <summary>
        /// Create a Dialogue Node.
        /// </summary>
        /// <param name="nodeName"> The name of the Node.</param>
        /// <param name="dialogueType"> The type of Node.</param>
        /// <param name="position"> The position of the Node.</param>
        /// <param name="shouldDraw"> If the node should be drawn on creation, defaulted to true. false when loading Nodes.</param>
        /// <returns> Returns a reference to the created Node.</returns>
        public DialogueSystemNode CreateDialogueNode(string nodeName, DialogueTypes dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DialogueSystem{dialogueType}DialogueNode");
            DialogueSystemNode node = (DialogueSystemNode)Activator.CreateInstance(nodeType);

            return CreateNode(node, nodeName, position, shouldDraw);
        }

        public DialogueSystemNode CreateEdgeNode(Vector2 position, bool shouldDraw = true)
        {
            DialogueSystemEdgeNode node = new DialogueSystemEdgeNode();
            return CreateNode(node, "", position, shouldDraw);
        }

        public DialogueSystemNode CreateNode(DialogueSystemNodeSaveData nodeData)
        {
            return nodeData.NodeType switch
            {
                NodeTypes.Dialogue => CreateDialogueNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false),
                NodeTypes.Edge => CreateEdgeNode(nodeData.Position, false),
                _ => null,
            };
        }

        private DialogueSystemNode CreateNode(DialogueSystemNode node, string nodeName, Vector2 position, bool shouldDraw = true)
        {
            node.Initialise(nodeName, this, position);

            if (shouldDraw) { node.Draw(); }

            AddUngroupedNode(node);

            return node;
        }

        /// <summary>
        /// Create a DialogueSystemGroup.
        /// </summary>
        /// <param name="title"> The name of the Group.</param>
        /// <param name="groupPosition"> The posititon of the Group on the Graph.</param>
        /// <returns> Returns the created Group.</returns>
        public DialogueSystemGroup CreateGroup(string title, Vector2 groupPosition)
        {
            DialogueSystemGroup group = new DialogueSystemGroup(title, groupPosition);
            AddGroup(group);
            AddElement(group);

            // When group is created check each element currently selected.
            foreach(GraphElement selectedElement in selection)
            {
                // If the selected element is not a Node continue.
                if (!(selectedElement is DialogueSystemNode)) continue;                

                // Add the Node to the Group that was just created.
                group.AddElement((DialogueSystemNode)selectedElement);
            }

            return group;
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Callback when some element(s) are deleted.
        /// </summary>
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                // Lists of elements to delete, cannot be done within the foreach loop.
                List<Edge> edgesToDelete = new List<Edge>();
                List<DialogueSystemNode> nodesToDelete = new List<DialogueSystemNode>();
                List<DialogueSystemGroup> groupsToDelete = new List<DialogueSystemGroup>();

                // For each element.
                foreach(GraphElement element in selection)
                {
                    // If the element is a Node.
                    if(element is DialogueSystemNode node)
                    {
                        // Add it to the Nodes to delete list.
                        nodesToDelete.Add(node);
                        continue;
                    }

                    // If the element is an Edge.
                    if (element is Edge edge)
                    {
                        // Add it to the Edges to delete list.
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    // If the element is a Group.
                    if (element is DialogueSystemGroup group)
                    {
                        // Add it to the Groups to delete list.
                        groupsToDelete.Add(group);
                        continue;
                    }
                }

                // For each Group to delete.
                foreach (DialogueSystemGroup group in groupsToDelete)
                {
                    // Reference to all the nodes in the group.
                    List<DialogueSystemNode> groupNodes = new List<DialogueSystemNode>();

                    // For each element in the group, add the Nodes to the List of Nodes in the Group.
                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (groupElement is DialogueSystemNode node) groupNodes.Add(node);
                    }

                    // Remove all the Nodes from the group, this puts them in the ungroupedNodes list,
                    // if they are also selected they will be deleted as ungrouped nodes using the nodesToDelete List.
                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);
                    RemoveElement(group);
                }

                // Can just safely delete all the connections here before the Nodes are deleted.
                DeleteElements(edgesToDelete);

                // For each Node to delete.
                foreach (DialogueSystemNode node in nodesToDelete)
                {
                    // Remove the node from the group, This calls the 'ElementsRemovedFromGroup' callback automatically.
                    if (node.Group != null) node.Group.RemoveElement(node);

                    RemoveUngroupedNode(node);
                    node.DisconnetAllPorts();
                    RemoveElement(node);
                }
            };
        }

        /// <summary>
        /// Callback when Elements are added to a Group.
        /// </summary>
        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                // Foreach Element.
                foreach (GraphElement element in elements)
                {
                    // If the Element is a Node.
                    if (element is DialogueSystemNode node)
                    {
                        // Remove the Node from the ungroupedNodes and add it to the groupedNodes.
                        RemoveUngroupedNode(node);
                        AddGroupedNode(node, (DialogueSystemGroup)group);
                    }
                }
            };
        }

        /// <summary>
        /// Callback when Elements are removed from a group.
        /// </summary>
        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                // Foreach Element.
                foreach (GraphElement element in elements)
                {
                    // If the Element is a Node.
                    if (element is DialogueSystemNode node)
                    {
                        // Remove the Node from the groupedNodes and add it to the ungroupedNodes.
                        RemoveGroupedNode(node, (DialogueSystemGroup)group);
                        AddUngroupedNode(node);
                    }
                }
            };
        }

        /// <summary>
        /// Callback when a Group is renamed.
        /// </summary>
        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DialogueSystemGroup dsGroup = (DialogueSystemGroup)group;
                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                // If the name is empty.
                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    // And it wasn't emtpy before (for example it's empty and they've added a space we don't increment the error amount).
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        // Add the error.
                        NameErrorsAmount++;
                    }
                }
                // If the name is not empty.
                else
                {
                    // And the previous name was.
                    if (string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        // Then the error has been fixed.
                        NameErrorsAmount--;
                    }
                }

                // Remove and add the group
                RemoveGroup(dsGroup);
                dsGroup.OldTitle = dsGroup.title;
                AddGroup(dsGroup);
            };
        }

        /// <summary>
        /// Callback when the graph view has changed
        /// </summary>
        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                // If there is some Edges created.
                if(changes.edgesToCreate != null)
                {
                    // For each Edge to created.
                    foreach(Edge edge in changes.edgesToCreate)
                    {
                        // Add the references to both Nodes.
                        DialogueSystemNode nextNode = (DialogueSystemNode)edge.input.node;

                        DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                // If there is some elements removed.
                if(changes.elementsToRemove != null)
                {
                    // For each element to remove.
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        // If some of them are Edges.
                        if (element is Edge edge)
                        {
                            // Reset the references on the output node.
                            DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData)edge.output.userData;
                            choiceData.NodeID = "";
                        }
                    }
                }

                return changes;
            };
        }
        #endregion

        #region Repeated Elements
        /// <summary>
        /// Adds a Node to the ungroupedNodes List.
        /// </summary>
        /// <param name="node"> Node to be added to the ungroupedNodes List.</param>
        public void AddUngroupedNode(DialogueSystemNode node)
        {
            // Make name lower-case so file names are not case sensative.
            string nodeName = node.NodeName.ToLower();

            // If ungrouped nodes does not contain a node with this name.
            if(!ungroupedNodes.ContainsKey(nodeName))
            {
                // Generate a new random colour used to represent duplicates so it can be referenced when a duplicate is made.
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();                
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            // Refernce the Node List once here.
            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            // Add the node.
            ungroupedNodesList.Add(node);

            // Reference that duplicated Node's error colour and apply it.
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // If this is the second node then set the first node to have the error colour, subsequent duplicates won't need to set this as number 2 will already have done it.
            if(ungroupedNodesList.Count == 2)
            {
                NameErrorsAmount++;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// Remove a Node from the ungroupedNodes List.
        /// </summary>
        /// <param name="node"> Node to be removed from the ungroupedNodes List.</param>
        public void RemoveUngroupedNode(DialogueSystemNode node)
        {
            // Make name lower-case so file names are not case sensative.
            string nodeName = node.NodeName.ToLower();
            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            // Remove the node from the List.
            ungroupedNodesList.Remove(node);

            // Reset the error style, it will be reset if this Node is being added to a Group.
            node.ResetStyle();

            // Only one Node left with this name, so reset the error style.
            if (ungroupedNodesList.Count == 1)
            {
                NameErrorsAmount--;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            // No Nodes left with this name so remove it's key from the dictionary.
            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        /// <summary>
        /// Add a Node to the groupedNodes Dictionary.
        /// </summary>
        /// <param name="node"> Node to be added to the groupedNodes Dictionary.</param>
        /// <param name="group"> Group that the Node will be added to.</param>
        public void AddGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            // Make name lower-case so file names are not case sensative.
            string nodeName = node.NodeName.ToLower();

            // Set the Group refernece in the Node.
            node.Group = group;

            // If the Group dictionary does not contain this group.
            if (!groupedNodes.ContainsKey(group))
            {
                // It's a new group so add it.
                groupedNodes.Add(group, new SerializableDictionary<string, DialogueSystemNodeErrorData>());
            }

            // If the group does not contain a node with the same name.
            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                // Generate the error colour for use later.
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();
                // Add the node
                nodeErrorData.Nodes.Add(node);
                groupedNodes[group].Add(nodeName, nodeErrorData);
                return;
            }

            // Reference the list once here.
            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            // Add the node.
            groupedNodesList.Add(node);
            
            // Reference that duplicated Node's error colour and apply it.
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // If this is the second node then set the first node to have the error colour, subsequent duplicates won't need to set this as number 2 will already have done it.
            if (groupedNodesList.Count == 2)
            {
                NameErrorsAmount++;
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// Removes Node from the groupedNodes Dictionary.
        /// </summary>
        /// <param name="node"> Node to be removed from the Dictionary.</param>
        /// <param name="group"> Group that the Node will be removed from.</param>
        public void RemoveGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            // Make name lower-case so file names are not case sensative.
            string nodeName = node.NodeName.ToLower();
            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            // Remove the Group reference from the Node.
            node.Group = null;

            // Remove Node from Group.
            groupedNodesList.Remove(node);
            // Reset the error status (it will be check again when the node is added to ungrouped nodes).
            node.ResetStyle();

            // If there only remains one node of that name in the group reset it's error status.
            if (groupedNodesList.Count == 1)
            {
                NameErrorsAmount--;
                groupedNodesList[0].ResetStyle();
                return;
            }

            // If there is no remaining Nodes of that name in that Group, remove the Key.
            if(groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                // If removing that node results in the group being empty, remove the Group entirly.
                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        /// <summary>
        /// Adds a new group to the Groups Dictionary.
        /// </summary>
        /// <param name="group"> Group to be added to the groups List.</param>
        private void AddGroup(DialogueSystemGroup group)
        {
            // Make name lower-case so file names are not case sensative.
            string groupName = group.title.ToLower();

            // If the Groups Dictionary does not already contain the name, then it's the first Group of this name.
            if (!groups.ContainsKey(groupName))
            {
                // Generate the Groups error colour for later.
                DialogueSystemGroupErrorData groupErrorData = new DialogueSystemGroupErrorData();
                
                // Add the group.
                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);
                return;
            }

            // Group name already exists/

            List<DialogueSystemGroup> groupList = groups[groupName].Groups;
            groupList.Add(group);
            
            // Add the error colour/
            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            // If this is the second Group then set the first Group to have the error colour, subsequent duplicates won't need to set this as number 2 will already have done it.
            if (groupList.Count == 2)
            {
                NameErrorsAmount++;
                groupList[0].SetErrorStyle(errorColor);
            }
        }
        
        /// <summary>
        /// Removes a Group from the groups Dictionary.
        /// </summary>
        /// <param name="group"> Group to be removed from the groups Disctionary.</param>
        private void RemoveGroup(DialogueSystemGroup group)
        {
            // Make name lower-case so file names are not case sensative.
            string oldGroupName = group.OldTitle.ToLower();

            List<DialogueSystemGroup> groupList = groups[oldGroupName].Groups;
            
            groupList.Remove(group);
            group.ResetStlye();

            // If there only remains one Group of that name in the Group reset it's error status.
            if (groupList.Count == 1)
            {
                NameErrorsAmount--;
                groupList[0].ResetStlye();
                return;
            }

            // If there is no remaining Groups of that name in that Group, remove the Key from the Dictionary.
            if (groupList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
        #endregion

        #region Elements Addition
        /// <summary>
        /// Adds the search window to the Graph View.
        /// </summary>
        private void AddSearchWindow()
        {
            // If the Search Window has not already been created.
            if (searchWindow == null)
            {
                // Create it and Initialise it.
                searchWindow = ScriptableObject.CreateInstance<DialogueSystemSearchWindow>();
                searchWindow.Initialise(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// Adds the Minimap VisualElement to the Graph View.
        /// </summary>
        private void AddMiniMap()
        {
            miniMap = new MiniMap() { anchored = true };
            miniMap.SetPosition(new Rect(15, 50, 200, 100));
            

            Add(miniMap);
            miniMap.visible = false;
        }

        /// <summary>
        /// Adds the Grid Background Visual Element to the Graph View.
        /// </summary>
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        /// <summary>
        /// Add the StyleSheet to the Graph View.
        /// </summary>
        private void AddStyles()
        {
            this.AddStyleSheets(
                "Dialogue System/DialogueSystemGraphViewStyles.uss", 
                "Dialogue System/DialogueSystemNodeStyles.uss"
                );
        }

        /// <summary>
        /// Add the Minimap styles to the Minimap.
        /// </summary>
        private void AddMiniMapStyles()
        {
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
            miniMap.style.color = new StyleColor(new Color32(0, 183, 235, 255));
        }
        #endregion

        #region Utility
        /// <summary>
        /// Converts screenSpaceMousePosition into GraphSpacePosiion
        /// </summary>
        /// <param name="mousePosition"> Screen Space Mouse Position</param>
        /// <param name="isSearchWindow"></param>
        /// <returns> Returns GraphSpace MousePosition.</returns>
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            if (isSearchWindow)
            {
                mousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            return localMousePosition;
        }

        /// <summary>
        /// Clears the Graph.
        /// </summary>
        public void ClearGraph()
        {
            // Remove the elements, automatically calls the remove callbacks.
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        /// <summary>
        /// Toggle the Minimap On/Off.
        /// </summary>
        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
        #endregion
    }
}