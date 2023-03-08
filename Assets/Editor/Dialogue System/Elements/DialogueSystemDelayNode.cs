using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Utilities;
    using Windows;
    using Types;

    public class DialogueSystemDelayNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = NodeTypes.Delay;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            mainContainer.style.backgroundColor = new Color(0.8f, 0.1f, 0.2f);

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "Next Node" };
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */
            foreach (DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            TextField delayTextField = DialogueSystemElementUtility.CreateTextArea(DialogueText, null, callback =>
            {
                if (float.TryParse(callback.newValue, out float delay))
                {
                    Delay = delay;
                    DialogueText = Delay.ToString();
                }
                else
                {
                    Debug.LogError("Cannot parse input into a float");
                }
            });

            delayTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            customDataContainer.Add(delayTextField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}