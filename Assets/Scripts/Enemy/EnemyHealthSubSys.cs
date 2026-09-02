using KH;
using UnityEngine;

public class EnemyHealthSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Enemy owner;

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
        owner.HealthController.AddOnHealthDecreaseListener(OnHealthDecreased);
        owner.HealthController.AddOnMaxHealthReachedListener(OnMaxHealth);
        owner.HealthController.AddOnDeathListener(OnDeath);
        owner.HealthController.AddOnReviveListener(OnRevive);
        owner.HealthController.AddOnHealthChangedListener(OnHealthChanged);
        owner.HealthController.AddOnMaxHealthChangedListener(OnMaxHealthChanged);
    }

    public void IOnDisable()
    {
        owner.HealthController.RemoveOnHealthDecreaseListener(OnHealthDecreased);
        owner.HealthController.RemoveOnMaxHealthReachedListener(OnMaxHealth);
        owner.HealthController.RemoveOnDeathListener(OnDeath);
        owner.HealthController.RemoveOnReviveListener(OnRevive);
        owner.HealthController.RemoveOnHealthChangedListener(OnHealthChanged);
        owner.HealthController.RemoveOnMaxHealthChangedListener(OnMaxHealthChanged);
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
        owner.healthSlider.IncreaseWidthBasedOnHealth(owner.HealthController.MaxHealth);
    }

    private void OnHealthChanged()
    {
        // Update the health slider's value
        owner.healthSlider.ChangeValue(owner.HealthController.Health, owner.HealthController.MaxHealth);
    }

    private void OnDeath()
    {
        owner.healthSlider.gameObject.SetActive(false);

        // TODO: Add animations & effects.

        KHPoolManager.Ins.Despawn(owner.data.ID, owner);
    }

    private void OnRevive()
    {
        owner.healthSlider.ResetSliders();
    }

    #endregion
}