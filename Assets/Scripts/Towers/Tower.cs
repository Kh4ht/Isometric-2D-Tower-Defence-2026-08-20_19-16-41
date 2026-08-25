using KH;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(AudioSource))]
public class Tower : KHManagedBehaviour
{
    #region FIELDS

    public const int TOWER_MAX_LEVEL = 5;

    // Components
    public AudioSource AudioS { get; private set; }

    // INSPECTOR

    public TowerStats stats;

    [ShowInInspector]

    [Foldout("DATA")]
    public TowerData data;
    [EndFoldout]
    private void Reset()
    {
        AudioS = GetComponent<AudioSource>();
        AudioS.playOnAwake = false;
    }

    private void Awake()
    {
        AudioS = GetComponent<AudioSource>();

        stats = new();
    }

    private void OnDrawGizmosSelected()
    {
        if (stats.enemyTargeted != null)
        {
            Gizmos.color = Color.red;
            // draw a line at the target
            Gizmos.DrawLine(transform.position, stats.enemyTargeted.transform.position);
        }
    }

    #endregion
    #region PRIVATE



    #endregion
    #region PUBLIC

    /// <summary>Increases tower level by 1</summary>
    public void LvlUp()
    {
        if (stats.lvl >= TOWER_MAX_LEVEL)
        {
            Debug.Log("Max level reached");
            return;
        }

        stats.lvl++;
    }

    public void PlaySFX(AudioClip audioClip)
    {
        if (audioClip == null)
            return;

        AudioS.PlayOneShot(audioClip);
    }

    #endregion
}