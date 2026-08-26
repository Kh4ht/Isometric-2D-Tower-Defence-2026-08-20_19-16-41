using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public List<Vector2Int> pathStartCells;
    public Vector2Int pathTargetCell;
}
