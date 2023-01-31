using UnityEngine;
using UnityEditor;

public class ConversationEventDrawerBase : PropertyDrawer
{
    protected float lineHeight = EditorGUIUtility.singleLineHeight + 2;
    protected int[] expandedHeight = { 6, 8, 2, 2 };

    protected SerializedProperty EventType;
    protected SerializedProperty UITemplate;
    protected SerializedProperty Character;
    protected SerializedProperty Text;
    protected SerializedProperty Quest;
    protected SerializedProperty Cutscene;

    protected virtual void Initialise(SerializedProperty property)
    {
        EventType = property.FindPropertyRelative("<EventType>k__BackingField");

        UITemplate = property.FindPropertyRelative("<UITemplate>k__BackingField");
        Character = property.FindPropertyRelative("<Character>k__BackingField");
        Text = property.FindPropertyRelative("<Text>k__BackingField");

        Quest = property.FindPropertyRelative("<Quest>k__BackingField");
        Cutscene = property.FindPropertyRelative("<Cutscene>k__BackingField");
    }

    // The property drawer for the speech event, returns the lines used
    protected int DrawSpeech(Rect position, int lines)
    {
        Rect rectTemplate = new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight);
        Rect rectCharacter = new Rect(position.min.x, position.min.y + lines * lineHeight, position.size.x, lineHeight);
        Rect rectText = new Rect(position.min.x, position.min.y + lines++ * lineHeight + 4, position.size.x, lineHeight * 4 + 4);

        EditorGUI.PropertyField(rectTemplate, UITemplate);
        EditorGUI.PropertyField(rectCharacter, Character);
        EditorGUI.PropertyField(rectText, Text, GUIContent.none);

        return lines + 4;
    }

    protected virtual int DrawBranch(Rect position, int lines)
    {
        return 0;
    }

    protected float GetHeight(int lines)
    {
        return lineHeight * lines + EditorGUIUtility.standardVerticalSpacing * lines;
    }
}

[CustomPropertyDrawer(typeof(ConversationEvent))]
public class ConversationEventDrawer : ConversationEventDrawerBase
{
    private SerializedProperty BranchType;
    private SerializedProperty BranchEvents;
    private SerializedProperty BranchConversations;

    protected override void Initialise(SerializedProperty property)
    {
        base.Initialise(property);

        BranchType = property.FindPropertyRelative("<BranchType>k__BackingField");
        BranchEvents = property.FindPropertyRelative("<BranchEvents>k__BackingField");
        BranchConversations = property.FindPropertyRelative("<BranchConversations>k__BackingField");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Initialise(property);

        if (property.isExpanded)
        {
            int totalLines = 1 + expandedHeight[EventType.intValue];

            float height = GetHeight(totalLines);
            if (EventType.intValue == (int)ConversationEvent.ConversationEventTypes.BRANCH)
            {
                if (BranchType.intValue == (int)ConversationEvent.BranchTypes.SHALLOW && BranchEvents.isExpanded)
                {
                    height += EditorGUI.GetPropertyHeight(BranchEvents);
                }
                else if (BranchType.intValue == (int)ConversationEvent.BranchTypes.DEEP && BranchConversations.isExpanded)
                {
                    height += EditorGUI.GetPropertyHeight(BranchConversations) + GetHeight(1);
                }
            }
            return height;
        }

        return GetHeight(1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialise(property);

        EditorGUI.BeginProperty(position, label, property);

        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, System.Enum.GetName(typeof(ConversationEvent.ConversationEventTypes), EventType.intValue));
        int lines = 1;

        if (property.isExpanded)
        {
            EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), EventType);

            switch (EventType.intValue)
            {
                case (int)ConversationEvent.ConversationEventTypes.SPEECH:
                    lines = DrawSpeech(position, lines);
                    break;

                case (int)ConversationEvent.ConversationEventTypes.BRANCH:
                    lines = DrawSpeech(position, lines);
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), BranchType);

                    switch (BranchType.intValue)
                    {
                        case (int)ConversationEvent.BranchTypes.SHALLOW:
                            BranchEvents.arraySize = BranchEvents.arraySize < 2 ? 2 : BranchEvents.arraySize > 4 ? 4 : BranchEvents.arraySize;
                            EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), BranchEvents);
                            lines += (int)(EditorGUI.GetPropertyHeight(BranchEvents) / lineHeight);
                            break;

                        case (int)ConversationEvent.BranchTypes.DEEP:
                            BranchConversations.arraySize = BranchConversations.arraySize < 2 ? 2 : BranchConversations.arraySize > 4 ? 4 : BranchConversations.arraySize;
                            EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), BranchConversations);
                            lines += (int)(EditorGUI.GetPropertyHeight(BranchConversations) / lineHeight);
                            EditorGUI.HelpBox(new Rect(position.min.x, position.min.y + lines++ * lineHeight + 6, position.size.x, lineHeight), "Deep Branch is the last node, any other events will *not* be processed", MessageType.Info);
                            break;
                    }

                    break;

                case (int)ConversationEvent.ConversationEventTypes.QUEST:
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), Quest);
                    break;

                case (int)ConversationEvent.ConversationEventTypes.CUTSCENE:
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), Cutscene);
                    break;
            }
        }

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(ShallowBranchConversationEvent))]
public class ShallowBranchConversationEventDrawer : ConversationEventDrawerBase
{
    private SerializedProperty BranchConversations;

    protected override void Initialise(SerializedProperty property)
    {
        base.Initialise(property);
        BranchConversations = property.FindPropertyRelative("<BranchConversations>k__BackingField");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Initialise(property);

        if (property.isExpanded)
        {
            int totalLines = 1 + expandedHeight[EventType.intValue];

            float height = GetHeight(totalLines);
            if (EventType.intValue == (int)ConversationEvent.ConversationEventTypes.BRANCH)
            {
                height += EditorGUI.GetPropertyHeight(BranchConversations) + GetHeight(1);

            }
            return height;
        }

        return GetHeight(1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialise(property);

        EditorGUI.BeginProperty(position, label, property);

        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, lineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, System.Enum.GetName(typeof(ConversationEvent.ConversationEventTypes), EventType.intValue));
        int lines = 1;

        if (property.isExpanded)
        {
            EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), EventType);

            switch (EventType.intValue)
            {
                case (int)ConversationEvent.ConversationEventTypes.SPEECH:
                    lines = DrawSpeech(position, lines);
                    break;

                case (int)ConversationEvent.ConversationEventTypes.BRANCH:
                    lines = DrawSpeech(position, lines);

                    BranchConversations.arraySize = BranchConversations.arraySize < 2 ? 2 : BranchConversations.arraySize > 4 ? 4 : BranchConversations.arraySize;
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), BranchConversations);
                    lines += (int)(EditorGUI.GetPropertyHeight(BranchConversations) / lineHeight);
                    EditorGUI.HelpBox(new Rect(position.min.x, position.min.y + lines++ * lineHeight + 6, position.size.x, lineHeight), "Deep Branch is the last node, any other events will *not* be processed", MessageType.Info);

                    break;

                case (int)ConversationEvent.ConversationEventTypes.QUEST:
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), Quest);
                    break;

                case (int)ConversationEvent.ConversationEventTypes.CUTSCENE:
                    EditorGUI.PropertyField(new Rect(position.min.x, position.min.y + lines++ * lineHeight, position.size.x, lineHeight), Cutscene);
                    break;
            }
        }

        EditorGUI.EndProperty();
    }
}