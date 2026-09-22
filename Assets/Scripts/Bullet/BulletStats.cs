using System;
using UnityEngine;

[Serializable]
public class BulletStats
{
    #region FIELDS

    [SerializeField] private Vector2 targetFirstPos;
    [SerializeField] private Vector2 targetLastPosBeforeDeath;
    [SerializeField] private Vector2 targetPos;
    [SerializeField] private Enemy target;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;

    // EVENTS
    public event Action<Vector2> OnTargetFirstPosChanged;
    public event Action<Enemy> OnTargetChanged;

    #endregion
    #region CONSTRUCTOR

    public BulletStats(BulletData data)
    {
        Reset(data, 0, null);
    }

    #endregion
    #region PUBLIC

    // ENCAPSULATION
    public Vector2 GetTargetFirstPos() => targetFirstPos;
    public void SetTargetFirstPos(Vector2 newValue)
    {
        if (newValue == targetFirstPos)
            return;

        targetFirstPos = newValue;
        OnTargetFirstPosChanged?.Invoke(newValue);
    }

    public Vector2 GetTargetLastPosBeforeDeath() => targetLastPosBeforeDeath;
    public void SetTargetLastPosBeforeDeath(Vector2 newValue)
    {
        if (newValue == targetLastPosBeforeDeath)
            return;

        targetLastPosBeforeDeath = newValue;
    }

    public Enemy GetTarget() => target;
    public void SetTarget(Enemy newValue)
    {
        if (newValue == target)
            return;

        target = newValue;
        OnTargetChanged?.Invoke(newValue);
    }

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float newValue)
    {
        if (newValue == moveSpeed)
            return;

        moveSpeed = newValue;
    }

    public float GetDamage() => damage;
    public void SetDamage(float newValue)
    {
        if (newValue == damage)
            return;

        damage = newValue;
    }

    public Vector2 GetTargetPos() => targetPos;
    public void SetTargetPos(Vector2 newValue)
    {
        if (newValue == targetPos)
            return;

        targetPos = newValue;
    }

    // PUBLIC API
    public void Reset(BulletData data, int mainTowerLvl, Enemy target)
    {
        this.target = target;

        if (target != null)
            targetFirstPos = target.transform.position;

        moveSpeed = data.moveSpeed[mainTowerLvl];
        damage = data.damage[mainTowerLvl];
    }

    public void UpdateEnemyLastPos()
    {
        if (target != null && !target.stats.GetHealthController().IsDead)
        {
            targetLastPosBeforeDeath = target.transform.position;
        }
    }

    #endregion
}