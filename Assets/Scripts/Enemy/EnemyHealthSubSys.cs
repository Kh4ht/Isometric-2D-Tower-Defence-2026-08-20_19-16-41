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
        owner.stats.healthController.AddOnHealthDecreaseListener(OnHealthDecreased);
        owner.stats.healthController.AddOnMaxHealthReachedListener(OnMaxHealth);
        owner.stats.healthController.AddOnDeathListener(OnDeath);
        owner.stats.healthController.AddOnReviveListener(OnRevive);
        owner.stats.healthController.AddOnHealthChangedListener(OnHealthChanged);
        owner.stats.healthController.AddOnMaxHealthChangedListener(OnMaxHealthChanged);
    }

    public void IOnDisable()
    {
        owner.stats.healthController.RemoveOnHealthDecreaseListener(OnHealthDecreased);
        owner.stats.healthController.RemoveOnMaxHealthReachedListener(OnMaxHealth);
        owner.stats.healthController.RemoveOnDeathListener(OnDeath);
        owner.stats.healthController.RemoveOnReviveListener(OnRevive);
        owner.stats.healthController.RemoveOnHealthChangedListener(OnHealthChanged);
        owner.stats.healthController.RemoveOnMaxHealthChangedListener(OnMaxHealthChanged);
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
        owner.healthSlider.IncreaseWidthBasedOnHealth(owner.stats.healthController.MaxHealth);
    }

    private void OnHealthChanged()
    {
        // Update the health slider's value
        owner.healthSlider.ChangeValue(owner.stats.healthController.Health, owner.stats.healthController.MaxHealth);
    }

    private void OnDeath()
    {
        owner.healthSlider.gameObject.SetActive(false);

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