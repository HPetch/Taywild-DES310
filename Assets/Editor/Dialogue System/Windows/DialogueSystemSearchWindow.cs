using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Windows
{
    using Elements;

    public class DialogueSystemSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueSystemGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(DialogueSystemGraphView dialogueSystemGraphView)
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
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = DialogueSystemNode.DialogueTypes.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = DialogueSystemNode.DialogueTypes.MultipleChoice,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Groups"), 1),
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
                case DialogueSystemNode.DialogueTypes.SingleChoice:
                    {
                        DialogueSystemSingleChoiceNode singleChoiceNode = (DialogueSystemSingleChoiceNode)graphView.CreateNode(/*"DialogueName", */DialogueSystemNode.DialogueTypes.SingleChoice, localMousePosition);

                        graphView.AddElement(singleChoiceNode);

                        return true;
                    }

                case DialogueSystemNode.DialogueTypes.MultipleChoice:
                    {
                        DialogueSystemMultipleChoiceNode multipleChoiceNode = (DialogueSystemMultipleChoiceNode)graphView.CreateNode(/*"DialogueName", */DialogueSystemNode.DialogueTypes.MultipleChoice, localMousePosition);

                        graphView.AddElement(multipleChoiceNode);

                        return true;
                    }

                case DialogueSystemGroup _:
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