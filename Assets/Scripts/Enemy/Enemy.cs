using System;
using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(CapsuleCollider2D), typeof(Animator))]
public class Enemy : KHManagedBehaviour, IKHPoolable, IKHManagedUpdate, IKHManagedFixedUpdate
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    private EnemyMovementSubSys enemyMovementSubSys;
    private EnemyHealthSubSys enemyHealthSubSys;
    private EnemyAnimatorSubSys enemyAnimatorSubSys;

    // COMPONENTS
    public CapsuleCollider2D Coll2d { get; private set; }
    public Animator animator { get; private set; }

    // INSPECTOR

    [Tab("STATS")]
    public EnemyStats stats;

    [Tab("DATA")]

    public DoubleSliders healthSlider;
    public EnemyData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        Coll2d = GetComponent<CapsuleCollider2D>();
        Coll2d.isTrigger = true;

        //Set Tag
        tag = GameTags.ENEMY;
    }

    private void Awake()
    {
        Coll2d = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();

        stats = new(data);

        kHSubSystems.AddRange(new IKHSubsystem[]
        {
            enemyMovementSubSys = new(this),
            enemyHealthSubSys = new(this),
            enemyAnimatorSubSys = new(this)
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
        kHSubSystems.UpdateAll();
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

#if UNITY_EDITOR
    private bool NotPlayMode => !Application.isPlaying;
    [DisableIf(nameof(NotPlayMode))]
    [Button(color = "green")]
    private void DamageEnemy(int damageAmount)
    {
        stats.healthController.RemoveHealth(damageAmount);
    }
#endif

    private void KidnapVillagerAndEndMission(Villager villager)
    {
        VillageManager.Ins.KidnapVillager(villager);

        KHPoolManager.Ins.Despawn(data.ID, this);
    }

    #endregion
    #region PUBLIC

    public void ResetEnemy(List<Vector2> path, int selectedPathIndex)
    {
        stats.Reset(data, path, selectedPathIndex);

        kHSubSystems.ResetAll();
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    #endregion
}