using KH;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

[DisallowMultipleComponent]
public class LevelManager : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    public static LevelManager Ins { get; private set; }
    private const float TIMESCALE_TWEEN_DURATION = 0.2f;

    private Tween timeScaleTween;
    private bool x2SpeedOn;

    // INSPECTOR

    [Tab("UI")]
    [SerializeField] private GameObject wonMenu;
    [SerializeField] private GameObject lostMenu;

    [Space]

    [SerializeField] private CanvasGroup pauseBlackBg;

    [Tab("STATS")]
    public bool LevelPaused { get; private set; }

    [EndTab]

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogWarning("More Than One Instance");
    }

    protected override void Start()
    {
        base.Start();

        VillageManager.Ins.OnVillagerKidnapped += OnVillagerKidnapped;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        VillageManager.Ins.OnVillagerKidnapped -= OnVillagerKidnapped;
    }

    public void KHManagedUpdate()
    {
        CheckIfWon();
    }

    #endregion
    #region PRIVATE

    private void CheckIfWon()
    {
        if (EnemySpawningSys.Ins.doneSpawning)
        {
            if (!KHPoolManager.Ins.GetAnyActive<Enemy>())
            {
                OnWon();
            }
        }
    }

    private void OnVillagerKidnapped()
    {
        if (VillageManager.Ins.villagers.Count <= 0)
            OnLost();
    }

    private void OnLost()
    {
        lostMenu.SetActive(true);
    }

    private void OnWon()
    {
        wonMenu.SetActive(true);
    }

    #endregion
    #region PUBLIC

    public void TogglePauseLevel()
    {
        LevelPaused = !LevelPaused;
        x2SpeedOn = false;

        // stop any in-flight timescale tween so pause/unpause spam doesn't stack tweens
        timeScaleTween.Stop();

        if (LevelPaused)
        // On level Paused
        {
            Time.timeScale = 0;

            pauseBlackBg.gameObject.SetActive(true);

            pauseBlackBg.alpha = 0;
            Tween.Alpha(pauseBlackBg, 0.5f, 0.1f, useUnscaledTime: true);
        }
        else
        // On level Unpaused
        {
            pauseBlackBg.gameObject.SetActive(false);

            pauseBlackBg.alpha = 0;

            timeScaleTween = Tween.GlobalTimeScale(endValue: 1f,
                                                   duration: TIMESCALE_TWEEN_DURATION);
        }
    }

    public void ToggleX2Speed()
    {
        x2SpeedOn = !x2SpeedOn;

        if (x2SpeedOn)
        // On x2 speed
        {
            Time.timeScale = 2;
        }
        else
        // On normal speed
        {
            Time.timeScale = 1;
        }
    }

    #endregion
}