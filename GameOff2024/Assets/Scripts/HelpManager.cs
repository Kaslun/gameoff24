using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class HelpManager : MonoBehaviour
{
    [SerializeField]
    private string filePath;

    [SerializeField]
    private TextMeshProUGUI outputText;

    [SerializeField]
    private TextReader textReader;

    public List<string> fileNames = new List<string>();

    [SerializeField]
    private string[] currentPage;

    public int helpPageNumber = 0;

    private void Start()
    {
        //LoadFiles(filePath);
    }

    public string GetCurrentPage(int pageNum)
    {
        if (pageNum > fileNames.Count)
        {
            return null;
        }

        if(currentPage.Length <= 0)
        {
            currentPage = textReader.ReadTextFile(filePath);
        }

        string outString = string.Empty;

        foreach (string line in currentPage)
        {
            outString += "\n" + line;
        }

        return outString;
    }

    private void LoadFiles(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);

        FileInfo[] files = dir.GetFiles();

        string[] tempFileNames = new string[files.Length];

        foreach (FileInfo file in files)
        {
            fileNames.Add(file.Name);
        }
    }
}
