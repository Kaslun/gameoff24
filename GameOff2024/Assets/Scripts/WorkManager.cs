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
    private TextManager textManager;
    [SerializeField]
    private TextReader textReader;
    [SerializeField]
    private ScreenManager screenManager;

    private void Start()
    {
    }

    private void OnEnable()
    {
        if (words.Length <= 0)
        {
            PopulateWordList();
        }
        
        currentWord = NewWord();

        StartCoroutine(textManager.TypeText(output, preFix + currentWord));
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            if(input.text.ToLower() == currentWord.ToLower() && !textManager.isRunning)
            {
                StartCoroutine(CorrectInput());
            }
            if (input.text.ToLower() == "exit" && !textManager.isRunning)
            {
                screenManager.SwitchScreens(0);
            }
        }
    }

    private void PopulateWordList()
    {
        print("Getting words");
        words = textReader.ReadTextFile(fileName);
        print("Found " + words.Length + " words");
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

    private IEnumerator CorrectInput()
    {
        StartCoroutine(textManager.DeleteText(output));

        while (textManager.isRunning)
        {
            yield return new WaitForSeconds(.1f);
        }

        input.text = string.Empty;
        string newWord = NewWord();
        currentWord = newWord;

        StartCoroutine(textManager.TypeText(output, preFix + newWord));
    }
}
