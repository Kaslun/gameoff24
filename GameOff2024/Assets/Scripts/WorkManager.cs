using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;

public class WorkManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    private TextMeshProUGUI output;
    [SerializeField]
    private string fileName;
    private string currentWord = "Hello";
    private int counter = 0;
    public string[] words;

    public bool canShutdown = false;

    [SerializeField]
    private string preFix = "Type: ";
    [SerializeField]
    private string postFix = "\n\nType 'exit' to leave.";
    [SerializeField]
    private string errorMessage = "Incorrect input. Your supervisor has been notified.";
    [SerializeField]
    private string secretWord;

    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private TextReader textReader;
    [SerializeField]
    private ScreenManager screenManager;
    [SerializeField]
    private CommandManager commandManager;

    private void OnDisable()
    {
        StartCoroutine(textManager.TypeText(output, "", true));
    }

    private void OnEnable()
    {
        counter = 0;
        if (words.Length <= 0)
        {
            PopulateWordList();
        }
        
        currentWord = NewWord();

        StartCoroutine(textManager.TypeText(output, preFix + currentWord, true));
    }

    private void Update()
    {
        if (!textManager.isRunning)
        {
            if (Input.GetButtonDown("Submit"))
            {
                if (input.text.ToLower() == "exit")
                {
                    screenManager.SwitchScreens(0);
                }

                if(input.text.ToLower() == "catharsis")
                {
                    print("Catharsis!");
                    canShutdown = true;

                    commandManager.RunCommand(Commands.shutdown);
                    return;
                }

                if (input.text.ToLower() == currentWord.ToLower())
                {
                    CorrectInput();
                }
                else if(input.text.ToLower() != currentWord.ToLower())
                {
                    print("Wrong... input was: " + input.text.ToLower());
                    print("Current word is: " + currentWord.ToLower());
                    WrongInput();
                }
            }

            if (!input.isFocused)
            {
                input.ActivateInputField();
            }
        }
    }

    private void PopulateWordList()
    {
        words = textReader.ReadTextFile(fileName, Application.streamingAssetsPath);
    }
    
    private string NewWord()
    {
        int rndNum = Random.Range(0, words.Length);
        string newWord = words[rndNum];

        counter++;

        if(counter % 5 == 0)
        {
            return secretWord;
        }

        while (newWord.ToLower() == currentWord.ToLower())
        {
            newWord = words[rndNum];
        }

        return newWord;
    }

    private void CorrectInput()
    {
        input.text = string.Empty;
        string newWord = NewWord();
        currentWord = newWord;


        if (currentWord.ToLower() == secretWord.ToLower())
        {
            print("Currentwork is the secret word");
            string colorWord = "<color=#008000>" + secretWord + "</color>";
            StartCoroutine(textManager.TypeText(output, preFix + colorWord + postFix, true));
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, preFix + currentWord + postFix, true));
        }
    }

    private void WrongInput()
    {
        input.text = string.Empty;
        string newWord = NewWord();
        currentWord = newWord;


        if (currentWord.ToLower() == secretWord.ToLower())
        {
            print("Currentwork is the secret word");
            string colorWord = "<color=#008000>" + secretWord + "</color>";
            StartCoroutine(textManager.TypeText(output, preFix + colorWord + postFix, true));
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, preFix + currentWord + postFix, true));
        }
    }
}
