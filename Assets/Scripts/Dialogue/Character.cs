using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Color Colour { get; private set; }
}