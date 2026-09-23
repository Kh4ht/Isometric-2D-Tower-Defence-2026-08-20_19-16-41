using KH;
using PrimeTween;

public class EnemyHealthSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

    private const int AFTER_DEATH_DELAY = 10;

    #endregion
    #region CONSTRUCTOR

    public EnemyHealthSubSys(Enemy owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IOnEnable()
    {
        owner.stats.GetHealthController().AddOnHealthDecreaseListener(OnHealthDecreased);
        owner.stats.GetHealthController().AddOnMaxHealthReachedListener(OnMaxHealth);
        owner.stats.GetHealthController().AddOnDeathListener(OnDeath);
        owner.stats.GetHealthController().AddOnReviveListener(OnRevive);
        owner.stats.GetHealthController().AddOnHealthChangedListener(OnHealthChanged);
        owner.stats.GetHealthController().AddOnMaxHealthChangedListener(OnMaxHealthChanged);
    }

    public void IOnDisable()
    {
        owner.stats.GetHealthController().RemoveOnHealthDecreaseListener(OnHealthDecreased);
        owner.stats.GetHealthController().RemoveOnMaxHealthReachedListener(OnMaxHealth);
        owner.stats.GetHealthController().RemoveOnDeathListener(OnDeath);
        owner.stats.GetHealthController().RemoveOnReviveListener(OnRevive);
        owner.stats.GetHealthController().RemoveOnHealthChangedListener(OnHealthChanged);
        owner.stats.GetHealthController().RemoveOnMaxHealthChangedListener(OnMaxHealthChanged);
    }

    #endregion
    #region PRIVATE

    private void OnMaxHealth()
    {
        owner.healthSlider.gameObject.SetActive(false);
    }

    private void OnHealthDecreased()
    {
        owner.healthSlider.gameObject.SetActive(true);
    }

    private void OnMaxHealthChanged()
    {
        // Update the health slider's width
        owner.healthSlider.IncreaseWidthBasedOnHealth(owner.stats.GetHealthController().MaxHealth);
    }

    private void OnHealthChanged()
    {
        // Update the health slider's value
        owner.healthSlider.ChangeValue(owner.stats.GetHealthController().Health, owner.stats.GetHealthController().MaxHealth);
    }

    private void OnDeath()
    {
        owner.healthSlider.gameObject.SetActive(false);

        LevelCoinManager.Ins.Aether.Add(owner.data.DeathAetherPrize);

        // Delay despawning to allow death animations and effects to finish, and to show player how many enemies he/she killed.
        Tween.Delay(AFTER_DEATH_DELAY, () =>
        {
            KHPoolManager.Ins.Despawn(owner.data.ID, owner);
        });
    }

    private void OnRevive()
    {
        owner.healthSlider.ResetSliders();
    }

    #endregion
}