using System.Collections.Generic;
using UnityEngine;

public static class DB
{
    #region FIELDS
    private static GameDataBase _db;

    public static GameDataBase Db
    {
        get
        {
            if (_db != null)
                return _db;

            // Load from Resources folder (recommended)
            _db = Resources.Load<GameDataBase>(nameof(GameDataBase));

            if (_db == null)
                Debug.LogError($"DB: Could not load {nameof(GameDataBase)} from Resources!");

            return _db;
        }
    }

    // CONSTS
    public const float COLOR_TWEEN_DURATION = 0.1f;

    // GETTERS
    public static List<TowerData> TowersDB => Db.towerDatas;
    public static List<EnemyData> EnemiesDB => Db.enemyDatas;

    #endregion
    #region PUBLIC

    public static Gradient GetTowerRangeIndicatorGradient(ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Fire => Db.fireTowerRangeIndicator,
            ElementType.Water => Db.WaterTowerRangeIndicator,
            ElementType.Earth => Db.EarthTowerRangeIndicator,
            _ => new Gradient(),
        };
    }

    public static TowerData GetTowerDataById(string id)
    {
        return TowersDB.Find(t => id == t.ID);
    }

    public static int GetHighestEnemyMaxHealth()
    {
        int highestMaxHealth = 0;

        foreach (EnemyData enemyData in EnemiesDB)
        {
            if (enemyData.defaultMaxHealth > highestMaxHealth)
                highestMaxHealth = enemyData.defaultMaxHealth;
        }

        return highestMaxHealth;
    }

    public static int GetLowestEnemyMaxHealth()
    {
        int lowestMaxHealth = int.MaxValue;

        foreach (EnemyData enemyData in EnemiesDB)
        {
            if (enemyData.defaultMaxHealth < lowestMaxHealth)
                lowestMaxHealth = enemyData.defaultMaxHealth;
        }

        return lowestMaxHealth;
    }

    #endregion
}