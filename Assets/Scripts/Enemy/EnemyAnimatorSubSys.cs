using KH;
using MyHelper;
using UnityEngine;

public class EnemyAnimatorSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

    // int
    private readonly int DIRECTION = Animator.StringToHash("Direction");

    // bool
    private readonly int WALK = Animator.StringToHash("Walk");

    private bool oldCanWalk;
    private int oldDirection;

    #endregion
    #region CONSTRUCTOR

    public EnemyAnimatorSubSys(Enemy owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IStart()
    {
        IReset();
    }

    public void IUpdate()
    {
        if (oldCanWalk != owner.stats.canWalk)
        {
            oldCanWalk = owner.stats.canWalk;
            owner.animator.SetBool(WALK, owner.stats.canWalk);
        }
        if (oldDirection != owner.stats.moveDir.ToAnimatorValue())
        {
            int newDir = owner.stats.moveDir.ToAnimatorValue();
            oldDirection = newDir;
            UpdateDirection(newDir);
        }
    }

    #endregion
    #region PRIVATE

    private void UpdateDirection(int newDir)
    {
        owner.animator.SetInteger(DIRECTION, newDir);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        oldCanWalk = owner.stats.canWalk;
        owner.animator.SetBool(WALK, owner.stats.canWalk);

        int newDir = owner.stats.moveDir.ToAnimatorValue();
        oldDirection = newDir;
        UpdateDirection(newDir);
    }

    #endregion
}