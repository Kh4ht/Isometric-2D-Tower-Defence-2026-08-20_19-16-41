using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using KH;
using PrimeTween;
using UnityEngine;
using VInspector;

[DisallowMultipleComponent]
public class EnemySpawningSys : KHManagedBehaviour
{
    #region FIELDS

    [KHResetStatic]
    public static EnemySpawningSys Ins { get; private set; }

    // EVENTS
    public event Action<bool> OnFinishedSpawningAllWaves;
    public event Action<bool> OnFinishedSpawningCurrentWave;
    public event Action<int> OnCurrentWaveIndexChange;

    // GETTERS
    public bool ReachedLastWave => currentWaveIndex >= waves.Count - 1;
    public float RestBetweenWavesDuration => restBetweenWavesDuration;
    public int WavesCount => waves.Count;
    public int CurrentWaveIndex => currentWaveIndex;

    // INSPECTOR
    [Tab("Stats")]
    [SerializeField] private bool finishedSpawningAllWaves = false;
    [SerializeField] private bool finishedSpawningCurrentWave = true;
    [SerializeField] private int currentWaveIndex;
    [SerializeField, Min(1)] private float restBetweenWavesDuration;

    [Tab("Waves")]
    [SerializeField] private List<WaveData> waves;
    [Tab("Data")]
    [SerializeField] private EnemyWavesStarterButton wavesStarterButton;

    [EndTab]

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogError($"More Than One Instance of type {nameof(EnemySpawningSys)}".AddColorTag(KHUtils.XMLColors.Red));
    }
    protected override void Start()
    {
        base.Start();

        RegisterEnemiesToPool();

        for (int i = 0; i < PathSys.Ins.pathStartCells.Count; i++)
        {
            Vector2Int cell = PathSys.Ins.pathStartCells[i];

            Instantiate(wavesStarterButton, cell.GetCellCenterWorld(), Quaternion.identity, transform).Init(i);
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            WaveData wave = waves[i];

            wave.name = $"Wave {i + 1}";

            CheckEnemyPathsCount(i);

            for (int j = 0; j < wave.enemyPaths.Count; j++)
            {
                EnemyPathData enemyPath = wave.enemyPaths[j];

                enemyPath.name = $"Path {j + 1}";

                for (int k = 0; k < enemyPath.entries.Count; k++)
                {
                    EntryData entry = enemyPath.entries[k];

                    entry.name = $"Entry {k + 1}";

                }
            }
        }
    }

    #endregion
    #region PRIVATE

    private bool NotPlayMode => !Application.isPlaying;
    [DisableIf(nameof(NotPlayMode))]
    [Button(color = "green")]
    public void StartSpawningEnemies()
    {
        if (GetFinishedSpawningAllWaves() || !GetFinishedSpawningCurrentWave())
            return;

        StartCoroutine(SpawnWaveCoroutine());
    }

    private void CheckEnemyPathsCount(int i)
    {
        WaveData wave = waves[i];

        if (wave.enemyPaths.Count > PathSys.InsEditor.pathStartCells.Count)
            Debug.LogError($"There Are More {nameof(wave.enemyPaths)} Than The {nameof(PathSys.InsEditor.pathStartCells)} in {wave.name}".AddColorTag(KHUtils.XMLColors.Red));
        else if (wave.enemyPaths.Count < PathSys.InsEditor.pathStartCells.Count)
            Debug.LogError($"There Are Less {nameof(wave.enemyPaths)} Than The {nameof(PathSys.InsEditor.pathStartCells)} in {wave.name}".AddColorTag(KHUtils.XMLColors.Red));
    }

    private IEnumerator SpawnWaveCoroutine()
    {
        SetFinishedSpawningCurrentWave(false);

        WaveData wave = waves[GetCurrentWaveIndex()];

        if (wave.startDelay > 0f)
            yield return new WaitForSeconds(wave.startDelay);

        int remainingPaths = wave.enemyPaths.Count;

        for (int pathIndex = 0; pathIndex < wave.enemyPaths.Count; pathIndex++)
        {
            EnemyPathData enemyPath = wave.enemyPaths[pathIndex];

            StartCoroutine(SpawnPathCoroutine(enemyPath: enemyPath,
                                              pathIndex: pathIndex,
                                              onFinished: () => remainingPaths--)
            );
        }

        // Wait until every path in current has finished.
        yield return new WaitUntil(() => remainingPaths <= 0);

        SetFinishedSpawningCurrentWave(true);

        if (ReachedLastWave)
        {
            SetFinishedSpawningAllWaves(true);
        }
        else
        {
            SetCurrentWaveIndex(GetCurrentWaveIndex() + 1);
            Tween.Delay(restBetweenWavesDuration, StartSpawningEnemies);
        }
    }

    private IEnumerator SpawnPathCoroutine(EnemyPathData enemyPath,
                                           int pathIndex,
                                           Action onFinished)
    {
        foreach (EntryData entry in enemyPath.entries)
        {
            if (entry.startDelay > 0f)
                yield return new WaitForSeconds(entry.startDelay);

            for (int i = 0; i < entry.repeatCount; i++)
            {
                SpawnEnemy(entry.enemyData, pathIndex);

                if (i < entry.repeatCount - 1 &&
                    entry.repeatDelay > 0f)
                {
                    yield return new WaitForSeconds(entry.repeatDelay);
                }
            }
        }

        onFinished?.Invoke();
    }

    private void SpawnEnemy(EnemyData enemyData, int pathIndex)
    {
        List<Vector2> path = PathSys.Ins.GetPath(pathIndex);

        Vector2 spawnPos = path[0];

        KHPoolManager.Ins.Spawn<Enemy>(enemyData.ID, spawnPos).ResetEnemy(path);
    }

    private void RegisterEnemiesToPool()
    {
        foreach (WaveData wave in waves)
        {
            foreach (EnemyPathData enemyPath in wave.enemyPaths)
            {
                foreach (EntryData entry in enemyPath.entries)
                {
                    KHPoolManager.Ins.Register(key: entry.enemyData.ID,
                                               prefab: entry.enemyData.prefab,
                                               showLogMessage: false);
                }
            }
        }
    }

    #endregion
    #region PUBLIC

    public bool GetFinishedSpawningAllWaves() => finishedSpawningAllWaves;
    public void SetFinishedSpawningAllWaves(bool newValue)
    {
        if (newValue == finishedSpawningAllWaves)
            return;

        finishedSpawningAllWaves = newValue;
        OnFinishedSpawningAllWaves?.Invoke(newValue);
    }

    public bool GetFinishedSpawningCurrentWave() => finishedSpawningCurrentWave;
    public void SetFinishedSpawningCurrentWave(bool newValue)
    {
        if (newValue == finishedSpawningCurrentWave)
            return;

        finishedSpawningCurrentWave = newValue;
        OnFinishedSpawningCurrentWave?.Invoke(newValue);
    }

    public int GetCurrentWaveIndex() => currentWaveIndex;
    public void SetCurrentWaveIndex(int newValue)
    {
        if (newValue == currentWaveIndex)
            return;

        currentWaveIndex = newValue;
        OnCurrentWaveIndexChange?.Invoke(newValue);
    }

    #endregion
}
