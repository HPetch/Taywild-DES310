using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace DialogueSystem.Elements
{
    using Utilities;
    using Windows;

    public class DialogueSystemNode : Node
    {
        public enum DialogueTypes { SingleChoice, MultipleChoice }

        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DialogueTypes DialogueType { get; set; }
        public DialogueSystemGroup Group { get; set; }

        private DialogueSystemGraphView graphView;
        private Color defaultBackgroundColor;

        public virtual void Initialise(DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            graphView = dialogueSystemGraphView;

            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.one));

            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */
            TextField dialogueNameTextField = DialogueSystemElementUtility.CreateTextField(DialogueName, null, callback =>
             {
                 if (Group == null)
                 {
                     graphView.RemoveUngroupedNode(this);
                     DialogueName = callback.newValue;
                     graphView.AddUngroupedNode(this);
                     return;
                 }

                 // Temporarly assign Group as RemoveGroupedNode will set this to null, and it's needed for AddGroupedNode
                 DialogueSystemGroup currentGroup = Group;
                 graphView.RemoveGroupedNode(this, Group);
                 DialogueName = callback.newValue;
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

            TextField textTextField = DialogueSystemElementUtility.CreateTextArea(Text);
            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}
