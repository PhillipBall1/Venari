using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ItemObject))]
public class ItemObjectEditor : Editor
{
    SerializedProperty basePropertiesProp;
    SerializedProperty foodPropertiesProp;
    SerializedProperty meleePropertiesProp;
    SerializedProperty rangedPropertiesProp;
    SerializedProperty armorPropertiesProp;
    SerializedProperty medicalPropertiesProp;
    SerializedProperty trapPropertiesProp;
    SerializedProperty constructionPropertiesProp;
    SerializedProperty equipableProp;
    SerializedProperty craftingPropertiesProp;
    SerializedProperty durabilityPropertiesprop;
    SerializedProperty ammuntionPropertiesProp;
    SerializedProperty cookingPropertiesProp;

    private GUIStyle headerStyle;

    private void Styles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Styles();

        //Structs display
        basePropertiesProp = serializedObject.FindProperty("baseProperties");
        foodPropertiesProp = serializedObject.FindProperty("foodProperties");
        meleePropertiesProp = serializedObject.FindProperty("meleeProperties");
        rangedPropertiesProp = serializedObject.FindProperty("rangedProperties");
        armorPropertiesProp = serializedObject.FindProperty("armorProperties");
        medicalPropertiesProp = serializedObject.FindProperty("medicalProperties");
        trapPropertiesProp = serializedObject.FindProperty("trapProperties");
        constructionPropertiesProp = serializedObject.FindProperty("constructionProperties");
        equipableProp = serializedObject.FindProperty("equipableProperties");
        craftingPropertiesProp = serializedObject.FindProperty("craftingProperties");
        durabilityPropertiesprop = serializedObject.FindProperty("durabilityProperties");
        ammuntionPropertiesProp = serializedObject.FindProperty("ammuntionProperties");
        cookingPropertiesProp = serializedObject.FindProperty("cookableProperties");

        //Bool values
        SerializedProperty craftable = basePropertiesProp.FindPropertyRelative("craftable");
        SerializedProperty durability = basePropertiesProp.FindPropertyRelative("hasDurability");
        SerializedProperty equipable = basePropertiesProp.FindPropertyRelative("equipable");
        SerializedProperty cookable = basePropertiesProp.FindPropertyRelative("cookable");


        ItemObject itemObject = (ItemObject)target;
        string[] hideInputs = new string[]
        {
            "equipable", 
            "hasDurability",
            "type",
            "id",
            "canBreak",
            "canBlock",
            "doesDamage",
            "isCooked",
        };
        SwitchDisplay(itemObject, hideInputs);

        BoolBasedDisplay(cookable.boolValue, cookingPropertiesProp, hideInputs);
        BoolBasedDisplay(equipable.boolValue, equipableProp, hideInputs);
        BoolBasedDisplay(durability.boolValue, durabilityPropertiesprop, hideInputs);
        BoolBasedDisplay(craftable.boolValue, craftingPropertiesProp, hideInputs);

        serializedObject.ApplyModifiedProperties();
    }

    private void BoolBasedDisplay(bool value, SerializedProperty propery, string[] hideInputs)
    {
        if (value) { DisplayProperty(propery, hideInputs); }
    }

    private void SwitchDisplay(ItemObject item, string[] hideInputs)
    {
        DisplayProperty(basePropertiesProp, hideInputs);
        switch (item.baseProperties.type)
        {
            case ItemObject.ItemType.Food:
                DisplayProperty(foodPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Melee:
                DisplayProperty(meleePropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Ranged:
                DisplayProperty(rangedPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Construction:
                DisplayProperty(constructionPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Armor:
                DisplayProperty(armorPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Medical:
                DisplayProperty(medicalPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Trap:
                DisplayProperty(trapPropertiesProp, hideInputs);
                break;
            case ItemObject.ItemType.Ammunition:
                DisplayProperty(ammuntionPropertiesProp, hideInputs);
                break;
        }
    }

    private void DisplayProperty(SerializedProperty property, string[] hideInputs)
    {
        if (property == null) return;

        EditorGUILayout.LabelField(FormatPropertyName(property), headerStyle);
        EditorGUI.indentLevel++;

        SerializedProperty child = property.Copy();
        bool enterChildren = true;

        while (child.NextVisible(enterChildren))
        {
            if (SerializedProperty.EqualContents(child, property.GetEndProperty()))
                break;

            if (System.Array.IndexOf(hideInputs, child.name) >= 0) continue;

            if (!ShouldDisplayProperty(property, child.name)) continue;

            EditorGUILayout.PropertyField(child, true);
            enterChildren = false;
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
    }

    private bool ShouldDisplayProperty(SerializedProperty baseProperty, string propertyName)
    {
        // Check for "damageRange"
        if (propertyName == "damageRange")
        {
            SerializedProperty doesDamageProp = baseProperty.FindPropertyRelative("doesDamage");
            return doesDamageProp != null && doesDamageProp.boolValue;
        }
        // Check for "cookedInfo"
        else if (propertyName == "cookedInfo")
        {
            SerializedProperty cookableProp = baseProperty.FindPropertyRelative("cookable");
            return cookableProp != null && cookableProp.boolValue;
        }
        // Return true for all other properties
        return true;
    }


    private string FormatPropertyName(SerializedProperty property)
    {
        string name = "";
        for (int i = 0; i < property.name.Length; i++)
        {
            if (i == property.name.Length - 10)
            {
                name += " ";
            }

            if (i == 0)
            {
                name += property.name[i].ToString().ToUpper();
                continue;
            }
            name += property.name[i];
        }

        return name;
    }
}
