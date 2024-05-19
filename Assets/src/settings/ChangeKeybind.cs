using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChangeKeybind : MonoBehaviour
{
    public PlayerKeybinds keybinds;
    public GameObject keybindListenerDisplay;
    private bool isWaitingForKey = false;
    private string actionToRebind = null;
    private HashSet<KeyCode> excludedKeyCodes = new HashSet<KeyCode> { KeyCode.Escape, KeyCode.Mouse0 };

    public void StartRebinding(string actionName)
    {
        if (isWaitingForKey)
            return; // Already waiting for a key, ignore new requests

        actionToRebind = actionName;
        isWaitingForKey = true;
        keybindListenerDisplay.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            isWaitingForKey= false;
            keybindListenerDisplay.gameObject.SetActive(false);
        }
        if (isWaitingForKey)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode) && !excludedKeyCodes.Contains(keyCode))
                {
                    // Check for conflicts
                    if (IsKeyAlreadyBound(keyCode))
                    {
                        Debug.Log($"Key {keyCode} is already bound to another action.");
                        keybindListenerDisplay.GetComponentInChildren<TMP_Text>().text = $"Key {keyCode} is already bound to another action.";
                    }
                    else
                    {
                        // No conflict, update the keybind
                        UpdateKeybind(actionToRebind, keyCode);
                        FindObjectOfType<UI_Keybinds>().UpdateKeybindDisplay(actionToRebind, keyCode);
                        isWaitingForKey = false;
                        actionToRebind = null;
                        keybindListenerDisplay.gameObject.SetActive(false);
                        break;
                    }
                }  
            }
        }
    }

    private bool IsKeyAlreadyBound(KeyCode keyCode)
    {
        foreach (var keybindAction in keybinds.keybindActions)
        {
            if (keybindAction.key == keyCode)
            {
                return true; // Key is already bound
            }
        }
        return false; // No conflicts found
    }

    private void UpdateKeybind(string actionName, KeyCode newKey)
    {
        foreach (var keybindAction in keybinds.keybindActions)
        {
            if (keybindAction.actionName == actionName)
            {
                keybindAction.key = newKey;
                break;
            }
        }
        // Optionally, save the updated keybinds to persistent storage here
    }
}
