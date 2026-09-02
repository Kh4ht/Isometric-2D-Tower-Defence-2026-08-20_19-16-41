using System.Collections.Generic;
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
    public GameObject prefab;
    public Sprite icon;

    [Space(20)]
    // PRICE
    [Foldout("Price")]
#if UNITY_EDITOR
    [Min(1)]
    [SerializeField] private float priceMultiplier = 1.2f;
#endif

    [Min(0)]
    public List<int> price = new(6);
    [EndFoldout]

    // RANGE
    [Foldout("Range")]
#if UNITY_EDITOR
    [Min(1)]
    [SerializeField] private float rangeMultiplier = 1.2f;
#endif

    [Min(0)]
    public List<float> range = new(6);
    [EndFoldout]

    [Space(20)]

    // SHOOTING
    public bool haveShootingSubSys = true;

    [EnableIf("haveShootingSubSys")]
    [Foldout("Shooting")]
    public Vector2 bulletSpawnOffset = new(0f, 0.5f);
    public BulletData bulletData;
    public float shootCooldown = 1f;

    [EndIf]
    [EndFoldout]


    [Space(20)]

    public int test65;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        price.KHMatchCount(Tower.TOWER_MAX_LEVEL);
        range.KHMatchCount(Tower.TOWER_MAX_LEVEL);

        if (string.IsNullOrEmpty(ID))
            id = Kh.GenerateId(name, 8);
    }

    #endregion
    #region PRIVATE

#if UNITY_EDITOR
    [Foldout("Price")]
    [Button(color = "green")]
    private void AutoSetPriceUpgrades()
    {
        for (int i = 1; i < Tower.TOWER_MAX_LEVEL; i++)
            price[i] = Mathf.RoundToInt(price[i - 1] * priceMultiplier);
    }

    [Foldout("Range")]
    [Button(color = "green")]
    private void AutoSetRangeUpgrades()
    {
        for (int i = 1; i < Tower.TOWER_MAX_LEVEL; i++)
            range[i] = (range[i - 1] * rangeMultiplier).KHRoundToDecimalPlaces();
    }
#endif

    #endregion
}
