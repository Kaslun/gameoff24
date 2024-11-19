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
    private string correctAnswers;

    private bool gameOver;

    private void Start()
    {
        screenManager = FindFirstObjectByType<ScreenManager>();
        textManager = FindFirstObjectByType<TextManager>();
        textReader = FindFirstObjectByType<TextReader>();
    }

    public void OnEnable()
    {
        PickRandomAnswer();
        PopulateOptions();

        input.text = "";
        wrongAnswers = "Wrong answers = ";
        LoadImage();
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

                StartCoroutine(textManager.TypeText(textOutput, wrongAnswers, false));
                while (textManager.isRunning)
                {
                    print("Writing text: " + textOutput.name + " : " + wrongAnswers);
                    yield return new WaitForSeconds(.1f);
                }

                LoadImage();

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
            StartCoroutine(textManager.TypeText(textOutput, "You win! Type 'exit' to end the game", true));
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
        answerOptions = textReader.ReadTextFile(textFileName);
    }

    private void LoadImage()
    {
        int pageNum = fails + 1;
        string[] asciiArt = textReader.ReadTextFile(folderName + "/" + imageFileName + "_" + pageNum);
        string outString = string.Empty;

        foreach (string s in asciiArt)
        {
            outString += s + "\n";
        }

        StartCoroutine(textManager.TypeText(imageOutput, outString, true));
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
