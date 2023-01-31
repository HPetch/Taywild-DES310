using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class TextmeshproEffect : MonoBehaviour
{
    [Tooltip("This component is **only** used for the ExecuteInEditMode")]
    [SerializeField] private TMP_Text textComponent;

    public Vector2 movementStrength = new(0.1f, 0.1f);
    public float movementSpeed = 1f;
    public float rainbowStrength = 10f;

    private void Update()
    {
#if !UNITY_EDITOR
        textComponent = DialogueController.Instance.UITemplate.TextField;
#endif
        textComponent.ForceMeshUpdate();

        // Loops each link tag
        foreach (TMP_LinkInfo link in textComponent.textInfo.linkInfo)
        {
            // Is it a rainbow tag? (<link="rainbow"></link>)
            if (link.GetLinkID() == "rainbow")
            {
                // Loops all characters containing the rainbow link.
                for (int character = link.linkTextfirstCharacterIndex; character < link.linkTextfirstCharacterIndex + link.linkTextLength; character++)
                {
                    TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[character]; // Gets info on the current character
                    int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material

                    Color32[] newColors = textComponent.textInfo.meshInfo[materialIndex].colors32;
                    Vector3[] newVertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

                    // Loop all vertexes of the current characters
                    for (int vert = 0; vert < 4; vert++)
                    {
                        if (charInfo.character == ' ') continue; // Skips spaces
                        int vertexIndex = charInfo.vertexIndex + vert;

                        // Offset and Rainbow effects.
                        Vector3 offset = new Vector2(Mathf.Sin((Time.realtimeSinceStartup * movementSpeed) + (vertexIndex * movementStrength.x)), Mathf.Cos((Time.realtimeSinceStartup * movementSpeed) + (vertexIndex * movementStrength.y))) * 10f;
                        Color32 rainbow = Color.HSVToRGB(((Time.realtimeSinceStartup * movementSpeed) + (vertexIndex * (0.001f * rainbowStrength))) % 1f, 1f, 1f);

                        // Sets the new effects
                        newColors[vertexIndex] = rainbow;
                        newVertices[vertexIndex] += offset;
                    }
                }
            }
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All); // IMPORTANT! applies all vertex and color changes.
    }
}