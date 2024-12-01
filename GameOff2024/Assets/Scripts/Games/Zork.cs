using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using Newtonsoft.Json;

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

    [SerializeField]
    private CommandManager commandManager;

    public Dictionary<string, bool> inventoryStates = new Dictionary<string, bool>();
    public Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void OnEnable()
    {
        if (jsonFile == null)
        {
            jsonFile = Resources.Load<TextAsset>(fileName);
        }

        LoadZorkData();
        InitializeInventory();
    }

    private void LoadZorkData()
    {
        zorkData = JsonConvert.DeserializeObject<ZorkData>(jsonFile.text);
        TryEnterRoom("Welcome");
    }

    private void InitializeInventory()
    {
        inventoryStates = new Dictionary<string, bool>();

        foreach (var item in zorkData.inventory)
        {
            inventoryStates[item.Key] = item.Key == "floppy"; // Player starts with the floppy disk
            print(item.Key);
        }

        // Initialize flags
        flags = new Dictionary<string, bool>();
    }

    private void Update()
    {
        if (!input.isFocused && !textManager.isRunning)
        {
            input.ActivateInputField();
        }

        if (Input.GetButtonDown("Submit") && !textManager.isRunning)
        {
            if (CommandManager.ParseCommand(input.text) == Commands.exit)
            {
                FindFirstObjectByType<ScreenManager>().SwitchScreens(0);
            }
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
                        TryUse(string.Join(" ", inputText.Skip(1)));
                    else
                        StartCoroutine(textManager.TypeText(output, "Use what?", true));
                    break;
                case ZorkCommands.help:
                    ShowHelp();
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
            if (roomKey != "welcome")
                DisplayAvailableConnections();
        }
        else if (flags.ContainsKey("move_home") && roomKey == "home")
        {
            // Allow moving to home if the flag is set
            currentRoom = zorkData.Rooms["home"];
            string roomText = $"{currentRoom.description}\n";
            StartCoroutine(textManager.TypeText(output, roomText, true));
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
            ProcessGameObject(itemKey, obj, actionType: "Take");
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "That item isn't here.", true));
        }
    }

    private void TryUse(string itemName)
    {
        string itemKey = itemName.Replace(" ", "_").ToLower();

        if (inventoryStates.TryGetValue(itemKey, out bool hasItem) && hasItem)
        {
            // Process inventory item
            if (zorkData.inventory.ContainsKey(itemKey))
            {
                var item = zorkData.inventory[itemKey];
                ProcessInventoryItem(itemKey, item, actionType: "Use");
            }
            else
            {
                StartCoroutine(textManager.TypeText(output, "You can't use that.", true));
            }
        }
        else if (currentRoom.objects != null && currentRoom.objects.ContainsKey(itemKey))
        {
            // Process room object
            var obj = currentRoom.objects[itemKey];
            ProcessGameObject(itemKey, obj, actionType: "Use");
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "You can't use that here.", true));
        }
    }

    private void TryLookAt(string objectName)
    {
        string objectKey = objectName.Replace(" ", "_").ToLower();

        // First, check if the object is in the current room
        if (currentRoom.objects != null && currentRoom.objects.ContainsKey(objectKey))
        {
            var obj = currentRoom.objects[objectKey];
            ProcessGameObject(objectKey, obj, actionType: "Look");
        }
        else if (inventoryStates.TryGetValue(objectKey, out bool hasItem) && hasItem)
        {
            // Look at inventory item
            if (zorkData.inventory.ContainsKey(objectKey))
            {
                var item = zorkData.inventory[objectKey];
                ProcessInventoryItem(objectKey, item, actionType: "Look");
            }
            else
            {
                StartCoroutine(textManager.TypeText(output, "You don't have that item.", true));
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "There's nothing like that here.", true));
        }
    }

    private void DisplayRoomDescription()
    {
        StartCoroutine(textManager.TypeText(output, currentRoom.GetRoomDescription(), true));
    }

    private void DisplayInventory()
    {
        string inventoryList = "You are carrying:\n";
        bool hasItems = false;

        foreach (var item in inventoryStates.Where(i => i.Value))
        {
            string itemKey = item.Key;
            inventoryList += $"- {FormatName(itemKey)}\n";

            if (zorkData.inventory.ContainsKey(itemKey))
            {
                var inventoryItem = zorkData.inventory[itemKey];
                foreach (var cond in inventoryItem.conditions)
                {
                    if (EvaluateCondition(cond.ConditionExpression))
                    {
                        inventoryList += $"{cond.Text}\n";
                        break;
                    }
                }
            }
            hasItems = true;
        }

        if (!hasItems)
        {
            inventoryList = "Your inventory is empty.";
        }

        StartCoroutine(textManager.TypeText(output, inventoryList, true));
    }

    private void ProcessGameObject(string objectName, GameObjectData objData, string actionType)
    {
        string textToDisplay = "";
        bool conditionMet = false;

        if (objData.conditions != null && objData.conditions.Count > 0)
        {
            foreach (var cond in objData.conditions)
            {
                if (string.Equals(cond.ActionType, actionType, StringComparison.OrdinalIgnoreCase) || string.Equals(cond.ActionType, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    if (EvaluateCondition(cond.ConditionExpression))
                    {
                        ApplyActions(cond.Actions, objectName);
                        textToDisplay = cond.Text;
                        conditionMet = true;
                        break;
                    }
                }
            }
        }
        else
        {
            // No conditions; default behavior
            if (actionType.Equals("Take", StringComparison.OrdinalIgnoreCase))
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
                ApplyActions(objData.actions, objectName);
                conditionMet = true;
            }
            else if (actionType.Equals("Look", StringComparison.OrdinalIgnoreCase))
            {
                textToDisplay = objData.text;
                conditionMet = true;
            }
            else if (actionType.Equals("Use", StringComparison.OrdinalIgnoreCase))
            {
                textToDisplay = "You can't use that.";
                conditionMet = true;
            }
        }

        if (conditionMet)
        {
            if (!string.IsNullOrEmpty(textToDisplay))
            {
                StartCoroutine(textManager.TypeText(output, textToDisplay, true));
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "You can't do that right now.", true));
        }
    }

    private void ProcessInventoryItem(string itemName, InventoryItem itemData, string actionType)
    {
        string textToDisplay = "";
        bool conditionMet = false;

        if (itemData.conditions != null && itemData.conditions.Count > 0)
        {
            foreach (var cond in itemData.conditions)
            {
                if (string.Equals(cond.ActionType, actionType, StringComparison.OrdinalIgnoreCase) || string.Equals(cond.ActionType, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    if (EvaluateCondition(cond.ConditionExpression))
                    {
                        ApplyActions(cond.Actions, itemName);
                        textToDisplay = cond.Text;
                        conditionMet = true;
                        break;
                    }
                }
            }
        }
        else
        {
            // No conditions; default behavior
            if (actionType.Equals("Look", StringComparison.OrdinalIgnoreCase))
            {
                textToDisplay = itemData.text;
                conditionMet = true;
            }
            else if (actionType.Equals("Use", StringComparison.OrdinalIgnoreCase))
            {
                textToDisplay = "You can't use that.";
                conditionMet = true;
            }
        }

        if (conditionMet)
        {
            if (!string.IsNullOrEmpty(textToDisplay))
            {
                StartCoroutine(textManager.TypeText(output, textToDisplay, true));
            }
        }
        else
        {
            StartCoroutine(textManager.TypeText(output, "You can't do that right now.", true));
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

        if (condition.Contains("&&"))
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
        else if (condition.StartsWith("!"))
        {
            string flag = condition.Substring(1);
            return !GetFlag(flag);
        }
        else
        {
            return GetFlag(condition);
        }
    }

    private void ApplyActions(List<string> actions, string objectName = "")
    {
        if (actions == null) return;

        foreach (var action in actions)
        {
            if (action.StartsWith("!"))
            {
                string flag = action.Substring(1);
                SetFlag(flag, false);
            }
            else if (action.Equals("take", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(objectName))
                {
                    inventoryStates[objectName] = true;
                }
            }
            else if (action.StartsWith("move_"))
            {
                // Handle move actions
                SetFlag(action, true);
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

    private void ShowHelp()
    {
        string helpText = "You can use commands like 'look at [object]', 'use [object]', 'take [object]', 'move to [location]', or 'inventory'.";
        StartCoroutine(textManager.TypeText(output, helpText, true));
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

    public string GetRoomDescription()
    {
        if (objects != null && objects.ContainsKey("room"))
        {
            return objects["room"].text;
        }
        return description;
    }
}

[System.Serializable]
public class GameObjectData
{
    public string text;
    public List<ConditionData> conditions;
    public List<string> actions;
}

[System.Serializable]
public class ConditionData
{
    [JsonProperty("condition")]
    public string ConditionExpression;

    [JsonProperty("text")]
    public string Text;

    [JsonProperty("actions")]
    public List<string> Actions;

    [JsonProperty("actionType")]
    public string ActionType;
}

[System.Serializable]
public class InventoryItem
{
    public string text;
    public List<ConditionData> conditions;
}

public class ConnectionData
{
    public string condition;
    public string text;
}

#endregion

public enum ZorkCommands
{
    look,
    take,
    move,
    inventory,
    use,
    help
}
