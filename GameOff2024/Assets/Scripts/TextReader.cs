using System.IO;
using UnityEngine;

public class TextReader : MonoBehaviour
{
    private string filePath = Application.streamingAssetsPath;

    public string[] ReadTextFile(string fileName)
    {
        string[] fileContent = File.ReadAllLines(Path.Combine(filePath, fileName + ".txt"));

        return fileContent;
    }

    public string[] ReadJSONFile(string fileName)
    {

        return null;
    }
}
