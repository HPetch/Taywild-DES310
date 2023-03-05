using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Utilities;
    using Windows;

    public class DialogueSystemEdgeNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);
            
            NodeType = Types.NodeTypes.Edge;
            NodeName = $"EdgeNode{ID}";

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "" };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            titleContainer.Clear();

            /* INPUT CONTAINER */
            Port inputPort = this.CreatePort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "";
            inputContainer.Add(inputPort);

            /* OUTPUT CONTAINER */
            foreach (DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}