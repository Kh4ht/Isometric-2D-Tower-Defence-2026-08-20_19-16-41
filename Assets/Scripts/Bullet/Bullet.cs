using System;
using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Bullet : KHManagedBehaviour, IKHManagedUpdate, IKHPoolable
{
    #region FIELDS

    // SUBSYSTEMS
    private readonly List<IKHSubsystem> kHSubSystems = new();
    public BulletCollisionSubSys bulletCollisionSubSys { get; private set; }

    // COMPONENTS
    public Rigidbody2D Rb2d { get; private set; }

    // INSPECTOR
    [Tab("STATS")]
    public BulletStats stats;

    [Tab("DATA")]
    public BulletData data;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        Rb2d = GetComponent<Rigidbody2D>();
        Rb2d.bodyType = RigidbodyType2D.Kinematic;

        //Set Tag
        // tag = GameTags.BULLET;
    }

    private void Awake()
    {
        Rb2d = GetComponent<Rigidbody2D>();

        stats = new(data);
    }

    protected override void Start()
    {
        base.Start();

        stats.targetPos = stats.target.transform.position;
    }

    public void KHManagedUpdate()
    {
        switch (data.type)
        {
            case BulletMoveType.Straight:
                StraightMove();
                break;
            case BulletMoveType.Parabolic:
                ParabolicMove();
                break;
            case BulletMoveType.Laser:
                LaserMove();
                break;
            case BulletMoveType.Follow:
                FollowMove();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        kHSubSystems.OnTriggerEnter2DAll(collision);
    }

    #endregion
    #region PRIVATE

    private void StraightMove()
    {
        Rb2d.linearVelocity = stats.moveSpeed
                              * Time.fixedDeltaTime
                              * stats.targetPos.normalized;
    }

    private void ParabolicMove()
    { }

    private void LaserMove()
    { }

    private void FollowMove()
    {

    }

    #endregion
    #region PUBLIC

    public void ResetBullet(BulletData bulletData, Enemy target)
    {
        stats.Reset(bulletData, target);

        kHSubSystems.ResetAll();
    }

    public void OnDespawn() { }

    public void OnSpawn() { }

    #endregion
}





#region BulletStats

[Serializable]
public class BulletStats
{
    public Vector2 targetPos = Vector2.zero;

    // Requires Initialization
    public float moveSpeed;
    public Enemy target;
    public float damage;

    // CONSTRUCTOR
    public BulletStats(BulletData data)
    {
        moveSpeed = data.moveSpeed;
        damage = data.damage;
    }

    // METHODS
    public void Reset(BulletData data, Enemy target)
    {
        this.target = target;
        moveSpeed = data.moveSpeed;
        damage = data.damage;
    }
}

#endregion