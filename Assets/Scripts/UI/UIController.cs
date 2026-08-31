using KH;
using UnityEngine;

public class UIController : KHUIController
{
    #region PUBLIC

    public void SceneLoad(string name)
    {
        SceneTransitionManager.Ins.LoadScene(name);
    }

    public void SceneReload()
    {
        SceneTransitionManager.Ins.ReloadScene();
    }

    public void SceneLoadMainMenu()
    {
        SceneTransitionManager.Ins.LoadMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}