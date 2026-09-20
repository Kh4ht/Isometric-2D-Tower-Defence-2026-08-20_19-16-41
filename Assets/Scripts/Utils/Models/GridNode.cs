using UnityEngine;

/// <summary>
///Represents a single tile within the grid used for pathfinding and placement logic.
/// </summary>
public class GridNode
{
    #region FIELDS

    /// <summary>
    /// Gets the grid coordinates of this node.
    /// </summary>
    public Vector2Int GetPos { get; }

    /// <summary>
    /// Gets the world-space position of this node.
    /// </summary>
    public Vector2 GetWorldPos { get; }

    private readonly bool isWalkable;
    private readonly bool isTowerPlacable;
    private readonly bool isVillageArea;

    /// <summary>
    /// Reference to the tower blocking this node.
    /// </summary>
    private Tower tower;

    public int GCost { get; set; }
    public int HCost { get; set; }

    // GETTERS

    public int FCost => GCost + HCost;

    /// <summary>
    ///  Is this node is currently blocked by a tower.
    /// </summary>
    public bool IsBlocked => tower != null;

    /// <summary>
    ///  Is this node is a decorative tile.
    /// </summary>
    public bool IsDecoration => !isWalkable && !isTowerPlacable && !isVillageArea;

    /// <summary>
    ///  Is this node can be walked on and is not blocked.
    /// </summary>
    public bool IsWalkable => isWalkable && !IsBlocked && !IsSimulatedBlocked;

    /// <summary>
    /// Is this node is walkable but not tower-placable.
    /// </summary>
    public bool IsWalkableOnly => isWalkable && !isTowerPlacable;

    /// <summary>
    /// Is this node can accept a tower placement.
    /// </summary>
    public bool IsTowerPlacable => isTowerPlacable && !IsBlocked;

    /// <summary>
    /// Is this node is tower-placable but not walkable.
    /// </summary>
    public bool IsTowerPlacableOnly => !isWalkable && isTowerPlacable;

    /// <summary>
    /// Gets or sets the parent node used during pathfinding.
    /// </summary>
    public GridNode Parent { get; set; }

    /// <summary>
    /// Temporary "what-if" block used only while validating a placement (see PathSys.WillBlockEnemyPath).
    /// It makes the node unwalkable for pathfinding without needing a real Tower reference.
    /// Always reset it to false after the simulation.
    /// </summary>
    public bool IsSimulatedBlocked { get; set; }

    #endregion
    #region CONSTRUCTOR

    /// <summary>
    /// Initializes a new instance of the <see cref="GridNode"/> class.
    /// </summary>
    /// <param name="pos">The grid coordinates of the node.</param>
    /// <param name="worldPos">The world position of the node.</param>
    /// <param name="isWalkable">Whether the node is walkable.</param>
    /// <param name="isTowerPlacable">Whether the node can accept a tower placement.</param>
    /// <param name="isVillageArea">Whether the node belongs to a village area.</param>
    public GridNode(Vector2Int pos,
                    Vector2 worldPos,
                    bool isWalkable,
                    bool isTowerPlacable,
                    bool isVillageArea)
    {
        GetPos = pos;
        GetWorldPos = worldPos;
        this.isWalkable = isWalkable;
        this.isTowerPlacable = isTowerPlacable;
        this.isVillageArea = isVillageArea;
    }

    #endregion
    #region PUBLIC

    /// <summary>
    /// Returns the tower currently blocking this node, if any.
    /// </summary>
    /// <returns>The blocking tower or <c>null</c> if the node is not blocked.</returns>
    public Tower GetTower() => tower;

    /// <summary>
    /// Assigns a tower to this node if it is tower-placable.
    /// <para>Set <c>null</c> to unblock</para>
    /// </summary>
    /// <param name="tower">The tower to place on this node, <c>null</c> to unblock.</param>
    public void SetTower(Tower tower)
    {
        if (isTowerPlacable)
            this.tower = tower;
        else
            Debug.Log($"Can't Block Because The Node Is NOT TowerPlacable");
    }

    #endregion
}