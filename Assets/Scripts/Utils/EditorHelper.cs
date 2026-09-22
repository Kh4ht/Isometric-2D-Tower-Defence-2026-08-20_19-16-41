#if UNITY_EDITOR

using System.Collections.Generic;
using KH;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class EditorHelper
    {
        public static void AutoSetListBasedOnFirstElement(this List<float> list, float multiplier)
        {
            list.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);

            for (int i = 1; i < list.Count; i++)
                list[i] = (list[i - 1] * multiplier).KHRoundToDecimalPlaces();
        }

        public static void AutoSetListBasedOnFirstElement(this List<int> list, float multiplier)
        {
            list.KHMatchCount(GameConsts.TOWER_MAX_LEVEL + 1);

            for (int i = 1; i < list.Count; i++)
                list[i] = Mathf.RoundToInt(list[i - 1] * multiplier);
        }
    }
}

#endif