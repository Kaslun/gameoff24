using TMPro;
using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

public class TextManager : MonoBehaviour
{

    public bool isRunning = false;

    public IEnumerator TypeText(TextMeshProUGUI outputField, string stringOut)
    {
        isRunning = true;
        foreach (char c in stringOut)
        {
            outputField.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        isRunning = false;
    }

    public IEnumerator DeleteText(TextMeshProUGUI output)
    {
        isRunning = true;
        string outString = output.text;
        int stringLength = outString.Length;
        for (int i = 0; i < stringLength; i++)
        {
            outString = outString.Remove(outString.Length - 1);
            output.text = outString;
            yield return new WaitForSeconds(0.03f / 2);
        }
        isRunning = false;
    }
}