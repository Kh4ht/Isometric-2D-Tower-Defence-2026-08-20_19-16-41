using System;
using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Bullet : KHManagedBehaviour, IKHManagedUpdate, IKHManagedFixedUpdate, IKHPoolable
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    public BulletCollisionSubSys bulletCollisionSubSys { get; private set; }
    public BulletMovementSubSys bulletMovementSubSys { get; private set; }

    // INSPECTOR
    [Tab("STATS")]
    public BulletStats stats;

    [Tab("DATA")]
    public BulletData data;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        stats = new(data);

        kHSubSystems.AddRange(new IKHSubsystem[]
        {
            bulletCollisionSubSys = new(this),
            bulletMovementSubSys = new(this),
        });
    }

    public void KHUpdate()
    {
        stats.UpdateEnemyLastPos();

        kHSubSystems.UpdateAll();
    }

    public void KHFixedUpdate()
    {
        kHSubSystems.FixedUpdateAll();
    }

    #endregion
    #region PRIVATE



    #endregion
    #region PUBLIC

    public void ResetBullet(BulletData bulletData, Enemy target)
    {
        stats.Reset(bulletData, target);

        kHSubSystems.ResetAll();
    }

    public void OnDespawn() { }

    public void OnSpawn() { }

    #endregion
}





#region BulletStats

[Serializable]
public class BulletStats
{
    public Vector2 targetFirstPos = Vector2.zero;
    public Vector2 targetLastPosBeforeDeath;

    // Requires Initialization
    public float moveSpeed;
    public Enemy target;
    public float damage;

    // CONSTRUCTOR
    public BulletStats(BulletData data)
    {
        moveSpeed = data.moveSpeed;
        damage = data.damage;
    }

    // METHODS
    public void Reset(BulletData data, Enemy target)
    {
        this.target = target;
        targetFirstPos = target.transform.position;
        moveSpeed = data.moveSpeed;
        damage = data.damage;
    }

    public void UpdateEnemyLastPos()
    {
        if (target != null && !target.HealthController.IsDead)
        {
            targetLastPosBeforeDeath = target.transform.position;
        }
    }
}

#endregion