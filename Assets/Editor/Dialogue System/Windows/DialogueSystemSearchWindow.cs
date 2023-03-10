using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Windows
{
    using Elements;
    using Types;

    public class DialogueSystemSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueSystemGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialise(DialogueSystemGraphView dialogueSystemGraphView)
        {
            graphView = dialogueSystemGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),

                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice Node", indentationIcon))
                {
                    userData = DialogueTypes.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice Node", indentationIcon))
                {
                    userData = DialogueTypes.MultipleChoice,
                    level = 2
                },

                new SearchTreeGroupEntry(new GUIContent("Events"), 1),
                new SearchTreeEntry(new GUIContent("Sound Effect Node", indentationIcon))
                {
                    userData = new DialogueSystemAudioNode(),
                    level = 2
                },

                new SearchTreeGroupEntry(new GUIContent("Utility"), 1),
                new SearchTreeEntry(new GUIContent("Edge Node", indentationIcon))
                {
                    userData = new DialogueSystemEdgeNode(),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Delay Node", indentationIcon))
                {
                    userData = new DialogueSystemDelayNode(),
                    level = 2
                },

                new SearchTreeGroupEntry(new GUIContent("Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case DialogueTypes.SingleChoice:
                    {
                        DialogueSystemSingleChoiceDialogueNode singleChoiceNode = (DialogueSystemSingleChoiceDialogueNode)graphView.CreateDialogueNode("NodeName", DialogueTypes.SingleChoice, localMousePosition);

                        graphView.AddElement(singleChoiceNode);

                        return true;
                    }

                case DialogueTypes.MultipleChoice:
                    {
                        DialogueSystemMultipleChoiceDialogueNode multipleChoiceNode = (DialogueSystemMultipleChoiceDialogueNode)graphView.CreateDialogueNode("NodeName", DialogueTypes.MultipleChoice, localMousePosition);

                        graphView.AddElement(multipleChoiceNode);

                        return true;
                    }

                case DialogueSystemAudioNode:
                    {
                        DialogueSystemAudioNode audioNode = (DialogueSystemAudioNode)graphView.CreateAudioNode("NodeName", localMousePosition);

                        graphView.AddElement(audioNode);

                        return true;
                    }

                case DialogueSystemEdgeNode:
                    {
                        DialogueSystemEdgeNode edgeNode = (DialogueSystemEdgeNode)graphView.CreateEdgeNode(localMousePosition);

                        graphView.AddElement(edgeNode);

                        return true;
                    }

                case DialogueSystemDelayNode:
                    {
                        DialogueSystemDelayNode delayNode = (DialogueSystemDelayNode)graphView.CreateDelayNode("NodeName", localMousePosition);

                        graphView.AddElement(delayNode);

                        return true;
                    }

                case DialogueSystemGroup:
                    {
                        graphView.CreateGroup("DialogueGroup", localMousePosition);
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }
    }
}