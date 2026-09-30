using KH;
using TMPro;
using UnityEngine;
using VInspector;

[DisallowMultipleComponent]
public class LevelCoinManager : KHManagedBehaviour
{
    #region FIELDS

    [KHResetStatic]
    public static LevelCoinManager Ins { get; private set; }

    // INSPECTOR
    [SerializeField]
    public KHCoin Aether = new(name: nameof(Aether),
                               startAmount: 10000);


    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogError($"More Than One Instance of type {nameof(LevelCoinManager)}".AddColorTag(KHUtils.XMLColors.Red));
    }

    #endregion
    #region PRIVATE

#if UNITY_EDITOR
    [Button(color = "green")]
    private void AddCoins(int amount)
    {
        Aether.Add(amount);
    }
    [Button(color = "green")]
    private void SpendCoins(int amount)
    {
        Aether.TrySpend(amount);
    }
#endif

    #endregion
}