using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VInspector;

public enum GameTilemap
{
    Ground,
    TowerPlacable,
    Walkable,
    Decoration,
}

[Serializable]
public class GameGrid
{
    #region FIELDS

    public GridNode[,] grid { get; private set; }

    private Vector2Int gridOrigin;

    // INSPECTOR

    public Tilemap groundTilemap;
    public Tilemap towerPlacableTilemap;
    public Tilemap walkableTilemap;
    public Tilemap decorationTilemap;
    public Tilemap villageAreaTilemap;

    #endregion
    #region PRIVATE

    private Tilemap GetGameTileMap(GameTilemap gameTilemap)
    {
        return gameTilemap switch
        {
            GameTilemap.Ground => groundTilemap,
            GameTilemap.TowerPlacable => towerPlacableTilemap,
            GameTilemap.Walkable => walkableTilemap,
            GameTilemap.Decoration => decorationTilemap,
            _ => null
        };
    }

    #endregion
    #region PUBLIC

    public void BuildGrid()
    {
        BoundsInt bounds = groundTilemap.cellBounds;

        gridOrigin = (Vector2Int)bounds.min;

        grid = new GridNode[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector2Int cell = new(x, y);

                bool walkable = walkableTilemap.HasTile((Vector3Int)cell);
                bool towerPlacable = towerPlacableTilemap.HasTile((Vector3Int)cell);
                bool villageArea = villageAreaTilemap.HasTile((Vector3Int)cell);

                int gridX = x - bounds.xMin;
                int gridY = y - bounds.yMin;

                grid[gridX, gridY] = new GridNode(pos: cell,
                                                  worldPos: GetCellCenterWorld(cell),
                                                  isWalkable: walkable,
                                                  isTowerPlacable: towerPlacable,
                                                  isVillageArea: villageArea);
            }
        }
    }

    public void BlockNodes(List<Vector2Int> cells, bool block, Tower newTower)
    {
        foreach (Vector2Int cell in cells)
            GetNode(cell).Block(block, newTower);
    }

    public void EraseAllTiles(GameTilemap gameTilemap)
    {
        Tilemap selectedTilemap = GetGameTileMap(gameTilemap);

        foreach (GridNode gridNode in grid)
        {
            selectedTilemap.SetTile((Vector3Int)gridNode.GetPos, null);
        }
    }

    public Vector2 GetCellCenterWorld(Vector2Int cell)
    {
        return walkableTilemap.GetCellCenterWorld((Vector3Int)cell);
    }

    public List<Vector2> GetCellsCenterWorld(List<Vector2Int> cells)
    {
        List<Vector2> result = new();

        foreach (Vector2Int cell in cells)
        {
            result.Add(GetCellCenterWorld(cell));
        }

        return result;
    }

    public GridNode GetNode(Vector2Int cell)
    {
        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        if (x < 0 || x >= grid.GetLength(0))
            return null;

        if (y < 0 || y >= grid.GetLength(1))
            return null;

        return grid[x, y];
    }

    public GridNode GetNode(Vector2 cell)
    {
        return GetNode(WorldToCell(cell));
    }

    public bool HasTile(GameTilemap gameTilemap, Vector2Int pos)
    {
        return GetGameTileMap(gameTilemap).HasTile((Vector3Int)pos);
    }

    public void SetTile(GameTilemap gameTilemap, Vector2Int pos, TileBase tile)
    {
        GetGameTileMap(gameTilemap).SetTile((Vector3Int)pos, tile);
    }

    public Vector2Int WorldToCell(Vector2 cell)
    {
        return (Vector2Int)walkableTilemap.WorldToCell(cell);
    }

    #endregion
}