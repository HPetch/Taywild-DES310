using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConversationEvent))]
public class ConversationEventDrawer : PropertyDrawer
{
    private const int lineHeightOffset = 2;
    private float lineHeight = EditorGUIUtility.singleLineHeight + lineHeightOffset;
    private int[] expandedHeight = { 6, 8, 2, 2 };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty EventType = property.FindPropertyRelative("<EventType>k__BackingField");
        int totalLines = 1;

        if (property.isExpanded)
        {
            totalLines += expandedHeight[EventType.intValue];
        }

        return lineHeight * totalLines + EditorGUIUtility.standardVerticalSpacing * totalLines;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty EventType = property.FindPropertyRelative("<EventType>k__BackingField");
        SerializedProperty UITemplate = property.FindPropertyRelative("<UITemplate>k__BackingField");
        SerializedProperty Character = property.FindPropertyRelative("<Character>k__BackingField");
        SerializedProperty Text = property.FindPropertyRelative("<Text>k__BackingField");
        SerializedProperty Quest = property.FindPropertyRelative("<Quest>k__BackingField");
        SerializedProperty Cutscene = property.FindPropertyRelative("<Cutscene>k__BackingField");

        EditorGUI.BeginProperty(position, label, property);

        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, System.Enum.GetName(typeof(ConversationEvent.ConversationEventType), EventType.intValue));
        int lines = 1;

        if (property.isExpanded)
        {
            Rect rectType = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
            EditorGUI.PropertyField(rectType, EventType);

            switch (EventType.intValue)
            {
                case (int)ConversationEvent.ConversationEventType.SPEECH:
                    {
                        Rect rectTemplate = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
                        Rect rectCharacter = new Rect(position.min.x, position.min.y + lines * lineHeight, position.size.x, lineHeight);
                        Rect rectText = new Rect(position.min.x, position.min.y + lines++ * lineHeight + lineHeightOffset * 2, position.size.x, lineHeight * 4);
                        
                        EditorGUI.PropertyField(rectTemplate, UITemplate);
                        EditorGUI.PropertyField(rectCharacter, Character);
                        EditorGUI.PropertyField(rectText, Text, GUIContent.none);
                    }
                    break;

                case (int)ConversationEvent.ConversationEventType.BRANCH:
                    {
                        Rect rectTemplate = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
                        Rect rectCharacter = new Rect(position.min.x, position.min.y + lines * lineHeight, position.size.x, lineHeight);
                        Rect rectText = new Rect(position.min.x, position.min.y + lines++ * lineHeight + lineHeightOffset * 2, position.size.x, lineHeight * 4);

                        EditorGUI.PropertyField(rectTemplate, UITemplate);
                        EditorGUI.PropertyField(rectCharacter, Character);
                        EditorGUI.PropertyField(rectText, Text, GUIContent.none);
                        // Branch code

                        lines += 5;
                        Rect rectHelpBox = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
                        EditorGUI.HelpBox(rectHelpBox, "Branch is the last node, any other events will *not* be processed", MessageType.Info);
                    }
                    break;

                case (int)ConversationEvent.ConversationEventType.QUEST:
                    Rect rectQuest = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
                    EditorGUI.PropertyField(rectQuest, Quest);
                    break;

                case (int)ConversationEvent.ConversationEventType.CUTSCENE:
                    Rect rectCutscene = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
                    EditorGUI.PropertyField(rectCutscene, Cutscene);
                    break;
            }
        }

        EditorGUI.EndProperty();
    }
}