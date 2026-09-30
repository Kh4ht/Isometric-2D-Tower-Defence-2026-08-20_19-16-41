using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    #region FIELDS

#if UNITY_EDITOR
    [HideInInspector] public string name;

    [SerializeField, TextArea(2, 10)]
    private string description;
#endif

    [Space, Space]

    [Tooltip("Amount of time before this wave starts.")]
    [Range(0.5f, 10f)]
    public float startDelay;

    [Space, Space]

    [Tooltip("Amount Of Paths Is Automatically Changed Based On Path System's Start Cells Count.")]
    public List<EnemyPathData> enemyPaths = new();

    #endregion
}
