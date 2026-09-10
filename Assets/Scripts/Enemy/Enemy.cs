using System;
using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Enemy : KHManagedBehaviour, IKHPoolable, IKHManagedUpdate, IKHManagedFixedUpdate
{
    #region FIELDS


    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    private EnemyMovementSubSys enemyMovementSubSys;
    private EnemyHealthSubSys enemyHealthSubSys;

    // COMPONENTS
    public KHHealthController HealthController { get; private set; }
    public Rigidbody2D Rb2d { get; private set; }
    public CapsuleCollider2D Coll2d { get; private set; }

    // INSPECTOR

    [Tab("OTHER")]

    public DoubleSliders healthSlider;

    [Header("DATA")]
    public EnemyData data;

    [Tab("STATS")]
    public EnemyStats stats;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        Rb2d = GetComponent<Rigidbody2D>();
        Rb2d.bodyType = RigidbodyType2D.Kinematic;

        Coll2d = GetComponent<CapsuleCollider2D>();
        Coll2d.isTrigger = true;

        //Set Tag
        tag = GameTags.ENEMY;
    }

    private void Awake()
    {
        Rb2d = GetComponent<Rigidbody2D>();

        HealthController = new(this, data.defaultMaxHealth, data.defaultMaxHealth);

        stats = new(data);

        kHSubSystems.AddRange(new IKHSubsystem[]
        {
            enemyMovementSubSys = new(this),
            enemyHealthSubSys = new(this)
        });
    }

    protected override void Start()
    {
        base.Start();

        healthSlider.IncreaseWidthBasedOnHealth(data.defaultMaxHealth);
        healthSlider.gameObject.SetActive(false);
    }

    public void KHUpdate()
    {
        // kHSubsystems.UpdateAll();
    }

    public void KHFixedUpdate()
    {
        kHSubSystems.FixedUpdateAll();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        kHSubSystems.OnEnableAll();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        kHSubSystems.OnDisableAll();
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

# if UNITY_EDITOR
    [Button(color = "green")]
    private void DamageEnemy(int damageAmount)
    {
        if (!Application.isPlaying)
            return;

        HealthController.RemoveHealth(damageAmount);
    }
#endif

    private void KidnapVillagerAndEndMission(Villager villager)
    {
        VillageManager.Ins.KidnapVillager(villager);

        KHPoolManager.Ins.Despawn(data.ID, this);
    }

    #endregion
    #region PUBLIC

    public void ResetEnemy(EnemyData enemyData, List<Vector2> path, int selectedPathIndex)
    {
        stats.Reset(enemyData, path, selectedPathIndex);

        HealthController?.Revive();

        kHSubSystems.ResetAll();
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    #endregion
}

#region EnemyStats

[Serializable]
public class EnemyStats
{
    public bool reachedVillageArea = false;
    public Vector2 moveDir = Vector2.zero;
    public int nextPathPointIndex = 1; // Start from 1 because enemy spawns on path[pathIndex = 0]
    public int GlobalNextPathPointIndex => nextPathPointIndex - PathSys.Ins.GetDifferenceFromShortestPath(pathIndex);

    // Requires Initialization
    public float moveSpeed;
    public List<Vector2> path = new();
    public int pathIndex;

    // CONSTRUCTOR
    public EnemyStats(EnemyData enemyData)
    {
        moveSpeed = enemyData.defaultMoveSpeed;
    }

    // METHODS
    public void ReachedVillagerArea()
    {
        reachedVillageArea = true;
    }

    public void Reset(EnemyData enemyData, List<Vector2> newPath, int selectedPathIndex)
    {
        reachedVillageArea = false;
        moveSpeed = enemyData.defaultMoveSpeed;
        path = new(newPath);
        this.pathIndex = selectedPathIndex;
        nextPathPointIndex = 1;
    }
}

#endregion