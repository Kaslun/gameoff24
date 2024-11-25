using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class HelpManager : MonoBehaviour
{
    [SerializeField]
    private string filePath;

    [SerializeField]
    private string folderPath;

    [SerializeField]
    private TextReader textReader;

    public int helpPageNumber = 0;

    public string GetCurrentPage(int pageNum)
    {
        string[] page = textReader.ReadTextFile(filePath, Application.streamingAssetsPath + "/" + folderPath);

        string outString = string.Empty;

        foreach (string line in page)
        {
            outString += "\n" + line;
        }

        return outString;
    }
}
