using System.Collections.Generic;
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
            if ((owner.stats.enemyTargeted = GetTarget()) != null)
            {
                shootCooldownTimer.Reset();
                SpawnBullet();
            }
        }
    }

    private Enemy GetTarget()
    {
        if (!owner.data.haveShootingSubSys)
            return null;

        List<Enemy> enemiesInRange = new();

        foreach (Enemy enemy in Helper.GetAllEnemies())
        {
            if (enemy.IsWithinRange(owner.transform.position, owner.stats.range))
                enemiesInRange.Add(enemy);
        }

        switch (owner.stats.targetSearchType)
        {
            case TargetSearchType.First:
                return enemiesInRange.GetFirstEnemy();

            case TargetSearchType.Last:
                return enemiesInRange.GetLastEnemy();

            case TargetSearchType.Strongest:
                return enemiesInRange.GetStrongestEnemy();

            case TargetSearchType.Weakest:
                return enemiesInRange.GetWeakestEnemy();

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
                                                              owner.stats.enemyTargeted);
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