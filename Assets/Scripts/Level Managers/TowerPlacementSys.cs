using System.Collections.Generic;
using System.Linq;
using MyClasses;
using MyHelper;
using KH;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

[DisallowMultipleComponent]
public class TowerPlacementSys : KHManagedBehaviour, IKHManagedUpdate
{
    #region FIELDS

    public static TowerPlacementSys Ins { get; private set; }

    private readonly SelectedCells selectedCells = new();

    private List<Vector2Int> hoveredCells;

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
            Debug.LogWarning("More Than One Instance");
    }

    public void ManagedUpdate()
    {
        RunMouseAndTowerPlacementLogic();
    }

    #endregion
    #region PRIVATE

    private void RunMouseAndTowerPlacementLogic()
    {
        if (Kh.IsMouseOverUI())
            return;

        // TODO: exit if mouse is pointing at a Tower or a UI element.

        List<Vector2Int> newHoveredCells = Helper.GetHoveredCells(PathSys.Ins.walkableTilemap,
                                                              (Vector2Int)PathSys.Ins.WorldToCell(Kh.GetMouseWorldPos())).ToList();

        if (hoveredCells == null
            || !hoveredCells.SequenceEqual(newHoveredCells)
            || !selectedCells.selected)
        {
            hoveredCells = newHoveredCells;

            if (!selectedCells.selected)
                DrawMouseHoverShadow(hoveredCells);
        }

        MouseClickLogic(newHoveredCells);
    }

    private void MouseClickLogic(List<Vector2Int> hoveredCells)
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!PathSys.Ins.ValidateTowerPlacementCells(hoveredCells)
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
            mouseHoverShadow[i].shadow.transform.position = PathSys.Ins.GetCellCenterWorld(hoveredCells[i]);

            mouseHoverShadow[i].spriteRenderer.color = PathSys.Ins.ValidateTowerPlacementCell(hoveredCells[i])
                ? new Color(0f, 1f, 0f, 0.2f)
                : new Color(1f, 0f, 0f, 0.2f);
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
                    selectedCells.GetCenterWorld(PathSys.Ins.walkableTilemap),
                    Quaternion.identity);
    }

    #endregion
}