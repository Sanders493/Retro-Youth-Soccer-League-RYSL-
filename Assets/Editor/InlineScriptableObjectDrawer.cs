// @Group: Dev Tools - Drawers
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class InlineSOReferenceDrawer : PropertyDrawer
{
    private static Dictionary<Object, bool> foldouts = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw the inline object field (default width, inline)
        EditorGUI.BeginProperty(position, label, property);
        Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(objectFieldRect, property, label, true);
        EditorGUI.EndProperty();

        // No object? Exit
        if (property.objectReferenceValue == null) return;

        // Toggle foldout for inner fields
        Object obj = property.objectReferenceValue;
        if (!foldouts.ContainsKey(obj))
            foldouts[obj] = false;

        foldouts[obj] = EditorGUI.Foldout(
            new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight),
            foldouts[obj], "Expand Fields", true);

        if (!foldouts[obj]) return;

        // Draw the SO fields underneath
        SerializedObject so = new SerializedObject(obj);
        so.Update();

        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true); // Skip script

        float yOffset = position.y + EditorGUIUtility.singleLineHeight * 2 + 4;
        EditorGUI.indentLevel++;
        while (prop.NextVisible(false))
        {
            float height = EditorGUI.GetPropertyHeight(prop, true);
            Rect propRect = new Rect(position.x, yOffset, position.width, height);
            EditorGUI.PropertyField(propRect, prop, true);
            yOffset += height + 2;
        }
        EditorGUI.indentLevel--;

        so.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // for main field
        if (property.objectReferenceValue == null) return height;

        height += EditorGUIUtility.singleLineHeight + 2; // foldout

        Object obj = property.objectReferenceValue;
        if (!foldouts.TryGetValue(obj, out bool expanded) || !expanded) return height;

        // Add the height of expanded properties
        SerializedObject so = new SerializedObject(obj);
        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true);

        while (prop.NextVisible(false))
        {
            height += EditorGUI.GetPropertyHeight(prop, true) + 2;
        }

        return height;
    }
}
