using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter2 : MonoBehaviour
{
    [SerializeField] private float shootCooldown;
    [SerializeField] private float shootCooldownTimer;
    [SerializeField] private Projectile2 projectilePrefab;
    [SerializeField] private Transform target;
    [SerializeField] private float projectileMoveSpeed;

    public void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        shootCooldownTimer += Time.deltaTime;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (shootCooldownTimer > shootCooldown)
        {
            Instantiate(projectilePrefab, transform.position, Quaternion.identity).Init(target, projectileMoveSpeed);

            shootCooldownTimer = 0;
        }
    }
}
