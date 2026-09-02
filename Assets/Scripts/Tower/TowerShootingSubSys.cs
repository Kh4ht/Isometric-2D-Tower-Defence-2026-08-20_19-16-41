using KH;
using MyHelper;
using UnityEngine;

public class TowerShootingSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Tower owner;

    private readonly KHTimer shootCooldownTimer = new();

    #endregion
    #region CONSTRUCTOR

    public TowerShootingSubSys(Tower owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IUpdate()
    {
        Shoot();
    }

    #endregion
    #region PRIVATE

    private void Shoot()
    {
        if (!owner.data.haveShootingSubSys)
            return;

        shootCooldownTimer.Run();

        if (shootCooldownTimer.DidExceed(owner.stats.shootCooldown))
        {
            shootCooldownTimer.Reset();
            SpawnBullet();
        }
    }

    private Enemy GetTarget()
    {
        if (!owner.data.haveShootingSubSys)
            return null;

        switch (owner.stats.targetSearchType)
        {
            case TargetSearchType.First:
                return owner.towerRangeSubSys.GetEnemiesInRange().GetFirstEnemy();

            case TargetSearchType.Last:
                return owner.towerRangeSubSys.GetEnemiesInRange().GetLastEnemy();

            case TargetSearchType.Strongest:
                return owner.towerRangeSubSys.GetEnemiesInRange().GetStrongestEnemy();

            case TargetSearchType.Weakest:
                return owner.towerRangeSubSys.GetEnemiesInRange().GetWeakestEnemy();

            default:
                Debug.LogWarning($"Unsupported {nameof(TargetSearchType)}: {owner.stats.targetSearchType}.");
                return null;
        }
    }

    private void SpawnBullet()
    {
        if (!owner.data.haveShootingSubSys)
            return;

        Vector2 spawnPos = (Vector2)owner.transform.position + owner.data.bulletSpawnOffset;

        KHPoolManager.Ins.Spawn<Bullet>(owner.data.bulletData.ID,
                                        spawnPos).ResetBullet(owner.data.bulletData,
                                                              GetTarget());
    }

    #endregion
    #region PUBLIC

    public void Reset()
    {
        if (!owner.data.haveShootingSubSys)
            return;

        shootCooldownTimer.Reset();
    }

    #endregion
}