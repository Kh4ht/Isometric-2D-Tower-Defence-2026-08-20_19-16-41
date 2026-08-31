using KH;
using UnityEngine;

public class EnemyMovement : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

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
        if (owner.stats.canFollowPath)
            FollowPath();
        else
            FollowVillager();
    }

    #endregion
    #region PRIVATE

    public void FollowVillager()
    {
        Villager villager = VillageManager.Ins.GetNearestVillager(owner.transform.position);

        if (villager == null)
        {
            Debug.Log("No Villagers Found");
            owner.rb2d.linearVelocity = Vector3.zero;
            return;
        }

        // Update Move Direction.
        owner.stats.moveDir = Kh.GetDir(owner.transform.position,
                                        villager.transform.position);

        // Add Velocity.
        owner.rb2d.linearVelocity = owner.stats.moveSpeed
                                    * Time.fixedDeltaTime
                                    * owner.stats.moveDir;
    }

    public void FollowPath()
    {
        if (Kh.SqrDistanceIsLessThan(owner.stats.path[owner.stats.pathIndex], owner.transform.position, 0.1f))
        {
            if (owner.stats.pathIndex + 1 < owner.stats.path.Count)
                owner.stats.pathIndex++;
        }

        // Update Move Direction.
        owner.stats.moveDir = Kh.GetDir(owner.transform.position, owner.stats.path[owner.stats.pathIndex]);

        // Add Velocity.
        owner.rb2d.linearVelocity = owner.stats.moveSpeed
                                    * Time.fixedDeltaTime
                                    * owner.stats.moveDir;
    }

    #endregion
}