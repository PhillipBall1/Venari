using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveLoadGame : MonoBehaviour
{
    public InventoryObject inventory;
    private string savePath;

    void Start()
    {
        savePath = Application.persistentDataPath + "/inventory.save";
        if (SaveExists())
        {
            LoadGame();
        }
    }

    public void SaveGame()
    {
        inventory.Save(savePath);
    }

    public void LoadGame()
    {
        inventory.Load(savePath);
    }

    public bool SaveExists()
    {
        return File.Exists(savePath);
    }
}
