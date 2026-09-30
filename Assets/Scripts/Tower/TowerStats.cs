using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;

[Serializable]
public class TowerStats
{
    #region FIELDS

    public enum TargetSearchType
    {
        First,
        Last,
        Strongest,
        Weakest,
    }

    [SerializeField] private float range;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float projectileMoveSpeed;
    [SerializeField] private float projectileParabolicArcHeightMultiplier;
    [SerializeField] private List<Vector2Int> occupiedCells;
    [SerializeField] private TargetSearchType targetSearchType;
    [SerializeField] private BulletTargetPosition projectileTargetPosType;
    [SerializeField] private BulletMoveType projectileMoveType;
    [SerializeField] private ElementType elementType;
    [SerializeField] private int sellPrice;
    [SerializeField] private int lvl;
    [SerializeField] private int nextUpgradePrice;
    [SerializeField] private Enemy enemyTargeted;
    [SerializeField] private AnimationCurve projectileSpeedMultiplierCurve;

    // EVENTS
    public event Action<float> OnRangeChanged;

    #endregion
    #region CONSTRUCTOR

    public TowerStats(TowerData towerData)
    {
        Reset(towerData, new());
    }

    #endregion
    #region PUBLIC

    // ENCAPSULATION
    public float GetRange() => range;
    public void SetRange(float newValue)
    {
        if (newValue == range)
            return;

        range = newValue;
        OnRangeChanged?.Invoke(newValue);
    }

    public List<Vector2Int> GetOccupiedCells() => occupiedCells;
    public void SetOccupiedCells(List<Vector2Int> newValue)
    {
        occupiedCells = new(newValue);
    }

    public TargetSearchType GetTargetSearchType() => targetSearchType;
    public void SetTargetSearchType(TargetSearchType newValue)
    {
        if (newValue == targetSearchType)
            return;

        targetSearchType = newValue;
    }

    public BulletTargetPosition GetProjectileTargetPosType() => projectileTargetPosType;
    public void SetProjectileTargetPosType(BulletTargetPosition newValue)
    {
        if (newValue == projectileTargetPosType)
            return;

        projectileTargetPosType = newValue;
    }

    public BulletMoveType GetProjectileMoveType() => projectileMoveType;
    public void SetProjectileMoveType(BulletMoveType newValue)
    {
        if (newValue == projectileMoveType)
            return;

        projectileMoveType = newValue;
    }

    public AnimationCurve GetProjectileSpeedMultiplierCurve() => projectileSpeedMultiplierCurve.CopyCurve();
    public void SetProjectileSpeedMultiplierCurve(AnimationCurve newValue)
    {
        if (newValue == projectileSpeedMultiplierCurve)
            return;

        projectileSpeedMultiplierCurve = newValue.CopyCurve();
    }

    public int GetSellPrice() => sellPrice;
    public void SetSellPrice(int newValue)
    {
        if (newValue == sellPrice)
            return;

        sellPrice = newValue;
    }

    public float GetShootCooldown() => shootCooldown;
    public void SetShootCooldown(float newValue)
    {
        if (newValue == shootCooldown)
            return;

        shootCooldown = newValue;
    }

    public float GetProjectileParabolicArcHeightMultiplier() => projectileParabolicArcHeightMultiplier;
    public void SetProjectileParabolicArcHeightMultiplier(float newValue)
    {
        if (newValue == projectileParabolicArcHeightMultiplier)
            return;

        projectileParabolicArcHeightMultiplier = newValue;
    }

    public float GetProjectileDamage() => projectileDamage;
    public void SetProjectileDamage(float newValue)
    {
        if (newValue == projectileDamage)
            return;

        projectileDamage = newValue;
    }

    public float GetProjectileMoveSpeed() => projectileMoveSpeed;
    public void SetProjectileMoveSpeed(float newValue)
    {
        if (newValue == projectileMoveSpeed)
            return;

        projectileMoveSpeed = newValue;
    }

    public int GetNextLvl => lvl + 1;
    public int GetLvl() => lvl;
    public void SetLvl(int newValue)
    {
        if (newValue == lvl)
            return;

        lvl = newValue;
    }

    public Enemy GetEnemyTargeted() => enemyTargeted;
    public void SetEnemyTargeted(Enemy newValue)
    {
        if (newValue == enemyTargeted)
            return;

        enemyTargeted = newValue;
    }

    public int GetNextUpgradePrice() => nextUpgradePrice;
    public void SetNextUpgradePrice(int newValue)
    {
        if (newValue == nextUpgradePrice)
            return;

        nextUpgradePrice = newValue;
    }

    public ElementType GetElementType() => elementType;
    public void SetElementType(ElementType newValue)
    {
        if (newValue == elementType)
            return;

        elementType = newValue;
    }

    // PUBLIC API
    public void Reset(TowerData towerData, List<Vector2Int> occupiedCells)
    {
        lvl = 0;
        enemyTargeted = null;
        shootCooldown = towerData.shootCooldown[0];
        range = towerData.range[0];
        sellPrice = towerData.PurchasePrice / 2;
        this.occupiedCells = new(occupiedCells);
        targetSearchType = TargetSearchType.First;
        nextUpgradePrice = towerData.price[1];
        projectileDamage = towerData.projectileDamage[0];
        projectileMoveSpeed = towerData.projectileMoveSpeed[0];
        elementType = towerData.elementType;
        projectileParabolicArcHeightMultiplier = towerData.projectileParabolicArcHeightMultiplier;
        projectileTargetPosType = towerData.projectileTargetPosType;
        projectileMoveType = towerData.projectileMoveType;
        projectileSpeedMultiplierCurve = towerData.projectileSpeedMultiplierCurve.CopyCurve();
    }

    #endregion
}