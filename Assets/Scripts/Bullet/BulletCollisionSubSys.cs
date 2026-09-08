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
        CheckTargetReached();
    }

    #endregion
    #region PRIVATE

    private void CheckTargetReached()
    {
        if (!targetIsDead && (owner.stats.target == null || owner.stats.target.HealthController.IsDead))
            targetIsDead = true;

        if (targetIsDead)
        {
            if (Kh.SqrDistanceIsLessThan(owner.transform.position, owner.stats.targetLastPosBeforeDeath, GameConsts.COMPARISON_DIS_1))
            {
                OnBulletCollided(owner.stats.target);
            }
        }
        else
        {
            if (Kh.SqrDistanceIsLessThan(owner, owner.stats.target, GameConsts.COMPARISON_DIS_2))
            {
                OnBulletCollided(owner.stats.target);
            }
        }
    }

    private void OnBulletCollided(Enemy enemy)
    {
        if (enemy != null)
        {
            // Apply damage to the enemy
            enemy.HealthController.Health -= owner.stats.damage;
        }

        // Destroy the bullet after hitting the enemy
        KHPoolManager.Ins.Despawn(owner.data.ID, owner);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        targetIsDead = false;
    }

    #endregion
}