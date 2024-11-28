using UnityEngine;
using TMPro;
using System;
using System.Collections;


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

    private string[] splitInput;

    [SerializeField]
    private GameObject[] screens;

    [SerializeField]
    private HelpManager helpManager;
    [SerializeField]
    private ScreenManager screenManager;
    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private ProgramManager programManager;
    [SerializeField]
    private LockManager lockManager;

    private bool isRunning = false;
    public bool isWaitingForPassword = false;

    private void Start()
    {
        helpManager = FindFirstObjectByType<HelpManager>();
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        programManager = FindFirstObjectByType<ProgramManager>();
        lockManager = FindFirstObjectByType<LockManager>();
        input.resetOnDeActivation = true;
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
    }

    private void Update()
    {
        if (!input.isFocused && !textManager.isRunning)
        {
            print("Focusing input field");
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
        {
            if (isWaitingForPassword)
            {
                print("Waiting for password");
                lockManager.TryUnlockFolder(input.text.ToLower());
                isWaitingForPassword = false;
            }
            else
            {
                print("Submitting command");
                StartCoroutine(SubmitCommand());
            }
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

        print("Running command");

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
                outString = "Programs in '" + programManager.currentFolderName + "':\n" + programManager.fileList;
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
            case Commands.shutdown:
                if (splitInput[0] == "shutdown")
                    break;
                screenManager.SwitchScreens(5);
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
            case Programs.zwork:
                screenManager.SwitchScreens(4);
                break;
        }

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
    shutdown,
    welcome
}

public enum Programs
{
    hangman,
    mastermind,
    globalthermalnuclearwarfare,
    error,
    zwork,
    work
}
