using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    using Utilities;
    using Windows;
    using Types;

    public class DialogueSystemSingleChoiceNode : DialogueSystemNode
    {
        public override void Initialise(DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(dialogueSystemGraphView, position);
            DialogueType = DialogueTypes.SingleChoice;
            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */ 
            foreach(string choice in Choices)
            {
                Port choicePort = this.CreatePort(choice);

                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}