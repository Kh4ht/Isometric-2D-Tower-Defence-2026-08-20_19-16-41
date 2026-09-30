using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPathData
{
    #region FIELDS

#if UNITY_EDITOR
    [HideInInspector] public string name;
#endif

    public List<EntryData> entries = new();

    #endregion
}
