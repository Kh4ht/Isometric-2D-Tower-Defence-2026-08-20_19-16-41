using UnityEngine;
using VInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    #region FIELDS

    [ReadOnly]
    public string sceneName;

    [SerializeField] private SceneAsset sceneAsset;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        if (sceneAsset != null)
            sceneName = sceneAsset.name;
    }

    #endregion
}