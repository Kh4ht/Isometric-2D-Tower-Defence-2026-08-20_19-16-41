using KH;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class TowerOptionBtn : KHManagedBehaviour
{
    #region FIELDS

    public enum Type
    {
        Buy,
        Sell,
        Upgrade,
        TargetOption,
    }

    private Tower selectedTower;

    [HideInInspector] public TowerData towerData;

    // COMPONENTS
    private Button button;

    // INSPECTOR
    public Type type = Type.Buy;
    [SerializeField] private TextMeshProUGUI buttonTxt;
    [SerializeField] private Image image;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();

        Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // The first OnEnable runs inside LevelTowerContainer.Awake (via Instantiate),
        // possibly before LevelCoinManager.Awake, so Ins can still be null.
        if (LevelCoinManager.Ins == null)
        {
            Debug.Log("LevelCoinManager.Ins is NULL");
            return;
        }

        LevelCoinManager.Ins.Aether.OnChanged += OnAetherChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (LevelCoinManager.Ins == null)
        {
            Debug.Log("LevelCoinManager.Ins is NULL");
            return;
        }

        LevelCoinManager.Ins.Aether.OnChanged -= OnAetherChanged;
    }

    #endregion
    #region PRIVATE

    private void OnAetherChanged(long newAmount, long delta) => UpdateTextColor();

    /// <summary>Colors the price text and toggles interactability based on whether Aether can afford it.</summary>
    public void UpdateTextColor()
    {
        if (LevelCoinManager.Ins == null)
        {
            Debug.LogWarning("LevelCoinManager.Ins is NULL");
            return;
        }

        int price;

        switch (type)
        {
            case Type.Buy:
                price = towerData.PurchasePrice;
                break;

            case Type.Upgrade:
                price = selectedTower.stats.GetNextUpgradePrice();
                break;

            default:
                return; // Sell / TargetOption don't cost coins
        }

        bool canAfford = LevelCoinManager.Ins.Aether.CanAfford(price);

        buttonTxt.color = canAfford ? DB.Db.coinAcceptanceColor : DB.Db.coinRejectionColor;
        button.interactable = canAfford;
    }

    private void Init()
    {
        switch (type)
        {
            case Type.Buy:
                name = towerData.name;
                image.sprite = towerData.icons[0];
                buttonTxt.SetText($"{towerData.PurchasePrice:N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (LevelCoinManager.Ins.Aether.TrySpend(towerData.PurchasePrice))
                        TowerPlacementSys.Ins.PlaceTowerOnSelectedPos(towerData);
                });
                break;

            case Type.Upgrade:
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
        selectedTower = tower;

        switch (type)
        {
            case Type.Buy:
                Debug.LogWarning($"Wrong Type {type}");
                break;

            case Type.Upgrade:
                if (tower.IsMaxLevel)
                {
                    gameObject.SetActive(false);
                    return;
                }

                image.sprite = tower.data.icons[tower.stats.GetLvl() + 1];
                buttonTxt.SetText($"{tower.stats.GetNextUpgradePrice():N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (LevelCoinManager.Ins.Aether.TrySpend(tower.stats.GetNextUpgradePrice()))
                    {
                        tower.UpgradeTower();
                        TowerPlacementSys.Ins.cells.Deselect();
                    }
                });

                UpdateTextColor();
                break;

            case Type.Sell:
                buttonTxt.SetText($"{tower.stats.GetSellPrice():N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    LevelCoinManager.Ins.Aether.Add(tower.stats.GetSellPrice());
                    tower.SellTower();
                    TowerPlacementSys.Ins.cells.Deselect();
                });

                break;

            case Type.TargetOption:
                buttonTxt.SetText($"{tower.stats.GetTargetSearchType()}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    tower.stats.SetTargetSearchType(tower.stats.GetTargetSearchType().KHCycle());
                    // TODO: change sprite of the button

                    buttonTxt.SetText($"{tower.stats.GetTargetSearchType()}");
                });

                break;

            default:
                Debug.LogWarning($"Unsupported type {nameof(type)}");
                break;
        }
    }

    #endregion
}