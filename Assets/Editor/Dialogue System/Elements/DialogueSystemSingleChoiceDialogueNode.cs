using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Utilities;
    using Windows;
    using Types;

    public class DialogueSystemSingleChoiceDialogueNode : DialogueSystemDialogueNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);
            DialogueType = DialogueTypes.SingleChoice;

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "Next Node" };
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */ 
            foreach(DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}