using System.Collections.Generic;
using System.Linq;
using MyHelper;
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

    [Tab("UI Controller")]

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

    #endregion
    #region PUBLIC

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