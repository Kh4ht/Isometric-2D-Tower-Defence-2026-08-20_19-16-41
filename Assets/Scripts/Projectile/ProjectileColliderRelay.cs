using KH;
using UnityEngine;

/// Sits on the GameObject that owns the bullet's collider (the visual root)
/// and forwards trigger events to the Bullet on the root.
public class ProjectileColliderRelay : KHManagedBehaviour
{
    private Projectile bullet;

    public void Init(Projectile owner) => bullet = owner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (bullet != null)
            bullet.HandleTriggerEnter(collision);
    }
}