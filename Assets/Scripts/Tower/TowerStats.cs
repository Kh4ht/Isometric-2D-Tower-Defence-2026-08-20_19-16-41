using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

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
    [SerializeField] private List<Vector2Int> occupiedCells;
    [SerializeField] private TargetSearchType targetSearchType;
    [SerializeField] private int sellPrice;
    [SerializeField] private int lvl;
    [SerializeField] private int nextUpgradePrice;
    [SerializeField] private float shootCooldown;
    [SerializeField] private Enemy enemyTargeted;

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

    // PUBLIC API
    public void Reset(TowerData towerData, List<Vector2Int> occupiedCells)
    {
        shootCooldown = towerData.shootCooldown[0];
        range = towerData.range[0];
        sellPrice = towerData.PurchasePrice / 2;
        this.occupiedCells = new(occupiedCells);
        lvl = 0;
        enemyTargeted = null;
        targetSearchType = TargetSearchType.First;
        nextUpgradePrice = towerData.price[1];
    }

    #endregion
}