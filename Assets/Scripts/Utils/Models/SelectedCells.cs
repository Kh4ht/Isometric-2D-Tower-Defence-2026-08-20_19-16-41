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

    public bool IsSelected { get; private set; }
    private Tower selectedTower;

    // INSPECTOR
    [SerializeField] private LevelTowerContainer horizontalTowersContainer;

    #endregion
    #region PUBLIC
    public void Deselect()
    {
        IsSelected = false;
        hoveredCells = null;

        horizontalTowersContainer.KH_UpHide();

        PathSys.Ins.EraseSecondaryPath();

        if (selectedTower != null)
        {
            selectedTower.OnSelected(false);
            selectedTower = null;
        }
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

    public void Select(Tower tower = null)
    {
        if (IsSelected)
        {
            Deselect();
            return;
        }

        selectedCells.Clear();
        selectedCells.AddRange(hoveredCells);

        IsSelected = true;

        horizontalTowersContainer.KH_UpShow();

        selectedTower = tower;
        horizontalTowersContainer.OnCellsSelected(tower);

        if (tower != null)
        {
            tower.OnSelected(true);
        }
        else
        {
            PathSys.Ins.DrawSecondaryPath(selectedCells);
        }
    }

    public void UpdateHoveredCells(List<Vector2Int> newHoveredCells, Action onHoverCellsUpdated)
    {
        if (hoveredCells == null || !hoveredCells.SequenceEqual(newHoveredCells))
        {
            hoveredCells = newHoveredCells;
            onHoverCellsUpdated();
        }
    }

    #endregion
}