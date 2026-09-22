using System.Collections.Generic;
using Assets.Scripts.Utils;
using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public class BulletData : ScriptableObject
{
    #region FIELDS

    [SerializeField, ReadOnly]
    private string id;
    public string ID => id;

    [Space(20)]

    public Bullet prefab;

    public ElementType elementType = ElementType.None;

    [Space(20)]

    [Foldout("MoveSpeed"), Min(0)] public List<float> moveSpeed = new(6); [EndFoldout]
    [Foldout("Damage"), Min(0)] public List<float> damage = new(6); [EndFoldout]

    [Space(10)]

    public BulletMoveType type = BulletMoveType.Straight;

    // [Space(10), ShowIf(nameof(type), BulletType.Straight)]

    // [Space(10), ShowIf(nameof(type), BulletType.Parabolic)]

    // [Space(10), ShowIf(nameof(type), BulletType.Laser)]

    // [Space(10), ShowIf(nameof(type), BulletType.Follow)]

    // [HideIf(nameof(type), BulletType.Laser)]
    // public float acceleration = 0f;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(ID))
            id = Kh.GenerateId(name, 8);
    }

    #endregion
    #region PRIVATE

#if UNITY_EDITOR
    [Foldout("Damage")]
    [Button(color = "green")]
    private void AutoSetDamageUpgrades(float multiplier)
    {
        damage.AutoSetListBasedOnFirstElement(multiplier);
    }
    [EndFoldout]

    [Foldout("MoveSpeed")]
    [Button(color = "green")]
    private void AutoSetMoveSpeedUpgrades(float multiplier)
    {
        moveSpeed.AutoSetListBasedOnFirstElement(multiplier);
    }
    [EndFoldout]

    [Space(100), SerializeField] private bool enableListCountEditingButton = false;
    [EnableIf(nameof(enableListCountEditingButton))]
    [Button(color = "green")]
    private void EditAllListsCount()
    {
        damage.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
        moveSpeed.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
    }
#endif

    #endregion
}