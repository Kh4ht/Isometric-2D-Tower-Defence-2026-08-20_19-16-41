using KH;
using UnityEngine;
using System.Collections.Generic;

public class LevelTowerContainer : UIController
{
    #region FIELDS

    private readonly List<LevelTowerButton> levelTowerButtons = new();

    // Inspector
    [SerializeField] private LevelTowerButton levelTowerButtonPrefab;

    #endregion
    #region UNITY EVENTS

    protected override void Awake()
    {
        base.Awake();

        SaveData saveData = KHSaveSystem.Load<SaveData>();

        foreach (string towerId in saveData.selectedTowerIDs)
        {
            LevelTowerButton levelTowerButton = Instantiate(levelTowerButtonPrefab, transform);

            levelTowerButton.towerData = DB.GetTowerDataById(towerId); ;

            levelTowerButtons.Add(levelTowerButton);
        }

        LevelTowerButton upgradeTowerButton = Instantiate(levelTowerButtonPrefab, transform);
        upgradeTowerButton.type = LevelTowerButton.Type.Upgrade;
        levelTowerButtons.Add(upgradeTowerButton);

        LevelTowerButton sellTowerButton = Instantiate(levelTowerButtonPrefab, transform);
        sellTowerButton.type = LevelTowerButton.Type.Sell;
        levelTowerButtons.Add(sellTowerButton);

        LevelTowerButton targetOptionsTowerButton = Instantiate(levelTowerButtonPrefab, transform);
        targetOptionsTowerButton.type = LevelTowerButton.Type.TargetOption;
        levelTowerButtons.Add(targetOptionsTowerButton);

        gameObject.SetActive(false);
    }

    #endregion
    #region PUBLIC

    public void ShowBuyOptions()
    {
        foreach (LevelTowerButton b in levelTowerButtons)
        {
            if (b.type == LevelTowerButton.Type.Buy)
                b.gameObject.SetActive(true);
            else
                b.gameObject.SetActive(false);
        }
    }

    public void ShowSellUpgradeOptions(Tower tower)
    {
        foreach (LevelTowerButton b in levelTowerButtons)
        {
            if (b.type == LevelTowerButton.Type.Buy)
            {
                b.gameObject.SetActive(false);
            }
            else
            {
                b.gameObject.SetActive(true);
                b.EditButton(tower);
            }
        }
    }

    #endregion
}