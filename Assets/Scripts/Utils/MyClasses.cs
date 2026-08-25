using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyClasses
{

    #region MouseHoverShadow

    [Serializable]
    public class MouseHoverShadow
    {
        public GameObject shadow;
        public SpriteRenderer spriteRenderer;
    }

    #endregion
    #region SelectedCells

    [Serializable]
    public class SelectedCells
    {
        public List<Vector2Int> cells = new();

        public bool selected;

        public void Select(List<Vector2Int> hoveredCells, KHUIController uIController)
        {
            cells.Clear();
            cells.AddRange(hoveredCells);

            selected = true;

            uIController.KH_UpShow();
        }

        public void Deselect(KHUIController uIController)
        {
            selected = false;

            uIController.KH_UpHide();
        }

        public void ToggleSelect(List<Vector2Int> hoveredCells, KHUIController uIController)
        {
            selected = !selected;

            if (selected)
                Select(hoveredCells, uIController);
            else
                Deselect(uIController);
        }

        public Vector3 GetCenterWorld(Tilemap tilemap)
        {
            if (cells == null || cells.Count == 0)
                return Vector3.zero;

            Vector3 center = Vector3.zero;

            foreach (Vector2Int cell in cells)
            {
                center += tilemap.GetCellCenterWorld(
                    new Vector3Int(cell.x, cell.y, 0)
                );
            }

            return center / cells.Count;
        }
    }

    #endregion
    #region GridNode

    public class GridNode
    {
        public Vector2Int CellPosition { get; }
        public Vector2 CellWorldPosition { get; }

        public bool IsWalkable { get; set; }

        public int GCost { get; set; }
        public int HCost { get; set; }

        public int FCost => GCost + HCost;

        public GridNode Parent { get; set; }

        public GridNode(Vector2Int cellPosition,
                        Vector2 cellWorldPosition,
                        bool isWalkable)
        {
            CellPosition = cellPosition;
            CellWorldPosition = cellWorldPosition;
            IsWalkable = isWalkable;
        }
    }

    #endregion
}