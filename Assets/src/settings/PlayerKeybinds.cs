using UnityEngine;

[CreateAssetMenu(fileName = "Keybinds", menuName = "System/Settings/Keybinds")]
public class PlayerKeybinds : ScriptableObject
{
    public KeybindAction[] keybindActions;
}


[System.Serializable]
public class KeybindAction
{
    public string actionName;
    public KeyCode key;
    public string section;
    public bool hold;
}
