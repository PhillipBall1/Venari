using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro elements
using System.Collections.Generic;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class UI_Keybinds : MonoBehaviour
{
    public PlayerKeybinds keybinds;
    public ChangeKeybind changeKeybindScript;
    public GameObject keybindPrefab;
    public Transform keybindListParent;

    void Start()
    {
        PopulateKeybindsUI();
    }

    public GameObject sectionTitlePrefab; // Assign this in the inspector

    private void PopulateKeybindsUI()
    {
        HashSet<string> addedSections = new HashSet<string>();

        foreach (var keybindAction in keybinds.keybindActions)
        {
            // Create the section title if it hasn't been added yet
            if (!addedSections.Contains(keybindAction.section))
            {
                GameObject sectionTitle = Instantiate(sectionTitlePrefab, keybindListParent);
                sectionTitle.GetComponentInChildren<TMP_Text>().text = keybindAction.section;

                addedSections.Add(keybindAction.section);
            }

            // Instantiate KeybindPrefab under keybindListParent, not under the section
            GameObject keybindItem = Instantiate(keybindPrefab, keybindListParent);
            keybindItem.GetComponentInChildren<TMP_Text>().text = keybindAction.actionName;
            Button changeButton = keybindItem.GetComponentInChildren<Button>();
            changeButton.transform.GetComponentInChildren<TMP_Text>().text = FormatKeyCode(keybindAction.key);
            string actionName = keybindAction.actionName;
            changeButton.onClick.AddListener(() => changeKeybindScript.StartRebinding(actionName));
        }
    }


    public void UpdateKeybindDisplay(string actionName, KeyCode newKey)
    {
        foreach (Transform child in keybindListParent)
        {
            TMP_Text textComponent = child.GetComponentInChildren<TMP_Text>();

            if (textComponent.text.StartsWith(actionName))
            {
                textComponent.text = actionName;
                child.GetComponentInChildren<Button>().transform.GetComponentInChildren<TMP_Text>().text = FormatKeyCode(newKey);
                break;
            }
        }
    }

    private string FormatKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                return "Control";

            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                return "Shift";

            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                return "Alt";

            default:
                if (keyCode.ToString().StartsWith("Alpha"))
                {
                    return keyCode.ToString().Substring(5);
                }
                else
                {
                    return keyCode.ToString();
                }
        }
    }
}
