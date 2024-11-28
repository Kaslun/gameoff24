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
    private string textFileName;

    [SerializeField]
    private string imageFileName;

    [SerializeField]
    private string folderName;

    private string folderPath;

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
    private TextMeshProUGUI textOutput;
    [SerializeField]
    private TextMeshProUGUI imageOutput;

    [SerializeField]
    private string wrongAnswers;
    [SerializeField] 
    private string wrongAnswersPrefix = "Wrong answers = ";
    [SerializeField]
    private string correctAnswer;
    [SerializeField]
    private string fillerAnswerText;

    private string previousGuesses;

    private bool gameOver;

    public void OnEnable()
    {
        folderPath = Application.streamingAssetsPath + "/" + folderName;
        PopulateOptions();
        PickRandomAnswer();

        input.text = "";
        wrongAnswers = string.Empty;
        LoadImage();
        fails = 0;
        score = 0;
        gameOver = false;
        previousGuesses = string.Empty;
    }

    private void PickRandomAnswer()
    {
        currentAnswer = answerOptions[Random.Range(0, answerOptions.Length)].ToLower();

        fillerAnswerText = string.Empty;

        foreach (char c in currentAnswer)
        {
            if (c == " "[0])
            {
                fillerAnswerText += " ";
            }
            else
            {
                fillerAnswerText += "_";
            }
        }

        StartCoroutine(textManager.TypeText(textOutput, fillerAnswerText, true));
    }

    private void Update()
    {
        if (!input.isFocused && !textManager.isRunning)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
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
                else if(!gameOver)
                {
                    StartCoroutine(CheckInput());
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
            StartCoroutine(textManager.TypeText(textOutput, "Input too long, please type only one symbol", true));
            yield break;
        }

        char c = input.text.ToLower().ToCharArray()[0];

        if (previousGuesses.Contains(c))
            yield break;

        previousGuesses += c;

        if (currentAnswer.Contains(c))
        {
            string tempFill = string.Empty;
            print("Guessed correct!");
            for (int i = 0; i < currentAnswer.Length; i++)
            {
                if (currentAnswer[i] == c)
                {
                    tempFill += c;
                    score++;
                }

                else if (fillerAnswerText[i] != '_')
                {
                    tempFill += fillerAnswerText[i];
                }

                else
                {
                    tempFill += '_';
                }
            }

            fillerAnswerText = tempFill;
            StartCoroutine(textManager.TypeText(textOutput, fillerAnswerText + "\n" + wrongAnswersPrefix + wrongAnswers, true));
            StartCoroutine(CheckScore());
        }
        else
        {
            if (wrongAnswers.Contains(input.text[0]))
                yield break;
            print("Guessed wrong...");
            fails++;
            wrongAnswers += input.text;

            StartCoroutine(textManager.TypeText(textOutput, fillerAnswerText + "\n" + wrongAnswersPrefix + wrongAnswers, true));
            while (textManager.isRunning)
            {
                print("Writing text: " + textOutput.name + " : " + wrongAnswers);
                yield return new WaitForSeconds(.1f);
            }

            LoadImage();

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
            StartCoroutine(textManager.TypeText(textOutput, "You win! Type 'exit' to end the game\n<color=#008000>Password = mastermind</color>", true));
            gameOver = true;
        }

        if(fails >= maxFails)
        {
            StartCoroutine(textManager.TypeText(textOutput, "You lost... Type 'exit' to end the game", true));
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
        print("Loading answers: " + folderPath + "/" + textFileName);
        answerOptions = textReader.ReadTextFile(textFileName, folderPath);
    }

    private void LoadImage()
    {
        int pageNum = fails + 1;
        print("Loading image: " + folderPath + "/"+ imageFileName + "_" + pageNum + ".txt");
        string[] asciiArt = textReader.ReadTextFile(imageFileName + "_" + pageNum + ".txt", folderPath);
        string outString = string.Empty;

        foreach (string s in asciiArt)
        {
            outString += s + "\n";
        }

        StartCoroutine(textManager.TypeText(imageOutput, outString, true));
    }
}
