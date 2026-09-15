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

    // trigger 
    private readonly int DEATH = Animator.StringToHash("Death");

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

    public void IOnEnable()
    {

    }

    public void IOnDisable()
    {
        owner.HealthController.RemoveOnDeathListener(OnDeath);
    }

    public void IUpdate()
    {
        if (owner.HealthController.IsDead)
            return;

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

    private void OnDeath()
    {
        owner.animator.SetBool(WALK, false);
        owner.animator.SetTrigger(DEATH);
    }

    private void UpdateDirection(int newDir)
    {
        owner.animator.SetInteger(DIRECTION, newDir);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        owner.HealthController.AddOnDeathListener(OnDeath);

        oldCanWalk = owner.stats.canWalk;
        owner.animator.SetBool(WALK, owner.stats.canWalk);

        int newDir = owner.stats.moveDir.ToAnimatorValue();
        oldDirection = newDir;
        UpdateDirection(newDir);
    }

    #endregion
}