using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneTransitionManager : MonoBehaviour
{
    #region FIELDS

    public static SceneTransitionManager Ins { get; private set; }
    private const float FADE_DURATION = 0.2f;

    //
    private bool isTransitioning;

    // INSPECTOR
    [SerializeField] private CanvasGroup sceneTransitionsBg;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogWarning("More Than One Instance");

        DontDestroyOnLoad(this);
    }

    #endregion
    #region PRIVATE

    private IEnumerator SceneTransition(string sceneName, int sceneIndex, bool useIndex)
    {
        isTransitioning = true;
        sceneTransitionsBg.blocksRaycasts = true;

        // Fade to black (unscaled, so this works even at Time.timeScale == 0)
        yield return Fade(0f, 1f).ToYieldInstruction();

        // Load scene
        if (useIndex)
            SceneManager.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneName);

        // Wait one frame so the new scene is loaded
        yield return null;

        // Fade from black
        yield return Fade(1f, 0f).ToYieldInstruction();

        isTransitioning = false;
        sceneTransitionsBg.blocksRaycasts = false;
    }

    private Tween Fade(float from, float to)
    {
        sceneTransitionsBg.alpha = from;
        return Tween.Alpha(sceneTransitionsBg, to, FADE_DURATION, useUnscaledTime: true);
    }

    #endregion
    #region PUBLIC

    public void LoadScene(string name)
    {
        if (isTransitioning)
            return;

        StartCoroutine(SceneTransition(name, -1, false));
    }

    public void LoadScene(int buildIndex)
    {
        if (isTransitioning)
            return;

        StartCoroutine(SceneTransition(null, buildIndex, true));
    }

    public void LoadMainMenu()
    {
        LoadScene(GameScenes.MAIN_MENU);
    }

    public void ReloadScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
}
