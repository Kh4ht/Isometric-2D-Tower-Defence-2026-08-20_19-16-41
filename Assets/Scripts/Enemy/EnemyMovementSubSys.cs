using KH;
using UnityEngine;

public class EnemyMovementSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

    #endregion
    #region CONSTRUCTOR

    public EnemyMovementSubSys(Enemy owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IFixedUpdate()
    {
        if (!owner.stats.canWalk || owner.HealthController.IsDead)
            return;

        if (!owner.stats.reachedVillageArea)
            FollowPath();
        else
            FollowNearestVillager();
    }

    #endregion
    #region PRIVATE

    private void FollowNearestVillager()
    {
        Villager villager = VillageManager.Ins.GetNearestVillager(owner.transform.position);

        if (villager == null)
        {
            Debug.Log("No Villagers Found");
            return;
        }

        // Update Move Direction.
        owner.stats.moveDir = Kh.GetDir(owner.transform.position, villager.transform.position);

        // Add Velocity.
        Move(villager.transform.position);
    }

    private void FollowPath()
    {
        if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.stats.NextPathPointPos, 0.1f))
        {
            if (owner.stats.nextPathPointIndex + 1 < owner.stats.path.Count)
                owner.stats.nextPathPointIndex++;
        }

        // Update Move Direction.
        owner.stats.moveDir = Kh.GetDir(owner.transform.position, owner.stats.path[owner.stats.nextPathPointIndex]);

        // Add Velocity.
        Move(owner.stats.path[owner.stats.nextPathPointIndex]);
    }

    private void Move(Vector2 targetPos)
    {
        owner.KHMoveTowards(targetPos, owner.stats.moveSpeed * Time.fixedDeltaTime);
    }

    #endregion
}