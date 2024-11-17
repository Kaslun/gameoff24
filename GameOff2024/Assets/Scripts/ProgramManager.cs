using NUnit.Framework;
using System;
using System.IO;
using TMPro;
using UnityEngine;

public class ProgramManager : MonoBehaviour
{
    [SerializeField]
    public FileInfo[] files;

    [SerializeField]
    public DirectoryInfo[] folders;

    [SerializeField]
    private string fileName = "programs";

    [SerializeField]
    private string folderPath = Application.streamingAssetsPath;

    [SerializeField]
    public string textFileContent;

    [SerializeField]
    private TextMeshProUGUI output;

    [SerializeField]
    private TextReader textReader;

    [SerializeField]
    private TextManager textManager;

    [SerializeField]
    private CommandManager commandManager;

    private void Start()
    {
        PopulateFiles();

        if(textReader == null)
        {
            textReader = FindFirstObjectByType<TextReader>();
        }

        if (textManager == null)
        {
            textManager = FindFirstObjectByType<TextManager>();
        }

        if (commandManager == null)
        {
            commandManager = FindFirstObjectByType<CommandManager>();
        }
    }

    public void TryRunProgram(string name)
    {
        string[] input = name.Split(".".ToCharArray()[0]);
        string extension = input[1];

        print(extension);

        if (Enum.TryParse<FileType>(extension.ToLower(), out FileType fileType))
        {
            print("Parsed extension succesfully");
            textFileContent = string.Empty;
            string outString = string.Empty;

            foreach (FileInfo f in files)
            {
                if (name.ToLower() == f.Name.ToLower())
                {
                    switch (fileType)
                    {
                        case FileType.txt:
                            print("Textfile found!");
                            string jointContent = string.Empty;
                            string[] splitContent = textReader.ReadTextFile(input[0]);

                            foreach (string c in splitContent)
                            {
                                jointContent += c + "<br>";
                            }

                            StartCoroutine(textManager.TypeText(output, jointContent, true));
                            break;
                        case FileType.exe:
                            print("Running program: " + input[0]);
                            commandManager.RunProgram(input[0]);
                            break;
                    }
                }
            }
        }
        else
            print("Unable to parse");
    }

    private void PopulateFiles()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        files = dirInfo.GetFiles();
        folders = dirInfo.GetDirectories();
    }

    public string GetFileList()
    {
        if(files.Length <= 0)
        {
            PopulateFiles();
        }
        string fileList = string.Empty;

        foreach(DirectoryInfo d in folders)
        {
            print(d.FullName);
            if(d.Extension != ".meta")
            {
                fileList += d.Name + "      ";
            }
        }

        foreach(FileInfo f in files)
        {
            print(f.Extension);
            if (f.Extension != ".meta")
            {
                fileList += f.Name.ToLower() + "      ";
            }
        }

        return fileList;
    }

    public enum FileType
    {
        txt,
        json,
        exe
    }
        
}
