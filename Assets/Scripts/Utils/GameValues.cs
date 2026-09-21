#region CONSTANTS

public static class GameConsts
{
    // TOWER
    public const int TOWER_MAX_LEVEL = 5;
    public const int TOWER_RANGE_SEGMENTS = 32;

    // MEASUREMENTS
    public const float ISO_Y_SCALE = 0.5f;
    public const float COMPARISON_DIS_1 = 0.25f;
    public const float COMPARISON_DIS_2 = 0.5f;

    // DAMAGE MULTIPLIERS
    public const float ELEMENT_BONUS_MULTIPLIER = 1.5f;
    public const float LOW_ELEMENT_MULTIPLIER = 0.75f;
    public const float MEDIUM_ELEMENT_MULTIPLIER = 0.5f;
    public const float HIGH_ELEMENT_MULTIPLIER = 0.25f;
    public const float IMMUNE_ELEMENT_MULTIPLIER = 0f;
}

#endregion
#region TAGS

public static class GameTags
{
    public const string ENEMY = "Enemy";
    public const string VILLAGER = "Villager";
}

#endregion
#region SCENES

public static class GameScenes
{
    public const int MAIN_MENU = 1;
}

#endregion
#region ENUMS

public enum ElementType
{
    None,
    Fire,
    Water,
    Earth,
}

public enum ElementStrength
{
    Low,
    Medium,
    High,
    Immune,
}

#endregion