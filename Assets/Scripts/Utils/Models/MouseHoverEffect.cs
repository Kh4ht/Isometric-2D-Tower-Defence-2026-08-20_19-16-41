using System;
using System.Collections.Generic;
using KH;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MouseHoverEffect : KHManagedBehaviour
{
    #region FIELDS
    public enum EffectColor
    {
        Red,
        Green,
        GreenSelected,
        Blue,
    }

    private const float MOVE_DURATION = 0.05f;
    private const float COLOR_TWEEN_DURATION = 0.1f;

    private EffectColor? currentColor;
    private Vector2? currentTargetPos;
    private SpriteRenderer spriteRenderer;
    private Tween moveTween;
    private Tween colorTween;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Tween.Scale(target: transform,
                    startValue: 1f,
                    endValue: 0.9f,
                    duration: 1,
                    cycles: -1,
                    cycleMode: CycleMode.Yoyo);
    }

    #endregion
    #region PUBLIC

    public void SetColor(EffectColor shadowColor)
    {
        if (currentColor == shadowColor)
            return;

        currentColor = shadowColor;

        Color color = shadowColor switch
        {
            EffectColor.Red => new Color(1f, 0f, 0f, 0.15f),
            EffectColor.Green => new Color(0f, 1f, 0f, 0.15f),
            EffectColor.GreenSelected => new Color(0f, 1f, 0f, 0.25f),
            EffectColor.Blue => new Color(0f, 0f, 1f, 0.15f),
            _ => Color.white
        };

        colorTween.Stop();
        colorTween = Tween.Color(target: spriteRenderer,
                                 endValue: color,
                                 duration: COLOR_TWEEN_DURATION,
                                 useUnscaledTime: true);
    }

    #endregion
    #region STATIC

    public static void SetColors(List<MouseHoverEffect> shadowColors, EffectColor shadowColor)
    {
        foreach (var shadow in shadowColors)
            shadow.SetColor(shadowColor);
    }

    public void Move(Vector2 targetPos)
    {
        if (currentTargetPos.HasValue && currentTargetPos.Value == targetPos)
            return;

        currentTargetPos = targetPos;

        moveTween.Stop();
        moveTween = Tween.Position(target: transform,
                                   endValue: targetPos,
                                   duration: MOVE_DURATION,
                                   useUnscaledTime: true);
    }

    #endregion
}