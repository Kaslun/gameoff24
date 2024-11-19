using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class ProgramManager : MonoBehaviour
{
    [SerializeField]
    public List<FileInfo> files = new List<FileInfo>();

    [SerializeField]
    public List<DirectoryInfo> folders = new List<DirectoryInfo>();

    private string folderPath = Application.streamingAssetsPath;

    [SerializeField]
    public string currentFolderName;

    [SerializeField]
    private string mainFolderName;

    [SerializeField]
    private string hiddenFolderName;

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

    [SerializeField]
    public int folderLayerCount = 0;

    private bool breakFunction = false;

    private void Start()
    {
        if (textReader == null)
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

        folderPath += "/" + mainFolderName + "/";
        currentFolderName = mainFolderName;
        PopulateFiles();

    }

    public void TryRunProgram(string name, FileType inputType )
    {
        string[] input = new string[2];
        string extension = string.Empty;

        if (name.Contains("."))
        {
            input = name.Split(".".ToCharArray()[0]);
            extension = input[1];
            print(extension);
        }
        else
        {
            extension = "dir";
        }

        if (Enum.TryParse<FileType>(extension.ToLower(), out FileType fileType))
        {
            print("Parsed extension succesfully");
            print("Extension = " + inputType.ToString());

            if (fileType != inputType)
            {
                switch (fileType)
                {
                    case FileType.txt:
                        StartCoroutine(textManager.TypeText(output, "Wrong command. Use 'Read example.txt' to read text tiles", true));
                        break;
                    case FileType.exe:
                        StartCoroutine(textManager.TypeText(output, "Wrong command. Use 'Run example.exe' to read text tiles", true));
                        break;
                    case FileType.dir:
                        StartCoroutine(textManager.TypeText(output, "Wrong command. Use 'Enter example.dir' to read text tiles", true));
                        break;
                }
            }
            else
            {
                switch (inputType)
                {
                    case FileType.txt:
                        foreach (FileInfo f in files)
                        {
                            if (name.ToLower() == f.Name.ToLower())
                            {
                                print("Textfile found!");
                                string jointContent = string.Empty;
                                string[] splitContent = textReader.ReadTextFile(input[0]);

                                foreach (string c in splitContent)
                                {
                                    jointContent += c + "\n";
                                }

                                StartCoroutine(textManager.TypeText(output, jointContent, true));
                            }
                            else
                            {
                                StartCoroutine(textManager.TypeText(output, "Couldn't find file.", true));
                            }
                        }
                        break;
                    case FileType.exe:
                        foreach (FileInfo f in files)
                        {
                            if (name.ToLower() == f.Name.ToLower())
                            {
                                print("Running program: " + input[0]);
                                commandManager.RunProgram(input[0]);
                                break;
                            }
                            print("Running...");
                        }
                        StartCoroutine(textManager.TypeText(output, "Couldn't find program.", true));
                        break;
                    case FileType.dir:
                        print("Checking folders...");
                        foreach (DirectoryInfo di in folders)
                        {
                            if (di.Name.ToLower() == name.ToLower())
                            {
                                print("Found folder: " + name);
                                EnterFolder(name);
                            }
                        }
                        break;
                }
            }
        }
    }

    public void EnterFolder(string folderName)
    {
        folderPath += "/" + folderName;
        PopulateFiles();
        currentFolderName += "/" + folderName;
        folderLayerCount++;
        commandManager.RunCommand(Commands.list);
    }

    public void ExitFolder()
    {
        folderPath = folderPath.Remove(folderPath.Length - currentFolderName.Length + 1);
        folderLayerCount--;
        currentFolderName = mainFolderName;
        PopulateFiles();
        commandManager.RunCommand(Commands.list);
    }

    private void PopulateFiles()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

        files = dirInfo.GetFiles().ToList<FileInfo>();
        folders = dirInfo.GetDirectories().ToList<DirectoryInfo>();
    }

    public string GetFileList()
    {
        if(files.Count <= 0)
        {
            PopulateFiles();
        }
        string fileList = string.Empty;

        foreach(DirectoryInfo d in folders)
        {
            if(d.Extension != ".meta")
            {
                fileList += d.Name + ".dir" + "      ";
            }
        }

        foreach(FileInfo f in files)
        {
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
        dir,
        exe
    }
        
}
