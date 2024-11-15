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
    private TextMeshProUGUI hangmanOutput;
    [SerializeField]
    private string errorMessage;
    [SerializeField]
    private string welcomeMessage = "Welcome to Corporate INC!";

    [SerializeField, Range(0,1)]
    private float textSpeed = 0.1f;
    [SerializeField]
    private GameObject[] screens;

    [SerializeField]
    private HelpManager helpManager;
    [SerializeField]
    private Hangman hangmanManager;
    [SerializeField]
    private ScreenManager screenManager;
    [SerializeField]
    private TextManager textManager;

    private bool lookingForGame = false;
    private bool isRunning = false;

    private void Start()
    {
        helpManager = FindFirstObjectByType<HelpManager>();
        hangmanManager = FindFirstObjectByType<Hangman>();
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        input.resetOnDeActivation = true;
    }

    private void OnDisable()
    {
        print("Commands are disabled");
        isRunning = false;
    }

    private void OnEnable()
    {
        EnableCommands();
    }

    private void EnableCommands()
    {
        if(output.text != string.Empty)
        {
            output.text = string.Empty;
        }
        RunCommand(Commands.welcome);
        input.ActivateInputField();
        isRunning = true;
        print("Enabled commands");
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (!input.isFocused && !textManager.isRunning)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
        {
            StartCoroutine(SubmitCommand());
        }
    }

    public IEnumerator SubmitCommand()
    {
        StartCoroutine(textManager.DeleteText(output));

        string s = input.text.Replace(" ", "");

        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }


        if (lookingForGame)
        {
            if (Enum.TryParse<Programs>(s.ToLower(), out Programs g))
            {
                RunGame(g);
                input.text = "";
                yield break;
            }
        }

        if (Enum.TryParse<Commands>(s.ToLower(), out Commands c))
        {
            RunCommand(c);
        }
        else
        {
            RunCommand(Commands.error);
        }

        input.text = "";
        input.DeactivateInputField();
    }

    public void RunCommand(Commands command)
    {
        string outString = string.Empty;
        print("Trying to run command");
        switch (command)
        {
            case Commands.help:
                helpManager.SetCurrentPage(helpManager.helpPageNumber);
                screenManager.SwitchScreens(1);
                break;
            case Commands.run:
                lookingForGame = true;
                outString = "Which program do you want to run?";
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
            case Commands.error:
                outString = errorMessage;
                break;
            case Commands.runhangman:
                RunGame(Programs.hangman);
                break;
            case Commands.welcome:
                outString = welcomeMessage;
                break;
        }

        StartCoroutine(textManager.TypeText(output, outString));
    }

    public static Commands ParseCommand(string input)
    {
        if (Enum.TryParse<Commands>(input.ToLower(), out Commands c))
        {
            return c;
        }

        else
            return Commands.error;
    }

    public static Programs ParseGame(string input)
    {
        if (Enum.TryParse<Programs>(input.ToLower(), out Programs g))
        {
            return g;
        }

        else
            return Programs.error;
    }

    private void RunGame(Programs program)
    {
        switch (program)
        {
            case Programs.error:
                RunCommand(Commands.error);
                break;
            case Programs.hangman:
                screenManager.SwitchScreens(1);
                break;
            case Programs.globalthermalnuclearwarfare:
                StartCoroutine(textManager.TypeText(output, "Wouldn't you rather like to play a game of chess, Dr. Falken?"));
                break;
            case Programs.work:
                screenManager.SwitchScreens(2);
                break;
        }

        lookingForGame = false;
        input.text = "";
    }
}

public enum Commands
{
    help,
    exit,
    run,
    runhangman,
    list,
    error,
    welcome,
    update,
}

public enum Programs
{
    hangman,
    globalthermalnuclearwarfare,
    error,
    work
}
