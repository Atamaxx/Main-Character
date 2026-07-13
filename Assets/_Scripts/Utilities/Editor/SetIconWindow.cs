using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SetIconWindow : EditorWindow
{
    const string k_menuPath = "Assets/Create/Set Icon..";

    List<Texture2D> m_icons = null;
    int m_selectedIcon = 0;
    Vector2 m_scrollPos = Vector2.zero;

    [MenuItem(k_menuPath, priority = 0)]
    public static void ShowMenuItem()
    {
        SetIconWindow window = (SetIconWindow)GetWindow(typeof(SetIconWindow));
        window.titleContent = new GUIContent("Set Icon");
        window.Show();
    }

    [MenuItem(k_menuPath, validate = true)]
    public static bool ShowMenuItemValidation()
    {
        foreach (Object asset in Selection.objects)
        {
            if (asset.GetType() != typeof(MonoScript))
                return false;
        }
        return true;
    }

    void OnGUI()
    {
        // Load icons only once.
        if (m_icons == null)
        {
            m_icons = new List<Texture2D>();
            string[] assetGuids = AssetDatabase.FindAssets("t:texture2D l:ScriptIcon");
            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (icon != null)
                    m_icons.Add(icon);
            }
        }

        // Check if any icons were found.
        if (m_icons == null || m_icons.Count == 0)
        {
            GUILayout.Label("No icons found");
            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                Close();
            }
            return;
        }

        // Convert each texture into a GUIContent.
        GUIContent[] iconContents = new GUIContent[m_icons.Count];
        for (int i = 0; i < m_icons.Count; i++)
        {
            iconContents[i] = new GUIContent(m_icons[i]);
        }

        // Create a custom GUIStyle to control the icon size.
        GUIStyle iconButtonStyle = new GUIStyle(GUI.skin.button);
        // Adjust these values to change the displayed size of your icons.
        iconButtonStyle.fixedWidth = 32;
        iconButtonStyle.fixedHeight = 32;

        // Compute the number of columns dynamically.
        int columns = 5; // fallback value
        if (position.width > 0)
        {
            // Each icon button is the fixed width plus the margins.
            int totalWidth = (int)iconButtonStyle.fixedWidth + iconButtonStyle.margin.horizontal;
            columns = Mathf.FloorToInt((position.width - 20) / totalWidth);
            columns = Mathf.Max(1, columns);
        }

        // Wrap the grid in a scroll view so you can see all icons.
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Height(position.height - 60));
        m_selectedIcon = GUILayout.SelectionGrid(m_selectedIcon, iconContents, columns, iconButtonStyle, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // Listen for keyboard events.
        if (Event.current != null)
        {
            if (Event.current.isKey)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    ApplyIcon(m_icons[m_selectedIcon]);
                    Close();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                }
            }
            // Check for a double-click on an icon.
            else if (Event.current.type == EventType.MouseDown &&
                     Event.current.button == 0 &&
                     Event.current.clickCount == 2)
            {
                ApplyIcon(m_icons[m_selectedIcon]);
                Close();
                Event.current.Use();
            }
        }

        if (GUILayout.Button("Apply", GUILayout.Width(100)))
        {
            ApplyIcon(m_icons[m_selectedIcon]);
            Close();
        }
    }

    void ApplyIcon(Texture2D icon)
    {
        AssetDatabase.StartAssetEditing();
        foreach (Object asset in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            MonoImporter monoImporter = AssetImporter.GetAtPath(path) as MonoImporter;
            if (monoImporter != null)
            {
                monoImporter.SetIcon(icon);
                AssetDatabase.ImportAsset(path);
            }
        }
        AssetDatabase.StopAssetEditing();
    }
}
