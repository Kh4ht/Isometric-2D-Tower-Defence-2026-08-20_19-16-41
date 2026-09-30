using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

public class Projectile : KHManagedBehaviour, IKHManagedUpdate, IKHManagedFixedUpdate, IKHPoolable
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    public ProjectileCollisionSubSys projectileCollisionSubSys { get; private set; }
    public ProjectileMovementSubSys projectileMovementSubSys { get; private set; }

    // COMPONENTS
    public CapsuleCollider2D Coll2d { get; private set; }

    public Transform VisualRoot => visualRoot;

    // GETTERS
    public string ID => id;

    // INSPECTOR
    [SerializeField, ReadOnly] private string id;

    // Optional child transform used purely for visuals (e.g. bobbing up/down
    // during a parabolic arc) without moving the actual collider/root
    // transform used for gameplay logic. Leave unassigned to skip the
    // visual hop while keeping the arc's timing/logic intact.
    [SerializeField] private Transform visualRoot;

    public ProjectileStats stats;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        // Collider now lives on the visual root
        Coll2d = GetComponentInChildren<CapsuleCollider2D>();

        if (Coll2d != null)
            Coll2d.isTrigger = true;
    }

    private void Awake()
    {
        // Collider lives on the visual root (falls back to any child)
        Coll2d = visualRoot != null
            ? visualRoot.GetComponentInChildren<CapsuleCollider2D>()
            : GetComponentInChildren<CapsuleCollider2D>();

        if (!Coll2d.TryGetComponent(out ProjectileColliderRelay relay))
            relay = Coll2d.gameObject.AddComponent<ProjectileColliderRelay>();

        relay.Init(this);

        stats = new();

        kHSubSystems.AddRange(new IKHSubsystem[]
        {
            projectileCollisionSubSys = new(this),
            projectileMovementSubSys = new(this),
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

    // Replaces the old private OnTriggerEnter2D (delete that one), so events
    // are never handled twice if the root also has a Rigidbody2D.
    public void HandleTriggerEnter(Collider2D collision)
    {
        kHSubSystems.OnTriggerEnter2DAll(collision);
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(ID))
            id = Kh.GenerateId(name, 8);
    }

    #endregion
    #region PUBLIC

    public void ResetBullet(TowerStats towerStats, Enemy target)
    {
        stats.Reset(towerStats, target);

        kHSubSystems.ResetAll();
    }

    public void OnDespawn() { }

    public void OnSpawn() { }

    #endregion
}
