using System;
using System.Collections.Generic;
using KH;
using Assets.Scripts.Utils;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    #region FIELDS

    [SerializeField] private List<Vector2> path = new();
    [SerializeField] private Vector2 moveDir = Vector2.zero;
    [SerializeField] private bool canMove;
    [SerializeField] private bool reachedVillageArea = false;
    [SerializeField] private ElementStrength elementStrength;
    [SerializeField] private ElementType elementType;
    [SerializeField] private float moveSpeed;
    [SerializeField] private KHHealthController healthController;
    [SerializeField] private int nextPathPoint = 1; // Start from 1 because enemy spawns on path[pathIndex = 0]

    // EVENTS
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<bool> OnCanMoveChanged;
    public event Action<bool> OnReachedVillageAreaChanged;
    public event Action<ElementStrength> OnElementStrengthChanged;
    public event Action<ElementType> OnElementTypeChanged;
    public event Action<List<Vector2>> OnPathChanged;
    public event Action<float> OnMoveSpeedChanged;
    public event Action<int> OnNextPathPointChanged;

    #endregion
    #region CONSTRUCTOR

    public EnemyStats(EnemyData enemyData)
    {
        healthController = new(enemyData.defaultMaxHealth, enemyData.defaultMaxHealth);
        elementType = enemyData.elementType;

        Reset(enemyData, new());
    }

    #endregion
    #region PUBLIC

    // ENCAPSULATION

    public KHHealthController GetHealthController() => healthController;

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

    public List<Vector2> GetPath() => path;
    public void SetPath(List<Vector2> newValue)
    {
        if (healthController.IsDead)
            return;

        path = new(newValue);
        OnPathChanged?.Invoke(newValue);
    }

    public bool GetReachedVillageArea() => reachedVillageArea;
    public void SetReachedVillageArea(bool newValue)
    {
        if (healthController.IsDead)
            return;

        reachedVillageArea = newValue;
        OnReachedVillageAreaChanged?.Invoke(newValue);
    }

    public ElementType GetElementType() => elementType;
    public void SetElementType(ElementType newValue)
    {
        if (healthController.IsDead)
            return;

        elementType = newValue;
        OnElementTypeChanged?.Invoke(newValue);
    }

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float newValue)
    {
        if (healthController.IsDead)
            return;

        moveSpeed = newValue;
        OnMoveSpeedChanged?.Invoke(newValue);
    }

    public int GetNextPathPoint() => nextPathPoint;
    public void SetNextPathPoint(int newValue)
    {
        if (healthController.IsDead)
            return;

        nextPathPoint = newValue;
        OnNextPathPointChanged?.Invoke(newValue);
    }

    // PUBLIC API
    public void TakeDamage(float damageAmount, ElementType attackerElementType)
    {
        healthController.Health -= elementStrength.DamageFilter(attackerElementType, elementType, damageAmount);
    }

    public void Reset(EnemyData enemyData, List<Vector2> newPath)
    {
        reachedVillageArea = false;
        moveSpeed = enemyData.defaultMoveSpeed;
        path = new(newPath);
        nextPathPoint = 1;
        canMove = true;
        elementStrength = enemyData.elementStrength;
        healthController?.Revive();
    }

    #endregion
}