using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using Newtonsoft.Json; // Make sure to include this

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

    public Dictionary<string, bool> inventoryStates = new Dictionary<string, bool>();
    public Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void OnEnable()
    {
        if (jsonFile == null)
        {
            jsonFile = Resources.Load<TextAsset>(fileName);
            print("jsonfile = " + jsonFile.name);
        }

        LoadZorkData();
        InitializeInventory();    
    }

    private void LoadZorkData()
    {
        print("Loading Zork data");
        zorkData = JsonConvert.DeserializeObject<ZorkData>(jsonFile.text);
        print("Num of rooms: " + zorkData.Rooms.Count);

        TryEnterRoom("Welcome");
    }

    private void InitializeInventory()
    {
        inventoryStates = new Dictionary<string, bool>();

        foreach (var item in zorkData.inventory)
        {
            inventoryStates[item.Key] = item.Key == "floppy"; // Default all items to "not owned"
            print(item.Key);
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
            input.text = "";
        }
    }

    private void ParseInput(string inputCommand)
    {
        string[] inputText = inputCommand.Trim().Split(' ');
        if (inputText.Length == 0) return;

        string command = inputText[0].ToLower();

        if (Enum.TryParse<ZorkCommands>(command, out ZorkCommands zc))
        {
            switch (zc)
            {
                case ZorkCommands.move:
                    if (inputText.Length > 1)
                        TryEnterRoom(string.Join(" ", inputText.Skip(1)));
                    else
                        StartCoroutine(textManager.TypeText(output, "Move where?", true));
                    break;
                case ZorkCommands.take:
                    if (inputText.Length > 1)
                        TryTakeItem(string.Join(" ", inputText.Skip(1)));
                    else
                        StartCoroutine(textManager.TypeText(output, "Take what?", true));
                    break;
                case ZorkCommands.look:
                    if (inputText.Length > 2 && inputText[1].ToLower() == "at")
                        TryLookAt(string.Join(" ", inputText.Skip(2)));
                    else if (inputText.Length > 1)
                        TryLookAt(string.Join(" ", inputText.Skip(1)));
                    else
                        DisplayRoomDescription();
                    break;
                case ZorkCommands.inventory:
                    DisplayInventory();
                    break;
                case ZorkCommands.use:
                    if (inputText.Length > 1)
                        TryUseItem(string.Join(" ", inputText.Skip(1)));
                    else
                        StartCoroutine(textManager.TypeText(output, "Use what?", true));
                    break;
                default:
                    StartCoroutine(textManager.TypeText(output, "I don't understand that command.", true));
                    break;
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "I don't understand that command.", true));
        }
    }

    private void TryEnterRoom(string newRoomName)
    {
        print("Trying to enter room: " + newRoomName);
        string roomKey = newRoomName.Replace(" ", "_").ToLower();
        if (zorkData.Rooms.ContainsKey(roomKey))
        {
            currentRoom = zorkData.Rooms[roomKey];
            string roomText = $"{currentRoom.description}\n";
            StartCoroutine(textManager.TypeText(output, roomText, true));

            // Optionally display available exits or connections
            if(roomKey != "welcome")
                DisplayAvailableConnections();

        }
        else
        {
            print("Trying to access wrong room");
            StartCoroutine(textManager.TypeText(output, "You can't go there.", true));
        }
    }

    private void TryTakeItem(string itemName)
    {
        string itemKey = itemName.Replace(" ", "_").ToLower();

        // Check if the item is in the room's objects
        if (currentRoom.objects != null && currentRoom.objects.ContainsKey(itemKey))
        {
            var obj = currentRoom.objects[itemKey];

            // Process taking the item
            ProcessGameObject(itemKey, obj, actionType: ActionType.Take);
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "That item isn't here.", true));
        }
    }

    private void TryUseItem(string itemName)
    {
        string itemKey = itemName.Replace(" ", "_").ToLower();
        if (inventoryStates.TryGetValue(itemKey, out bool hasItem) && hasItem)
        {
            if (zorkData.inventory.ContainsKey(itemKey))
            {
                var item = zorkData.inventory[itemKey];

                if (item.conditions != null)
                {
                    foreach (var conditionPair in item.conditions)
                    {
                        string conditionKey = conditionPair.Key;
                        var conditionData = conditionPair.Value;

                        if (EvaluateCondition(conditionKey))
                        {
                            ApplyActions(conditionData.actions);
                            StartCoroutine(textManager.TypeText(output, conditionData.text, true));
                            return;
                        }
                    }
                }
                else
                {
                    StartCoroutine(textManager.TypeText(output, item.text, true));
                }
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "You don't have that item.", true));
        }
    }

    private void TryLookAt(string objectName)
    {
        string objectKey = objectName.Replace(" ", "_").ToLower();

        // First, check if the object is in the current room
        if (currentRoom.objects != null && currentRoom.objects.ContainsKey(objectKey))
        {
            var obj = currentRoom.objects[objectKey];
            ProcessGameObject(objectKey, obj, actionType: ActionType.Look);
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "There's nothing like that here.", true));
        }
    }

    private void DisplayRoomDescription()
    {
        StartCoroutine(textManager.TypeText(output, currentRoom.description, true));
    }

    private void DisplayInventory()
    {
        string inventoryList = "You are carrying:\n";
        bool hasItems = false;

        foreach (var item in inventoryStates.Where(i => i.Value))
        {
            inventoryList += $"- {item.Key.Replace("_", " ")}\n";

            print("item name: " + item);

            print("has item:" + zorkData.inventory.ContainsKey(item.Key));
            inventoryList += $"{zorkData.inventory[item.Key.Replace("_", " ")].text}\n\n";
            hasItems = true;
        }

        if (!hasItems)
        {
            inventoryList = "Your inventory is empty.";
        }

        StartCoroutine(textManager.TypeText(output, inventoryList, true));
    }

    private void ProcessGameObject(string objectName, GameObjectData objData, ActionType actionType)
    {
        string textToDisplay = "";
        bool conditionMet = false;

        if (objData.conditions != null && objData.conditions.Count > 0)
        {
            foreach (var conditionPair in objData.conditions)
            {
                string conditionKey = conditionPair.Key;
                ConditionData conditionData = conditionPair.Value;

                if (EvaluateCondition(conditionKey))
                {
                    conditionMet = true;

                    // Only apply actions if the actionType matches
                    if (conditionData.actionType == actionType || conditionData.actionType == ActionType.Any)
                    {
                        if (actionType == ActionType.Take)
                        {
                            // Check if the object can be taken
                            if (conditionData.actions != null && conditionData.actions.Contains("take"))
                            {
                                inventoryStates[objectName] = true;
                                textToDisplay = conditionData.text ?? $"You take the {FormatName(objectName)}.";
                            }
                            else
                            {
                                textToDisplay = "You can't take that.";
                            }
                        }
                        else if (actionType == ActionType.Look)
                        {
                            textToDisplay = conditionData.text;
                        }

                        ApplyActions(conditionData.actions);
                        break; // Stop after first valid condition
                    }
                }
            }
        }
        else
        {
            // No conditions; default behavior
            if (actionType == ActionType.Take)
            {
                if (objData.actions != null && objData.actions.Contains("take"))
                {
                    inventoryStates[objectName] = true;
                    textToDisplay = objData.text ?? $"You take the {FormatName(objectName)}.";
                }
                else
                {
                    textToDisplay = "You can't take that.";
                }
            }
            else if (actionType == ActionType.Look)
            {
                textToDisplay = objData.text;
            }

            ApplyActions(objData.actions);
        }

        if (!string.IsNullOrEmpty(textToDisplay))
        {
            StartCoroutine(textManager.TypeText(output, textToDisplay, true));
        }
    }

    private void DisplayAvailableConnections()
    {
        if (currentRoom.connections != null && currentRoom.connections.Count > 0)
        {
            string connectionsText = "You can go to:\n";

            foreach (var connection in currentRoom.connections)
            {
                if (connection is string)
                {
                    connectionsText += $"- {FormatName(connection.ToString())}\n";
                }
                else if (connection is Newtonsoft.Json.Linq.JObject connObj)
                {
                    // Handle conditional connections
                    foreach (var prop in connObj)
                    {
                        string connName = prop.Key;
                        var connData = prop.Value.ToObject<ConnectionData>();

                        if (EvaluateCondition(connData.condition))
                        {
                            connectionsText += $"- {FormatName(connName)}\n";
                        }
                    }
                }
            }

            StartCoroutine(textManager.TypeText(output, connectionsText, true));
        }
    }

    private bool EvaluateCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        condition = condition.Replace(" ", "");
        if (condition.StartsWith("!"))
        {
            string flag = condition.Substring(1);
            return !GetFlag(flag);
        }
        else if (condition.Contains("&&"))
        {
            var conditions = condition.Split(new[] { "&&" }, StringSplitOptions.None);
            foreach (var cond in conditions)
            {
                if (!EvaluateCondition(cond.Trim()))
                    return false;
            }
            return true;
        }
        else if (condition.Contains("||"))
        {
            var conditions = condition.Split(new[] { "||" }, StringSplitOptions.None);
            foreach (var cond in conditions)
            {
                if (EvaluateCondition(cond.Trim()))
                    return true;
            }
            return false;
        }
        else
        {
            return GetFlag(condition);
        }
    }

    private void ApplyActions(List<string> actions)
    {
        if (actions == null) return;

        foreach (var action in actions)
        {
            if (action.StartsWith("!"))
            {
                string flag = action.Substring(1);
                SetFlag(flag, false);
            }
            else
            {
                SetFlag(action, true);
            }
        }
    }

    private bool GetFlag(string flag)
    {
        return flags.ContainsKey(flag) && flags[flag];
    }

    private void SetFlag(string flag, bool value)
    {
        if (flags.ContainsKey(flag))
        {
            flags[flag] = value;
        }
        else
        {
            flags.Add(flag, value);
        }
    }

    private string FormatName(string name)
    {
        return name.Replace("_", " ");
    }
}

#region Data Classes

[System.Serializable]
public class ZorkData
{
    public Dictionary<string, Room> Rooms;
    public Dictionary<string, InventoryItem> inventory;
}

[System.Serializable]
public class Room
{
    public string description;
    public List<object> connections; // Can be strings or dictionaries (for conditional connections)
    public Dictionary<string, GameObjectData> objects;
}

[System.Serializable]
public class GameObjectData
{
    public string text;
    public Dictionary<string, ConditionData> conditions;
    public List<string> actions;
}

[System.Serializable]
public class ConditionData
{
    public string text;
    public List<string> actions;
    public ActionType actionType = ActionType.Any; // New property to specify the action type
}

[System.Serializable]
public class InventoryItem
{
    public Dictionary<string, ConditionData> conditions;
    public string text;
}

public class ConnectionData
{
    public string condition;
    public string text;
}

#endregion

public enum ActionType
{
    Look,
    Take,
    Use,
    Any // For actions that can occur on any interaction
}

public enum ZorkCommands
{
    look,
    take,
    move,
    inventory,
    use
}
