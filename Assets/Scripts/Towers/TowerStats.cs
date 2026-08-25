using System;
using VInspector;

[Serializable]
public class TowerStats
{
    public bool canShoot = true;

    [ReadOnly] public int lvl = 0;

    public Enemy enemyTargeted;
}