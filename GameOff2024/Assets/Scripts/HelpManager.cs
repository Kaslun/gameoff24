using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class HelpManager : MonoBehaviour
{
    [SerializeField]
    private string filePath = Application.dataPath + "/StreamingAssets/HelpPages";

    [SerializeField]
    private TextMeshProUGUI outputText;

    public List<string> fileNames = new List<string>();

    public int helpPageNumber = 0;

    private void Start()
    {
        //LoadFiles(filePath);
    }

    public void SetCurrentPage(int pageNum)
    {
        if (pageNum > fileNames.Count)
        {
            return;
        }

        string[] page = File.ReadAllLines(filePath + fileNames[pageNum]);

        foreach (string line in page)
        {
            outputText.text += "<br>" + line;
        }
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
