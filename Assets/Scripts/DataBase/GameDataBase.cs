using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameDataBase", menuName = "Scriptable Objects/GameDataBase")]
public class GameDataBase : ScriptableObject
{
    public List<TowerData> towerDatas;
    public List<EnemyData> enemyDatas;

    private void OnValidate()
    {
        towerDatas.KHAutoFillDataBase();
        enemyDatas.KHAutoFillDataBase();
    }
}




public static class DB
{
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

    public static List<TowerData> TowersDB => Db.towerDatas;
    public static List<EnemyData> EnemiesDB => Db.enemyDatas;

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
}