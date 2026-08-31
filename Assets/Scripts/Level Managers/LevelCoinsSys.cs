using KH;
using UnityEngine;

public class LevelCoinsSys : KHManagedBehaviour
{
    #region FIELDS

    public static LevelCoinsSys Ins { get; private set; }

    [SerializeField] private int startingCoins = 0;

    public int CurrentCoins { get; private set; }

    // Fired whenever the amount changes. Passes (newAmount, delta).
    public event System.Action<int, int> OnCoinsChanged;

    #endregion
    #region UNITY EVENTS

    private void Awake()
    {
        if (Ins == null)
            Ins = this;
        else
            Debug.LogWarning("More Than One Instance");

        CurrentCoins = startingCoins;
    }

    #endregion
    #region PUBLIC

    /// <summary>Add coins (e.g. picked up, reward). Amount must be positive.</summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        ChangeCoins(amount);
    }

    /// <summary>Try to spend coins. Returns false (and does nothing) if not enough.</summary>
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (CurrentCoins < amount)
            return false;

        ChangeCoins(-amount);
        return true;
    }

    public bool HasEnough(int amount) => CurrentCoins >= amount;

    #endregion
    #region PRIVATE

    private void ChangeCoins(int delta)
    {
        CurrentCoins = Mathf.Max(0, CurrentCoins + delta);
        OnCoinsChanged?.Invoke(CurrentCoins, delta);
    }

    #endregion
}