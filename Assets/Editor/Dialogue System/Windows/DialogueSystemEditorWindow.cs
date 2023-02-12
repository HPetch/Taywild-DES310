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
        private DialogueSystemGraphView graphView;

        private readonly string defaultFileName = "DialogueFileName";
        private TextField fileNameTextField;
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
            graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            Toolbar toolBar = new Toolbar();
            
            fileNameTextField = DialogueSystemElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = DialogueSystemElementUtility.CreateButton("Save", () => Save());
            
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

        #region Toolbar Actions
        private void Save()
        {
            if(string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalide file name.",
                    "Please ensure the file name you have entered is valid.\nAsk Max for details.",
                    "Continue"
                    );

                return;
            }

            DialogueSystemIOUtility.Initialise(graphView, fileNameTextField.value);
            DialogueSystemIOUtility.Save();
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