using KH;
using UnityEngine;
using System.Collections.Generic;

public class LevelTowerContainer : UIController
{
    #region FIELDS

    private readonly List<LevelTowerContainerOption> levelTowerButtons = new();

    // Inspector
    [SerializeField] private LevelTowerContainerOption towerOptionBtn;

    #endregion
    #region UNITY EVENTS

    protected override void Awake()
    {
        base.Awake();

        SaveData saveData = KHSaveSystem.Load<SaveData>();

        foreach (TowerData td in saveData.GetSelectedTowerDatas())
        {
            LevelTowerContainerOption levelTowerButton = Instantiate(towerOptionBtn, transform);

            levelTowerButton.towerData = td;
            levelTowerButton.levelTowerContainer = this;

            levelTowerButtons.Add(levelTowerButton);
        }

        LevelTowerContainerOption upgradeTowerButton = Instantiate(towerOptionBtn, transform);
        upgradeTowerButton.levelTowerContainer = this;
        upgradeTowerButton.type = LevelTowerContainerOption.Type.Upgrade;
        levelTowerButtons.Add(upgradeTowerButton);

        LevelTowerContainerOption sellTowerButton = Instantiate(towerOptionBtn, transform);
        sellTowerButton.levelTowerContainer = this;
        sellTowerButton.type = LevelTowerContainerOption.Type.Sell;
        levelTowerButtons.Add(sellTowerButton);

        LevelTowerContainerOption targetOptionsTowerButton = Instantiate(towerOptionBtn, transform);
        targetOptionsTowerButton.levelTowerContainer = this;
        targetOptionsTowerButton.type = LevelTowerContainerOption.Type.TargetOption;
        levelTowerButtons.Add(targetOptionsTowerButton);

        gameObject.SetActive(false);
    }

    #endregion
    #region PUBLIC

    public void ResetButtonClickedOnce()
    {
        foreach (LevelTowerContainerOption b in levelTowerButtons)
        {
            b.SetButtonClickedOnce(false);
        }
    }

    public void OnCellsSelected(Tower tower)
    {
        foreach (LevelTowerContainerOption b in levelTowerButtons)
        {
            b.SetButtonClickedOnce(false);

            if (tower == null)
            {
                bool isBuy = b.type == LevelTowerContainerOption.Type.Buy;

                b.gameObject.SetActive(isBuy);

                if (isBuy)
                    b.UpdateTextColor();
            }
            else
            {
                if (b.type == LevelTowerContainerOption.Type.Buy)
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
    }

    // public void ShowBuyOptions()
    // {
    //     foreach (LevelTowerContainerOption b in levelTowerButtons)
    //     {
    //         b.SetButtonClickedOnce(false);

    //         bool isBuy = b.type == LevelTowerContainerOption.Type.Buy;

    //         b.gameObject.SetActive(isBuy);

    //         if (isBuy)
    //             b.UpdateTextColor();
    //     }
    // }

    // public void ShowSellUpgradeOptions(Tower tower)
    // {
    //     foreach (LevelTowerContainerOption b in levelTowerButtons)
    //     {
    //         b.SetButtonClickedOnce(false);

    //         if (b.type == LevelTowerContainerOption.Type.Buy)
    //         {
    //             b.gameObject.SetActive(false);
    //         }
    //         else
    //         {
    //             b.gameObject.SetActive(true);
    //             b.EditButton(tower);
    //         }
    //     }
    // }

    #endregion
}