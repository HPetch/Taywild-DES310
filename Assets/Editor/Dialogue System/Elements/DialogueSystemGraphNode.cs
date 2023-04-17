using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace DialogueSystem.Elements
{
    using Windows;
    using Types;
    using ScriptableObjects;

    public class DialogueSystemGraphNode : DialogueSystemNode
    {
        public override void Initialise(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialise(nodeName, dialogueSystemGraphView, position);

            NodeType = NodeTypes.Graph;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            mainContainer.style.backgroundColor = new Color(0.5f, 0.0f, 0.5f);
        }

        public override void Draw()
        {
            base.Draw();

            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            ObjectField grapgObjectField = new ObjectField
            {
                objectType = typeof(DialogueSystemDialogueContainerSO),
                allowSceneObjects = false,
                value = Graph,
            };

            grapgObjectField.RegisterValueChangedCallback(callback => {
                Graph = grapgObjectField.value as DialogueSystemDialogueContainerSO;
            });

            customDataContainer.Add(grapgObjectField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}