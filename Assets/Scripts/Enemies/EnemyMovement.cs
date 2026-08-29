using KH;
using UnityEngine;

public class EnemyMovement : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

    // Start from 1 because enemy spawns on path[pathIndex = 0]
    private int pathIndex = 1;

    #endregion
    #region CONSTRUCTOR

    public EnemyMovement(Enemy owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IUpdate()
    {

    }

    public void IFixedUpdate()
    {
        FollowPath();
    }

    #endregion
    #region PRIVATE

    private void UpdateMoveDir()
    {
        owner.stats.moveDir = Kh.GetDir(owner.transform.position, owner.stats.path[pathIndex]);
    }

    public void FollowPath()
    {
        if (!owner.stats.canFollowPath)
            return;

        Debug.Log($"pathIndex: {pathIndex}");

        Debug.Log($"path.Count: {owner.stats.path.Count}");

        if (Kh.SqrDistanceIsLessThan(owner.stats.path[pathIndex], owner.transform.position, 0.1f))
        {
            if (pathIndex + 1 < owner.stats.path.Count)
                pathIndex++;
        }

        UpdateMoveDir();

        owner.rb2d.linearVelocity = owner.stats.moveSpeed
                                    * Time.fixedDeltaTime
                                    * owner.stats.moveDir;
    }

    #endregion
}