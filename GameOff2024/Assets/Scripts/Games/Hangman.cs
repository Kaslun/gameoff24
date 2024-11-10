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
    private TextMeshProUGUI[] textFields;

    [SerializeField]
    private char[] currentAnswer;

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
    private TMP_InputField input;
    [SerializeField]
    private TextMeshProUGUI output;

    [SerializeField]
    private string wrongAnswers;

    public bool isRunning;
    private bool gameOver;

    private void Start()
    {
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        wrongAnswers = "Wrong answers = ";
        isRunning = false;
        gameOver = false;
    }

    public void OnEnable()
    {
        PickRandomAnswer();
        input.text = "";
        wrongAnswers = "Wrong answers = ";
        fails = 0;
        score = 0;
        isRunning = true;
        gameOver = false;
    }

    private void PickRandomAnswer()
    {
        currentAnswer = answerOptions[Random.Range(0, answerOptions.Length)].ToLower().ToCharArray();
        PopulateAnswers(currentAnswer);

    }

    private void Update()
    {
        if (!input.isFocused && !textManager.isRunning)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
        {
            if (gameOver)
            {
                if (CommandManager.ParseCommand(input.text) == Commands.exit)
                {
                    StartCoroutine(EndGame());
                }
            }
            else
            {
                StartCoroutine(CheckInput());
                print("Checking input");
            }
        }
    }

    public IEnumerator CheckInput()
    {
        StartCoroutine(textManager.DeleteText(output));

        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        if (input.text.Length > 1)
        {
            StartCoroutine(textManager.TypeText(output, "Input too long, please type only one symbol"));
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
            print("Incorrect guess");
            fails++;
            wrongAnswers += input.text;

            StartCoroutine(textManager.DeleteText(output));

            while (textManager.isRunning)
            {
                yield return new WaitForSeconds(.1f);
                print("Deleting text: " + output.name);
            }

            StartCoroutine(textManager.TypeText(output, wrongAnswers));
            while (textManager.isRunning)
            {
                print("Writing text: " + output.name + " : " + wrongAnswers);
                yield return new WaitForSeconds(.1f);
            }

            StartCoroutine(CheckScore());
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
            StartCoroutine(textManager.TypeText(output, "You win! Type 'exit' to end the game"));
            gameOver = true;
        }

        if(fails >= maxFails)
        {
            StartCoroutine(textManager.DeleteText(output));
            while (textManager.isRunning)
            {
                yield return new WaitForSeconds(.1f);
            }

            StartCoroutine(textManager.TypeText(output, "You lost... Type 'exit' to end the game"));
            foreach(TextMeshProUGUI t in textFields)
            {
                t.gameObject.SetActive(true);
            }

            gameOver = true;
        }

        input.ActivateInputField();
    }

    private IEnumerator EndGame()
    {
        isRunning = false;

        StartCoroutine(textManager.DeleteText(output));

        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        screenManager.SwitchScreens(0);
    }

    private void PopulateAnswers(char[] answer)
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            textFields[i].text = answer[i].ToString();
            textFields[i].gameObject.SetActive(false);
        }
    }
}
