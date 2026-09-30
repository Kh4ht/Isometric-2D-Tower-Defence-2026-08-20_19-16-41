using System.Collections.Generic;
using KH;
using Assets.Scripts.Utils;
using UnityEngine;

public class ProjectileCollisionSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Projectile owner;

    private bool targetIsDead = false;

    #endregion
    #region CONSTRUCTOR

    public ProjectileCollisionSubSys(Projectile owner)
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
        switch (owner.stats.GetMoveType())
        {
            case BulletMoveType.StraightOrParabolic:
                CollideWhenReachingTargetPos();
                break;

            case BulletMoveType.Laser:
                LaserBulletColl();
                break;
        }
    }

    public void IOnTriggerEnter2D(Collider2D collision)
    {
        switch (owner.stats.GetMoveType())
        {
            case BulletMoveType.StraightOrParabolic:
                StraightBulletColl(collision);
                break;

            case BulletMoveType.Laser:
                LaserBulletColl();
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

        DamageEnemy(owner.stats.GetTarget());
    }

    private void LaserBulletColl() { }

    private void CollideWhenReachingTargetPos()
    {
        if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.stats.GetTargetPos(), GameConsts.COMPARISON_DIS_1))
        {
            IEnumerable<Enemy> enemiesInRange = Helper.GetAllAliveEnemiesInRange(owner.stats.GetTargetPos(), GameConsts.COMPARISON_DIS_1 + 1f);

            foreach (Enemy enemy in enemiesInRange)
            {
                if (owner.Coll2d.IsTouching(enemy.Coll2d))
                {
                    DamageEnemy(enemy);

                    KHPoolManager.Ins.Despawn(owner.ID, owner);
                    return;
                }
            }

            KHPoolManager.Ins.Despawn(owner.ID, owner);
        }
    }

    private void DamageEnemy(Enemy enemy)
    {
        enemy.stats.TakeDamage(owner.stats.GetDamage(), owner.stats.GetElementType());

        // Destroy the bullet after damaging the enemy
        KHPoolManager.Ins.Despawn(owner.ID, owner);
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
