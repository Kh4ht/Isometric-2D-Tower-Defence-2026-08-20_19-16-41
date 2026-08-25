using KH;
using UnityEngine.UI;

public class TowerSelector : KHManagedBehaviour
{
    #region FIELDS
    public TowerData towerData;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        if (towerData != null)
        {
            name = towerData.name;
        }

        if (TryGetComponent(out Image img) && towerData.icon != null)
        {
            img.sprite = towerData.icon;
        }
    }

    #endregion
    #region PUBLIC

    public void SaveSelectedTower()
    {
        SaveData saveData = KHSaveSystem.Load<SaveData>();

        saveData.selectedTowerIDs.Clear();
        saveData.selectedTowerIDs.Add(towerData.ID);

        KHSaveSystem.Save(saveData);
    }

    #endregion
}