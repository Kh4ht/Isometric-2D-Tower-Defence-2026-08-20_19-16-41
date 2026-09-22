using System.Collections.Generic;
using Assets.Scripts.Utils;
using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    #region FIELDS

    [SerializeField, ReadOnly]
    private string id;
    public string ID => id;

    [Space(20)]
    public Tower prefab;

    [Space(20)]
    public List<Sprite> icons;

    [Foldout("Price")][Min(0)] public List<int> price = new(6); [EndFoldout]

    [Foldout("Range")][Min(0)] public List<float> range = new(6); [EndFoldout]

    [Space(20)]

    public bool haveShootingSubSys = true;

    [EnableIf("haveShootingSubSys")]
    [Foldout("Shooting")]
    public Vector2 bulletSpawnOffset = new(0f, 0.5f);
    public BulletData bulletData;

    [Foldout("Shooting/ShootCooldown")][Min(0)] public List<float> shootCooldown = new(6); [EndFoldout]

    [EndIf]

    // GETTERS
    public int PurchasePrice => price[0];

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
    [Foldout("Price")]
    [Button(color = "green")]
    private void AutoSetPriceUpgrades(float multiplier)
    {
        price.AutoSetListBasedOnFirstElement(multiplier);
    }

    [EndFoldout]

    [Foldout("Range")]
    [Button(color = "green")]
    private void AutoSetRangeUpgrades(float multiplier)
    {
        range.AutoSetListBasedOnFirstElement(multiplier);
    }

    [EndFoldout]

    [Foldout("Shooting/ShootCooldown")]
    [Button(color = "green")]
    private void AutoShootCooldownUpgrades(float multiplier)
    {
        shootCooldown.AutoSetListBasedOnFirstElement(multiplier);
    }

    [EndFoldout]

    [Space(100)]
    [SerializeField] private bool enableListCountEditingButton;
    [EnableIf(nameof(enableListCountEditingButton))]
    [Button(color = "green")]
    private void EditAllListsCount()
    {
        icons.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
        range.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
        price.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
        shootCooldown.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);
    }
#endif

    #endregion
}