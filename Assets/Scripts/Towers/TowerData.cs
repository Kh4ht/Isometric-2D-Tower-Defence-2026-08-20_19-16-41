using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    [SerializeField, ReadOnly, Foldout("ID")]
    private string id;
    public string ID => id;
    [EndFoldout]

    public GameObject prefab;
    public Sprite icon;


    [Foldout("ID")]
    [Button]
    private void GenerateIdIfNull()
    {
        if (!string.IsNullOrEmpty(ID))
            return;

        id = Kh.GenerateId(name, 8);
    }

    private void OnValidate()
    {
        GenerateIdIfNull();
    }
}
