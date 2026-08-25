using KH;
using UnityEngine.UI;
using UnityEngine;

public class LevelTowerContainer : KHManagedBehaviour
{
    #region FIELDS

    // Inspector
    [SerializeField] private LevelTowerButton levelTowerButtonPrefab;

    #endregion
    #region UNITY EVENTS

    protected override void Start()
    {
        base.Start();

        SaveData saveData = KHSaveSystem.Load<SaveData>();

        foreach (string towerId in saveData.selectedTowerIDs)
        {
            TowerData towerData = DB.GetTowerDataById(towerId);

            if (towerData == null)
                Debug.Log("towerData == null");

            LevelTowerButton levelTowerButton = Instantiate(levelTowerButtonPrefab, transform);

            levelTowerButton.towerData = towerData;

            if (levelTowerButton.GetComponent<Image>() == null)
                Debug.Log("GetComponent<Image>() == null");

            if (towerData.icon == null)
                Debug.Log("towerData.icon == null");

            levelTowerButton.GetComponent<Image>().sprite = towerData.icon;
            levelTowerButton.name = towerData.name;
        }
    }

    #endregion
}