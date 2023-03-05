using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Dialogue/Character")]
public class DialogueCharacter : ScriptableObject
{
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public Sprite[] Portraits { get; private set; }
    [field: SerializeField] public Color Colour { get; private set; }
}