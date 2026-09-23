using System;
using System.Collections.Generic;
using KH;
using Assets.Scripts.Utils;
using UnityEngine;
using VInspector;
[RequireComponent(typeof(AudioSource), typeof(SpriteRenderer))]

[RequireComponent(typeof(LineRenderer))]
public class Tower : KHManagedBehaviour, IKHManagedUpdate, IKHPoolable
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubsystems = new();
    public TowerShootingSubSys towerShootingSubSys { get; private set; }
    public TowerAnimatorSubSys towerAnimatorSubSys { get; private set; }

    // COMPONENTS
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    public LineRenderer lineRenderer { get; private set; }

    // GETTERS
    public bool IsMaxLevel => stats.GetLvl() >= GameConsts.TOWER_MAX_LEVEL;

    // EVENTS
    public event Action<bool> OnTowerSelected;

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

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = DB.GetTowerRangeIndicatorGradient(data.elementType);

        stats = new(data);

        kHSubsystems.Clear();

        if (data.haveShootingSubSys)
            kHSubsystems.Add(towerShootingSubSys = new(this));

        kHSubsystems.AddRange(new IKHSubsystem[]
        {
            towerAnimatorSubSys = new(this),
        });
    }

    protected override void Start()
    {
        base.Start();

        RegisterBulletsToPool();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        kHSubsystems.OnDisableAll();
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

            if (stats.GetEnemyTargeted() != null)
            {
                Gizmos.color = Color.red;
                // draw a line at the target
                Gizmos.DrawLine(transform.position, stats.GetEnemyTargeted().transform.position);
            }
        }

        void DrawTowerRange()
        {
            float range = stats.GetRange();

            if (range <= 0f)
                return;

            Vector2 origin = transform.position;

            Gizmos.color = Color.cyan;

            Vector2 prevPoint = Helper.TileCircleToWorld(origin, range, 0);

            for (int i = 1; i <= GameConsts.TOWER_RANGE_SEGMENTS; i++)
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
        KHPoolManager.Ins.Register(data.bulletData.ID,
                                   data.bulletData.prefab,
                                   showLogMessage: false);
    }

    #endregion
    #region PUBLIC

    /// <summary>
    /// Updates the tower's selection indicator when selected by the player,
    /// allowing its upgrade and sell data to appear.
    /// </summary>
    public void OnSelected(bool selected)
    {
        OnTowerSelected?.Invoke(selected);
    }

    public Tower ResetTower(List<Vector2Int> occupiedCells)
    {
        stats.Reset(data, occupiedCells);

        PathSys.Ins.BlockCells(cells: occupiedCells,
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

        stats.SetLvl(stats.GetLvl() + 1);

        spriteRenderer.sprite = data.icons[stats.GetLvl()];
        stats.SetSellPrice(stats.GetSellPrice() + (data.price[stats.GetLvl()] / 2));
        stats.SetNextUpgradePrice(data.price[Mathf.Clamp(stats.GetLvl() + 1, 1, GameConsts.TOWER_MAX_LEVEL)]);
        stats.SetRange(data.range[stats.GetLvl()]);
        stats.SetShootCooldown(data.shootCooldown[stats.GetLvl()]);
    }

    public void SellTower()
    {
        PathSys.Ins.BlockCells(cells: stats.GetOccupiedCells(),
                               newTower: null);

        // TODO: Get Money.

        KHPoolManager.Ins.Despawn(data.ID, this);
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    #endregion
}