using System.Collections.Generic;
using MyHelper;
using PrimeTween;
using UnityEngine;

public class TowerAnimatorSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Tower owner;

    private readonly List<Vector2> lineRendererPositions = new();
    private int loopIndex;
    private float lastT = 0f;

    private const float RANGE_ANIM_DURATION = 0.1f;
    private const float SNAKE_ANIM_DURATION = 0.1f;

    private Tween rangeTween;
    private Tween snakeTween;

    #endregion
    #region CONSTRUCTOR

    public TowerAnimatorSubSys(Tower owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IOnDisable()
    {
        // REMOVE LISTENERS
        owner.stats.OnRangeChanged -= OnRangeChanged;
        owner.OnTowerSelected -= OnTowerSelected;

        rangeTween.Stop();
        snakeTween.Stop();
    }

    #endregion
    #region PRIVATE

    private void OnRangeChanged(float range)
    {
        // Range changed from outside (e.g. upgrade) — snap, don't animate.
        rangeTween.Stop();
        DrawRangeIndicator(range);
    }

    private void OnTowerSelected(bool selected)
    {
        float targetRadius = selected ? owner.stats.GetRange() : 0f;
        float startRadius = selected ? 0f : owner.stats.GetRange();

        if (selected)
            owner.lineRenderer.enabled = true;
        else
            snakeTween.Stop();

        snakeTween.Stop();
        rangeTween.Stop();
        rangeTween = Tween.Custom(startValue: startRadius,
                                  endValue: targetRadius,
                                  duration: RANGE_ANIM_DURATION,
                                  onValueChange: radius =>
                                  {
                                      DrawRangeIndicator(radius);
                                  },
                                  ease: Ease.InOutSine)
                                  .OnComplete(() =>
                                  {
                                      if (!selected)
                                      {
                                          owner.lineRenderer.enabled = false;
                                      }
                                      else
                                      {
                                          RunSnakeTween();
                                      }
                                  });


    }

    private void RunSnakeTween()
    {
        snakeTween.Stop();

        loopIndex = 0;
        lastT = 0f;

        snakeTween = Tween.Custom(startValue: 0f,
                                  endValue: 1f,
                                  duration: SNAKE_ANIM_DURATION,
                                  cycles: -1,
                                  onValueChange: (t) =>
                                  {
                                      ChangeVerticesPos(t);
                                  });
    }

    private void ChangeVerticesPos(float t)
    {
        int count = lineRendererPositions.Count;

        if (count == 0)
            return;

        // A new lap started when t drops back down instead of continuing to climb.
        if (t < lastT)
            loopIndex = (loopIndex + 1) % count;

        lastT = t;

        for (int i = 0; i < owner.lineRenderer.positionCount; i++)
        {
            int idx = (i + loopIndex) % count;
            int nextIdx = (idx + 1) % count;

            owner.lineRenderer.SetPosition(i, Vector2.Lerp(lineRendererPositions[idx], lineRendererPositions[nextIdx], t));
        }
    }

    private void DrawRangeIndicator(float radius)
    {
        if (radius <= 0f)
        {
            owner.lineRenderer.positionCount = 0;
            return;
        }

        Vector2 origin = owner.transform.position;
        owner.lineRenderer.positionCount = GameConsts.TOWER_RANGE_SEGMENTS;

        lineRendererPositions.Clear();

        for (int i = 0; i < owner.lineRenderer.positionCount; i++)
        {
            Vector2 point = Helper.TileCircleToWorld(origin, radius, i);
            owner.lineRenderer.SetPosition(i, point);
            lineRendererPositions.Add(point);
        }
    }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        // ADD LISTENERS
        owner.stats.OnRangeChanged += OnRangeChanged;
        owner.OnTowerSelected += OnTowerSelected;

        owner.lineRenderer.useWorldSpace = true;
        owner.lineRenderer.loop = true;

        DrawRangeIndicator(0f); // make sure a pooled tower's old line is cleared too
        lineRendererPositions.Clear();
        loopIndex = 0;
    }

    #endregion
}