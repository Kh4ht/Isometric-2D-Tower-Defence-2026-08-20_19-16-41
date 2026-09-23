using System;
using KH;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class LevelTowerContainerOption : KHManagedBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    [HideInInspector] public LevelTowerContainer levelTowerContainer;

    // COMPONENTS
    private Button button;
    private Image image;
    private bool buttonClickedOnce;
    private bool isHighlighted;

    // INSPECTOR
    public Type type = Type.Buy;
    [SerializeField] private TextMeshProUGUI optionTxt;
    [SerializeField] private Image optionImage;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        image.color = DB.Db.towerOptionNormalColor;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlighted = true;
        RefreshBackgroundColor();

        switch (type)
        {
            case Type.Buy:
                TowerPlacementSys.Ins.EnableSecondaryTowerRangeIndicator(towerData);
                break;

            case Type.Upgrade:
                TowerPlacementSys.Ins.EnableSecondaryTowerRangeIndicator(selectedTower);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
        RefreshBackgroundColor();

        if (type == Type.Buy || type == Type.Upgrade)
            TowerPlacementSys.Ins.DisableSecondaryTowerRangeIndicator();
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

        optionTxt.color = canAfford ? DB.Db.coinAcceptanceColor : DB.Db.coinRejectionColor;
        button.interactable = canAfford;
    }

    /// <summary>
    /// First click arms this button (and disarms the others); second click runs onConfirm.
    /// Gives visual feedback while armed so the "tap again" state isn't invisible.
    /// </summary>
    private void HandleConfirmableClick(Action onFirstClick, Action onConfirm)
    {
        if (!buttonClickedOnce)
        {
            levelTowerContainer.ResetButtonClickedOnce();
            SetButtonClickedOnce(true);

            onFirstClick?.Invoke();
        }
        else
        {
            onConfirm();
        }
    }

    /// <summary>
    /// Picks the background color from current state (disabled > selected > highlighted > normal)
    /// and tweens to it. Call this whenever interactable, armed, or hover state changes.
    /// </summary>
    private void RefreshBackgroundColor()
    {
        Color targetColor;

        if (!button.interactable)
            targetColor = DB.Db.towerOptionDisabledColor;
        else if (buttonClickedOnce)
            targetColor = DB.Db.towerOptionSelectedColor;
        else if (isHighlighted)
            targetColor = DB.Db.towerOptionHighlightedColor;
        else
            targetColor = DB.Db.towerOptionNormalColor;

        Tween.Color(target: image,
                    startValue: image.color,
                    endValue: targetColor,
                    duration: DB.COLOR_TWEEN_DURATION);
    }

    private void Init()
    {
        switch (type)
        {
            case Type.Buy:
                name = towerData.name;
                optionImage.sprite = towerData.icons[0];
                optionTxt.SetText($"{towerData.PurchasePrice:N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    HandleConfirmableClick(
                        onFirstClick: null,
                        onConfirm: () =>
                        {
                            if (LevelCoinManager.Ins.Aether.TrySpend(towerData.PurchasePrice))
                                TowerPlacementSys.Ins.PlaceTowerOnSelectedPos(towerData);
                        });
                });
                break;

            case Type.Upgrade:
                name = Type.Upgrade.ToString();
                break;

            case Type.Sell:
                optionImage.sprite = DB.Db.sellTowerImage;
                name = Type.Sell.ToString();
                break;

            case Type.TargetOption:
                optionImage.sprite = DB.Db.targetOptionsImage;
                name = Type.TargetOption.ToString();
                break;

            default:
                Debug.LogWarning($"Unsupported type {nameof(type)}");
                break;
        }
    }

    #endregion
    #region PUBLIC

    public bool GetButtonClickedOnce() => buttonClickedOnce;
    public void SetButtonClickedOnce(bool newValue)
    {
        if (newValue == buttonClickedOnce)
            return;

        buttonClickedOnce = newValue;

        Tween.Color(target: image,
                    startValue: image.color,
                    endValue: buttonClickedOnce ? DB.Db.towerOptionSelectedColor : DB.Db.towerOptionNormalColor,
                    duration: DB.COLOR_TWEEN_DURATION);
    }

    public void EditButton(Tower tower)
    {
        selectedTower = tower;

        switch (type)
        {
            case Type.Upgrade:
                if (tower.IsMaxLevel)
                {
                    gameObject.SetActive(false);
                    return;
                }

                optionImage.sprite = tower.data.icons[tower.stats.GetLvl() + 1];
                optionTxt.SetText($"{tower.stats.GetNextUpgradePrice():N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    HandleConfirmableClick(
                        onFirstClick: null,
                        onConfirm: () =>
                        {
                            if (LevelCoinManager.Ins.Aether.TrySpend(tower.stats.GetNextUpgradePrice()))
                            {
                                tower.UpgradeTower();
                                TowerPlacementSys.Ins.cells.Deselect();
                            }
                        });
                });

                UpdateTextColor();
                break;

            case Type.Sell:
                optionTxt.SetText($"{tower.stats.GetSellPrice():N0}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    HandleConfirmableClick(
                        onFirstClick: null,
                        onConfirm: () =>
                        {
                            LevelCoinManager.Ins.Aether.Add(tower.stats.GetSellPrice());
                            tower.SellTower();
                            TowerPlacementSys.Ins.cells.Deselect();
                        });
                });

                break;

            case Type.TargetOption:
                optionTxt.SetText($"{tower.stats.GetTargetSearchType()}");

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    levelTowerContainer.ResetButtonClickedOnce();

                    tower.stats.SetTargetSearchType(tower.stats.GetTargetSearchType().KHCycle());
                    // TODO: change sprite of the button

                    optionTxt.SetText($"{tower.stats.GetTargetSearchType()}");
                });

                break;

            default:
                Debug.LogWarning($"Unsupported type {nameof(type)}");
                break;
        }

        //Make sure disabled color is set, without hovering
        RefreshBackgroundColor();
    }

    #endregion
}