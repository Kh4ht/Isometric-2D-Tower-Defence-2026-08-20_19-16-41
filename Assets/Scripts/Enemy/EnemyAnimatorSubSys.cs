using Assets.Scripts.Utils;
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

    public void IOnDisable()
    {
        // REMOVE LISTENERS
        owner.stats.GetHealthController().RemoveOnDeathListener(OnDeath);
        owner.stats.OnMoveDirChanged -= OnMoveDirChanged;
        owner.stats.OnCanMoveChanged -= OnCanMoveChanged;
    }

    #endregion
    #region PRIVATE

    private void OnDeath()
    {
        owner.animator.SetBool(WALK, false);
        owner.animator.SetTrigger(DEATH);
    }

    private void OnMoveDirChanged(Vector2 newDir)
    {
        owner.animator.SetInteger(DIRECTION, newDir.ToAnimatorValue());
    }

    private void OnCanMoveChanged(bool canMove)
    {
        owner.animator.SetBool(WALK, canMove);
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        // ADD LISTENERS
        owner.stats.GetHealthController().AddOnDeathListener(OnDeath);
        owner.stats.OnMoveDirChanged += OnMoveDirChanged;
        owner.stats.OnCanMoveChanged += OnCanMoveChanged;

        OnMoveDirChanged(owner.stats.GetMoveDir());
        OnCanMoveChanged(owner.stats.GetCanMove());
    }

    #endregion
}