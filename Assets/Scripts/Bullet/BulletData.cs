using KH;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public class BulletData : ScriptableObject
{
    #region FIELDS

    [SerializeField, ReadOnly]
    private string id;
    public string ID => id;

    [Space(20)]

    public Bullet prefab;

    [Space(20)]

    public float moveSpeed;
    public float damage;

    [Space(10)]

    public BulletMoveType type = BulletMoveType.Straight;

    // [Space(10), ShowIf(nameof(type), BulletType.Straight)]

    // [Space(10), ShowIf(nameof(type), BulletType.Parabolic)]

    // [Space(10), ShowIf(nameof(type), BulletType.Laser)]

    // [Space(10), ShowIf(nameof(type), BulletType.Follow)]

    // [HideIf(nameof(type), BulletType.Laser)]
    // public float acceleration = 0f;

    #endregion
    #region UNITY EVENTS

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(ID))
            id = Kh.GenerateId(name, 8);
    }

    #endregion
}



#region Bullet Type

public enum BulletMoveType
{
    Straight,
    Parabolic,
    Laser,
    Follow,
}

#endregion