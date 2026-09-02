using System.Collections;
using System.Collections.Generic;
using KH;
using MyHelper;
using UnityEngine;

public class BulletCollisionSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Bullet owner;

    #endregion
    #region CONSTRUCTOR

    public BulletCollisionSubSys(Bullet owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IOnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameTags.ENEMY))
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                // Apply damage to the enemy
                enemy.HealthController.Health -= owner.stats.damage;

                // Destroy the bullet after hitting the enemy
                KHPoolManager.Ins.Despawn(owner.data.ID, owner);
            }
            else
                Debug.LogWarning("Enemy component not found on the collided object.");
        }
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        // Nothing to reset for this subsystem
    }

    #endregion
}