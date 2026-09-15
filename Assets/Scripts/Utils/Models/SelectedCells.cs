using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class SelectedCells
{
    #region FIELDS
    [HideInInspector] public List<Vector2Int> selectedCells = new();
    [HideInInspector] public List<Vector2Int> hoveredCells = new();

    public bool Selected { get; private set; }

    // INSPECTOR
    [SerializeField] private LevelTowerContainer horizontalTowersContainer;

    #endregion
    #region PUBLIC

    public bool IsHoveringSameTower(out Tower tower)
    {
        Tower t = null;

        foreach (var cell in hoveredCells)
        {
            if (!PathSys.Ins.gameGrid.GetNode(cell).IsBlocked || PathSys.Ins.gameGrid.GetNode(cell).Tower == null)
            {
                tower = null;
                return false;
            }

            if (t == null)
            {
                t = PathSys.Ins.gameGrid.GetNode(cell).Tower;
                continue;
            }

            if (PathSys.Ins.gameGrid.GetNode(cell).Tower != t)
            {
                tower = null;
                return false;
            }
        }


        tower = t;
        return true;
    }

    public void Deselect()
    {
        Selected = false;
        hoveredCells = null;

        horizontalTowersContainer.KH_UpHide();
    }

    /// <summary>
    /// Computes the average world-space position of all cells in <see cref="selectedCells"/>,
    /// i.e. the center point of the selected cell group.
    /// </summary>
    /// <returns>
    /// The averaged world-space center of all selected cells, or <see cref="Vector2.zero"/>
    /// if <see cref="selectedCells"/> is null or empty.
    /// </returns>
    public Vector2 GetCenterWorld()
    {
        if (selectedCells == null || selectedCells.Count == 0)
            return Vector2.zero;

        Vector2 center = Vector2.zero;

        foreach (Vector2Int cell in selectedCells)
        {
            center += PathSys.Ins.gameGrid.GetCellCenterWorld(
                new Vector2Int(cell.x, cell.y)
            );
        }

        return center / selectedCells.Count;
    }

    public void SelectHovered()
    {
        if (Selected)
        {
            Deselect();
            return;
        }

        selectedCells.Clear();
        selectedCells.AddRange(hoveredCells);

        Selected = true;

        horizontalTowersContainer.KH_UpShow();

        horizontalTowersContainer.ShowBuyOptions();
    }

    public void SelectTower(Tower tower)
    {
        if (Selected)
        {
            Deselect();
            return;
        }

        selectedCells.Clear();
        selectedCells.AddRange(hoveredCells);

        Selected = true;

        horizontalTowersContainer.KH_UpShow();

        horizontalTowersContainer.ShowSellUpgradeOptions(tower);
    }

    public void UpdateHoveredCells(List<Vector2Int> newHoveredCells, Action onUpdated)
    {
        if (hoveredCells == null || !hoveredCells.SequenceEqual(newHoveredCells))
        {
            hoveredCells = newHoveredCells;
            onUpdated();
        }
    }

    #endregion
}