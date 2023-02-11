using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Utilities;

    public class DialogueSystemMultipleChoiceNode : DialogueSystemNode
    {
        public override void Initialise(Vector2 position)
        {
            base.Initialise(position);
            DialogueType = DialogueTypes.MultipleChoice;
            Choices.Add("New Choice");
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */
            Button addChoiceButton = DialogueSystemElementUtility.CreateButton("Add Choice", () =>
                {
                    Port choicePort = CreateChoicePort("New Choice");
                    Choices.Add("New Choice");
                    outputContainer.Add(choicePort);
                });

            addChoiceButton.AddToClassList("ds-node__button");
            
            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT CONTAINER */
            foreach (string choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Element Creation
        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = DialogueSystemElementUtility.CreateButton("X");
            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DialogueSystemElementUtility.CreateTextField(choice);
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