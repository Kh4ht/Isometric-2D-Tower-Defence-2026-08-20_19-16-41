using System;
using Assets.Scripts.Utils;
using UnityEngine;

[Serializable]
public class ProjectileStats
{
    #region FIELDS

    [SerializeField] private float moveSpeed;
    [SerializeField] private float parabolicArcHeightMultiplier;
    [SerializeField] private float damage;
    [SerializeField] private Vector2 targetFirstPos;
    [SerializeField] private Vector2 targetLastPosBeforeDeath;
    [SerializeField] private Enums.ProjectileTargetPosition targetPosType;
    [SerializeField] private Enums.ProjectileMoveType moveType;
    [SerializeField] private Enums.ElementType elementType;
    [SerializeField] private Enemy target;
    [SerializeField] private AnimationCurve speedMultiplierCurve;

    // EVENTS
    public event Action<Vector2> OnTargetFirstPosChanged;
    public event Action<Enemy> OnTargetChanged;

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

    public Enums.ElementType GetElementType() => elementType;
    public void SetElementType(Enums.ElementType newValue)
    {
        if (newValue == elementType)
            return;

        elementType = newValue;
    }

    public float GetParabolicArcHeightMultiplier() => parabolicArcHeightMultiplier;
    public void SetParabolicArcHeightMultiplier(float newValue)
    {
        if (newValue == parabolicArcHeightMultiplier)
            return;

        parabolicArcHeightMultiplier = newValue;
    }

    public Enums.ProjectileMoveType GetMoveType() => moveType;
    public void SetMoveType(Enums.ProjectileMoveType newValue)
    {
        if (newValue == moveType)
            return;

        moveType = newValue;
    }

    public Vector2 GetTargetPos()
    {
        return targetPosType switch
        {
            Enums.ProjectileTargetPosition.FirstTargetPos => targetFirstPos,
            Enums.ProjectileTargetPosition.FollowTargetPos => targetLastPosBeforeDeath,

            _ => Vector2.zero,
        };
    }

    public AnimationCurve GetSpeedMultiplierCurve() => speedMultiplierCurve.CopyCurve();
    public void SetSpeedMultiplierCurve(AnimationCurve newValue)
    {
        if (newValue == speedMultiplierCurve)
            return;

        speedMultiplierCurve = newValue.CopyCurve();
    }


    // PUBLIC API
    public void Reset(TowerStats towerStats, Enemy target)
    {
        this.target = target;

        if (target != null)
            targetFirstPos = target.transform.position;

        if (towerStats != null)
        {
            moveSpeed = towerStats.GetProjectileMoveSpeed();
            damage = towerStats.GetProjectileDamage();
            elementType = towerStats.GetElementType();
            targetPosType = towerStats.GetProjectileTargetPosType();
            moveType = towerStats.GetProjectileMoveType();
            speedMultiplierCurve = towerStats.GetProjectileSpeedMultiplierCurve();
            parabolicArcHeightMultiplier = towerStats.GetProjectileParabolicArcHeightMultiplier();
        }
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