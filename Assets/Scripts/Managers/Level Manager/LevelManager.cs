using System.Collections.Generic;
using KH;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using VInspector;

public class LevelManager : KHManagedBehaviour
{
    #region FIELDS

    public static LevelManager Ins { get; private set; }

    // INSPECTOR

    [Foldout("TILE MAPs")]
    public Tilemap groundTilemap;
    public Tilemap towerPlacableTilemap;
    public Tilemap walkableTilemap;

    [Foldout("TILEs")]
    public TileBase groundRuleTile;
    public TileBase groundNormalTile;

    [Foldout("DATA")]
    public LevelData levelData;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Destroy(gameObject);
    }

    #endregion
    #region PUBLIC

    public Vector2 CellToWorld(Vector2Int cell)
    {
        return walkableTilemap.GetCellCenterWorld((Vector3Int)cell);
    }

    public bool CanPlaceTower(List<Vector2Int> cells)
    {
        if (!ValidateTowerPlacementCells(cells))
            return false;

        if (!PathSys.Ins.CanBlockCells(cells))
            return false;

        return true;
    }

    /// <summary>
    /// Check if a tower can be placed on this cell.
    /// </summary>
    public bool ValidateTowerPlacementCell(Vector2Int hoveredCell)
    {
        if (PathSys.Ins.GetNode(hoveredCell).IsWalkable)
            return true;

        return false;
    }
    public bool ValidateTowerPlacementCells(List<Vector2Int> hoveredCells)
    {
        foreach (var hoveredCell in hoveredCells)
        {
            if (ValidateTowerPlacementCell(hoveredCell))
                continue;
            else
                return false;
        }

        return true;
    }
    #endregion
}