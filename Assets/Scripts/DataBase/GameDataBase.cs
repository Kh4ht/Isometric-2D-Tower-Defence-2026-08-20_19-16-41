using System.Collections.Generic;
using UnityEngine;
using VInspector;


[CreateAssetMenu(fileName = "GameDataBase", menuName = "Scriptable Objects/GameDataBase")]
public class GameDataBase : ScriptableObject
{
    #region FIELDS

    [Tab("Sprites")]

    public Sprite upgradeTowerImage, sellTowerImage, targetOptionsImage;

    [EndTab, Tab("Colors")]

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

    [EndTab, Tab("Datas")]

    [Space(20)]
    public List<TowerData> towerDatas;
    public List<EnemyData> enemyDatas;

    [EndTab]

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        towerDatas.KHAutoFillDataBase();
        enemyDatas.KHAutoFillDataBase();
    }

    #endregion
}