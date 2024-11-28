using NUnit.Framework;
using UnityEngine;

public class LockManager : MonoBehaviour
{
    [SerializeField]
    private ProgramManager programManager;

    [SerializeField]
    private TextManager textManager;

    [SerializeField]
    private Folder[] folderList;

    [SerializeField]
    public string folderToUnlock;

    private void Start()
    {
        programManager = FindFirstObjectByType<ProgramManager>();
        textManager = FindFirstObjectByType<TextManager>();
    }

    public void PopulateFolderList(Folder[] lockedFolderList)
    {
        folderList = lockedFolderList;
    }

    public bool IsFolderLocked(string folderName)
    {
        foreach (Folder f in folderList)
        {
            if (f.name.ToLower().Equals(folderName.ToLower()))
            {
                folderToUnlock = folderName.ToLower();
                return true;
            }
        }

        folderToUnlock = "";
        return false;
    }

    public void TryUnlockFolder(string password)
    {

        foreach (Folder f in folderList)
        {
            print("Trying to unlock folder: " + f.name);
            if (f.name.ToLower().Equals(folderToUnlock.ToLower()) && f.password.ToLower().Equals(password.ToLower()))
            {
                print("Entering locked folder: " + f.name);
                programManager.EnterFolder(f.name.ToLower());
            }
        }
    }
}

public class Folder
{
    public string name;
    public bool isLocked;
    public string password;
}