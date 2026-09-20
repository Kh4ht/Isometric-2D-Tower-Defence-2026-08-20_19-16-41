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
    [SerializeField] private TargetSearchType targetSearchType = TargetSearchType.First;
    [SerializeField] private int sellPrice;
    [SerializeField] private float shootCooldown;

    [ReadOnly] public int lvl = 0;
    public Enemy enemyTargeted = null;

    // EVENTS
    public event Action<float> OnRangeChanged;
    public event Action<float> OnShootCooldownChanged;
    public event Action<int> OnSellPriceChanged;
    public event Action<TargetSearchType> OnTargetSearchTypeChanged;
    public event Action<List<Vector2Int>> OnOccupiedCellsChanged;

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
        OnOccupiedCellsChanged?.Invoke(newValue);
    }

    public TargetSearchType GetTargetSearchType() => targetSearchType;
    public void SetTargetSearchType(TargetSearchType newValue)
    {
        if (newValue == targetSearchType)
            return;

        targetSearchType = newValue;
        OnTargetSearchTypeChanged?.Invoke(newValue);
    }

    public int GetSellPrice() => sellPrice;
    public void SetSellPrice(int newValue)
    {
        if (newValue == sellPrice)
            return;

        sellPrice = newValue;
        OnSellPriceChanged?.Invoke(newValue);
    }

    public float GetShootCooldown() => shootCooldown;
    public void SetShootCooldown(float newValue)
    {
        if (newValue == shootCooldown)
            return;

        shootCooldown = newValue;
        OnShootCooldownChanged?.Invoke(newValue);
    }

    // PUBLIC API
    public void Reset(TowerData towerData, List<Vector2Int> occupiedCells)
    {
        shootCooldown = towerData.shootCooldown;
        range = towerData.range[0];
        sellPrice = towerData.price[0] / 2;
        this.occupiedCells = new(occupiedCells);
    }

    #endregion
}