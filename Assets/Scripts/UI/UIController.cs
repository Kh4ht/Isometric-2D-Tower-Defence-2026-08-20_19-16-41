using System.Collections;
using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
public class UIController : ManagedBehaviour
{
    #region Fields

    private static readonly WaitForSeconds waitTime = new(0.5f);
    public const float UI_SHOW_SPEED = 0.25f;
    public const float UI_HIDE_SPEED = 0.2f;

    private Vector3 _startScale;
    private Vector3 _startPos;

    // Components
    private CanvasGroup canvasGroup;
    private RectTransform parentCanvasRect;
    private AudioSource audioSource;

    // Tween
    private Tween activeTween;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        _startScale = transform.localScale;
        _startPos = transform.localPosition;

        if (TryGetComponent(out CanvasGroup canvasG))
        {
            canvasGroup = canvasG;
            parentCanvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
    }

    #endregion
    #region PRIVATE

    private void RestoreDefaults(bool wait1Sec = false)
    {
        transform.localScale = _startScale;
        transform.localPosition = _startPos;

        canvasGroup.alpha = 1;

        if (wait1Sec)
        {
            StartCoroutine(DelayedInteractable());
        }
        else
        {
            canvasGroup.interactable = true;
        }
    }

    private IEnumerator DelayedInteractable()
    {
        yield return waitTime;
        canvasGroup.interactable = true;
    }

    #endregion
    #region PUBLIC

    public void LoadStartScene()
    {
        // UIManager.Ins.LoadScene(GameScenes.START);
    }

    public void LoadPlayerScreenScene()
    {
        // // When pause level timescale == 0, so we need to make 1.
        // Time.timeScale = 1;

        // UIManager.Ins.LoadScene(GameScenes.PLAYER_SCREEN);
    }

    public void LoadPlayLevel()
    {
        // UIManager.Ins.LoadScene(PlayerPrefs.GetInt(GamePP.CURRENT_LEVEL, 1).ToString());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePauseLevel()
    {
        // LevelManager.Ins.TogglePauseLevel();
    }


    // TRANSITIONS & ANIMATIONS
    #region  POP

    public void KH_PopShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            canvasGroup.interactable = false;

            transform.localScale = Vector3.zero;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.Scale(
                transform,
                endValue: _startScale,
                duration: UI_SHOW_SPEED,
                ease: Ease.OutBack
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_PopHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.Scale(
                transform,
                endValue: Vector3.zero,
                duration: UI_HIDE_SPEED,
                ease: Ease.InBack
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_PopToggle()
    {
        KH_PopHide();
        KH_PopShow();
    }

    #endregion
    #region LEFT

    public void KH_LeftShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            transform.localPosition = _startPos + Vector3.left * parentCanvasRect.rect.width;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos,
                duration: UI_SHOW_SPEED,
                ease: Ease.OutCubic
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_LeftHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos + Vector3.left * parentCanvasRect.rect.width,
                duration: UI_HIDE_SPEED,
                ease: Ease.InCubic
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_LeftToggle()
    {
        KH_LeftHide();
        KH_LeftShow();
    }

    #endregion
    #region RIGHT

    public void KH_RightShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            canvasGroup.interactable = true;
            gameObject.SetActive(true);

            transform.localPosition = _startPos + Vector3.right * parentCanvasRect.rect.width;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos,
                duration: UI_SHOW_SPEED,
                ease: Ease.OutCubic
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_RightHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos + Vector3.right * parentCanvasRect.rect.width,
                duration: UI_HIDE_SPEED,
                ease: Ease.InCubic
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_RightToggle()
    {
        KH_RightHide();
        KH_RightShow();
    }

    #endregion
    #region UP

    public void KH_UpShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            canvasGroup.interactable = true;
            gameObject.SetActive(true);

            transform.localPosition = _startPos + Vector3.up * parentCanvasRect.rect.width;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos,
                duration: UI_SHOW_SPEED,
                ease: Ease.OutCubic
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_UpHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos + Vector3.up * parentCanvasRect.rect.width,
                duration: UI_HIDE_SPEED,
                ease: Ease.InCubic
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_UpToggle()
    {
        KH_UpHide();
        KH_UpShow();
    }

    #endregion
    #region DOWN

    public void KH_DownShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            canvasGroup.interactable = true;
            gameObject.SetActive(true);

            transform.localPosition = _startPos + Vector3.down * parentCanvasRect.rect.width;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos,
                duration: UI_SHOW_SPEED,
                ease: Ease.OutCubic
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_DownHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.LocalPosition(
                transform,
                endValue: _startPos + Vector3.down * parentCanvasRect.rect.width,
                duration: UI_HIDE_SPEED,
                ease: Ease.InCubic
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_DownToggle()
    {
        KH_DownHide();
        KH_DownShow();
    }

    #endregion
    #region FADE

    public void KH_FadeShow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.Alpha(
                canvasGroup,
                endValue: 1f,
                duration: UI_SHOW_SPEED
            ).OnComplete(() =>
            {
                RestoreDefaults(true);
            });
        }
    }

    public void KH_FadeHide()
    {
        if (gameObject.activeInHierarchy)
        {
            canvasGroup.interactable = false;

            if (activeTween.isAlive)
            {
                activeTween.Stop();
            }
            activeTween = Tween.Alpha(
                canvasGroup,
                endValue: 0f,
                duration: UI_HIDE_SPEED
            ).OnComplete(() =>
            {
                RestoreDefaults();
                gameObject.SetActive(false);
            });
        }
    }

    public void KH_FadeToggle()
    {
        KH_FadeHide();
        KH_FadeShow();
    }

    #endregion

    #endregion
}