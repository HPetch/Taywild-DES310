using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DialogueSystem.Windows
{
    using Utilities;

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private readonly string defaultFileName = "Dialogue File Name";
        private Button saveButton;

        [MenuItem("Window/Dialogue Graph")]
        public static void ShowExample()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolBar();
            AddStyles();
        }

        #region Elements Addition
        private void AddGraphView()
        {
            DialogueSystemGraphView graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            Toolbar toolBar = new Toolbar();
            
            TextField fileNameTextField = DialogueSystemElementUtility.CreateTextField(defaultFileName, "File Name:");
            saveButton = DialogueSystemElementUtility.CreateButton("Save");
            
            toolBar.Add(fileNameTextField);
            toolBar.Add(saveButton);

            toolBar.AddStyleSheets("Dialogue System/DialogueSystemToolbarStyles.uss");
            rootVisualElement.Add(toolBar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("Dialogue System/DialogueSystemVariables.uss");
        }
        #endregion

        #region Utility
        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        #endregion
    }
}