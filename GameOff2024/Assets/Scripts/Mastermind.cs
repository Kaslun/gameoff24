using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;


public class Mastermind : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI output;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private List<int> currentCode = new List<int>();
    [SerializeField]
    private string symbols = string.Empty;
    [SerializeField]
    private int codeLength = 4;
    [SerializeField]
    private CommandManager commandManager;
    [SerializeField]
    private ScreenManager screenManager;
    [SerializeField]
    private string[] hiddenCodes;
    [SerializeField]
    private bool gameOver = false;
    [SerializeField]
    private int maxFails = 5;
    [SerializeField]
    private int fails = 0;

    private void Start()
    {
        commandManager = FindFirstObjectByType<CommandManager>();
        screenManager = FindFirstObjectByType<ScreenManager>();
    }

    private void OnEnable()
    {
        gameOver = false;
        fails = 0;
        GenerateNumber();
        StartCoroutine(textManager.TypeText(output, "Welcome to Mastermind! Try to guess the correct 4-digit number in 5 tries\n'^' means higher\n'v' means lower\n'o means you have the correct number", true));
    }

    private void GenerateNumber()
    {
        currentCode.Clear();
        for (int i = 0; i < codeLength; i++)
        {
            currentCode.Add(Random.Range(0, 9));
        }
    }

    private void Update()
    {
        if (textManager.isRunning)
            return;

        if (!input.isFocused)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit"))
        {
            if (CommandManager.ParseCommand(input.text) == Commands.exit)
            {
                EndGame();
            }

            else
            {
                if (hiddenCodes.Contains<string>(input.text))
                {
                    print("You found a hidden code!");
                }
                else
                {
                    CheckInput();
                    print("Checking input");
                }
            }
        }
    }

    private void CheckInput()
    {
        if (input.text.Length > 4 || input.text.Length < 4)
            StartCoroutine(textManager.TypeText(output, "Input must be a 4-digit number", true));

        List<int> codeInput = new List<int>();
        symbols = string.Empty;

        foreach (char c in input.text)
        {
            if (int.TryParse(c.ToString(), out int i))
            {
                codeInput.Add(i);
            }
            else
            {
                StartCoroutine(textManager.TypeText(output, "Input must be a 4-digit number", true));
                break;
            }
        }

        for(int i = 0; i < codeInput.Count; i++)
        {
            if (codeInput[i] < currentCode[i])
            {
                symbols += "^";
            }
            if (codeInput[i] > currentCode[i])
            {
                symbols += "v";
            }
            if (codeInput[i] == currentCode[i])
            {
                symbols += "o";
            }
        }

        StartCoroutine(textManager.TypeText(output, symbols, false));
        StartCoroutine(CheckScore());

    }

    private IEnumerator CheckScore()
    {
        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        if (symbols == "oooo")
        {
            StartCoroutine(textManager.TypeText(output, "You win! Type 'exit' to end the game", true));
            gameOver = true;
        }

        if (fails >= maxFails)
        {
            StartCoroutine(textManager.TypeText(output, "You lost... Type 'exit' to end the game", true));
            gameOver = true;
        }

        input.ActivateInputField();
        input.text = "";
    }

    private void EndGame()
    {
        StartCoroutine(textManager.TypeText(output, "", true));
        screenManager.SwitchScreens(0);
    }
}
