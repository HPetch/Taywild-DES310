using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConversationUITemplate))]
public class ConversationUITemplatePropertyDrawer : PropertyDrawer
{
    private readonly float lineHeight = EditorGUIUtility.singleLineHeight + 2;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLines = 1;

        if (property.isExpanded)
        {
            totalLines += 3;
        }

        return lineHeight * totalLines + EditorGUIUtility.standardVerticalSpacing * totalLines;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty characterImage = property.FindPropertyRelative("<CharacterImage>k__BackingField");
        SerializedProperty characterName = property.FindPropertyRelative("<CharacterName>k__BackingField");
        SerializedProperty textField = property.FindPropertyRelative("<TextField>k__BackingField");

        EditorGUI.BeginProperty(position, label, property);

        int index = System.Convert.ToInt32(property.propertyPath.Substring(property.propertyPath.IndexOf("[")).Replace("[", "").Replace("]", ""));
        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, System.Enum.GetName(typeof(ConversationUITemplate.ConversationUITemplates), index));
        int lines = 1;


        if (property.isExpanded)
        {
            Rect rectImage = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
            EditorGUI.PropertyField(rectImage, characterImage);

            Rect rectName = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
            EditorGUI.PropertyField(rectName, characterName);

            Rect rectText = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
            EditorGUI.PropertyField(rectText, textField);
        }

        EditorGUI.EndProperty();
    }
}
