using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(CapsuleCollider2D), typeof(Animator), typeof(Rigidbody2D))]
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
    public Rigidbody2D Rb2D { get; private set; }
    public Animator animator { get; private set; }

    // GETTERS
    public Vector2 NextPathPointPos => stats.GetPath()[stats.GetNextPathPoint()];
    /// <summary>
    /// How many path points are still ahead of this enemy (lower = closer to the goal).
    /// Every route ends at the same target and every step costs the same, so this is comparable
    /// between enemies even when they are on different (or per-enemy) paths.
    /// </summary>
    public int RemainingPathPoints => stats.GetPath().Count - stats.GetNextPathPoint();

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

        Rb2D = GetComponent<Rigidbody2D>();
        Rb2D.bodyType = RigidbodyType2D.Kinematic;

        //Set Tag
        tag = GameTags.ENEMY;
    }

    private void Awake()
    {
        Coll2d = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        Rb2D = GetComponent<Rigidbody2D>();

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
        stats.GetHealthController().RemoveHealth(damageAmount);
    }
#endif

    private void KidnapVillagerAndEndMission(Villager villager)
    {
        VillageManager.Ins.KidnapVillager(villager);

        KHPoolManager.Ins.Despawn(data.ID, this);
    }

    #endregion
    #region PUBLIC

    public void ResetEnemy(List<Vector2> path)
    {
        stats.Reset(data, path);

        kHSubSystems.ResetAll();
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    #endregion
}