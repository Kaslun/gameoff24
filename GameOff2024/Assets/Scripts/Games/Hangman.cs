using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using System.Collections;

public class Hangman : MonoBehaviour
{
    [SerializeField]
    private string[] answerOptions;

    [SerializeField]
    private string fileName;

    [SerializeField]
    private TextMeshProUGUI[] textFields;

    [SerializeField]
    private string currentAnswer;

    [SerializeField] 
    private string guessedAnswer;

    [SerializeField] 
    private string[] hiddenCodes;

    [SerializeField]
    private int score;

    [SerializeField]
    private int fails;

    [SerializeField]
    private int maxFails;

    [SerializeField]
    private ScreenManager screenManager;

    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private TextReader textReader;

    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private TextMeshProUGUI output;

    [SerializeField]
    private string wrongAnswers;
    [SerializeField]
    private string correctAnswers;

    private bool gameOver;

    private void Start()
    {
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        textReader = FindFirstObjectByType<TextReader>();
        wrongAnswers = "Wrong answers = ";
        gameOver = false;
        PopulateOptions();
    }

    public void OnEnable()
    {
        PickRandomAnswer();
        PopulateOptions();

        input.text = "";
        wrongAnswers = "Wrong answers = ";
        fails = 0;
        score = 0;
        gameOver = false;
    }

    private void PickRandomAnswer()
    {
        currentAnswer = answerOptions[Random.Range(0, answerOptions.Length)].ToLower();
        PopulateAnswers(currentAnswer);

    }

    private void Update()
    {
        if (textManager.isRunning)
        {
            return;
        }
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
                    StartCoroutine(CheckInput());
                    print("Checking input");
                }
            }
        }
    }

    public IEnumerator CheckInput()
    {
        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        if (input.text.Length > 1)
        {
            StartCoroutine(textManager.TypeText(output, "Input too long, please type only one symbol", true));
            yield break;
        }

        if (textManager.isRunning)
        {
            print("TextManager is running");
            yield break;
        }

        char c = input.text.ToLower().ToCharArray()[0];

        if (currentAnswer.Contains<char>(c))
        {
            print("Guessed correct!");
            for (int i = 0; i < currentAnswer.Length; i++)
            {
                if(currentAnswer[i] == c)
                {
                    textFields[i].gameObject.SetActive(true);
                    score++;
                    StartCoroutine(CheckScore());
                }
            }
        }
        else
        {
            if (wrongAnswers.Contains<char>(input.text[0]))
            {
                print("Incorrect guess");
                fails++;
                wrongAnswers += input.text;

                StartCoroutine(textManager.TypeText(output, wrongAnswers, true));
                while (textManager.isRunning)
                {
                    print("Writing text: " + output.name + " : " + wrongAnswers);
                    yield return new WaitForSeconds(.1f);
                }

                StartCoroutine(CheckScore());
            }
        }
        input.text = "";
    }

    private IEnumerator CheckScore()
    {
        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        if (score >= currentAnswer.Length)
        {
            StartCoroutine(textManager.TypeText(output, "You win! Type 'exit' to end the game", true));
            gameOver = true;
        }

        if(fails >= maxFails)
        {
            StartCoroutine(textManager.TypeText(output, "You lost... Type 'exit' to end the game", true));
            foreach(TextMeshProUGUI t in textFields)
            {
                t.gameObject.SetActive(true);
            }

            gameOver = true;
        }

        input.ActivateInputField();
    }

    private void EndGame()
    {
        screenManager.SwitchScreens(0);
    }


    private void PopulateOptions()
    {
        answerOptions = textReader.ReadTextFile(fileName);
    }

    private void PopulateAnswers(string answer)
    {
        for(int i = 0; i < answer.Length; i++)
        {
            foreach (char c in correctAnswers)
            {
                if(answer[i] == c)
                {
                    guessedAnswer += answer[i];
                    break;
                }
            }
        }

        for (int i = 0; i < textFields.Length; i++)
        {
            textFields[i].text = answer[i].ToString();
            textFields[i].gameObject.SetActive(false);
        }
    }
}
