using UnityEngine;
using UnityEngine.Android;

public class GridNode
{
    public Vector2Int GetPos { get; }
    public Vector2 GetWorldPos { get; }

    private readonly bool isWalkable;
    private readonly bool isTowerPlacable;
    private readonly bool isVillageArea;
    public bool IsBlocked { get; private set; }

    /// <summary>
    /// Reference to the tower blocking this Node
    /// </summary>
    public Tower Tower { get; private set; }

    public int GCost { get; set; }
    public int HCost { get; set; }

    // GETTERS

    public int FCost => GCost + HCost;

    public bool IsDecoration => !isWalkable && !isTowerPlacable && !isVillageArea;
    public bool IsWalkable => isWalkable && !IsBlocked;
    public bool IsWalkableOnly => isWalkable && !isTowerPlacable;
    public bool IsTowerPlacable => isTowerPlacable && !IsBlocked;
    public bool IsTowerPlacableOnly => !isWalkable && isTowerPlacable;

    public GridNode Parent { get; set; }

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

    /// <summary>
    /// Set to false if you want the Node to be NOT Walkable
    /// </summary>
    public void Block(bool block, Tower towerBlockingNode)
    {
        if (isTowerPlacable)
        {
            IsBlocked = block;
            Tower = IsBlocked ? towerBlockingNode : null;
        }
        else
            Debug.Log($"Can't Block Because The Node Is NOT TowerPlacable");
    }
}