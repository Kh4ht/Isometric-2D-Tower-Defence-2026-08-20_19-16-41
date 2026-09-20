using System;
using System.Collections.Generic;
using System.Linq;
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
    [KHResetStatic]
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

    [KHResetStatic]
    public static PathSys Ins { get; private set; }
    private const int MOVE_COST = 10;

    private readonly List<List<Vector2Int>> currentPaths = new();
    private List<List<Vector2Int>> oldPaths = new();

    // GETTERS

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // INSPECTOR

    [Tab("STATS")]
    public Tilemap walkableTilemap;

    public GameGrid gameGrid;

    public TileBase groundRuleTile;
    public TileBase towerPlacableOnlyTile;
    public TileBase groundNormalTile;
    public TileBase decorationTile;

    [Tab("DATA")]
    public List<Vector2Int> pathStartCells;
    public Vector2Int pathTargetCell;

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

        gameGrid.BuildGrid();

        UpdatePaths();
    }

    private void OnDrawGizmos()
    {
        DrawStartAndTargetGizmos();

        DrawGridGizmos();

        void DrawStartAndTargetGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2Int pos in pathStartCells)
                Gizmos.DrawCube(walkableTilemap.GetCellCenterWorld((Vector3Int)pos), new Vector2(0.2f, 0.2f));

            Gizmos.color = Color.green;
            Gizmos.DrawCube(walkableTilemap.GetCellCenterWorld((Vector3Int)pathTargetCell), new Vector2(0.2f, 0.2f));
        }

        void DrawGridGizmos()
        {
            if (gameGrid.grid == null)
                return;

            foreach (GridNode node in gameGrid.grid)
            {
                if (node.IsDecoration)
                    continue;

                if (node.IsWalkable)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawSphere(node.GetWorldPos, 0.025f);
            }
        }
    }

    #endregion
    #region PRIVATE

#if UNITY_EDITOR
    private bool NotPlayMode => !Application.isPlaying;
    [EnableIf(nameof(NotPlayMode))]
    [Button(color = "green")]
    private void AutoDrawTiles()
    {
        gameGrid.BuildGrid();

        gameGrid.EraseAllTiles(GameTilemap.Decoration);

        foreach (GridNode gridNode in gameGrid.grid)
        {
            if (!gameGrid.HasTile(GameTilemap.Ground, gridNode.GetPos))
                continue;

            if (gridNode.IsTowerPlacableOnly)
            {
                gameGrid.SetTile(GameTilemap.Ground, gridNode.GetPos, towerPlacableOnlyTile);
                continue;
            }

            if (gridNode.IsDecoration)
            {
                gameGrid.SetTile(GameTilemap.Decoration, gridNode.GetPos, decorationTile);
            }
        }
    }
    [EndFoldout]
