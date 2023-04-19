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

    public class DialogueSystemSetQuestNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = NodeTypes.SetQuest;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            mainContainer.style.backgroundColor = new Color(1.5f, 0.75f, 0.0f);

            DialogueSystemChoiceSaveData choiceData;

            choiceData = new DialogueSystemChoiceSaveData() { Text = "Next Node" };
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

            EnumField questEnumField = new EnumField(QuestTypes.LucasFlowerQ1a);

            questEnumField.RegisterValueChangedCallback(callback =>
            {
                Quest = (QuestTypes)questEnumField.value;
            });

            customDataContainer.Add(questEnumField);

            EnumField queststateEnumField = new EnumField(QuestStates.InProgress);

            queststateEnumField.RegisterValueChangedCallback(callback =>
            {
                QuestState = (QuestStates)queststateEnumField.value;
            });

            customDataContainer.Add(queststateEnumField);


            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}