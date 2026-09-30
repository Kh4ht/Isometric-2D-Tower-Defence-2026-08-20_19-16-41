using System;
using KH;
using PrimeTween;
using UnityEngine;
using VInspector;

[DisallowMultipleComponent]
public class LevelManager : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    [KHResetStatic]
    public static LevelManager Ins { get; private set; }
    private const float TIMESCALE_TWEEN_DURATION = 0.2f;

    private Tween timeScaleTween;
    private bool x2SpeedOn;
    public bool LevelPaused { get; private set; }

    public event Action OnWon;
    public event Action OnLost;

    // INSPECTOR
    [SerializeField] private CanvasGroup pauseBlackBg;

    [Tab("STATS")]

    [EndTab]

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogError($"More Than One Instance of type {nameof(LevelManager)}".AddColorTag(KHUtils.XMLColors.Red));
    }

    public void KHUpdate()
    {
        CheckIfWon();
    }

    #endregion
    #region PRIVATE

    private void CheckIfWon()
    {
        if (EnemySpawningSys.Ins.GetFinishedSpawningAllWaves())
        {
            if (!KHPoolManager.Ins.GetAnyActive<Enemy>())
            {
                OnWon();
            }
        }
    }

    #endregion
    #region PUBLIC

    public void OnVillagerKidnapped()
    {
        if (VillageManager.Ins.villagers.Count <= 0)
            OnLost();
    }

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