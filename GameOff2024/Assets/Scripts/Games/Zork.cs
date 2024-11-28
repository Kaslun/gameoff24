using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

public class Zork : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI output;

    [SerializeField]
    private TMP_InputField input;

    [SerializeField]
    private string fileName;

    [SerializeField]
    private TextAsset jsonFile;

    [SerializeField]
    public ZorkData zorkData;

    [SerializeField]
    private Room currentRoom;

    [SerializeField]
    private TextManager textManager;

    public Dictionary<string, bool> inventoryStates;

    private void OnEnable()
    {
        if (jsonFile == null)
        {
            jsonFile = Resources.Load<TextAsset>(fileName);
            print("jsonfile = " + jsonFile.name);
        }

        LoadZorkData();
        TryEnterRoom(zorkData.rooms[0].name);
    }

    private void LoadZorkData()
    {
        print("Loading Zork data");
        zorkData = JsonUtility.FromJson<ZorkData>(jsonFile.text);
        print("Num of rooms: " + zorkData.rooms.Length);

    }

    private void InitializeInventory()
    {
        inventoryStates = new Dictionary<string, bool>();
        InitializeInventory();

        foreach (var item in zorkData.inventory)
        {
            inventoryStates[item.name] = false; // Default all items to "not owned"
        }
    }

    private void Update()
    {
        if (!input.isFocused && !textManager.isRunning)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
        {
            ParseInput(input.text);
        }
    }

    private void ParseInput(string inputCommand)
    {
        string[] inputText = inputCommand.Split(" ");
        string command = inputText[0];

        if (Enum.TryParse<ZorkCommands>(command, out ZorkCommands zc))
        {
            switch (zc)
            {
                case ZorkCommands.move:
                    TryEnterRoom(inputText[1]);
                    break;
                case ZorkCommands.take:
                    TryTakeItem(inputText[1]);
                    break;
                case ZorkCommands.look:
                    DisplayRoomDescription();
                    break;
                case ZorkCommands.inventory:
                    DisplayInventory();
                    break;
                case ZorkCommands.use:
                    TryUseItem(inputText[1]);
                    break;
            }
        }
    }

    private void TryEnterRoom(string newRoom)
    {
        foreach (var room in zorkData.rooms)
        {
            if (newRoom.ToLower() == room.name.ToLower())
            {
                currentRoom = room;

                string roomText = $"{currentRoom.name}\n{currentRoom.description}\n";
                StartCoroutine(textManager.TypeText(output, roomText, true));
                return;
            }
        }

        StartCoroutine(textManager.TypeText(output, "You can't go there.", true));
    }

    private void TryTakeItem(string itemName)
    {
        var item = currentRoom.items.FirstOrDefault(i => i.name.ToLower() == itemName.ToLower());
        if (item != null)
        {
            inventoryStates[item.name] = true;
            string itemText = $"You take the {item.name}.";
            StartCoroutine(textManager.TypeText(output, itemText, true));
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "That item isn't here.", true));
        }
    }

    private void TryUseItem(string itemName)
    {
        if (inventoryStates.TryGetValue(itemName, out bool hasItem) && hasItem)
        {
            var item = zorkData.inventory.FirstOrDefault(i => i.name == itemName);
            if (item != null)
            {
                foreach (var condition in item.conditions)
                {
                    if (EvaluateCondition(condition.condition))
                    {
                        ApplyStateChange(condition.stateChange);
                        StartCoroutine(textManager.TypeText(output, condition.description, true));
                        return;
                    }
                }
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "You don't have that item.", true));
        }
    }

    private void DisplayRoomDescription()
    {
        StartCoroutine(textManager.TypeText(output, currentRoom.description, true));
    }

    private void DisplayInventory()
    {
        string inventoryList = "You are carrying:\n";
        foreach (var item in inventoryStates.Where(i => i.Value))
        {
            inventoryList += $"- {item.Key}\n";
        }
        StartCoroutine(textManager.TypeText(output, inventoryList, true));
    }

    private bool EvaluateCondition(string condition)
    {
        return condition.Split("&&")
            .Select(c => c.Trim())
            .All(c => inventoryStates.ContainsKey(c.TrimStart('!'))
                      && inventoryStates[c.TrimStart('!')] == !c.StartsWith("!"));
    }

    private void ApplyStateChange(Dictionary<string, bool> stateChange)
    {
        foreach (var state in stateChange)
        {
            inventoryStates[state.Key] = state.Value;
        }
    }
}

[System.Serializable]
public class ZorkData
{
    public Room[] rooms;
    public InventoryItem[] inventory;
}

[System.Serializable]
public class Room
{
    public string name;
    public string description;
    public string[] connected;
    public Objects[] items;
}

[System.Serializable]
public class Objects
{
    public string name;
    public string description;
    public string canBeUsedOn;
}

[System.Serializable]
public class InventoryItem
{
    public string name;
    public Condition[] conditions;
}

[System.Serializable]
public class Condition
{
    public string condition;
    public string description;
    public Dictionary<string, bool> stateChange;
}

public enum ZorkCommands
{
    look,
    take,
    move,
    inventory,
    use
}
