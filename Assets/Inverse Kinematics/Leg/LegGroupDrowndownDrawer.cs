using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LegGroupDropdownAttribute))]
public class LegGroupDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure we’re working with an integer.
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use LegGroupDropdown with int.");
            return;
        }

        // Retrieve the Leg component from the target object.
        Leg leg = property.serializedObject.targetObject as Leg;
        if (leg == null)
        {
            EditorGUI.LabelField(position, label.text, "Leg component not found.");
            return;
        }

        // Use the legsManager reference on the Leg.
        LegsManager legsManager = leg.LegsManager;
        if (legsManager == null)
        {
            EditorGUI.LabelField(position, label.text, "No Leg Manager assigned.");
            return;
        }

        // Get the list of groups from the manager.
        if (legsManager.legGroups == null || legsManager.legGroups.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No groups available.");
            return;
        }

        // Create a string array for the dropdown options.
        string[] options = legsManager.legGroups.ToArray();

        // Display the popup and assign the chosen index to the property.
        property.intValue = EditorGUI.Popup(position, label.text, property.intValue, options);
    }
}
