using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    #region ElementType

    public enum ElementType
    {
        None,
        Fire,
        Water,
        Earth,
        Air,
    }

    public static readonly Dictionary<ElementType, ElementType> ElementTypeResistedBy = new()
        {
            { ElementType.Fire, ElementType.Water },
            { ElementType.Water, ElementType.Air },
            { ElementType.Air, ElementType.Earth },
            { ElementType.Earth, ElementType.Fire },
        };

    public static Gradient GetGradientByElementType(this ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Fire => DB.Db.fireTowerRangeIndicator,
            ElementType.Water => DB.Db.WaterTowerRangeIndicator,
            ElementType.Earth => DB.Db.EarthTowerRangeIndicator,
            ElementType.Air => DB.Db.AirTowerRangeIndicator,

            _ => new Gradient(),
        };
    }

    #endregion
    #region ProjectileMoveType

    public enum ProjectileMoveType
    {
        StraightOrParabolic,
        Laser,
    }

    #endregion
    #region ProjectileTargetPosition

    public enum ProjectileTargetPosition
    {
        FollowTargetPos,
        FirstTargetPos
    }

    #endregion
    #region ElementStrength

    public enum ElementStrength
    {
        Low,
        Medium,
        High,
        Immune,
    }

    public static float ReduceDamage(this ElementStrength receiverElementStrength, float damageAmount)
    {
        return receiverElementStrength switch
        {
            ElementStrength.Low => damageAmount * Consts.LOW_ELEMENT_MULTIPLIER,
            ElementStrength.Medium => damageAmount * Consts.MEDIUM_ELEMENT_MULTIPLIER,
            ElementStrength.High => damageAmount * Consts.HIGH_ELEMENT_MULTIPLIER,
            ElementStrength.Immune => damageAmount * Consts.IMMUNE_ELEMENT_MULTIPLIER,

            _ => damageAmount,
        };
    }

    #endregion
}