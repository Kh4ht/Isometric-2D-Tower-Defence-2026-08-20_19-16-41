using KH;

public class Villager : KHManagedBehaviour
{
    #region FIELDS



    #endregion
    #region UNITY EVENTS



    #endregion
    #region PRIVATE

    private void KidnapVillager()
    {
        VillageController.Ins.currentVillagersCount--;
    }

    #endregion
}