using KH;
using PrimeTween;
using TMPro;
using UnityEngine;
using VInspector;

[DisallowMultipleComponent]
public class LevelUIManager : KHManagedBehaviour
{
    #region FIELDS

    [KHResetStatic]
    public static LevelUIManager Ins { get; private set; }

    // INSPECTOR
    [Header("Image")]
    [SerializeField] private GameObject wonMenu;
    [SerializeField] private GameObject lostMenu;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI wavesCountTxt;
    [SerializeField] private TextMeshProUGUI aetherTxt;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogError($"More Than One Instance of type {nameof(LevelManager)}".AddColorTag(KHUtils.XMLColors.Red));
    }

    protected override void Start()
    {
        base.Start();

        wavesCountTxt.SetText($"{0}/{EnemySpawningSys.Ins.WavesCount}");
        aetherTxt.SetText($"{LevelCoinManager.Ins.Aether.Amount:N0}");

        EnemySpawningSys.Ins.OnFinishedSpawningCurrentWave += UpdateWavesCountTxt;
        LevelManager.Ins.OnWon += ShowWonMenu;
        LevelManager.Ins.OnLost += ShowLostMenu;
        LevelCoinManager.Ins.Aether.OnChanged += OnCoinsChanged;
    }

    private void OnDestroy()
    {
        EnemySpawningSys.Ins.OnFinishedSpawningCurrentWave -= UpdateWavesCountTxt;
        LevelManager.Ins.OnWon -= ShowWonMenu;
        LevelManager.Ins.OnLost -= ShowLostMenu;
        LevelCoinManager.Ins.Aether.OnChanged -= OnCoinsChanged;
    }

    #endregion
    #region PRIVATE

    private void OnCoinsChanged(long newAmount, long delta)
    {
        aetherTxt.SetText($"{newAmount:N0}");
    }

    private void ShowLostMenu()
    {
        lostMenu.SetActive(true);
    }

    private void ShowWonMenu()
    {
        wonMenu.SetActive(true);
    }

    private void UpdateWavesCountTxt(bool finished)
    {
        if (!finished)
            wavesCountTxt.SetText($"{EnemySpawningSys.Ins.CurrentWaveIndex + 1}/{EnemySpawningSys.Ins.WavesCount}");
    }

    #endregion
}
