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

                new SearchTreeGroupEntry(new GUIContent("Functionality"), 1),
                new SearchTreeEntry(new GUIContent("Sound Effect Node", indentationIcon))
                {
                    userData = new DialogueSystemAudioNode(),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Graph Node", indentationIcon))
                {
                    userData = new DialogueSystemGraphNode(),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Get Quest Node", indentationIcon))
                {
                    userData = new DialogueSystemGetQuestNode(),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Set Quest Node", indentationIcon))
                {
                    userData = new DialogueSystemSetQuestNode(),
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
                        DialogueSystemAudioNode audioNode = (DialogueSystemAudioNode)graphView.CreateAudioNode("AudioNode", localMousePosition);

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
                        DialogueSystemDelayNode delayNode = (DialogueSystemDelayNode)graphView.CreateDelayNode("DelayNode", localMousePosition);

                        graphView.AddElement(delayNode);

                        return true;
                    }

                case DialogueSystemGraphNode:
                    {
                        DialogueSystemGraphNode graphNode = (DialogueSystemGraphNode)graphView.CreateGraphNode("GraphNode", localMousePosition);

                        graphView.AddElement(graphNode);

                        return true;
                    }

                case DialogueSystemGetQuestNode:
                    {
                        DialogueSystemGetQuestNode questNode = (DialogueSystemGetQuestNode)graphView.CreateGetQuestNode("GetQuestNode", localMousePosition);

                        graphView.AddElement(questNode);

                        return true;
                    }

                case DialogueSystemSetQuestNode:
                    {
                        DialogueSystemSetQuestNode questNode = (DialogueSystemSetQuestNode)graphView.CreateSetQuestNode("SetQuestNode", localMousePosition);

                        graphView.AddElement(questNode);

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