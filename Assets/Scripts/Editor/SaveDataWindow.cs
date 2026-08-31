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
        DrawMaxHeldTowerCount();

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

        if (saveData.selectedTowerIDs == null ||
            saveData.selectedTowerIDs.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No selected towers.",
                MessageType.Info
            );

            return;
        }

        for (int i = 0; i < saveData.selectedTowerIDs.Count; i++)
        {
            EditorGUILayout.LabelField(
                $"{i}: {saveData.selectedTowerIDs[i]}"
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

        if (saveData.unlockedTowers == null ||
            saveData.unlockedTowers.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No unlocked towers.",
                MessageType.Info
            );

            return;
        }

        for (int i = 0; i < saveData.unlockedTowers.Count; i++)
        {
            EditorGUILayout.LabelField(
                $"{i}: {saveData.unlockedTowers[i]}"
            );
        }
    }

    private void DrawMaxHeldTowerCount()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Max Held Tower Count",
            EditorStyles.boldLabel
        );

        saveData.maxSelectedTowersCount =
            EditorGUILayout.IntField(
                saveData.maxSelectedTowersCount
            );
    }

    private void LoadSaveData()
    {
        saveData = KHSaveSystem.Load<SaveData>();
        Repaint();
    }
}

#endif