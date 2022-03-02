#if UINITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class VisibleOnly : PropertyAttribute
{
    
}

#if UINITY_EDITOR
[CustomPropertyDrawer(typeof(VisibleOnly))]
public class VisibleOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif