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
    public Rooms roomsInJSON;

    [SerializeField]
    private List<Items> itemsInRoom;

    [SerializeField]
    private Room currentRoom;

    [SerializeField]
    private int currentRoomNumber = 0;

    [SerializeField]
    private TextManager textManager;

    public List<Items> inventory;

    private void OnEnable()
    {
        if (jsonFile == null)
        {
            jsonFile = Resources.Load<TextAsset>(fileName);
            PopulateRooms();
        }

        TryEnterRoom(roomsInJSON.rooms[0].name);
    }

    private void PopulateRooms()
    {
        print("Populating rooms");
        roomsInJSON = JsonUtility.FromJson<Rooms>(jsonFile.text);
        print("Num of rooms: " + roomsInJSON.rooms.Length);
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
                    foreach (Items i in itemsInRoom)
                    {
                        if (i.name.ToLower() == inputText[1].ToLower())
                        {
                            inventory.Add(i);
                            itemsInRoom.Remove(i);
                            string itemText = "You take the " + i.name;
                            StartCoroutine(textManager.TypeText(output, itemText, true));
                            break;
                        }
                    }
                    break;
                case ZorkCommands.look:
                    string descText = currentRoom.description;
                    StartCoroutine(textManager.TypeText(output, descText, true));
                    break;

            }
        }
    }

    private void TryEnterRoom(string newRoom)
    {
        print("Running TryEnterRoom with: " + newRoom);
        for (int i = 0; i < roomsInJSON.rooms.Length; i++)
        {
            print("Trying to find room...");
            if (newRoom.ToLower() == roomsInJSON.rooms[i].name.ToLower())
            {
                print("Found room: " + newRoom);
                currentRoom = roomsInJSON.rooms[i];

                string roomText = string.Empty;
                roomText += roomsInJSON.rooms[0].name + "\n";
                roomText += roomsInJSON.rooms[0].description + "\n";

                itemsInRoom = roomsInJSON.rooms[i].items.ToList<Items>();

                StartCoroutine(textManager.TypeText(output, roomText, true));
                return;
            }
        }
    }
}

[System.Serializable]
public class Rooms
{
    public Room[] rooms;
}

[System.Serializable]
public class Room
{
    public string name;
    public string description;
    public string[] connectedRooms;
    public Items[] items;
}

[System.Serializable]
public class Items
{
    public string name;
    public string description;
    public string canBeUsedOn;
}

public enum ZorkCommands
{
    look,
    take,
    move
}
