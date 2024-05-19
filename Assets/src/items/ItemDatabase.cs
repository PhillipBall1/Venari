using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Item Database", menuName = "System/Items/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemObject> itemObjects;

    public void OnValidate()
    {
        itemObjects.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ItemObject", new[] { "Assets/assets/items" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemObject loadedItem = AssetDatabase.LoadAssetAtPath<ItemObject>(path);
            if (loadedItem != null)
            {
                itemObjects.Add(loadedItem);
            }
        }
        AssignIDs();
    }

    private void AssignIDs()
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            if (itemObjects[i] != null)
            {
                itemObjects[i].baseProperties.id = i;
            }
        }
    }

    public ItemObject GetItemByName(string name)
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            if (itemObjects[i].name == name)
            {
                return itemObjects[i];
            }
        }
        return null;
    }

    public List<ItemObject> GetList() { return itemObjects; }
}
