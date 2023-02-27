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
    
    public class DialogueSystemNode : Node
    {       
        public string ID { get; set; }

        public string DialogueName { get; set; }
        public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DialogueTypes DialogueType { get; set; }
        public DialogueSystemGroup Group { get; set; }

        protected DialogueSystemGraphView graphView;
        private Color defaultBackgroundColor;

        public virtual void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            graphView = dialogueSystemGraphView;

            DialogueName = nodeName;
            Choices = new List<DialogueSystemChoiceSaveData>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.one));

            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        #region Overrided Methods
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }
        #endregion

        public virtual void Draw()
        {
            /* TITLE CONTAINER */
            TextField dialogueNameTextField = DialogueSystemElementUtility.CreateTextField(DialogueName, null, callback =>
             {
                 TextField target = (TextField)callback.target;
                 target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                 if(string.IsNullOrEmpty(target.value))
                 {
                     if(!string.IsNullOrEmpty(DialogueName))
                     {
                         graphView.NameErrorsAmount++;
                     }
                 }
                 else
                 {
                     if(string.IsNullOrEmpty(DialogueName))
                     {
                         graphView.NameErrorsAmount--;
                     }
                 }

                 if (Group == null)
                 {
                     graphView.RemoveUngroupedNode(this);
                     DialogueName = target.value;
                     graphView.AddUngroupedNode(this);
                     return;
                 }

                 // Temporarly assign Group as RemoveGroupedNode will set this to null, and it's needed for AddGroupedNode
                 DialogueSystemGroup currentGroup = Group;
                 graphView.RemoveGroupedNode(this, Group);
                 DialogueName = target.value;
                 graphView.AddGroupedNode(this, currentGroup);
             });
            
            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextField);

            /* INPUT CONTAINER */
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DialogueSystemElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DialogueSystemElementUtility.CreateTextArea(Text, null, callback => { Text = callback.newValue; });
            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

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
            foreach(Port port in container.Children())
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

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        #endregion
    }
}