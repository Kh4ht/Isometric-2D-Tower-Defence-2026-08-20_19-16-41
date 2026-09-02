using System.Collections;
using System.Collections.Generic;
using MyHelper;
using UnityEngine;

public class TowerRangeSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Tower owner;

    // Must match however your grid squashes Y relative to X (1 tile step = x:1, y:0.5)
    private const float ISO_Y_SCALE = 0.5f;
    private const int GIZMO_SEGMENTS = 32;

    #endregion
    #region CONSTRUCTOR

    public TowerRangeSubSys(Tower owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IOnDrawGizmosSelected()
    {
        DrawRangeGizmo();
    }

    #endregion
    #region PRIVATE

    private void DrawRangeGizmo()
    {
        float range = owner.stats.range;

        if (range <= 0f)
            return;

        Vector2 origin = owner.transform.position;

        Gizmos.color = Color.cyan;

        Vector2 prevPoint = TileCircleToWorld(origin, range, 0);

        for (int i = 1; i <= GIZMO_SEGMENTS; i++)
        {
            Vector2 nextPoint = TileCircleToWorld(origin, range, i);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    /// Returns the world-space point for step "i" of a circle of radius "range"
    /// (in tile units), squashed back into isometric world space.
    private Vector2 TileCircleToWorld(Vector2 origin, float range, int step)
    {
        float angle = (step / (float)GIZMO_SEGMENTS) * Mathf.PI * 2f;

        // Point on a normal circle, in tile space
        Vector2 tileSpacePoint = new Vector2(
            Mathf.Cos(angle) * range,
            Mathf.Sin(angle) * range
        );

        // Re-apply the isometric squash to get back to world space
        tileSpacePoint.y *= ISO_Y_SCALE;

        return origin + tileSpacePoint;
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        // Nothing to reset for this subsystem
    }

    public IEnumerable<Enemy> GetEnemiesInRange()
    {
        foreach (Enemy enemy in Helper.GetAllEnemies())
            if (IsWithinRange(enemy.transform.position))
                yield return enemy;
    }

    public bool IsWithinRange(Vector2 pos)
    {
        Vector2 origin = owner.transform.position;
        Vector2 delta = pos - origin;

        // Undo the isometric squash so distance is measured in tile units
        delta.y /= ISO_Y_SCALE;

        float distanceInTiles = delta.magnitude;
        return distanceInTiles <= owner.stats.range;
    }

    #endregion
}