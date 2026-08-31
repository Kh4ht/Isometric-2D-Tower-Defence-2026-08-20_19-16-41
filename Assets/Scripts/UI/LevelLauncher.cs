using KH;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLauncher : KHManagedBehaviour
{
    #region FIELDS

    [SerializeField] private Button loadLevelBtn;
    [SerializeField] private TextMeshProUGUI titleTxt;

    #endregion
    #region PUBLIC

    public void SelectLevel(LevelData levelData)
    {
        loadLevelBtn.onClick.RemoveAllListeners();
        loadLevelBtn.onClick.AddListener(() => SceneTransitionManager.Ins.LoadScene(levelData.sceneName));

        titleTxt.text = levelData.sceneName;
    }

    #endregion
}