using System;
using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    #region FIELDS

    [SerializeField] private Vector2 moveDir = Vector2.zero;
    [SerializeField] private bool canMove;
    [SerializeField] private ElementStrength elementStrength;
    public readonly ElementType elementType;
    public KHHealthController healthController;
    public bool reachedVillageArea = false;
    public int nextPathPointIndex = 1; // Start from 1 because enemy spawns on path[pathIndex = 0]

    // Requires Initialization
    public float moveSpeed;
    public List<Vector2> path = new();
    public int pathIndex;

    // GETTERS
    public Vector2 NextPathPointPos => path[nextPathPointIndex];
    public int GlobalNextPathPointIndex => nextPathPointIndex - PathSys.Ins.GetDifferenceFromShortestPath(pathIndex);

    // EVENTS
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<bool> OnCanMoveChanged;
    public event Action<ElementStrength> OnElementStrengthChanged;

    #endregion
    #region CONSTRUCTOR

    public EnemyStats(EnemyData enemyData)
    {
        Reset(enemyData, new(), 0);
        healthController = new(enemyData.defaultMaxHealth, enemyData.defaultMaxHealth);
        elementType = enemyData.elementType;
    }

    #endregion
    #region PUBLIC

    public Vector2 GetMoveDir() => moveDir;
    public void SetMoveDir(Vector2 newValue)
    {
        if (healthController.IsDead)
            return;

        newValue = newValue.KHRoundToDecimalPlaces(4);

        if (newValue == moveDir)
            return;

        moveDir = newValue;
        OnMoveDirChanged?.Invoke(newValue);
    }

    public bool GetCanMove() => canMove;
    public void SetCanMove(bool newValue)
    {
        if (healthController.IsDead)
            return;

        if (newValue == canMove)
            return;

        canMove = newValue;
        OnCanMoveChanged?.Invoke(newValue);
    }

    public ElementStrength GetElementStrength() => elementStrength;
    public void SetElementStrength(ElementStrength newValue)
    {
        if (healthController.IsDead)
            return;

        if (newValue == elementStrength)
            return;

        elementStrength = newValue;
        OnElementStrengthChanged?.Invoke(newValue);
    }

    public void TakeDamage(float damageAmount, ElementType attackerElementType)
    {
        healthController.Health -= elementStrength.DamageFilter(attackerElementType, elementType, damageAmount);
    }

    public void Reset(EnemyData enemyData, List<Vector2> newPath, int selectedPathIndex)
    {
        reachedVillageArea = false;
        moveSpeed = enemyData.defaultMoveSpeed;
        path = new(newPath);
        pathIndex = selectedPathIndex;
        nextPathPointIndex = 1;
        canMove = true;
        elementStrength = enemyData.elementStrength;
        healthController?.Revive();
    }

    public void ReachedVillagerArea()
    {
        if (healthController.IsDead)
            return;

        reachedVillageArea = true;
    }

    #endregion
}
