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
    [SerializeField]
    private string[] splitInput;

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
    [SerializeField]
    private ProgramManager programManager;

    private bool lookingForGame = false;
    private bool isRunning = false;

    private void Start()
    {
        helpManager = FindFirstObjectByType<HelpManager>();
        hangmanManager = FindFirstObjectByType<Hangman>();
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        programManager = FindFirstObjectByType<ProgramManager>();
        input.resetOnDeActivation = true;
    }

    private void OnDisable()
    {
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

    private string[] SplitInput(string input)
    {
        string[] splitInput = input.Split(" ".ToCharArray()[0]);
        return splitInput;
    }

    public IEnumerator SubmitCommand()
    {
        splitInput = SplitInput(input.text);

        string s = splitInput[0].ToLower();

        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        RunCommand(ParseCommand(s));

        input.text = "";
        input.DeactivateInputField();
    }

    public void RunCommand(Commands command)
    {
        string outString = string.Empty;
        print("Trying to run command");

        if (splitInput.Length > 1)
            print("Split input 1: " + splitInput[1]);

        switch (command)
        {
            case Commands.help:
                outString = helpManager.GetCurrentPage(0);
                break;
            case Commands.run:
                if (splitInput.Length <= 1)
                    outString = errorMessage;
                else
                    programManager.TryRunProgram(splitInput[1], ProgramManager.FileType.exe);
                break;
            case Commands.read:
                if (splitInput.Length <= 1)
                    outString = errorMessage;
                else
                    programManager.TryRunProgram(splitInput[1], ProgramManager.FileType.txt);
                break;
            case Commands.exit:
                if(programManager.folderLayerCount >= 1)
                {
                    programManager.ExitFolder();
                }
                break;
            case Commands.list:
                outString = "Programs in '" + programManager.currentFolderName + "':\n" + programManager.GetFileList();
                break;
            case Commands.enter:
                if (splitInput.Length <= 1)
                    outString = errorMessage;
                else
                    programManager.TryRunProgram(splitInput[1], ProgramManager.FileType.dir);
                break;
            case Commands.error:
                outString = errorMessage;
                break;
            case Commands.welcome:
                outString = welcomeMessage;
                break;
        }

        StartCoroutine(textManager.TypeText(output, outString, true));
    }

    public static Commands ParseCommand(string input)
    {
        if (Enum.TryParse<Commands>(input, out Commands c))
        {
            return c;
        }

        else
            return Commands.error;
    }

    private Programs ParseProgram(string input)
    {
        print(input);

        if (Enum.TryParse<Programs>(input, out Programs g))
        {
            return g;
        }

        else
            return Programs.error;
    }

    public void RunProgram(string programName)
    {
        Programs program = ParseProgram(programName);

        switch (program)
        {
            case Programs.error:
                RunCommand(Commands.error);
                break;
            case Programs.hangman:
                screenManager.SwitchScreens(1);
                break;
            case Programs.globalthermalnuclearwarfare:
                StartCoroutine(textManager.TypeText(output, "Wouldn't you rather like to play a game of hangman, Dr. Falken?", true));
                break;
            case Programs.work:
                screenManager.SwitchScreens(2);
                break;
            case Programs.mastermind:
                screenManager.SwitchScreens(3);
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
    enter,
    list,
    read,
    error,
    welcome
}

public enum Programs
{
    hangman,
    mastermind,
    globalthermalnuclearwarfare,
    error,
    work
}
