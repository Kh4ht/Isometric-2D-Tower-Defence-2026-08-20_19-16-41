using System.Collections.Generic;
using KH;
using Assets.Scripts.Utils;
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
        shootCooldownTimer.Run();

        if (shootCooldownTimer.DidExceed(owner.stats.GetShootCooldown()))
        {
            owner.stats.SetEnemyTargeted(GetTarget());

            if (owner.stats.GetEnemyTargeted() != null)
            {
                shootCooldownTimer.Reset();
                SpawnBullet();
            }
        }
    }

    private Enemy GetTarget()
    {
        List<Enemy> enemiesInRange = new();

        foreach (Enemy enemy in Helper.GetAllAliveEnemies())
        {
            if (enemy.IsWithinRange(owner.transform.position, owner.stats.GetRange()))
                enemiesInRange.Add(enemy);
        }

        switch (owner.stats.GetTargetSearchType())
        {
            case TowerStats.TargetSearchType.First:
                return enemiesInRange.GetFirstEnemy();

            case TowerStats.TargetSearchType.Last:
                return enemiesInRange.GetLastEnemy();

            case TowerStats.TargetSearchType.Strongest:
                return enemiesInRange.GetStrongestEnemy();

            case TowerStats.TargetSearchType.Weakest:
                return enemiesInRange.GetWeakestEnemy();

            default:
                Debug.LogWarning($"Unsupported {nameof(TowerStats.TargetSearchType)}: {owner.stats.GetTargetSearchType()}.");
                return null;
        }
    }

    private void SpawnBullet()
    {
        Vector2 spawnPos = (Vector2)owner.transform.position + owner.data.bulletSpawnOffset;

        KHPoolManager.Ins.Spawn<Bullet>(owner.data.bulletData.ID,
                                        spawnPos).ResetBullet(owner.data.bulletData,
                                                              owner.stats.GetLvl(),
                                                              owner.stats.GetEnemyTargeted());
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        shootCooldownTimer.Reset();
    }

    #endregion
}