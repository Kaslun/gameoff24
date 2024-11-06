using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Rendering;
using System.Linq;
using Unity.VisualScripting;
using System.Threading;

public class CommandManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private TextMeshProUGUI output;
    [SerializeField]
    private string errorMessage;
    [SerializeField, Range(0,1)]
    private float textSpeed = 0.1f;

    private bool isRunning = false;

    private void Start()
    {
        input.ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit") && !isRunning)
        {
            StartCoroutine(DeleteText());
            while (isRunning)
            {
                return;
            }

            print("Input recieved");
            print("User input: " + input.text);
            if (Enum.TryParse<Commands>(input.text.ToLower(), out Commands c))
            {
                RunCommand(c);
            }
            else
            {
                output.text += "<br>";
                StartCoroutine(TypeText(errorMessage));
            }

            input.text = "";
            input.DeactivateInputField();
        }
    }

    public void RunCommand(Commands command)
    {
        string outString = string.Empty;
        output.text += "<br>";
        print("Trying to run command");
        switch (command)
        {
            case Commands.help:
                outString = "Help";
                break;
            case Commands.exit:
                outString = "Exit";
                break;
            case Commands.list:
                outString = "List";
                break;
            case Commands.update:
                outString = "Update";
                break;
        }

        StartCoroutine(TypeText(outString));
    }

    public IEnumerator TypeText(string stringOut)
    {
        isRunning = true;
        foreach (char c in stringOut)
        {
            output.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isRunning = false;
        input.ActivateInputField();
    }

    public IEnumerator DeleteText()
    {
        print("Deleting text...");
        isRunning = true;
        string outString = output.text;
        int stringLength = outString.Length;
        print("Output: " + outString);
        for(int i = 0; i <= stringLength; i++)
        {
            print(i + " is being deleted");
            outString.Remove(outString.Length - 1);
            output.text = outString;
            yield return new WaitForSeconds(textSpeed);
        }
        isRunning = false;
    }
}

public enum Commands
{
    help,
    exit,
    list,
    update
}
