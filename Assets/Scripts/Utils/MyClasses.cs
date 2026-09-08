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
        public enum ShadowColor
        {
            Red,
            Green,
            GreenSelected,
        }

        public GameObject shadow;
        private SpriteRenderer spriteRenderer;

        public static void SetColors(List<MouseHoverShadow> shadowColors, ShadowColor shadowColor)
        {
            foreach (var shadow in shadowColors)
                shadow.SetColor(shadowColor);
        }

        public void SetColor(ShadowColor shadowColor)
        {
            if (spriteRenderer == null)
                spriteRenderer = shadow.GetComponent<SpriteRenderer>();

            switch (shadowColor)
            {
                case ShadowColor.Red:
                    spriteRenderer.color = new Color(1f, 0f, 0f, 0.15f);
                    break;

                case ShadowColor.Green:
                    spriteRenderer.color = new Color(0f, 1f, 0f, 0.15f);
                    break;

                case ShadowColor.GreenSelected:
                    spriteRenderer.color = new Color(0f, 1f, 0f, 0.25f);
                    break;
            }
        }
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
        public bool IsTowerPlacable { get; set; }

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