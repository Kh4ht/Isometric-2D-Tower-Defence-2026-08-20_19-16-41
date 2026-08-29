using System;
using System.Collections.Generic;
using KH;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : KHManagedBehaviour, IKHPoolable, IKHManagedUpdate, IKHManagedFixedUpdate
{
    #region FIELDS

    public EnemyStats stats;

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubsystems = new();
    private EnemyMovement enemyMovement;

    // COMPONENTS

    public Rigidbody2D rb2d { get; private set; }

    // INSPECTOR

    [Header("DATA")]
    [SerializeField] private EnemyData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.gravityScale = 0;
    }

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        stats = new(data);

        kHSubsystems.AddRange(new IKHSubsystem[]
        {
            enemyMovement = new(this),
        });
    }

    public void ManagedUpdate()
    {
        kHSubsystems.UpdateAll();
    }

    public void ManagedFixedUpdate()
    {
        kHSubsystems.FixedUpdateAll();
    }

    #endregion
    #region PRIVATE

    private void StopFollowingPath()
    {
        // Stop A* movement
    }

    private void StartChasingVillager(Villager villager)
    {
        // Start movement toward villager
    }


    #endregion
    #region PUBLIC

    public void ResetStats(EnemyData enemyData, List<Vector2> path)
    {
        stats = new(enemyData)
        {
            path = path
        };
    }

    public void ReachedVillagerArea()
    {
        if (!stats.canFollowPath)
            return;

        stats.canFollowPath = false;

        StopFollowingPath();

        // Villager nearestVillager = VillagerManager.Ins.GetNearestVillager(transform.position);

        // if (nearestVillager == null)
        //     return;

        // StartChasingVillager(nearestVillager);
    }

    public void OnSpawn()
    {

    }

    public void OnDespawn()
    {

    }

    #endregion
}

#region EnemyStats

[Serializable]
public class EnemyStats
{
    public bool canFollowPath = true;
    public float moveSpeed;
    public Vector2 moveDir;
    public List<Vector2> path = new();

    public EnemyStats(EnemyData enemyData)
    {
        moveSpeed = enemyData.defaultMoveSpeed;
    }
}

#endregion