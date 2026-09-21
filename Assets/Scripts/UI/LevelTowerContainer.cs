using KH;
using UnityEngine;
using System.Collections.Generic;

public class LevelTowerContainer : UIController
{
    #region FIELDS

    private readonly List<TowerOptionBtn> levelTowerButtons = new();

    // Inspector
    [SerializeField] private TowerOptionBtn towerOptionBtn;

    #endregion
    #region UNITY EVENTS

    protected override void Awake()
    {
        base.Awake();

        SaveData saveData = KHSaveSystem.Load<SaveData>();

        foreach (TowerData td in saveData.GetSelectedTowerDatas())
        {
            TowerOptionBtn levelTowerButton = Instantiate(towerOptionBtn, transform);

            levelTowerButton.towerData = td;

            levelTowerButtons.Add(levelTowerButton);
        }

        TowerOptionBtn upgradeTowerButton = Instantiate(towerOptionBtn, transform);
        upgradeTowerButton.type = TowerOptionBtn.Type.Upgrade;
        levelTowerButtons.Add(upgradeTowerButton);

        TowerOptionBtn sellTowerButton = Instantiate(towerOptionBtn, transform);
        sellTowerButton.type = TowerOptionBtn.Type.Sell;
        levelTowerButtons.Add(sellTowerButton);

        TowerOptionBtn targetOptionsTowerButton = Instantiate(towerOptionBtn, transform);
        targetOptionsTowerButton.type = TowerOptionBtn.Type.TargetOption;
        levelTowerButtons.Add(targetOptionsTowerButton);

        gameObject.SetActive(false);
    }

    #endregion
    #region PUBLIC

    public void ShowBuyOptions()
    {
        foreach (TowerOptionBtn b in levelTowerButtons)
        {
            bool isBuy = b.type == TowerOptionBtn.Type.Buy;

            b.gameObject.SetActive(isBuy);

            if (isBuy)
                b.UpdateTextColor();
        }
    }

    public void ShowSellUpgradeOptions(Tower tower)
    {
        foreach (TowerOptionBtn b in levelTowerButtons)
        {
            if (b.type == TowerOptionBtn.Type.Buy)
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