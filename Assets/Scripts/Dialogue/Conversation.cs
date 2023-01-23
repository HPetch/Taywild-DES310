using UnityEngine;

[System.Serializable]
public class Speech
{
    public Character character;

    [TextArea(3, 10)]
    public string text;
}

[CreateAssetMenu(fileName = "Conversation", menuName = "Dialogue/Conversation")]
public class Conversation : ScriptableObject
{
    public Speech[] converstation;

    [Space(10)] 

    [Range(0, 1)]
    public float ConversationStartDelay = 0.0f;
    [Range(0, 1)]
    public float ConversationEndDelay = 0.2f;

    //public Objective triggeredObjective;
}

[CreateAssetMenu(fileName = "Character", menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite image;
    public Color color;
}