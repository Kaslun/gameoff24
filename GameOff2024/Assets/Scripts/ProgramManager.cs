using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class ProgramManager : MonoBehaviour
{
    public List<FileInfo> files = new List<FileInfo>();

    public List<DirectoryInfo> folders = new List<DirectoryInfo>();

    private string folderPath = Application.streamingAssetsPath;

    [SerializeField]
    public string currentFolderName;

    [SerializeField]
    private string mainFolderName;

    [SerializeField]
    private string hiddenFolderName;

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
    private LockManager lockManager;

    [SerializeField]
    public int folderLayerCount = 0;

    public string fileList;

    private void Start()
    {

        textReader = FindFirstObjectByType<TextReader>();
        textManager = FindFirstObjectByType<TextManager>();
        commandManager = FindFirstObjectByType<CommandManager>();
        lockManager = FindFirstObjectByType<LockManager>();

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

            if(inputType == FileType.dir && extension == "enc")
            {
                inputType = FileType.enc;
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, $"Could not find {name}", true));
            return;
        }

        if (Enum.TryParse<FileType>(extension.ToLower(), out FileType fileType))
        {
            print("Parsed extension succesfully");
            print("Extension = " + inputType.ToString());

            if (fileType != inputType)
            {
                print("Checking if wrong filetype");
                switch (fileType)
                {
                    case FileType.txt:
                        StartCoroutine(textManager.TypeText(output, $"Wrong command. Use 'Read {name}' to read the text file", true));
                        break;
                    case FileType.exe:
                        StartCoroutine(textManager.TypeText(output, $"Wrong command. Use 'Run {name}' to run the program", true));
                        break;
                    case FileType.dir:
                        StartCoroutine(textManager.TypeText(output, $"Wrong command. Use 'Enter {name}' to enter the folder", true));
                        break;
                    case FileType.enc:
                        StartCoroutine(textManager.TypeText(output, $"Wrong command. Use 'Enter {name}' to enter the folder", true));
                        break;
                }
            }
            else
            {
                print("Checking input type on switch");
                switch (inputType)
                {
                    case FileType.txt:
                        foreach (FileInfo f in files)
                        {
                            if (name.ToLower() == f.Name.ToLower())
                            {
                                string jointContent = string.Empty;
                                string[] splitContent = textReader.ReadTextFile(f.Name.ToLower(), folderPath);

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
                                commandManager.RunProgram(input[0]);
                                break;
                            }
                        }
                        StartCoroutine(textManager.TypeText(output, "Couldn't find program.", true));
                        break;
                    case FileType.dir:
                        EnterFolder(input[0]);
                        break; 
                    case FileType.enc:
                        foreach (DirectoryInfo di in folders)
                        {
                            if (di.Name.ToLower() == name.ToLower())
                            {
                                print("Checking if folder is locked");
                                if (lockManager.IsFolderLocked(name.ToLower()))
                                {
                                    print("Folder is locked");
                                    commandManager.isWaitingForPassword = true;
                                    StartCoroutine(textManager.TypeText(output, "Type in password...", true));
                                }
                                break;
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
        currentFolderName = folderName;
        folderLayerCount++;
        commandManager.RunCommand(Commands.list);
    }

    public void ExitFolder()
    {
        folderPath = folderPath.Remove(folderPath.Length - currentFolderName.Length);
        folderLayerCount--;
        currentFolderName = mainFolderName;
        PopulateFiles();
        commandManager.RunCommand(Commands.list);
    }

    private void PopulateFiles()
    {
        print("folderpath = " +  folderPath);
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

        files = dirInfo.GetFiles().ToList<FileInfo>();
        folders = dirInfo.GetDirectories().ToList<DirectoryInfo>();

        GetFileList();
    }

    public void GetFileList()
    {
        if(files.Count <= 0)
        {
            PopulateFiles();
        }

        fileList = string.Empty;

        List<Folder> lockedFolderList = new List<Folder>();

        foreach(DirectoryInfo d in folders)
        {
            if(d.Extension != ".meta")
            {
                if(d.Extension == ".enc")
                {
                    Folder f = new();
                    f.name = d.Name.ToLower();
                    f.isLocked = true;
                    string password = folderPath + d.Name + "/" + d.GetFiles()[0].Name;
                    f.password = textReader.ReadTextFile(password, folderPath + d.Name + "/")[0];

                    print("Adding folder: " + f.name);
                    lockedFolderList.Add(f);

                    fileList += d.Name + "      ";
                }
                else
                {
                    fileList += d.Name + ".dir" + "      ";

                }
            }
        }

        lockManager.PopulateFolderList(lockedFolderList.ToArray());

        foreach(FileInfo f in files)
        {
            if (f.Extension != ".meta" && f.Name != "password.txt")
            {
                fileList += f.Name.ToLower() + "      ";
            }
        }
    }

    public enum FileType
    {
        txt,
        dir,
        enc,
        exe
    }
        
}
