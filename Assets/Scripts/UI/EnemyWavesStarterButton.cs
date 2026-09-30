using KH;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class EnemyWavesStarterButton : KHManagedBehaviour
{
    #region FIELDS

    [SerializeField] private Button button;
    [SerializeField] private Image restBetweenWavesTimerSlider;
    private int pathIndex;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            EnemySpawningSys.Ins.StartSpawningEnemies();
        });

        EnemySpawningSys.Ins.OnFinishedSpawningCurrentWave += OnFinishedSpawningCurrentWave;
    }

    private void OnDestroy()
    {
        EnemySpawningSys.Ins.OnFinishedSpawningCurrentWave -= OnFinishedSpawningCurrentWave;
    }

    #endregion
    #region PRIVATE

    private void OnFinishedSpawningCurrentWave(bool finished)
    {
        if (!EnemySpawningSys.Ins.ReachedLastWave && finished)
        {
            gameObject.SetActive(true);

            restBetweenWavesTimerSlider.fillAmount = 1f;

            Tween.Custom(startValue: 1f,
                         endValue: 0f,
                         duration: EnemySpawningSys.Ins.RestBetweenWavesDuration,
                         onValueChange: (f) =>
                         {
                             restBetweenWavesTimerSlider.fillAmount = f;
                         },
                         ease: Ease.Linear);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    #endregion
    #region PUBLIC

    public EnemyWavesStarterButton Init(int pathIndex)
    {
        this.pathIndex = pathIndex;

        return this;
    }

    #endregion
}