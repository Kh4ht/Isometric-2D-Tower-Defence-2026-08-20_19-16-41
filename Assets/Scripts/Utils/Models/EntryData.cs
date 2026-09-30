using System;
using UnityEngine;

[Serializable]
public class EntryData
{
    #region FIELDS

#if UNITY_EDITOR
    [HideInInspector] public string name;

    [SerializeField, TextArea(2, 10)]
    private string description;
#endif

    [Space, Space]

    public EnemyData enemyData;

    [Space, Space]

    [Tooltip("Amount of time before this entry starts.")]
    [Range(0.5f, 10f)]
    public float startDelay;

    [Tooltip("Repeats The Same Entry, Instead Of Duplicates")]
    [Min(1)] public int repeatCount = 1;

    [Tooltip("Delay between each repetition.")]
    [Range(0.5f, 10f)]
    public float repeatDelay = 1f;

    #endregion
}
