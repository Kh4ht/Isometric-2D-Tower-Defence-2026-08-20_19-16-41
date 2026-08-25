using System.Collections.Generic;
using KH;
using MyClasses;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyHelper
{
    public static class Helper
    {
        #region offsets
        private static Vector2Int[] offsets =
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1)
        };

        #endregion
        #region GetDistance

        public static int GetDistanceAlgorithm(GridNode a, GridNode b, int moveCost)
        {
            int xDistance =
                Mathf.Abs(a.CellPosition.x - b.CellPosition.x);

            int yDistance =
                Mathf.Abs(a.CellPosition.y - b.CellPosition.y);

            return (xDistance + yDistance) * moveCost;
        }

        #endregion
        #region RetracePath

        public static List<Vector2Int> RetracePath(GridNode startNode,
                                                   GridNode targetNode)
        {
            List<Vector2Int> path = new();

            GridNode currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.CellPosition);

                currentNode = currentNode.Parent;
            }

            path.Add(startNode.CellPosition);

            path.Reverse();

            return path;
        }

        #endregion
        #region GetHoveredCells

        public static IEnumerable<Vector2Int> GetHoveredCells(Tilemap tilemap,
                                                           Vector2Int currentCell)
        {
            Vector2 mouseWorldPos = Kh.GetMouseWorldPos();

            Vector2Int[] origins =
            {
                currentCell,
                currentCell + Vector2Int.left,
                currentCell + Vector2Int.down,
                currentCell + new Vector2Int(-1, -1)
            };

            Vector2Int closestOrigin = origins[0];
            float closestDistance = float.MaxValue;

            foreach (Vector2Int origin in origins)
            {
                Vector2 center =
                    GetFootprintCenter(tilemap, origin);

                float distance =
                    (mouseWorldPos - center).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestOrigin = origin;
                }
            }

            yield return closestOrigin;
            yield return closestOrigin + Vector2Int.right;
            yield return closestOrigin + Vector2Int.up;
            yield return closestOrigin + Vector2Int.one;
        }

        #endregion
        #region GetFootprintCenter

        private static Vector2 GetFootprintCenter(
        Tilemap tilemap,
        Vector2Int origin)
        {
            Vector3 a = tilemap.GetCellCenterWorld((Vector3Int)origin);
            Vector3 b = tilemap.GetCellCenterWorld((Vector3Int)(origin + Vector2Int.right));
            Vector3 c = tilemap.GetCellCenterWorld((Vector3Int)(origin + Vector2Int.up));
            Vector3 d = tilemap.GetCellCenterWorld((Vector3Int)(origin + Vector2Int.one));

            return (a + b + c + d) / 4f;
        }

        #endregion
    }
}