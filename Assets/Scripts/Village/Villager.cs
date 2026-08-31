using System;
using KH;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Villager : KHManagedBehaviour
{
    #region FIELDS


    // COMPONENTS
    public Rigidbody2D rb2d { get; private set; }
    public CapsuleCollider2D coll2d { get; private set; }

    // INSPECTOR
    public VillagerStats stats = new();

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;

        coll2d = GetComponent<CapsuleCollider2D>();
        coll2d.isTrigger = true;

        // Set tag
        tag = GameTags.VILLAGER;
    }

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        coll2d = GetComponent<CapsuleCollider2D>();
    }

    #endregion
}

#region VillagerStats

[Serializable]
public class VillagerStats
{
    // TODO: empty class, remove if not necessary
}

#endregion