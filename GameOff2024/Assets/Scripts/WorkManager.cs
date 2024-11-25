using System.Collections;
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
    public string[] words;

    [SerializeField]
    private string preFix = "Type: ";
    [SerializeField]
    private string postFix = "\n\nType 'exit' to leave.";
    [SerializeField]
    private string errorMessage = "Incorrect input. Your supervisor has been notified.";

    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private TextReader textReader;
    [SerializeField]
    private ScreenManager screenManager;

    private void OnDisable()
    {
        StartCoroutine(textManager.TypeText(output, "", true));
    }

    private void OnEnable()
    {
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

                if (input.text.ToLower() == currentWord.ToLower())
                {
                    CorrectInput();
                }
                else if(input.text.ToLower() != currentWord.ToLower())
                {
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

        while (newWord == currentWord)
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

        StartCoroutine(textManager.TypeText(output, preFix + newWord + postFix, true));
    }

    private void WrongInput()
    {
        input.text = string.Empty;
        string newWord = NewWord();
        currentWord = newWord;

        StartCoroutine(textManager.TypeText(output, errorMessage + "\n" + preFix + newWord + postFix, true));
    }
}