#endif

    /// <summary>
    /// Finds the shortest walkable path between two grid cells using the A* algorithm.
    /// </summary>
    /// <param name="startCell">The starting grid position.</param>
    /// <param name="targetCell">The destination grid position.</param>
    /// <returns>
    /// A list of grid positions from the start to the target, or null if either cell is invalid
    /// or not walkable.
    /// </returns>
    private List<Vector2Int> FindPathAlgorithm(Vector2Int startCell, Vector2Int targetCell)
    {
        ResetNodes();

        GridNode startNode = gameGrid.GetNode(startCell);
        GridNode targetNode = gameGrid.GetNode(targetCell);

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

    // private void UpdatePaths()
    // {
    //     // Store the old paths       
    //     oldPaths = new(currentPaths);

    //     // Update Paths based on the updated GameGrid 
    //     currentPaths.Clear();

    //     foreach (Vector2Int startPos in pathStartCells)
    //         currentPaths.Add(FindPathAlgorithm(startPos, pathTargetCell));

    //     // Update The Alive Enemy path    
    //     foreach (Enemy enemy in Helper.GetAllAliveEnemies())
    //     {
    //         int reachedPathIndex = enemy.stats.nextPathPointIndex;
    //         List<Vector2> oldEnemyPath = enemy.stats.GetPath();
    //         List<Vector2> newEnemyPath = GetPath(enemy.stats.pathIndex);
    //         int oldRemainingCount = oldEnemyPath.Count - reachedPathIndex;
    //         int newRemainingCount = newEnemyPath.Count - reachedPathIndex;
    //         List<Vector2> oldSuffix = oldEnemyPath.GetRange(reachedPathIndex, oldRemainingCount);

    //         bool unaffected = false;

    //         if (newEnemyPath.Count >= oldRemainingCount)
    //         {
    //             List<Vector2> newSuffix = newEnemyPath.GetRange(newEnemyPath.Count - oldRemainingCount, oldRemainingCount);
    //             unaffected = oldSuffix.SequenceEqual(newSuffix);
    //         }

    //         // Same remaining route, just shifted — realign the index, don't teleport progress.         
    //         if (unaffected)
    //             enemy.stats.nextPathPointIndex = newEnemyPath.Count - oldRemainingCount;
    //         // else: Do not change nextPathPointIndex  

    //         enemy.stats.SetPath(newEnemyPath);
    //     }

    //     DrawPaths();
    // }


    /// <summary>
    /// Recomputes all spawn routes after the grid changes, then re-paths each alive enemy
    /// from where it actually is. An enemy keeps its current route unless that route is
    /// blocked or a strictly shorter one exists.
    /// </summary>
    private void UpdatePaths()
    {
        // Store the old paths
        oldPaths = new(currentPaths);

        // Update the shared spawn -> target paths (used for visuals and new spawns)
        currentPaths.Clear();
        foreach (Vector2Int startPos in pathStartCells)
            currentPaths.Add(FindPathAlgorithm(startPos, pathTargetCell));

        // Each enemy decides for itself
        foreach (Enemy enemy in Helper.GetAllAliveEnemies())
            RepathEnemy(enemy);

        DrawPaths();
    }

    private void RepathEnemy(Enemy enemy)
    {
        List<Vector2> oldPath = enemy.stats.GetPath();
        int nextIndex = enemy.stats.GetNextPathPoint();
        int oldRemaining = oldPath.Count - nextIndex;

        // Best route from the enemy's current position
        List<Vector2Int> bestCells = FindPathFromEnemy(enemy, oldPath, nextIndex);

        // Enemy is completely cut off (shouldn't happen: placement is validated). Keep what it has.
        if (bestCells == null)
            return;

        // Keep the current route if it is still walkable and not longer than the new best.
        // "<=" is intentional: on ties we keep the old route so enemies don't jitter between equal paths.
        if (IsRouteWalkable(oldPath, nextIndex) && oldRemaining <= bestCells.Count)
            return;

        List<Vector2> newPath = gameGrid.GetCellsCenterWorld(bestCells);

        enemy.stats.SetPath(newPath);

        // Set the index AFTER SetPath in case SetPath resets it.
        // Skip the first waypoint if the enemy is already past its center, so it doesn't step backwards.
        enemy.stats.SetNextPathPoint(ShouldSkipFirstPoint(enemy.transform.position, newPath) ? 1 : 0);
    }

    /// <summary>
    /// A* from the enemy's current cell. If that cell just became blocked (a tower was placed
    /// on/under the enemy), fall back to the last waypoint the enemy already passed.
    /// </summary>
    private List<Vector2Int> FindPathFromEnemy(Enemy enemy, List<Vector2> oldPath, int nextIndex)
    {
        Vector2Int cell = enemy.transform.position.WorldToCell();

        List<Vector2Int> path = FindPathAlgorithm(cell, pathTargetCell);
        if (path != null)
            return path;

        if (nextIndex > 0 && nextIndex - 1 < oldPath.Count)
        {
            Vector2Int previousCell = (Vector2Int)walkableTilemap.WorldToCell(oldPath[nextIndex - 1]);
            return FindPathAlgorithm(previousCell, pathTargetCell);
        }

        return null;
    }

    /// <summary>
    /// True if every remaining waypoint of the route is still on a walkable node.
    /// </summary>
    private bool IsRouteWalkable(List<Vector2> route, int fromIndex)
    {
        for (int i = fromIndex; i < route.Count; i++)
        {
            Vector2Int cell = (Vector2Int)walkableTilemap.WorldToCell(route[i]);
            GridNode node = gameGrid.GetNode(cell);

            if (node == null || !node.IsWalkable)
                return false;
        }

        return true;
    }

    /// <summary>
    /// The first waypoint is the center of the enemy's own cell. If the enemy is already closer to
    /// the second waypoint than the first one is, walking to the first would be a step backwards.
    /// </summary>
    private static bool ShouldSkipFirstPoint(Vector2 enemyPos, List<Vector2> path)
    {
        if (path.Count < 2)
            return false;

        return Kh.GetSqrDistance(enemyPos, path[1]) < Kh.GetSqrDistance(path[0], path[1]);
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
                gameGrid.SetTile(GameTilemap.Ground, gameGrid.GetNode(cell).GetPos, groundNormalTile);
            }
        }

        // Draw each newly computed route using the path indicator tile.
        foreach (List<Vector2Int> path in currentPaths)
        {
            foreach (Vector2Int cell in path)
            {
                gameGrid.SetTile(GameTilemap.Ground, cell, groundRuleTile);
            }
        }
    }

    // Reset pathFinding metadata before running a fresh A* search.
    private void ResetNodes()
    {
        foreach (GridNode node in gameGrid.grid)
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
            GridNode neighbor = gameGrid.GetNode(node.GetPos + direction);

            if (neighbor == null)
                continue;

            if (!neighbor.IsWalkable)
                continue;

            yield return neighbor;
        }
    }

    #endregion
    #region PUBLIC

    public int GetDifferenceFromShortestPath(int pathIndex)
    {
        int shortestPathCount = int.MaxValue;

        foreach (List<Vector2Int> path in currentPaths)
        {
            if (path.Count < shortestPathCount)
                shortestPathCount = path.Count;
        }

        return currentPaths[pathIndex].Count - shortestPathCount;
    }

    public bool CanPlaceTower(List<Vector2Int> cells)
    {
        if (!CanPlaceTowerOnCells(cells))
            return false;

        if (WillBlockEnemyPath(cells))
            return false;

        return true;
    }

    /// <summary>
    /// Check if a tower can be placed on this cell.
    /// </summary>
    public bool IsTowerPlacable(Vector2Int cell)
    {
        GridNode node = gameGrid.GetNode(cell);

        if (node == null)
        {
            Debug.Log("node is null");
            return false;
        }

        return node.IsTowerPlacable;
    }

    public bool CanPlaceTowerOnCells(List<Vector2Int> hoveredCells)
    {
        foreach (var hoveredCell in hoveredCells)
        {
            if (IsTowerPlacable(hoveredCell))
                continue;
            else
                return false;
        }

        return true;
    }

    public List<Vector2> GetPath(int index)
    {
        return gameGrid.GetCellsCenterWorld(currentPaths[index]);
    }

    public bool WillBlockEnemyPath(IEnumerable<Vector2Int> cells)
    {
        List<GridNode> affectedNodes = new();

        foreach (Vector2Int cell in cells)
        {
            GridNode node = gameGrid.GetNode(cell);

            if (node == null)
            {
                Debug.Log($"{nameof(node)} is NULL");
                return true;
            }

            if (!node.IsTowerPlacable)
                return true;

            affectedNodes.Add(node);
        }

        foreach (GridNode node in affectedNodes)
            node.Block(true, null);

        bool willBlockPath = false;

        foreach (Vector2Int startCell in pathStartCells)
        {
            if (FindPathAlgorithm(startCell, pathTargetCell) == null)
            {
                willBlockPath = true;
                break;
            }
        }

        foreach (GridNode node in affectedNodes)
            node.Block(false, null);

        return willBlockPath;
    }

    public void BlockCells(List<Vector2Int> cells, bool block, Tower newTower)
    {
        gameGrid.BlockNodes(cells, block, newTower);

        UpdatePaths();
    }

    #endregion
}