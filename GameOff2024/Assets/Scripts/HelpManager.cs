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

    public string GetCurrentPage(int pageNum)//hjernen er ikke p� plass nok til � hekte p� help_01 som intromelding, please do this (flyttet commands til help_02 s� alle kan leses og satte det til � v�re help)
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
