using System.Collections.Generic;
using System.Linq;
using MyClasses;
using MyHelper;
using KH;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using Unity.Mathematics;

public class TowerPlacementSys : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    public static TowerPlacementSys Ins { get; private set; }

    private readonly SelectedCells selectedCells = new();

    // INSPECTOR

    [Foldout("UI Controller")]
    [SerializeField] private KHUIController horizontalTowersContainer;
    [EndFoldout]

    [SerializeField] private List<MouseHoverShadow> mouseHoverShadow = new();

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Destroy(gameObject);
    }

    public void ManagedUpdate()
    {
        RunMouseAndPlacementLogic();
    }

    #endregion
    #region PRIVATE

    private void RunMouseAndPlacementLogic()
    {
        if (Kh.IsMouseOverUI())
            return;

        // TODO: exit if mouse is pointing at a Tower or a UI element.

        List<Vector2Int> hoveredCells = Helper.GetHoveredCells(LevelManager.Ins.walkableTilemap,
                                                              (Vector2Int)LevelManager.Ins.walkableTilemap.WorldToCell(Kh.GetMouseWorldPos())).ToList();

        if (!selectedCells.selected)
            DrawMouseHoverShadow(hoveredCells);

        MouseClickLogic(hoveredCells);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            selectedCells.Deselect(horizontalTowersContainer);
    }

    private void MouseClickLogic(List<Vector2Int> hoveredCells)
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!LevelManager.Ins.ValidateTowerPlacementCells(hoveredCells)
            || !PathSys.Ins.CanBlockCells(hoveredCells))
        {
            // TODO: little feedback or rejection sound effect.

            selectedCells.Deselect(horizontalTowersContainer);
            return;
        }

        selectedCells.ToggleSelect(hoveredCells, horizontalTowersContainer);
    }

    private void DrawMouseHoverShadow(List<Vector2Int> hoveredCells)
    {
        for (int i = 0; i < Mathf.Min(hoveredCells.Count, mouseHoverShadow.Count); i++)
        {
            mouseHoverShadow[i].shadow.SetActive(true);
            mouseHoverShadow[i].shadow.transform.position = LevelManager.Ins.CellToWorld(hoveredCells[i]);

            mouseHoverShadow[i].spriteRenderer.color = LevelManager.Ins.ValidateTowerPlacementCell(hoveredCells[i])
                ? new Color(0f, 1f, 0f, 0.25f)
                : new Color(1f, 0f, 0f, 0.25f);
        }
    }

    #endregion
    #region PUBLIC

    public void PlaceTowerOnSelectedPos(TowerData towerData)
    {
        if (!selectedCells.selected)
        {
            selectedCells.Deselect(horizontalTowersContainer);
            return;
        }

        PathSys.Ins.BlockCells(selectedCells.cells);

        selectedCells.Deselect(horizontalTowersContainer);

        Instantiate(towerData.prefab,
                    selectedCells.GetCenterWorld(LevelManager.Ins.walkableTilemap),
                    quaternion.identity);
    }

    #endregion
}