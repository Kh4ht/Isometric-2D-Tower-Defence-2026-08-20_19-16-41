using KH;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class TowerSelector : KHManagedBehaviour
{
    #region FIELDS
    public TowerData towerData;

    #endregion
    #region UNITY EVENTS

    private void Reset()
    {
        if (towerData != null)
        {
            name = towerData.name;
        }

        if (towerData != null && towerData.icons != null && towerData.icons.Count > 0)
        {
            GetComponent<Image>().sprite = towerData.icons[0];
        }
    }

    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SaveSelectedTower);
    }

    private void OnValidate()
    {
        if (towerData != null)
            name = towerData.name;

        if (towerData.icons != null && towerData.icons.Count > 0)
            GetComponent<Image>().sprite = towerData.icons[0];
    }

    #endregion
    #region PUBLIC

    public void SaveSelectedTower()
    {
        SaveData saveData = KHSaveSystem.Load<SaveData>();

        saveData.SelectTower(towerData.ID);

        KHSaveSystem.Save(saveData);
    }

    #endregion
}