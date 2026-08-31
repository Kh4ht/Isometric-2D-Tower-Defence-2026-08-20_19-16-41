using System;
using System.Collections.Generic;
using MyClasses;
using MyHelper;
using UnityEngine;
using KH;
using VInspector;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PathSys : KHManagedBehaviour
{
    #region FIELDS

#if UNITY_EDITOR
    private static PathSys insEditor;
    public static PathSys InsEditor
    {
        get
        {
            if (insEditor != null)
                return insEditor;

            return insEditor = FindAnyObjectByType<PathSys>();
        }
    }
#endif

    public static PathSys Ins { get; private set; }
    private const int MOVE_COST = 10;

    [HideInInspector, NonSerialized] public GridNode[,] mapGrid;

    private Vector2Int gridOrigin;

    public readonly List<List<Vector2Int>> currentPaths = new();
    private List<List<Vector2Int>> oldPaths = new();

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // INSPECTOR

    [Foldout("TILE MAPs")]
    public Tilemap groundTilemap;
    public Tilemap towerPlacableTilemap;
    public Tilemap walkableTilemap;
    public Tilemap decorationTilemap;

    [Foldout("TILEs")]
    public TileBase groundRuleTile;
    public TileBase groundNormalTile;
    public TileBase decorationTile;

    [Foldout("DATA")]
    public PathSysData data;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogWarning("More Than One Instance");
    }

    protected override void Start()
    {
        base.Start();

        BuildGrid();

        UpdatePaths();
    }

    private void OnDrawGizmos()
    {
        if (data == null)
            return;


        DrawStartAndTargetGizmos();

        DrawWalkableGridGizmos();

        void DrawStartAndTargetGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2Int pos in data.pathStartCells)
                Gizmos.DrawCube(GetCellCenterWorld(pos), new Vector2(0.2f, 0.2f));

            Gizmos.color = Color.green;
            Gizmos.DrawCube(GetCellCenterWorld(data.pathTargetCell), new Vector2(0.2f, 0.2f));
        }

        void DrawWalkableGridGizmos()
        {
            if (mapGrid == null)
                return;

            for (int i = 0; i < mapGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mapGrid.GetLength(1); j++)
                {
                    GridNode gridNode = mapGrid[i, j];

                    if (gridNode.IsWalkable)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.red;

                    Gizmos.DrawSphere(gridNode.CellWorldPosition, 0.025f);
                }
            }
        }
    }

    #endregion
    #region PUBLIC

    public bool CanPlaceTower(List<Vector2Int> cells)
    {
        if (!ValidateTowerPlacementCells(cells))
            return false;

        if (!Ins.CanBlockCells(cells))
            return false;

        return true;
    }

    /// <summary>
    /// Check if a tower can be placed on this cell.
    /// </summary>
    /// 
    public bool ValidateTowerPlacementCell(Vector2Int hoveredCell)
    {
        GridNode node = Ins.GetNode(hoveredCell);

        if (node == null)
            return false;

        return node.IsWalkable;
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

    public Vector2 GetCellCenterWorld(Vector2Int cell)
    {
        return walkableTilemap.GetCellCenterWorld((Vector3Int)cell);
    }

    public List<Vector2> GetCellCenterWorld(List<Vector2Int> cells)
    {
        List<Vector2> result = new();

        foreach (Vector2Int cell in cells)
        {
            result.Add(GetCellCenterWorld(cell));
        }

        return result;
    }

    public Vector3Int WorldToCell(Vector2 cell)
    {
        return walkableTilemap.WorldToCell(cell);
    }


    public GridNode GetNode(Vector2Int cell)
    {
        int x = cell.x - gridOrigin.x;
        int y = cell.y - gridOrigin.y;

        if (x < 0 || x >= mapGrid.GetLength(0))
            return null;

        if (y < 0 || y >= mapGrid.GetLength(1))
            return null;

        return mapGrid[x, y];
    }

    public bool CanBlockCells(IEnumerable<Vector2Int> cells)
    {
        List<(GridNode node, bool wasWalkable)> affectedNodes = new();

        foreach (Vector2Int cell in cells)
        {
            GridNode node = Ins.GetNode(cell);

            if (!node.IsWalkable)
                return false;

            affectedNodes.Add((node, node.IsWalkable));
        }

        foreach ((GridNode node, _) in affectedNodes)
            node.IsWalkable = false;

        bool canReachTarget = true;

        foreach (Vector2Int startCell in data.pathStartCells)
        {
            if (Ins.FindPathAlgorithm(startCell, data.pathTargetCell) == null)
            {
                canReachTarget = false;
                break;
            }
        }

        foreach ((GridNode node, bool wasWalkable) in affectedNodes)
            node.IsWalkable = wasWalkable;

        return canReachTarget;
    }

    // A* PATH FINDING ALGORITHM
    public List<Vector2Int> FindPathAlgorithm(Vector2Int startCell,
                                               Vector2Int targetCell)
    {
        ResetNodes();

        GridNode startNode = GetNode(startCell);
        GridNode targetNode = GetNode(targetCell);

        if (startNode == null || targetNode == null)
        {
            Debug.Log($"{nameof(startNode)} or {nameof(targetNode)} is NULL");
            return null;
        }

        if (!startNode.IsWalkable || !targetNode.IsWalkable)
        {
            Debug.Log($"{nameof(startNode)} or {nameof(targetNode)} is not on a Walkable Tilemap Area");
            return null;
        }

        startNode.GCost = 0;

        List<GridNode> openSet = new();
        HashSet<GridNode> closedSet = new();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                GridNode node = openSet[i];

                if (node.FCost < currentNode.FCost ||
                    node.FCost == currentNode.FCost &&
                    node.HCost < currentNode.HCost)
                {
                    currentNode = node;
                }
            }

            if (currentNode == targetNode)
                return Helper.RetracePath(startNode, targetNode);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (GridNode neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int newMovementCost = currentNode.GCost + MOVE_COST;

                if (newMovementCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCost;

                    neighbor.HCost = Helper.GetDistanceAlgorithm(neighbor, targetNode, MOVE_COST);

                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.Log("No Paths");
        return null;
    }

    public void BlockCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            GetNode(cell).IsWalkable = false;
        }

        UpdatePaths();
    }

    public void UnBlockCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            GetNode(cell).IsWalkable = true;
        }

        UpdatePaths();
    }

    #endregion
    #region PRIVATE

