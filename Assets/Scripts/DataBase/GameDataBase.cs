using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameDataBase", menuName = "Scriptable Objects/GameDataBase")]
public class GameDataBase : ScriptableObject
{
    #region FIELDS

    public Sprite upgradeTowerImage, sellTowerImage, targetOptionsImage;

    [Space(20)]
    public Color coinAcceptanceColor;
    public Color coinRejectionColor;

    [Space(20)]
    public Color towerOptionNormalColor;
    public Color towerOptionHighlightedColor;
    public Color towerOptionSelectedColor;
    public Color towerOptionDisabledColor;

    [Space(20)]
    public Gradient fireTowerRangeIndicator;
    public Gradient WaterTowerRangeIndicator;
    public Gradient EarthTowerRangeIndicator;

    [Space(20)]
    public List<TowerData> towerDatas;
    public List<EnemyData> enemyDatas;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        towerDatas.KHAutoFillDataBase();
        enemyDatas.KHAutoFillDataBase();
    }

    #endregion
}