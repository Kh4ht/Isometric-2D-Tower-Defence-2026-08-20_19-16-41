using System.Collections.Generic;
using System.Linq;
using MyHelper;
using KH;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using UnityEngine.Rendering;

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
            Debug.LogWarning("More Than One Instance");
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

        // Optimization: we only resolve the hovered cell set from the mouse position once per frame,
        // but we do not run the expensive hover/placement validation logic unless the hovered cells changed.
        // This avoids reprocessing the same selection every frame while the cursor is stationary.
        List<Vector2Int> newHoveredCells = Helper.GetHoveredCells().ToList();

        GridNode gridNodeMousePointingAt = Helper.GetNodeMouseIsPointingAt();

        if (!cells.Selected && gridNodeMousePointingAt.IsBlocked)
        {
            MouseHoverLogic(newHoveredCells, gridNodeMousePointingAt);
        }

        // UpdateHoveredCells() is the guard that prevents redundant hover logic when the cursor is still on
        // the same grid cells. The callback only executes when the hovered selection actually changes,
        // so MouseHoverLogic() and related work are not called every frame for the same tile.
        cells.UpdateHoveredCells(newHoveredCells, () =>
        {
            if (!cells.Selected)
                MouseHoverLogic(cells.hoveredCells, gridNodeMousePointingAt);
        });

        MouseClickLogic(newHoveredCells);
    }

    private void MouseClickLogic(List<Vector2Int> hoveredCells)
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        GridNode pointingAtNode = Helper.GetNodeMouseIsPointingAt();

        // If the node under the mouse is already blocked
        if (pointingAtNode.IsBlocked)
        {
            cells.SelectTower(pointingAtNode.Tower);
            return;
        }

        if (!PathSys.Ins.ValidateTowerPlacementCells(hoveredCells)
            || PathSys.Ins.WillBlockEnemyPath(hoveredCells))
        {
            // TODO: little feedback or rejection sound effect.

            cells.Deselect();
            return;
        }

        cells.SelectHovered();

        if (cells.Selected)
            MouseHoverEffect.SetColors(mouseHoverEffect, MouseHoverEffect.EffectColor.GreenSelected);
    }

    private void MouseHoverLogic(List<Vector2Int> hoveredCells, GridNode pointingAtNode)
    {
        // If the node under the mouse is already blocked, show a blue hover indicator
        if (pointingAtNode.IsBlocked)
        {
            for (int i = 0; i < Mathf.Min(hoveredCells.Count, mouseHoverEffect.Count); i++)
            {
                mouseHoverEffect[i].Move(pointingAtNode.Tower.transform.position);
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Blue);
            }

            return;
        }

        bool willBlockPath = PathSys.Ins.WillBlockEnemyPath(hoveredCells);

        for (int i = 0; i < Mathf.Min(hoveredCells.Count, mouseHoverEffect.Count); i++)
        {
            mouseHoverEffect[i].Move(PathSys.Ins.gameGrid.GetCellCenterWorld(hoveredCells[i]));

            if (willBlockPath)
            {
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Red);
                continue;
            }

            if (PathSys.Ins.IsTowerPlacable(hoveredCells[i]))
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Green);
            else
                mouseHoverEffect[i].SetColor(MouseHoverEffect.EffectColor.Red);
        }
    }

    #endregion
    #region PUBLIC

    public void PlaceTowerOnSelectedPos(TowerData towerData)
    {
        if (!cells.Selected)
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