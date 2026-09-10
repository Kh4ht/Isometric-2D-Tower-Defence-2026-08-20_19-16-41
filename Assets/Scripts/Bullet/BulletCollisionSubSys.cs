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
        CheckEnemyDead();

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
            // Apply damage to the enemy
            owner.stats.target.HealthController.Health -= owner.stats.damage;
        }

        // Destroy the bullet after hitting the enemy
        KHPoolManager.Ins.Despawn(owner.data.ID, owner);
    }

    private void CheckEnemyDead()
    {
        if (!targetIsDead && (owner.stats.target == null || owner.stats.target.HealthController.IsDead))
        {
            owner.stats.target = null;
            targetIsDead = true;
        }
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        targetIsDead = false;
    }

    #endregion
}