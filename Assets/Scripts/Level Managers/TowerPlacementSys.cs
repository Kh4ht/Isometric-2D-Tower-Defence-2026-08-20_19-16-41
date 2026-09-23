using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using KH;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// Manages the tower placement workflow, including hover previews, selection,
/// validation, and instantiating towers on valid map cells.
/// </summary>
[DisallowMultipleComponent]
public class TowerPlacementSys : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    [KHResetStatic]
    public static TowerPlacementSys Ins { get; private set; }

    private GridNode gridNodeMousePointingAt;

    // INSPECTOR
    [SerializeField] private LineRenderer secondaryTowerRangeIndicator;
    [SerializeField] private SpriteRenderer secondaryTowerSpriteIndicator;

    public SelectedCells cells = new();

    [Space(20)]

    [SerializeField] private List<MouseHoverEffect> mouseHoverEffect = new();

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogError($"More Than One Instance of type {nameof(TowerPlacementSys)}".AddColorTag(KHUtils.XMLColors.Red));

        secondaryTowerRangeIndicator.useWorldSpace = true;
        secondaryTowerRangeIndicator.loop = true;
        DisableSecondaryTowerRangeIndicator();
    }

    protected override void Start()
    {
        base.Start();

        RegisterSelectedTowersToPool();
    }

    public void KHUpdate()
    {
        if (LevelManager.Ins.LevelPaused)
            return;

        RunMouseAndTowerPlacementLogic();
    }

    #endregion
    #region PRIVATE

    private void RegisterSelectedTowersToPool()
    {
        SaveData saveD = KHSaveSystem.Load<SaveData>();

        foreach (TowerData td in saveD.GetSelectedTowerDatas())
        {
            KHPoolManager.Ins.Register(td.ID, td.prefab);
        }
    }

    private void RunMouseAndTowerPlacementLogic()
    {
        if (Kh.IsMouseOverUI())
            return;

        gridNodeMousePointingAt = Helper.GetNodeMouseIsPointingAt();

        // UpdateHoveredCells() is the guard that prevents redundant hover logic when the cursor is still on
        // the same grid cells. The callback only executes when the hovered selection actually changes,
        // so MouseHoverLogic() and related work are not called every frame for the same tile.
        cells.UpdateHoveredCells(
            newHoveredCells: Helper.GetHoveredCells().ToList(),
            onHoverCellsUpdated: () =>
            {
                OnMouseHover();
            });

        if (!cells.IsSelected)
        {
            // Hide mouse hover effect if pointing at non tower placable
            if (gridNodeMousePointingAt == null || gridNodeMousePointingAt.IsDecoration)
                MouseHoverEffect.EnableSpriteRenderer(mouseHoverEffect, false);
            else
                MouseHoverEffect.EnableSpriteRenderer(mouseHoverEffect, true);
        }

        // when pointing at a blocked cell, move the hover effect
        if (gridNodeMousePointingAt != null && gridNodeMousePointingAt.IsBlocked)
            OnMouseHover();

        OnMouseClick();
    }

    private void OnMouseClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame || gridNodeMousePointingAt == null)
            return;

        if (gridNodeMousePointingAt.IsDecoration && cells.IsSelected)
        {
            cells.Deselect();
            return;
        }


        // If the node under the mouse is already blocked
        if (gridNodeMousePointingAt.IsBlocked)
        {
            cells.Select(gridNodeMousePointingAt.GetTower());
            return;
        }

        if (!PathSys.Ins.CanPlaceTowerOnCells(cells.hoveredCells)
            || PathSys.Ins.WillBlockEnemyPath(cells.hoveredCells))
        {
            // TODO: little feedback or rejection sound effect.

            cells.Deselect();
            return;
        }

        cells.Select();

        if (cells.IsSelected)
            MouseHoverEffect.SetColors(mouseHoverEffect, MouseHoverEffect.EffectColor.GreenSelected);
    }

    private void OnMouseHover()
    {
        if (cells.IsSelected || gridNodeMousePointingAt == null)
            return;

        // If the node under the mouse is already blocked, show a blue hover indicator
        if (gridNodeMousePointingAt.IsBlocked)
        {
            for (int i = 0; i < Mathf.Min(cells.hoveredCells.Count, mouseHoverEffect.Count); i++)
            {
                mouseHoverEffect[i].Move(gridNodeMousePointingAt.GetTower().stats.GetOccupiedCells()[i].GetCellCenterWorld());
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Blue);
            }

            return;
        }

        bool willBlockPath = PathSys.Ins.WillBlockEnemyPath(cells.hoveredCells);

        for (int i = 0; i < Mathf.Min(cells.hoveredCells.Count, mouseHoverEffect.Count); i++)
        {
            mouseHoverEffect[i].Move(cells.hoveredCells[i].GetCellCenterWorld());

            if (willBlockPath)
            {
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Red);
                continue;
            }

            if (PathSys.Ins.IsTowerPlacable(cells.hoveredCells[i]))
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Green);
            else
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Red);
        }
    }

    private void DrawSecondaryRangeCircle(Vector2 origin, float radius, ElementType elementType)
    {
        if (radius <= 0f)
        {
            DisableSecondaryTowerRangeIndicator();
            return;
        }

        secondaryTowerRangeIndicator.enabled = true;
        secondaryTowerRangeIndicator.colorGradient = DB.GetTowerRangeIndicatorGradient(elementType);
        secondaryTowerRangeIndicator.positionCount = GameConsts.TOWER_RANGE_SEGMENTS;

        for (int i = 0; i < GameConsts.TOWER_RANGE_SEGMENTS; i++)
            secondaryTowerRangeIndicator.SetPosition(i, Helper.TileCircleToWorld(origin, radius, i));
    }

    #endregion
    #region PUBLIC

    /// <summary>Preview an already-placed tower's range at its NEXT level (used while hovering the Upgrade button).</summary>
    public void EnableSecondaryTowerRangeIndicator(Tower tower)
    {
        if (tower == null)
        {
            DisableSecondaryTowerRangeIndicator();
            return;
        }

        int nextLvl = Mathf.Min(tower.stats.GetLvl() + 1, tower.data.range.Count - 1);

        DrawSecondaryRangeCircle(tower.transform.position, tower.data.range[nextLvl], tower.data.elementType);
    }

    /// <summary>Preview a not-yet-placed tower's range at the currently selected cell (used while hovering a Buy button).</summary>
    public void EnableSecondaryTowerRangeIndicator(TowerData towerData)
    {
        if (towerData == null || !cells.IsSelected)
        {
            DisableSecondaryTowerRangeIndicator();
            return;
        }

        DrawSecondaryRangeCircle(cells.GetCenterWorld(), towerData.range[0], towerData.elementType);
    }

    public void DisableSecondaryTowerRangeIndicator()
    {
        secondaryTowerRangeIndicator.positionCount = 0;
        secondaryTowerRangeIndicator.enabled = false;
    }

    public void PlaceTowerOnSelectedPos(TowerData towerData)
    {
        if (!cells.IsSelected)
        {
            cells.Deselect();
            return;
        }

        cells.Deselect();

        Vector2 towerPos = cells.GetCenterWorld();

        KHPoolManager.Ins.Spawn<Tower>(towerData.ID, towerPos, Quaternion.identity).ResetTower(cells.selectedCells);
    }

    #endregion
}