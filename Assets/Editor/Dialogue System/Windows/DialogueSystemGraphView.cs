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

    public class DialogueSystemGraphView : GraphView
    {
        public DialogueSystemGraphView()
        {
            AddManipulators();
            AddGridBackground();

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
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Dialogue Group", actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }
        #endregion

        #region Elements Creation
        private DialogueSystemNode CreateNode(DialogueSystemNode.DialogueTypes dialogueType, Vector2 nodePosition)
        {
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DialogueSystem{dialogueType}Node");
            DialogueSystemNode node = (DialogueSystemNode)Activator.CreateInstance(nodeType);

            node.Initialise(nodePosition);
            node.Draw();
            return node;
        }

        private Group CreateGroup(string title, Vector2 groupPosition)
        {
            Group group = new Group() { title = title };
            group.SetPosition(new Rect(groupPosition, Vector2.one));

            return group;
        }
        #endregion

        #region Elements Addition
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
    }
}