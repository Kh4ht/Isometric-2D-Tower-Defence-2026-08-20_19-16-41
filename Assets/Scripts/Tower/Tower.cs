using System;
using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(AudioSource), typeof(SpriteRenderer))]
public class Tower : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    public const int TOWER_MAX_LEVEL = 5;

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubsystems = new();
    public TowerShootingSubSys towerShootingSubSys { get; private set; }

    // COMPONENTS
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    // GETTERS
    public bool IsMaxLevel => stats.lvl >= TOWER_MAX_LEVEL;

    // INSPECTOR
    [Tab("STATS")]
    public TowerStats stats;

    [ShowInInspector]

    [Tab("DATA")]
    public TowerData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        stats = new(data);

        kHSubsystems.Clear();
        kHSubsystems.AddRange(new IKHSubsystem[]
        {
            towerShootingSubSys = new(this),
        });
    }

    protected override void Start()
    {
        base.Start();

        RegisterBulletsToPool();
    }

    public void KHUpdate()
    {
        kHSubsystems.UpdateAll();
    }

    private void OnDrawGizmosSelected()
    {
        PointAtEnemy();

        DrawTowerRange();

        DrawBulletSpawnPoint();

        void DrawBulletSpawnPoint()
        {
            if (data == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere((Vector2)transform.position + data.bulletSpawnOffset, 0.05f);
        }

        void PointAtEnemy()
        {
            if (data == null || !data.haveShootingSubSys)
                return;

            if (stats.enemyTargeted != null)
            {
                Gizmos.color = Color.red;
                // draw a line at the target
                Gizmos.DrawLine(transform.position, stats.enemyTargeted.transform.position);
            }
        }

        void DrawTowerRange()
        {
            float range = stats.range;

            if (range <= 0f)
                return;

            Vector2 origin = transform.position;

            Gizmos.color = Color.cyan;

            Vector2 prevPoint = Helper.TileCircleToWorld(origin, range, 0);

            for (int i = 1; i <= GameConsts.GIZMO_SEGMENTS; i++)
            {
                Vector2 nextPoint = Helper.TileCircleToWorld(origin, range, i);
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }

    #endregion
    #region PRIVATE

    private void RegisterBulletsToPool()
    {
        KHPoolManager.Ins.Register(data.bulletData.ID, data.bulletData.prefab);
    }

    #endregion
    #region PUBLIC

    public Tower ResetTower(List<Vector2Int> occupiedCells)
    {
        stats.Reset(data, occupiedCells);

        PathSys.Ins.BlockCells(cells: occupiedCells,
                               block: true,
                               newTower: this);

        spriteRenderer.sprite = data.icons[0];

        kHSubsystems.ResetAll();

        return this;
    }

    /// <summary>Increases tower level by 1</summary>
    public void UpgradeTower()
    {
        if (IsMaxLevel)
        {
            Debug.Log("Max level reached");
            return;
        }

        stats.lvl++;

        spriteRenderer.sprite = data.icons[stats.lvl];
        stats.sellPrice += data.price[stats.lvl] / 2;
    }

    public void SellTower()
    {
        PathSys.Ins.BlockCells(cells: stats.occupiedCells,
                               block: false,
                               newTower: null);

        // TODO: Get Money.

        Destroy(gameObject);
    }

    #endregion
}




#region TowerStats

[Serializable]
public class TowerStats
{
    [ReadOnly] public int lvl = 0;
    public TargetSearchType targetSearchType = TargetSearchType.First;
    public Enemy enemyTargeted = null;

    // Requires Initialization
    public int sellPrice;
    public float shootCooldown;
    [Min(0)] public float range;
    public List<Vector2Int> occupiedCells;

    // CONSTRUCTOR
    public TowerStats(TowerData towerData)
    {
        Reset(towerData, new());
    }

    // METHODS
    public void Reset(TowerData towerData, List<Vector2Int> occupiedCells)
    {
        shootCooldown = towerData.shootCooldown;
        range = towerData.range[0];
        sellPrice = towerData.price[0] / 2;
        this.occupiedCells = occupiedCells;
    }
}

#endregion
#region TargetSearchType

public enum TargetSearchType
{
    First,
    Last = 50,
    Strongest,
    Weakest,
}

#endregion