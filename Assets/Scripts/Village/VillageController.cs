using System.Collections.Generic;
using KH;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class VillageManager : KHManagedBehaviour
{
    #region FIELDS

    public static VillageManager Ins { get; private set; }
    private const int MAX_VILLAGER_COUNT = 20;

    private readonly List<Vector3Int> villageCells = new();
    private Vector2 minWorldPosition;
    private Vector2 maxWorldPosition;

    // COMPONENTS
    private Tilemap villageAreaTilemap;

    public List<Villager> villagers = new();
    public event System.Action OnVillagerKidnapped;


    // INSPECTOR
    [SerializeField] private Villager villagerPrefab;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        villageAreaTilemap = GetComponent<Tilemap>();

        if (Ins == null)
            Ins = this;
        else
            Debug.LogWarning("More Than One Instance");
    }

    protected override void Start()
    {
        base.Start();

        CacheVillageCells();
        CacheVillageArea();
        SpawnVillagers();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameTags.ENEMY))
            return;

        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.ReachedVillagerArea();
            return;
        }
        else
            Debug.Log($"Component\'{nameof(Enemy)}\' is Not Found");
    }

    #endregion
    #region PRIVATE

    private void CacheVillageArea()
    {
        BoundsInt bounds = villageAreaTilemap.cellBounds;

        bool foundTile = false;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!villageAreaTilemap.HasTile(cell))
                continue;

            Vector3 worldPos = villageAreaTilemap.GetCellCenterWorld(cell);

            if (!foundTile)
            {
                minWorldPosition = worldPos;
                maxWorldPosition = worldPos;
                foundTile = true;
                continue;
            }

            minWorldPosition.x = Mathf.Min(minWorldPosition.x, worldPos.x);
            minWorldPosition.y = Mathf.Min(minWorldPosition.y, worldPos.y);

            maxWorldPosition.x = Mathf.Max(maxWorldPosition.x, worldPos.x);
            maxWorldPosition.y = Mathf.Max(maxWorldPosition.y, worldPos.y);
        }
    }

    private void CacheVillageCells()
    {
        villageCells.Clear();

        BoundsInt bounds = villageAreaTilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!villageAreaTilemap.HasTile(cell))
                continue;

            villageCells.Add(cell);
        }
    }

    private Vector3 GetRandomVillagePosition()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = Random.Range(minWorldPosition.x, maxWorldPosition.x);
            float y = Random.Range(minWorldPosition.y, maxWorldPosition.y);

            Vector3 position = new(x, y, 0f);

            Vector3Int cell = villageAreaTilemap.WorldToCell(position);

            if (villageAreaTilemap.HasTile(cell))
            {
                return position;
            }
        }

        Debug.LogWarning("Could not find a valid village position.");

        return villageAreaTilemap.GetCellCenterWorld(villageCells[0]);
    }

    private void SpawnVillagers()
    {
        // Vector2 spawnPos = GetRandomVillagePosition();

        this.KHRunBatched(count: MAX_VILLAGER_COUNT,
                          action: (i) => villagers.Add(Instantiate(villagerPrefab, GetRandomVillagePosition(), Quaternion.identity)),
                          batchSize: 1);
    }

    #endregion
    #region PUBLIC

    public void KidnapVillager(Villager villager)
    {
        villagers.Remove(villager);

        OnVillagerKidnapped?.Invoke();

        villager.gameObject.SetActive(false);
    }

    public Villager GetNearestVillager(Vector2 pos)
    {
        float nearestSqrDis = float.MaxValue;
        Villager nearestVillager = null;

        foreach (Villager v in villagers)
        {
            if (Kh.GetSqrDistance(pos, v.transform.position) < nearestSqrDis)
            {
                nearestSqrDis = Kh.GetSqrDistance(pos, v.transform.position);
                nearestVillager = v;
            }
        }

        return nearestVillager;
    }

    #endregion
}