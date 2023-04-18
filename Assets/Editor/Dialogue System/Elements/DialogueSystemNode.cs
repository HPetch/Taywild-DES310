using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Utilities;
    using Windows;
    using Types;
    using ScriptableObjects;

    public class DialogueSystemNode : Node
    {
        public string ID { get; set; }

        public string NodeName { get; set; }
        public NodeTypes NodeType { get; set; }
        public DialogueSystemGroup Group { get; set; }

        public List<DialogueSystemChoiceSaveData> Choices { get; set; }

        public DialogueTypes DialogueType { get; set; }
        public DialogueCharacter Character { get; set; }
        public string DialogueText { get; set; }

        public AudioClip SoundEffect { get; set; }
        public float Delay { get; set; }
        public DialogueSystemDialogueContainerSO Graph { get; set; }
        public QuestTypes Quest { get; set; }
        

        protected DialogueSystemGraphView graphView;
        protected Color defaultBorderColor;
        protected float defaultBorderWidth;

        private TextField nodeNameTextField;

        public virtual void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            graphView = dialogueSystemGraphView;

            NodeName = nodeName;
            Choices = new List<DialogueSystemChoiceSaveData>();

            SetPosition(new Rect(position, Vector2.one));

            defaultBorderColor = titleContainer.style.borderTopColor.value;
            defaultBorderWidth = titleContainer.style.borderTopWidth.value;
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */
            nodeNameTextField = DialogueSystemElementUtility.CreateTextField(NodeName, null, callback =>
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(NodeName))
                    {
                        graphView.NameErrorsAmount++;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(NodeName))
                    {
                        graphView.NameErrorsAmount--;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                    NodeName = target.value;
                    graphView.AddUngroupedNode(this);
                    return;
                }

                // Temporarly assign Group as RemoveGroupedNode will set this to null, and it's needed for AddGroupedNode
                DialogueSystemGroup currentGroup = Group;
                graphView.RemoveGroupedNode(this, Group);
                NodeName = target.value;
                graphView.AddGroupedNode(this, currentGroup);
            });

            nodeNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, nodeNameTextField);

            /* INPUT CONTAINER */
            Port inputPort = this.CreatePort("Input Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input Connection";
            inputContainer.Add(inputPort);            
        }

        protected void RenameNode(string _nodeName)
        {
            nodeNameTextField.value = _nodeName;
        }

        #region Overrided Methods
        public override void BuildContextualMenu(ContextualMenuPopulateEvent menuEvent)
        {
            menuEvent.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            menuEvent.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(menuEvent);
        }
        #endregion

        #region Utility
        public void DisconnetAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            return !((Port)inputContainer.Children().First()).connected;
        }

        public void SetErrorStyle(Color errorColor)
        {
            titleContainer.style.borderTopColor = errorColor;
            titleContainer.style.borderTopWidth = 5f;
        }

        public void ResetErrorStyle()
        {
            titleContainer.style.borderTopColor = defaultBorderColor;
            titleContainer.style.borderTopWidth = defaultBorderWidth;
        }
        #endregion
    }
}