using KH;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class LevelTowerButton : KHManagedBehaviour
{
    #region FIELDS

    [HideInInspector]
    public TowerData towerData;

    #endregion
    #region UNITY EVENTS

    protected override void Start()
    {
        base.Start();

        GetComponent<Button>().onClick.AddListener(() => TowerPlacementSys.Ins.PlaceTowerOnSelectedPos(towerData));
    }

    #endregion
}