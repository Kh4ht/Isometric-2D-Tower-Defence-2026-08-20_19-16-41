using System;
using System.Collections.Generic;
using UnityEngine;

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

        public void Select(List<Vector2Int> hoveredCells, UIController uIController)
        {
            cells.Clear();
            cells.AddRange(hoveredCells);

            selected = true;

            uIController.KH_UpShow();
        }

        public void Deselect(UIController uIController)
        {
            selected = false;

            uIController.KH_UpHide();
        }

        public void ToggleSelect(List<Vector2Int> hoveredCells, UIController uIController)
        {
            selected = !selected;

            if (selected)
                Select(hoveredCells, uIController);
            else
                Deselect(uIController);
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