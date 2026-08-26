using KH;

public class Enemy : KHManagedBehaviour
{
    private bool followingPath = true;

    public void ReachedVillagerArea()
    {
        if (!followingPath)
            return;

        followingPath = false;

        StopFollowingPath();

        // Villager nearestVillager = VillagerManager.Ins.GetNearestVillager(transform.position);

        // if (nearestVillager == null)
        //     return;

        // StartChasingVillager(nearestVillager);
    }

    private void StopFollowingPath()
    {
        // Stop A* movement
    }

    private void StartChasingVillager(Villager villager)
    {
        // Start movement toward villager
    }
}