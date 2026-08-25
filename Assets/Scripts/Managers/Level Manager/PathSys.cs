using System;
using System.Collections.Generic;
using MyClasses;
using MyHelper;
using UnityEngine;
using KH;
using System.Data.Common;

public class PathSys : KHManagedBehaviour
{
    #region FIELDS

    public static PathSys Ins { get; private set; }
    private const int MOVE_COST = 10;

    [HideInInspector, NonSerialized] public GridNode[,] mapGrid;

    private Vector2Int gridOrigin;

    private List<List<Vector2Int>> currentPaths = new();

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Destroy(gameObject);
    }

    protected override void Start()
    {
        base.Start();

        BuildGrid();

        UpdatePaths();
    }

    private void OnDrawGizmos()
    {
        if (LevelManager.Ins == null || LevelManager.Ins.levelData == null)
            return;


        DrawStartAndTargetGizmos();

        DrawWalkableGridGizmos();

        void DrawStartAndTargetGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2Int pos in LevelManager.Ins.levelData.pathStartCells)
                Gizmos.DrawCube(LevelManager.Ins.walkableTilemap.GetCellCenterWorld((Vector3Int)pos), new Vector2(0.2f, 0.2f));

            Gizmos.color = Color.green;
            Gizmos.DrawCube(LevelManager.Ins.walkableTilemap.GetCellCenterWorld((Vector3Int)LevelManager.Ins.levelData.pathTargetCell), new Vector2(0.2f, 0.2f));
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

            if (node == null)
                return false;

            if (!node.IsWalkable)
                return false;

            affectedNodes.Add((node, node.IsWalkable));
        }

        foreach ((GridNode node, _) in affectedNodes)
            node.IsWalkable = false;

        bool canReachTarget = true;

        foreach (Vector2Int startCell in LevelManager.Ins.levelData.pathStartCells)
        {
            if (Ins.FindPathAlgorithm(startCell, LevelManager.Ins.levelData.pathTargetCell) == null)
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
            return null;

        if (!startNode.IsWalkable || !targetNode.IsWalkable)
            return null;

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

    private void UpdatePaths()
    {
        currentPaths.Clear();

        foreach (Vector2Int pos in LevelManager.Ins.levelData.pathStartCells)
        {
            List<Vector2Int> currentPath = FindPathAlgorithm(pos,
                                                             LevelManager.Ins.levelData.pathTargetCell);

            if (currentPath != null)
                currentPaths.Add(currentPath);
        }

        DrawPaths();
    }

    // PATH VISUALIZER
    private void DrawPaths()
    {
        // Restore every cell to the Rule Tile
        foreach (GridNode node in mapGrid)
        {
            if (node == null)
                continue;

            LevelManager.Ins.groundTilemap.SetTile((Vector3Int)node.CellPosition,
                                                   LevelManager.Ins.groundNormalTile);
        }

        foreach (List<Vector2Int> path in currentPaths)
        {
            if (path == null)
                return;

            foreach (Vector2Int cell in path)
            {
                LevelManager.Ins.groundTilemap.SetTile((Vector3Int)cell, LevelManager.Ins.groundRuleTile);
            }
        }
    }

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

    private IEnumerable<GridNode> GetNeighbors(GridNode node)
    {
        foreach (Vector2Int direction in Directions)
        {
            GridNode neighbor =
                GetNode(node.CellPosition + direction);

            if (neighbor == null)
                continue;

            if (!neighbor.IsWalkable)
                continue;

            yield return neighbor;
        }
    }

    private void BuildGrid()
    {
        BoundsInt bounds = LevelManager.Ins.walkableTilemap.cellBounds;

        gridOrigin = (Vector2Int)bounds.min;

        mapGrid = new GridNode[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector2Int cell = new(x, y);

                bool walkable = LevelManager.Ins.walkableTilemap.HasTile((Vector3Int)cell);

                int gridX = x - bounds.xMin;
                int gridY = y - bounds.yMin;

                mapGrid[gridX, gridY] = new GridNode(cellPosition: cell,
                                                     cellWorldPosition: LevelManager.Ins.walkableTilemap.GetCellCenterWorld((Vector3Int)cell),
                                                     isWalkable: walkable);
            }
        }
    }

    #endregion
}