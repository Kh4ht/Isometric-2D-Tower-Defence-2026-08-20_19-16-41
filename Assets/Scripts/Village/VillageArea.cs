using UnityEngine;

public class VillageArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            Debug.Log("enemy == null");
            return;
        }

        enemy.ReachedVillagerArea();
    }
}