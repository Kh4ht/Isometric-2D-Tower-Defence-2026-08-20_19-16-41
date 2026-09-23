using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    #region FIELDS

    [SerializeField, ReadOnly, Foldout("ID")]
    private string id;
    public string ID => id;
    [EndFoldout]

    public Enemy prefab;
    public Sprite icon;

    public ElementType elementType = ElementType.None;

    [DisableIf(nameof(elementType), ElementType.None)]
    public ElementStrength elementStrength = ElementStrength.Low;
    [EndIf]

    public int DeathAetherPrize;

    [Space(20)]

    [Min(1)] public int defaultMaxHealth = 100;

    [Min(0)] public float defaultMoveSpeed = 100;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(ID))
            return;

        id = Kh.GenerateId(name, 8);
    }

    #endregion
}