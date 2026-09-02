using KH;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DoubleSliders : KHManagedBehaviour
{
    #region FIELDS

    private const int MAX_ADDITIONAL_WIDTH = 200;
    private const float WHITE_DELAY = 0.5f;
    private const float WHITE_DURATION = 0.5f;

    private RectTransform rectTransform;
    private float originalWidth;
    private Tween whiteTween;

    [SerializeField] private Slider mainGreen;
    [SerializeField] private Slider secondaryWhite;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        ResetSliders();

        originalWidth = rectTransform.sizeDelta.x;
    }

    #endregion
    #region PUBLIC

    public void ResetSliders()
    {
        mainGreen.value = 1f;
        secondaryWhite.value = 1f;
    }

    public void ChangeValue(float currentValue, int maxValue)
    {
        float targetValue = currentValue / maxValue;

        mainGreen.value = targetValue;

        if (whiteTween.isAlive)
            whiteTween.Stop();

        Tween.Custom(startValue: secondaryWhite.value,
                     endValue: targetValue,
                     duration: WHITE_DURATION,
                     onValueChange: newVal => secondaryWhite.value = newVal,
                     startDelay: WHITE_DELAY);
    }

    public void IncreaseWidthBasedOnHealth(int maxHealth)
    {
        int lowest = DB.GetLowestEnemyMaxHealth();
        int highest = DB.GetHighestEnemyMaxHealth();

        // 0 when maxHealth == lowest, 1 when maxHealth == highest (clamped in between)
        float t = Mathf.InverseLerp(lowest, highest, maxHealth);

        float additionalWidth = Mathf.Lerp(0f, MAX_ADDITIONAL_WIDTH, t);

        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = originalWidth + additionalWidth;
        rectTransform.sizeDelta = sizeDelta;
    }

    #endregion
}