#if UNITY_EDITOR
    [Button, Foldout("EDITOR TOOLS")]
    private void AutoDrawTiles()
    {
        BuildGrid();

        for (int i = 0; i < mapGrid.GetLength(0); i++)
        {
            for (int j = 0; j < mapGrid.GetLength(1); j++)
            {
                GridNode gridNode = mapGrid[i, j];

                if (!groundTilemap.HasTile((Vector3Int)gridNode.CellPosition))
                    continue;

                // TODO: Add !gridNode.IsTowerPlacable check also

                if (!gridNode.IsWalkable)
                {
                    decorationTilemap.SetTile((Vector3Int)gridNode.CellPosition, decorationTile);
                }
            }
        }
    }
    [EndFoldout]
#endif

    private void UpdatePaths()
    {
        // Store the old path
        oldPaths = new(currentPaths);

        currentPaths.Clear();

        foreach (Vector2Int pos in data.pathStartCells)
        {
            List<Vector2Int> currentPath = FindPathAlgorithm(pos,
                                                             data.pathTargetCell);

            currentPaths.Add(currentPath);
        }

        DrawPaths();
    }

    // PATH VISUALIZER
    // Clear the previous route visuals before repainting the current valid paths.
    private void DrawPaths()
    {
        // Restore any previously drawn path tiles to their default floor appearance.
        foreach (List<Vector2Int> path in oldPaths)
        {
            foreach (Vector2Int cell in path)
            {
                groundTilemap.SetTile((Vector3Int)GetNode(cell).CellPosition,
                                                       groundNormalTile);
            }
        }

        // Draw each newly computed route using the path indicator tile.
        foreach (List<Vector2Int> path in currentPaths)
        {
            foreach (Vector2Int cell in path)
            {
                groundTilemap.SetTile((Vector3Int)cell, groundRuleTile);
            }
        }
    }

    // Reset pathFinding metadata before running a fresh A* search.
    private void ResetNodes()
    {
        foreach (GridNode node in mapGrid)
        {
            if (node == null)
                continue;

            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.Parent = null;
        }
    }

    // Return only valid walkable neighbors for the current node during pathFinding.
    private IEnumerable<GridNode> GetNeighbors(GridNode node)
    {
        foreach (Vector2Int direction in Directions)
        {
            GridNode neighbor = GetNode(node.CellPosition + direction);

            if (neighbor == null)
                continue;

            if (!neighbor.IsWalkable)
                continue;

            yield return neighbor;
        }
    }

    private void BuildGrid()
    {
        BoundsInt bounds = groundTilemap.cellBounds;

        gridOrigin = (Vector2Int)bounds.min;

        mapGrid = new GridNode[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector2Int cell = new(x, y);

                bool walkable = walkableTilemap.HasTile((Vector3Int)cell);

                int gridX = x - bounds.xMin;
                int gridY = y - bounds.yMin;

                mapGrid[gridX, gridY] = new GridNode(cellPosition: cell,
                                                     cellWorldPosition: GetCellCenterWorld(cell),
                                                     isWalkable: walkable);
            }
        }
    }

    #endregion
}

#region PathSysData

[Serializable]
public class PathSysData
{
    public List<Vector2Int> pathStartCells;
    public Vector2Int pathTargetCell;
}

#endregion