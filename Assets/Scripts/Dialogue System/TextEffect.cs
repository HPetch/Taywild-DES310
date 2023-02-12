using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class TextEffect : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    // Rainbow Settings
    private readonly float rainbowStrength = 10f;
    private readonly float rainbowSpeed = 0.25f;

    // Wave Settings
    private readonly Vector2 waveStrength = new(0.1f, 0.1f);
    private readonly float waveHeight = 4f;
    private readonly float waveSpeed = 2f;

    // Jiggle Settings
    private readonly Vector2 jiggleStrength = new(4f, 2f);

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdateText();
    }

    public void UpdateText()
    {
#if UNITY_EDITOR
        textComponent = GetComponent<TextMeshProUGUI>();
#endif

        textComponent.ForceMeshUpdate();

        // Loops each link tag
        for (int i = 0; i < textComponent.textInfo.linkCount; i++)
        {
            TMP_LinkInfo link = textComponent.textInfo.linkInfo[i];

            switch (link.GetLinkID())
            {
                // Is it a rainbow tag? (<link="rainbow"></link>)
                case "rainbow":
                    // Loops all characters containing the rainbow link.
                    for (int character = link.linkTextfirstCharacterIndex; character < link.linkTextfirstCharacterIndex + link.linkTextLength; character++)
                    {
                        TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[character]; // Gets info on the current character
                        int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material

                        Color32[] newColors = textComponent.textInfo.meshInfo[materialIndex].colors32;

                        // Loop all vertexes of the current characters
                        for (int vert = 0; vert < 4; vert++)
                        {
                            if (charInfo.character == ' ') continue; // Skips spaces
                            int vertexIndex = charInfo.vertexIndex + vert;

                            // Rainbow effect.
                            Color32 rainbow = Color.HSVToRGB(((Time.time * rainbowSpeed) + (vertexIndex * (0.001f * rainbowStrength))) % 1f, 1f, 1f);

                            // Sets the new effect
                            newColors[vertexIndex] = rainbow;
                        }
                    }
                    break;

                // Is it a wave tag? (<link="wave"></link>)
                case "wave":
                    // Loops all characters containing the wave link.
                    for (int character = link.linkTextfirstCharacterIndex; character < link.linkTextfirstCharacterIndex + link.linkTextLength; character++)
                    {
                        TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[character]; // Gets info on the current character
                        int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material

                        Vector3[] newVertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

                        // Loop all vertexes of the current characters
                        for (int vert = 0; vert < 4; vert++)
                        {
                            if (charInfo.character == ' ') continue; // Skips spaces
                            int vertexIndex = charInfo.vertexIndex + vert;

                            // Wave effect.
                            //Vector3 offset = new Vector2(Mathf.Sin((Time.time * waveSpeed) + (vertexIndex * waveStrength.x)), Mathf.Cos((Time.time * waveSpeed) + (vertexIndex * waveStrength.y))) * waveHeight;
                            Vector3 offset = new Vector2(0f, Mathf.Cos((Time.time * waveSpeed) + (vertexIndex * waveStrength.y))) * waveHeight;

                            // Sets the new effect
                            newVertices[vertexIndex] += offset;
                        }
                    }
                    break;

                // Is it a jiggle tag? (<link="jiggle"></link>)
                case "jiggle":
                    // Loops all characters containing the jiggle link.
                    for (int character = link.linkTextfirstCharacterIndex; character < link.linkTextfirstCharacterIndex + link.linkTextLength; character++)
                    {
                        TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[character]; // Gets info on the current character
                        int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material                                               

                        Vector3[] newVertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

                        // jiggle effect.                        
                        Vector3 offset = Application.isPlaying ? new Vector2(Random.Range(-1, 1) * Time.deltaTime, Random.Range(-1, 1) * Time.deltaTime) * jiggleStrength * 100f : new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));

                        // Loop all vertexes of the current characters
                        for (int vert = 0; vert < 4; vert++)
                        {
                            if (charInfo.character == ' ') continue; // Skips spaces
                            int vertexIndex = charInfo.vertexIndex + vert;

                            // Sets the new effect
                            newVertices[vertexIndex] += offset;
                        }
                    }
                    break;
            }
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All); // IMPORTANT! applies all vertex and color changes.
    }

    public void ClearText()
    {       
        textComponent.ForceMeshUpdate(true, true);
        textComponent.textInfo.ClearAllMeshInfo();
        textComponent.textInfo.ResetVertexLayout(true);
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}