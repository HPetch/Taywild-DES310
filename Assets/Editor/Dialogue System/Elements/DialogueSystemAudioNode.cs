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

    public class DialogueSystemAudioNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = NodeTypes.Audio;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            mainContainer.style.backgroundColor = new Color(0.2f, 0.8f, 0.1f);

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

            ObjectField AudioClipObjectField = new ObjectField
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = SoundEffect,
            };

            AudioClipObjectField.RegisterValueChangedCallback(callback => {
                SoundEffect = AudioClipObjectField.value as AudioClip;
            });

            customDataContainer.Add(AudioClipObjectField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}