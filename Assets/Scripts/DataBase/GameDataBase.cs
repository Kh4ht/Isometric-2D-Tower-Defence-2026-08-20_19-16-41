using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

[CreateAssetMenu(fileName = "GameDataBase", menuName = "Scriptable Objects/GameDataBase")]
public class GameDataBase : ScriptableObject
{
    public List<TowerData> towerDatas;
    public List<LevelData> levelDatas;

    [Button]
    private void UpdateDataBase()
    {
        towerDatas.KHAutoFillDataBase();
        levelDatas.KHAutoFillDataBase();
    }

    private void OnValidate()
    {
        UpdateDataBase();
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
            _db = Resources.Load<GameDataBase>("GameDataBase");

            if (_db == null)
                Debug.LogError("DB: Could not load GameDataBase from Resources!");

            return _db;
        }
    }


    public static List<TowerData> TowersDB => Db.towerDatas;
    public static List<LevelData> LevelsDB => Db.levelDatas;

    public static TowerData GetTowerDataById(string id)
    {
        return TowersDB.Find(t => id == t.ID);
    }
}