using KH;

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

    public void IUpdate()
    {
        CheckTargetDead();

        switch (owner.data.type)
        {
            case BulletMoveType.Straight:
                StraightBulletColl();
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

    private void StraightBulletColl()
    {
        if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.stats.targetFirstPos, GameConsts.COMPARISON_DIS_1))
        {
            OnBulletCollision();
        }
    }

    private void ParabolicBulletColl() { }

    private void LaserBulletColl() { }

    private void FollowBulletColl() { }

    private void OnBulletCollision()
    {
        if (!targetIsDead)
        {
            DamageEnemy(owner.stats.target);
        }

        // Destroy the bullet after hitting the enemy
        KHPoolManager.Ins.Despawn(owner.data.ID, owner);
    }

    private void CheckTargetDead()
    {
        if (!targetIsDead && (owner.stats.target == null || owner.stats.target.stats.healthController.IsDead))
        {
            owner.stats.target = null;
            targetIsDead = true;
        }
    }

    private void DamageEnemy(Enemy enemy)
    {
        enemy.stats.TakeDamage(owner.stats.damage, owner.data.elementType);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        targetIsDead = false;
    }

    #endregion
}