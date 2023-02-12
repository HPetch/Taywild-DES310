using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Utilities;
    using Windows;
    using Types;

    public class DialogueSystemMultipleChoiceNode : DialogueSystemNode
    {
        public override void Initialise(DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(dialogueSystemGraphView, position);
            DialogueType = DialogueTypes.MultipleChoice;

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "New Choice" };
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */
            Button addChoiceButton = DialogueSystemElementUtility.CreateButton("Add Choice", () =>
                {
                    DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "New Choice" };
                    Choices.Add(choiceData);

                    Port choicePort = CreateChoicePort(choiceData);
                    outputContainer.Add(choicePort);
                });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT CONTAINER */
            foreach (DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Element Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;
            DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData)userData;

            Button deleteChoiceButton = DialogueSystemElementUtility.CreateButton("X", () =>
                {
                    if (Choices.Count == 1)
                    {
                        return;
                    }

                    if (choicePort.connected)
                    {
                        graphView.DeleteElements(choicePort.connections);
                    }

                    Choices.Remove(choiceData);
                    graphView.RemoveElement(choicePort);
                });

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DialogueSystemElementUtility.CreateTextField(choiceData.Text, null, callback => { choiceData.Text = callback.newValue; });
            choiceTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
        #endregion
    }
}