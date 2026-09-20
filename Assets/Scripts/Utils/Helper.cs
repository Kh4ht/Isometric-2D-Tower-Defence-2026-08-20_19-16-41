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

        public static Vector2Int WorldToCell(this Vector2 vector2)
        {
            return PathSys.Ins.gameGrid.WorldToCell(vector2);
        }

        public static Vector2Int WorldToCell(this Vector3 vector3)
        {
            return PathSys.Ins.gameGrid.WorldToCell(vector3);
        }

        public static GridNode GetNodeMouseIsPointingAt()
        {
            return PathSys.Ins.gameGrid.GetNode(Kh.GetMouseWorldPos());
        }

        public static Vector2 GetCellCenterWorld(this Vector2Int vector2Int)
        {
            return PathSys.Ins.gameGrid.GetCellCenterWorld(vector2Int);
        }

        public static List<Vector2> GetCellsCenterWorld(this List<Vector2Int> vector2Ints)
        {
            return PathSys.Ins.gameGrid.GetCellsCenterWorld(vector2Ints);
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
        #region DAMAGE

        private static float Filter(this ElementStrength receiverElementStrength, float damageAmount)
        {
            return receiverElementStrength switch
            {
                ElementStrength.Low => damageAmount * GameConsts.LOW_ELEMENT_MULTIPLIER,
                ElementStrength.Medium => damageAmount * GameConsts.MEDIUM_ELEMENT_MULTIPLIER,
                ElementStrength.High => damageAmount * GameConsts.HIGH_ELEMENT_MULTIPLIER,
                ElementStrength.Immune => damageAmount * GameConsts.IMMUNE_ELEMENT_MULTIPLIER,
                _ => damageAmount,
            };
        }

        private static readonly Dictionary<ElementType, ElementType> ResistedBy = new()
        {
            { ElementType.Fire, ElementType.Water },   // Water resists Fire
            { ElementType.Water, ElementType.Earth },  // Earth resists Water
            { ElementType.Earth, ElementType.Fire },   // Fire resists Earth
        };

        public static float DamageFilter(this ElementStrength receiverElementStrength, ElementType attackerElementType, ElementType receiverElementType, float damageAmount)
        {
            if (attackerElementType == ElementType.None || receiverElementType == ElementType.None || attackerElementType == receiverElementType)
                return damageAmount;

            if (ResistedBy.TryGetValue(attackerElementType, out var resister) && resister == receiverElementType)
                return receiverElementStrength.Filter(damageAmount);

            // if attacker isn't resisted by receiver, receiver must be the "weak" side
            return damageAmount * GameConsts.ELEMENT_BONUS_MULTIPLIER;
        }

        #endregion
        #region TileCircleToWorld

        /// Returns the world-space point for step "i" of a circle of radius "range"
        /// (in tile units), squashed back into isometric world space.
        public static Vector2 TileCircleToWorld(Vector2 origin, float range, int step)
        {
            float angle = (step / (float)GameConsts.TOWER_RANGE_SEGMENTS) * Mathf.PI * 2f;

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
        /// Gets all currently active enemies that are still alive.
        /// </summary>
        /// <returns>An enumeration of living enemies.</returns>
        public static IEnumerable<Enemy> GetAllAliveEnemies()
        {
            foreach (Enemy enemy in KHPoolManager.Ins.GetAllActive<Enemy>())
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                yield return enemy;
            }
        }

        /// <summary>
        /// Gets all currently active enemies that are still alive and within the specified range of the given center point.
        /// </summary>
        /// <param name="center">The center point to check from.</param>
        /// <param name="range">The maximum allowed distance, from the center point.</param>
        /// <returns>An enumeration of living enemies within range.</returns>
        public static IEnumerable<Enemy> GetAllAliveEnemiesInRange(Vector2 center, float range)
        {
            foreach (Enemy enemy in KHPoolManager.Ins.GetAllActive<Enemy>())
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                if (!Kh.SqrDistanceIsLessThan(enemy.transform.position, center, range))
                    continue;

                yield return enemy;
            }
        }

        /// <summary>
        /// Gets the enemy that is closest to the goal (fewest path points remaining).
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy closest to the goal, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetFirstEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy firstEnemy = null;

            foreach (Enemy enemy in enemies)
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                if (firstEnemy == null || enemy.RemainingPathPoints < firstEnemy.RemainingPathPoints)
                {
                    firstEnemy = enemy;

                    continue;
                }

                if (enemy.RemainingPathPoints == firstEnemy.RemainingPathPoints)
                {
                    if (Kh.GetSqrDistance(enemy.transform.position, enemy.NextPathPointPos)
                        < Kh.GetSqrDistance(firstEnemy.transform.position, firstEnemy.NextPathPointPos))
                    {
                        firstEnemy = enemy;
                    }
                }
            }

            return firstEnemy;
        }

        /// <summary>
        /// Gets the enemy that is farthest from the goal (most path points remaining).
        /// </summary>
        /// <param name="enemies">The enemies to search.</param>
        /// <returns>The enemy farthest from the goal, or <see langword="null"/> if the collection is empty.</returns>
        public static Enemy GetLastEnemy(this IEnumerable<Enemy> enemies)
        {
            Enemy lastEnemy = null;

            foreach (Enemy enemy in enemies)
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                if (lastEnemy == null || enemy.RemainingPathPoints > lastEnemy.RemainingPathPoints)
                {
                    lastEnemy = enemy;

                    continue;
                }

                if (enemy.RemainingPathPoints == lastEnemy.RemainingPathPoints)
                {
                    if (Kh.GetSqrDistance(enemy.transform.position, enemy.NextPathPointPos)
                        > Kh.GetSqrDistance(lastEnemy.transform.position, lastEnemy.NextPathPointPos))
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
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                if (weakestEnemy == null || enemy.stats.GetHealthController().Health < weakestEnemy.stats.GetHealthController().Health)
                    weakestEnemy = enemy;
            }

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
            {
                if (enemy.stats.GetHealthController().IsDead)
                    continue;

                if (strongestEnemy == null || enemy.stats.GetHealthController().Health > strongestEnemy.stats.GetHealthController().Health)
                    strongestEnemy = enemy;
            }

            return strongestEnemy;
        }

        #endregion
    }
}