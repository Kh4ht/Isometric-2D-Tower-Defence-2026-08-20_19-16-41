using System;
using System.Collections.Generic;
using KH;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Enemy : KHManagedBehaviour, IKHPoolable, IKHManagedUpdate, IKHManagedFixedUpdate
{
    #region FIELDS

    public EnemyStats stats;

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubsystems = new();
    private EnemyMovement enemyMovement;

    // COMPONENTS

    public Rigidbody2D rb2d { get; private set; }
    public CapsuleCollider2D coll2d { get; private set; }

    // INSPECTOR

    [Header("DATA")]
    [SerializeField] private EnemyData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;

        coll2d = GetComponent<CapsuleCollider2D>();
        coll2d.isTrigger = true;

        //Set Tag
        tag = GameTags.ENEMY;
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

    public void KHManagedUpdate()
    {
        kHSubsystems.UpdateAll();
    }

    public void KHManagedFixedUpdate()
    {
        kHSubsystems.FixedUpdateAll();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameTags.VILLAGER))
        {
            KidnapVillagerAndEndMission(collision.GetComponent<Villager>());
        }
    }

    #endregion
    #region PRIVATE

    private void KidnapVillagerAndEndMission(Villager villager)
    {
        VillageManager.Ins.KidnapVillager(villager);

        KHPoolManager.Ins.Despawn(data.ID, this);
    }

    #endregion
    #region PUBLIC

    public void ResetEnemy(EnemyData enemyData, List<Vector2> path)
    {
        stats.Reset(enemyData, path);
    }

    public void ReachedVillagerArea()
    {
        stats.canFollowPath = false;
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

    // Start from 1 because enemy spawns on path[pathIndex = 0]
    public int pathIndex = 1;

    public EnemyStats(EnemyData enemyData)
    {
        moveSpeed = enemyData.defaultMoveSpeed;
    }

    public void Reset(EnemyData enemyData, List<Vector2> newPath)
    {
        canFollowPath = true;
        moveSpeed = enemyData.defaultMoveSpeed;
        path = newPath;
        pathIndex = 1;
    }
}

#endregion