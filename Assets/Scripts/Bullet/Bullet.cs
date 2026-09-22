using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Bullet : KHManagedBehaviour, IKHManagedUpdate, IKHManagedFixedUpdate, IKHPoolable
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    public BulletCollisionSubSys bulletCollisionSubSys { get; private set; }
    public BulletMovementSubSys bulletMovementSubSys { get; private set; }

    // COMPONENTS
    public CapsuleCollider2D Coll2d { get; private set; }

    // INSPECTOR
    [Tab("STATS")]
    public BulletStats stats;

    [Tab("DATA")]
    public BulletData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        Coll2d = GetComponent<CapsuleCollider2D>();
        Coll2d.isTrigger = true;
    }

    private void Awake()
    {
        Coll2d = GetComponent<CapsuleCollider2D>();

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        kHSubSystems.OnTriggerEnter2DAll(collision);
    }

    #endregion
    #region PUBLIC

    public void ResetBullet(BulletData bulletData, int mainTowerLvl, Enemy target)
    {
        stats.Reset(bulletData, mainTowerLvl, target);

        kHSubSystems.ResetAll();
    }

    public void OnDespawn() { }

    public void OnSpawn() { }

    #endregion
}