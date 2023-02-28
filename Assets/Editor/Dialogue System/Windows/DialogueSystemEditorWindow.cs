using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DialogueSystem.Windows
{
    using Utilities;

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private DialogueSystemGraphView graphView;

        private readonly string defaultFileName = "DialogueFileName";
        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

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
            Button loadButton = DialogueSystemElementUtility.CreateButton("Load", () => Load());
            Button clearButton = DialogueSystemElementUtility.CreateButton("Clear", () => DisplayClearGraphPopup());
            Button resetButton = DialogueSystemElementUtility.CreateButton("Reset", () => DisplayResetGraphPopup());
            miniMapButton = DialogueSystemElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolBar.Add(fileNameTextField);
            toolBar.Add(saveButton);
            toolBar.Add(loadButton);
            toolBar.Add(clearButton);
            toolBar.Add(resetButton);
            toolBar.Add(miniMapButton);

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
            if (string.IsNullOrEmpty(fileNameTextField.value))
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

        private void Load()
        {
            bool toLoad = EditorUtility.DisplayDialog(
                "Are you sure?",
                "You will lose any unsaved data.\nDo you wish to continue?",
                "Load"
                );

            if (!toLoad) return;

            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/Dialogue System/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath)) return;

            Clear();

            DialogueSystemIOUtility.Initialise(graphView, Path.GetFileNameWithoutExtension(filePath));
            DialogueSystemIOUtility.Load();
        }

        private void DisplayClearGraphPopup()
        {
            bool toClear = EditorUtility.DisplayDialog(
                "Are you sure?",
                "This will clear the enitre graph, but retain the file name.\nDo you wish to continue?",
                "Clear"
                );

            if (toClear) Clear();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void DisplayResetGraphPopup()
        {
            bool toReset = EditorUtility.DisplayDialog(
                "Are you sure?",
                "This will clear the enitre graph, including the file name.\nDo you wish to continue?",
                "Reset"
                );

            if (toReset) ResetGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();
            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }
        #endregion

        #region Utility
        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

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