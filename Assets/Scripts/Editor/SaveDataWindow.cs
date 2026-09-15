#if UNITY_EDITOR

using KH;
using UnityEditor;
using UnityEngine;

public class KHSaveDataWindow : EditorWindow
{
    private SaveData saveData;

    [MenuItem("Tools/KH/Save Data Viewer")]
    public static void ShowWindow()
    {
        GetWindow<KHSaveDataWindow>("Save Data");
    }

    private void OnEnable()
    {
        LoadSaveData();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Save Data",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Reload"))
        {
            LoadSaveData();
        }

        EditorGUILayout.Space(10);

        if (saveData == null)
        {
            EditorGUILayout.HelpBox(
                "No save data loaded.",
                MessageType.Warning
            );

            return;
        }

        DrawSelectedTowers();
        DrawUnlockedTowers();

        EditorGUILayout.Space(15);

        if (GUILayout.Button("Save Current Data"))
        {
            KHSaveSystem.Save(saveData);
            Debug.Log("Save data saved.");
        }
    }

    private void DrawSelectedTowers()
    {
        EditorGUILayout.LabelField(
            "Selected Towers",
            EditorStyles.boldLabel
        );

        if (saveData.SelectedTowerIDs == null ||
            saveData.SelectedTowerIDs.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No selected towers.",
                MessageType.Info
            );

            return;
        }

        for (int i = 0; i < saveData.SelectedTowerIDs.Count; i++)
        {
            EditorGUILayout.LabelField(
                $"{i}: {saveData.SelectedTowerIDs[i]}"
            );
        }
    }

    private void DrawUnlockedTowers()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Unlocked Towers",
            EditorStyles.boldLabel
        );

        if (saveData.UnlockedTowers == null ||
            saveData.UnlockedTowers.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No unlocked towers.",
                MessageType.Info
            );

            return;
        }

        for (int i = 0; i < saveData.UnlockedTowers.Count; i++)
        {
            EditorGUILayout.LabelField(
                $"{i}: {saveData.UnlockedTowers[i]}"
            );
        }
    }



    private void LoadSaveData()
    {
        saveData = KHSaveSystem.Load<SaveData>();
        Repaint();
    }
}

#endif