using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    public Sprite headEquipmentSlotSprite;
    public Sprite bodyEquipmentSlotSprite;
    public Sprite legEquipmentSlotSprite;
    public List<GameObject> hitDecals;
    public ItemDatabase database;

    public Color bgColor;
    public Color mgColor;
    public Color fgColor;
    public Color rColor;
    public Color gColor;

    void Awake()
    {

        // Ensure there's only one instance of this object in the game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Makes the manager persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        bgColor = GetColorByHex("#11151C");
        mgColor = GetColorByHex("#161C24");
        fgColor = GetColorByHex("#21262C");
        rColor = GetColorByHex("#90686A");
        gColor = GetColorByHex("#779169");
    }

    public Color GetColorByHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color newColor))
        {
            return newColor;
        }
        else
        {
            return Color.white;
        }
    }
}
