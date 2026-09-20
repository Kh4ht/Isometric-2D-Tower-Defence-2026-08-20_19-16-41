using System;
using System.Collections;
using System.Collections.Generic;
using KH;
using UnityEngine;
using VInspector;

[DisallowMultipleComponent]
public class EnemySpawningSys : KHManagedBehaviour
{
    #region FIELDS

    [KHResetStatic]
    public static EnemySpawningSys Ins { get; private set; }

    public event Action OnFinishedSpawning;

    private bool startedSpawning;

    // INSPECTOR

    [Tab("Stats")]
    public bool doneSpawning = false;
    [Tab("Waves")]
    [SerializeField] private List<WaveData> waves;

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
    private void StartSpawningEnemies()
    {
        if (startedSpawning)
            return;

        startedSpawning = true;
        StartCoroutine(SpawnWavesCoroutine());
    }

    private void CheckEnemyPathsCount(int i)
    {
        WaveData wave = waves[i];

        if (wave.enemyPaths.Count > PathSys.InsEditor.pathStartCells.Count)
            Debug.LogError($"There Are More {nameof(wave.enemyPaths)} Than The {nameof(PathSys.InsEditor.pathStartCells)} in {wave.name}".AddColorTag(KHUtils.XMLColors.Red));
        else if (wave.enemyPaths.Count < PathSys.InsEditor.pathStartCells.Count)
            Debug.LogError($"There Are Less {nameof(wave.enemyPaths)} Than The {nameof(PathSys.InsEditor.pathStartCells)} in {wave.name}".AddColorTag(KHUtils.XMLColors.Red));
    }

    private IEnumerator SpawnWavesCoroutine()
    {
        foreach (WaveData wave in waves)
        {
            if (wave.startDelay > 0f)
                yield return new WaitForSeconds(wave.startDelay);

            int remainingPaths = wave.enemyPaths.Count;

            for (int pathIndex = 0; pathIndex < wave.enemyPaths.Count; pathIndex++)
            {
                EnemyPathData enemyPath = wave.enemyPaths[pathIndex];

                StartCoroutine(SpawnPathCoroutine(enemyPath,
                                                  pathIndex,
                                                  () => remainingPaths--)
                );
            }

            // Wait until every path has finished.
            yield return new WaitUntil(() => remainingPaths <= 0);
        }

        OnFinishedSpawning?.Invoke();
        doneSpawning = true;
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
}



#region WaveData

[Serializable]
public class WaveData
{
#if UNITY_EDITOR
    [HideInInspector] public string name;

    [SerializeField, TextArea(2, 10)]
    private string description;
#endif

    [Space, Space]

    [Tooltip("Amount of time before this wave starts.")]
    [Range(0.5f, 10f)]
    public float startDelay;

    [Space, Space]

    [Tooltip("Amount Of Paths Is Automatically Changed Based On Path System's Start Cells Count.")]
    public List<EnemyPathData> enemyPaths = new();
}

#endregion
#region EntryData

[Serializable]
public class EntryData
{
#if UNITY_EDITOR
    [HideInInspector] public string name;

    [SerializeField, TextArea(2, 10)]
    private string description;
#endif

    [Space, Space]

    public EnemyData enemyData;

    [Space, Space]

    [Tooltip("Amount of time before this entry starts.")]
    [Range(0.5f, 10f)]
    public float startDelay;

    [Tooltip("Repeats The Same Entry, Instead Of Duplicates")]
    [Min(1)] public int repeatCount = 1;

    [Tooltip("Delay between each repetition.")]
    [Range(0.5f, 10f)]
    public float repeatDelay = 1f;



}

#endregion
#region EnemyPathData

[Serializable]
public class EnemyPathData
{
#if UNITY_EDITOR
    [HideInInspector] public string name;
#endif

    public List<EntryData> entries = new();
}

#endregion