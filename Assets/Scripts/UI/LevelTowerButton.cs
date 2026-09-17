using KH;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class LevelTowerButton : KHManagedBehaviour
{
    #region FIELDS

    public enum Type
    {
        Buy,
        Sell,
        Upgrade,
        TargetOption,
    }

    // COMPONENTS

    private Image image;
    private Button button;


    public Type type = Type.Buy;

    [HideInInspector]
    public TowerData towerData;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();

        Init();
    }

    #endregion
    #region PRIVATE

    private void Init()
    {
        switch (type)
        {
            case Type.Buy:
                name = towerData.name;
                image.sprite = towerData.icons[0];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    TowerPlacementSys.Ins.PlaceTowerOnSelectedPos(towerData);
                });
                break;

            case Type.Upgrade:
                image.sprite = DB.Db.upgradeTowerImage;
                name = Type.Upgrade.ToString();
                break;

            case Type.Sell:
                image.sprite = DB.Db.sellTowerImage;
                name = Type.Sell.ToString();
                break;

            case Type.TargetOption:
                image.sprite = DB.Db.targetOptionsImage;
                name = Type.TargetOption.ToString();
                break;

            default:
                Debug.LogWarning($"Unsupported type {nameof(type)}");
                break;
        }
    }

    #endregion
    #region PUBLIC

    public void EditButton(Tower tower)
    {
        // Change sprite

        // Change button listeners
        switch (type)
        {
            case Type.Buy:
                Debug.LogError("Wrong Type");
                break;

            case Type.Upgrade:
                if (tower.IsMaxLevel)
                {
                    gameObject.SetActive(false);
                    return;
                }

                image.sprite = tower.data.icons[tower.stats.lvl + 1];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    tower.UpgradeTower();
                    TowerPlacementSys.Ins.cells.Deselect();
                });

                break;

            case Type.Sell:
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    tower.SellTower();
                    TowerPlacementSys.Ins.cells.Deselect();
                });

                break;

            case Type.TargetOption:
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    tower.stats.SetTargetSearchType(tower.stats.GetTargetSearchType().KHCycle());
                    // TODO: change sprite of the button
                });

                break;

            default:
                Debug.LogWarning($"Unsupported type {nameof(type)}");
                break;
        }
    }

    #endregion
}