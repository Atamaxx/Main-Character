using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[InitializeOnLoad]
public static class HierarchyIconDisplay
{
    static HierarchyIconDisplay()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        //if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null) return;

        Component[] components = obj.GetComponents<Component>();
        if (components == null || components.Length == 0) return;

        Component component = components.Length > 1 ? components[1] : components[0];
        if (component == null) return;

        Type type = component.GetType();

        GUIContent content = EditorGUIUtility.ObjectContent(component, type);
        if (content == null) return;
        
        content.text = null;
        content.tooltip = type.Name;

        if (content.image == null)
            return;

        bool isSelected = Selection.instanceIDs.Contains(instanceID);
        bool isHovered = Event.current != null && selectionRect.Contains(Event.current.mousePosition);
        bool isWindowFocused = EditorWindow.focusedWindow != null && 
                              EditorWindow.mouseOverWindow != null && 
                              EditorWindow.focusedWindow == EditorWindow.mouseOverWindow;

        Color color = UnityEditorBackgroundColor.Get(isSelected, isHovered, isWindowFocused);
        Rect backgroundRect = selectionRect;
        backgroundRect.width = 18.5f;
        EditorGUI.DrawRect(backgroundRect, color);

        EditorGUI.LabelField(selectionRect, content);
    }
}