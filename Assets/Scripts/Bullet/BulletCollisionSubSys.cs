using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;

public class BulletCollisionSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Bullet owner;

    private bool targetIsDead = false;

    #endregion
    #region CONSTRUCTOR

    public BulletCollisionSubSys(Bullet owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IOnDisable()
    {
        if (owner.stats.GetTarget() != null)
            owner.stats.GetTarget().stats.GetHealthController().RemoveOnDeathListener(OnTargetDeath);
    }

    public void IUpdate()
    {
        switch (owner.data.type)
        {
            case BulletMoveType.Straight:
                OnTargetDeadCollision();
                break;

            case BulletMoveType.Parabolic:
                ParabolicBulletColl();
                break;

            case BulletMoveType.Laser:
                LaserBulletColl();
                break;

            case BulletMoveType.Follow:
                FollowBulletColl();
                break;
        }
    }

    public void IOnTriggerEnter2D(Collider2D collision)
    {
        switch (owner.data.type)
        {
            case BulletMoveType.Straight:
                StraightBulletColl(collision);
                break;

            case BulletMoveType.Parabolic:
                ParabolicBulletColl();
                break;

            case BulletMoveType.Laser:
                LaserBulletColl();
                break;

            case BulletMoveType.Follow:
                FollowBulletColl();
                break;
        }
    }

    #endregion
    #region PRIVATE

    private void OnTargetDeath()
    {
        owner.stats.GetTarget().stats.GetHealthController().RemoveOnDeathListener(OnTargetDeath);

        owner.stats.SetTarget(null);
        targetIsDead = true;
    }

    private void StraightBulletColl(Collider2D collision)
    {
        if (targetIsDead)
            return;

        if (collision.TryGetComponent(out Enemy enemy) && enemy != owner.stats.GetTarget())
            return;

        OnBulletCollisionWithTarget();
    }

    private void ParabolicBulletColl() { }

    private void LaserBulletColl() { }

    private void FollowBulletColl() { }

    private void OnBulletCollisionWithTarget()
    {
        if (!targetIsDead)
        {
            DamageEnemy(owner.stats.GetTarget());
        }

        // Destroy the bullet after hitting the enemy
        KHPoolManager.Ins.Despawn(owner.data.ID, owner);
    }

    private void OnTargetDeadCollision()
    {
        if (!targetIsDead)
            return;

        if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.stats.GetTargetPos(), GameConsts.COMPARISON_DIS_1))
        {
            IEnumerable<Enemy> enemiesInRange = Helper.GetAllAliveEnemiesInRange(owner.stats.GetTargetPos(), GameConsts.COMPARISON_DIS_1 + 0.75f);

            foreach (Enemy enemy in enemiesInRange)
            {
                if (owner.Coll2d.IsTouching(enemy.Coll2d))
                {
                    DamageEnemy(enemy);

                    KHPoolManager.Ins.Despawn(owner.data.ID, owner);
                    return;
                }
            }

            KHPoolManager.Ins.Despawn(owner.data.ID, owner);
        }
    }

    private void DamageEnemy(Enemy enemy)
    {
        enemy.stats.TakeDamage(owner.stats.GetDamage(), owner.data.elementType);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        targetIsDead = false;
        owner.stats.GetTarget().stats.GetHealthController().AddOnDeathListener(OnTargetDeath);
    }

    #endregion
}