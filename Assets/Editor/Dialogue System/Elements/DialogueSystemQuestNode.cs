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
    
    public class DialogueSystemQuestNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = NodeTypes.Quest;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            mainContainer.style.backgroundColor = new Color(1.0f, 0.5f, 0.0f);

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData() { Text = "Not Accepted" };
            Choices.Add(choiceData);

            choiceData = new DialogueSystemChoiceSaveData() { Text = "In Progress" };
            Choices.Add(choiceData);

            choiceData = new DialogueSystemChoiceSaveData() { Text = "Read for Hand-in" };
            Choices.Add(choiceData);

            choiceData = new DialogueSystemChoiceSaveData() { Text = "Comlpeted" };
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

            EnumField questEnumField = new EnumField(Quests.LucanQuest1);

            questEnumField.RegisterValueChangedCallback(callback => {
                Quest = (Quests)questEnumField.value;
            });

            customDataContainer.Add(questEnumField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}