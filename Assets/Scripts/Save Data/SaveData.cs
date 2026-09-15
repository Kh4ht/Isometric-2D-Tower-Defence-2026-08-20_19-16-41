using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    #region FIELDS

    [SerializeField]
    private List<string> selectedTowerIDs = new();

    [SerializeField]
    private int maxSelectedTowersCount = 4;

    [SerializeField]
    private List<string> unlockedTowers = new();

    // GETTERS
    public IReadOnlyList<string> SelectedTowerIDs => selectedTowerIDs;
    public int MaxSelectedTowersCount => maxSelectedTowersCount;
    public IReadOnlyList<string> UnlockedTowers => unlockedTowers;


    #endregion
    #region PUBLIC

    public bool IsTowerSelected(string towerID)
    {
        return selectedTowerIDs.Contains(towerID);
    }

    // public bool IsTowerUnlocked(string towerID)
    // {
    //     return unlockedTowers.Contains(towerID);
    // }

    public bool CanSelectMoreTowers()
    {
        return selectedTowerIDs.Count < maxSelectedTowersCount;
    }

    public bool SelectTower(string towerID)
    {
        if (IsTowerSelected(towerID))
            return false;

        if (!CanSelectMoreTowers())
            return false;

        selectedTowerIDs.Add(towerID);
        return true;
    }

    public bool RemoveSelectedTower(string towerID)
    {
        return selectedTowerIDs.Remove(towerID);
    }

    // public void UnlockTower(string towerID)
    // {
    //     if (!unlockedTowers.Contains(towerID))
    //         unlockedTowers.Add(towerID);
    // }

    public IEnumerable<TowerData> GetSelectedTowerDatas()
    {
        for (int i = 0; i < selectedTowerIDs.Count; i++)
        {
            yield return DB.GetTowerDataById(selectedTowerIDs[i]);
        }
    }

    #endregion
}