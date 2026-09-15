using System.Collections.Generic;
using KH;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyHelper
{
    public static class Helper
    {
        #region FIELDS

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
                Mathf.Abs(a.GetPos.x - b.GetPos.x);

            int yDistance =
                Mathf.Abs(a.GetPos.y - b.GetPos.y);

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
                path.Add(currentNode.GetPos);

                currentNode = currentNode.Parent;
            }

            path.Add(startNode.GetPos);

            path.Reverse();

            return path;
        }

        #endregion
        #region GRID

        public static GridNode GetNodeMouseIsPointingAt()
        {
            return PathSys.Ins.gameGrid.GetNode(Kh.GetMouseWorldPos());
        }

        public static IEnumerable<Vector2Int> GetHoveredCells()
        {
            Vector2Int currentCell = PathSys.Ins.gameGrid.WorldToCell(Kh.GetMouseWorldPos());

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
                Vector2 center = GetFootprintCenter(PathSys.Ins.gameGrid.walkableTilemap, origin);

                float distance = (mouseWorldPos - center).sqrMagnitude;

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
        #region ANIMATION

        public static int ToAnimatorValue(this Vector2 dir)
        {
            if (dir.x >= 0 && dir.y >= 0)
            {
                return 1;
            }
            else if (dir.x >= 0 && dir.y < 0)
            {
                return 2;
            }
            else if (dir.x < 0 && dir.y <= 0)
            {
                return 3;
            }
            else
            {
                return 4;
            }
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
        #region TileCircleToWorld

        /// Returns the world-space point for step "i" of a circle of radius "range"
        /// (in tile units), squashed back into isometric world space.
        public static Vector2 TileCircleToWorld(Vector2 origin, float range, int step)
        {
            float angle = (step / (float)GameConsts.GIZMO_SEGMENTS) * Mathf.PI * 2f;

            // Point on a normal circle, in tile space
            Vector2 tileSpacePoint = new Vector2(
                Mathf.Cos(angle) * range,
                Mathf.Sin(angle) * range
            );

            // Re-apply the isometric squash to get back to world space
            tileSpacePoint.y *= GameConsts.ISO_Y_SCALE;

            return origin + tileSpacePoint;
        }

        #endregion
        #region IsWithinRange

        public static bool IsWithinRange(this MonoBehaviour target, Vector2 origin, float range)
        {
            Vector2 delta = (Vector2)target.transform.position - origin;

            // Undo the isometric squash so distance is measured in tile units
            delta.y /= GameConsts.ISO_Y_SCALE;

            float distanceInTiles = delta.magnitude;
            return distanceInTiles <= range;
        }

        #endregion
        #region ENEMY QUERIES

        /// <summary>
        /// Gets all currently active enemies from the pool manager.
        /// </summary>
        public static IEnumerable<Enemy> GetAllAliveEnemies()
        {
            return KHPoolManager.Ins.GetAllActive<Enemy>();
        }

        /// <summary>
        /// Gets the enemy that has progressed the farthest along the path.
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy with the highest path index, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetFirstEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy firstEnemy = null;

            foreach (Enemy enemy in enemies)
            {
                if (firstEnemy == null || enemy.stats.GlobalNextPathPointIndex > firstEnemy.stats.GlobalNextPathPointIndex)
                {
                    firstEnemy = enemy;

                    continue;
                }

                if (enemy.stats.GlobalNextPathPointIndex == firstEnemy.stats.GlobalNextPathPointIndex)
                {
                    if (Kh.GetSqrDistance(enemy.transform.position, enemy.stats.NextPathPointPos)
                        < Kh.GetSqrDistance(firstEnemy.transform.position, firstEnemy.stats.NextPathPointPos))
                    {
                        firstEnemy = enemy;
                    }
                }
            }

            return firstEnemy;
        }

        /// <summary>
        /// Gets the enemy that has progressed the least along the path.
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy with the lowest path index, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetLastEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy lastEnemy = null;

            foreach (Enemy enemy in enemies)
            {
                if (lastEnemy == null || enemy.stats.GlobalNextPathPointIndex < lastEnemy.stats.GlobalNextPathPointIndex)
                {
                    lastEnemy = enemy;

                    continue;
                }

                if (enemy.stats.GlobalNextPathPointIndex == lastEnemy.stats.GlobalNextPathPointIndex)
                {
                    if (Kh.GetSqrDistance(enemy.transform.position, enemy.stats.NextPathPointPos)
                        > Kh.GetSqrDistance(lastEnemy.transform.position, lastEnemy.stats.NextPathPointPos))
                    {
                        lastEnemy = enemy;
                    }
                }
            }

            return lastEnemy;
        }

        /// <summary>
        /// Gets the enemy with the lowest current health.
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy with the lowest health, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetWeakestEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy weakestEnemy = null;

            foreach (Enemy enemy in enemies)
                if (weakestEnemy == null || enemy.HealthController.Health < weakestEnemy.HealthController.Health)
                    weakestEnemy = enemy;

            return weakestEnemy;
        }

        /// <summary>
        /// Gets the enemy with the highest current health.
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy with the highest health, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetStrongestEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy strongestEnemy = null;

            foreach (Enemy enemy in enemies)
                if (strongestEnemy == null || enemy.HealthController.Health > strongestEnemy.HealthController.Health)
                    strongestEnemy = enemy;

            return strongestEnemy;
        }

        #endregion
    }
}