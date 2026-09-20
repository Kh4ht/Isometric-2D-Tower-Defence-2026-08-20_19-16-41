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
        if (!owner.stats.GetCanMove() || owner.stats.GetHealthController().IsDead)
            return;

        if (!owner.stats.GetReachedVillageArea())
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
        owner.stats.SetMoveDir(Kh.GetDir(owner.transform.position, villager.transform.position));

        // Add Velocity.
        Move(villager.transform.position);
    }

    private void FollowPath()
    {
        if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.NextPathPointPos, 0.1f))
        {
            if (owner.stats.GetNextPathPoint() + 1 < owner.stats.GetPath().Count)
                owner.stats.SetNextPathPoint(owner.stats.GetNextPathPoint() + 1);
        }

        // Update Move Direction.
        owner.stats.SetMoveDir(Kh.GetDir(owner.transform.position, owner.stats.GetPath()[owner.stats.GetNextPathPoint()]));

        // Add Velocity.
        Move(owner.stats.GetPath()[owner.stats.GetNextPathPoint()]);
    }

    private void Move(Vector2 targetPos)
    {
        owner.KHMoveTowards(targetPos, owner.stats.GetMoveSpeed() * Time.fixedDeltaTime);
    }

    #endregion
}