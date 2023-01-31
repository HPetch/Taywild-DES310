using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class FancyText : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private bool fancyText = false;

    private void Update()
    {
        textField = GetComponent<TextMeshProUGUI>();

        string text = textField.text;
        string output = "";
        string label = "";

        foreach (char letter in text.ToCharArray())
        {
            // If the current letter is part of some fancy text we do not want to delay between the 'char's as the user won't see them
            if (letter == '[')
            {
                fancyText = true;
                label = "";
            }
            else if (fancyText)
            {
                if (letter == ']')
                {
                    fancyText = false;
                    ProcessLabel(label);
                }
                else
                {
                    label += letter;
                }
            }
            else
            {
                output += letter;
            }
        }

        textField.text = output;
    }

    private void ProcessLabel(string label)
    {

    }
}

public class FancyTextEffect
{
    List<char> effectString = new List<char>();
}