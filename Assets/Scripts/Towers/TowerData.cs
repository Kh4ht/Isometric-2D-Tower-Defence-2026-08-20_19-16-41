using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    [field: Foldout("ID")]
    [field: ReadOnly, ShowInInspector]
    public string ID { get; private set; }
    [EndFoldout]

    public GameObject prefab;
    public Sprite icon;


    [Foldout("ID")]
    [Button]
    private void GenerateIdIfNull()
    {
        if (!string.IsNullOrEmpty(ID))
            return;

        ID = Kh.GenerateId(name, 8);
    }

    private void OnValidate()
    {
        GenerateIdIfNull();
    }
}
