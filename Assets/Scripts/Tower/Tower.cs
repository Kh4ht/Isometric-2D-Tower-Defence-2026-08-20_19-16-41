using System;
using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(AudioSource))]
public class Tower : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    public const int TOWER_MAX_LEVEL = 6;

    // Subsystems
    private readonly List<IKHSubsystem> kHSubsystems = new();
    public TowerShootingSubSys towerShootingSubSys { get; private set; }

    // Components
    public AudioSource AudioS { get; private set; }

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
        AudioS = GetComponent<AudioSource>();
        AudioS.playOnAwake = false;
    }

    private void Awake()
    {
        AudioS = GetComponent<AudioSource>();

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

    public void KHManagedUpdate()
    {
        kHSubsystems.UpdateAll();
    }

    private void OnDrawGizmosSelected()
    {
        PointAtEnemy();

        DrawTowerRange();



        void PointAtEnemy()
        {
            if (!data.haveShootingSubSys)
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

    public void ResetTower(TowerData towerData)
    {
        stats.Reset(towerData);

        kHSubsystems.ResetAll();
    }

    /// <summary>Increases tower level by 1</summary>
    public void UpgradeTower()
    {
        if (stats.lvl >= TOWER_MAX_LEVEL)
        {
            Debug.Log("Max level reached");
            return;
        }

        stats.lvl++;
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

    public float shootCooldown;
    [Min(0)] public float range;


    // CONSTRUCTOR
    public TowerStats(TowerData towerData)
    {
        shootCooldown = towerData.shootCooldown;
        range = towerData.range[0];
    }

    // METHODS
    public void Reset(TowerData towerData)
    {
        shootCooldown = towerData.shootCooldown;
        range = towerData.range[0];
    }
}

#endregion
#region TargetSearchType

public enum TargetSearchType
{
    First,
    Last,
    Strongest,
    Weakest,
}

#endregion