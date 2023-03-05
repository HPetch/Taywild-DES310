using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DialogueSystem.Elements
{
    using Utilities;
    using Windows;

    public class DialogueSystemDialogueNode : DialogueSystemNode
    {
        protected Color defaultBackgroundColor;

        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = Types.NodeTypes.Dialogue;
            DialogueText = "Dialogue text.";

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
        }

        public override void Draw()
        {
            base.Draw();

            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            ObjectField characterObjectField = new ObjectField
            {
                objectType = typeof(DialogueCharacter),
                allowSceneObjects = false,
                value = Character,
            };

            characterObjectField.RegisterValueChangedCallback(callback => {
                Character = characterObjectField.value as DialogueCharacter;
                SetCharacterStyle(Character);
            });

            SetCharacterStyle(Character);

            Foldout textFoldout = DialogueSystemElementUtility.CreateFoldout("Dialogue Text");

            TextField dialoguetextTextField = DialogueSystemElementUtility.CreateTextArea(DialogueText, null, callback => { DialogueText = callback.newValue; });

            dialoguetextTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );
            
            customDataContainer.Add(characterObjectField);
            textFoldout.Add(dialoguetextTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        public void SetCharacterStyle(DialogueCharacter character)
        {
            if (character) mainContainer.style.backgroundColor = character.Colour;
            else ResetCharacterStyle();
        }

        public void ResetCharacterStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}