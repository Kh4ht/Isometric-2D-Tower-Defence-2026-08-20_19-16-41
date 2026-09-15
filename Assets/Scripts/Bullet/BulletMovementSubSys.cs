using KH;
using UnityEngine;

public class BulletMovementSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Bullet owner;

    #endregion
    #region CONSTRUCTOR

    public BulletMovementSubSys(Bullet owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IFixedUpdate()
    {
        switch (owner.data.type)
        {
            case BulletMoveType.Straight:
                StraightMove();
                break;

            case BulletMoveType.Parabolic:
                ParabolicMove();
                break;

            case BulletMoveType.Laser:
                LaserMove();
                break;

            case BulletMoveType.Follow:
                FollowMove();
                break;
        }
    }

    #endregion
    #region PRIVATE

    private void StraightMove()
    {
        owner.KHMoveTowards(owner.stats.targetFirstPos, owner.stats.moveSpeed * Time.fixedDeltaTime);
    }

    private void ParabolicMove() { }

    private void LaserMove() { }

    private void FollowMove() { }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        // Nothing to reset.
    }

    #endregion
}