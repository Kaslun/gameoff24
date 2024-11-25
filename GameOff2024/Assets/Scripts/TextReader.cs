using System.IO;
using UnityEngine;

public class TextReader : MonoBehaviour
{
    public string[] ReadTextFile(string fileName, string filePath)
    {
        print("Trying to read file: " +  fileName);

        string[] fileContent = File.ReadAllLines(Path.Combine(filePath, fileName));

        return fileContent;
    }
}
