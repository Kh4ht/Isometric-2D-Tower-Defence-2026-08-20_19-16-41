using KH;
using UnityEngine;

public class ProjectileMovementSubSys : IKHSubsystem
{
    #region FIELDS

    private readonly Projectile owner;

    // Below this the bullet would practically stall if the curve hits 0
    private const float MIN_SPEED_MULTIPLIER = 0.05f;

    // Shared flight state
    private Vector2 launchPos;
    private float flightProgress;   // parabolic: 0..1 integrated progress
    private float startDistance;    // straight: distance at launch

    // Parabolic runtime state
    private float parabolicFlightDuration;

    #endregion
    #region CONSTRUCTOR

    public ProjectileMovementSubSys(Projectile owner)
    {
        this.owner = owner;
    }

    #endregion
    #region UNITY EVENTS

    public void IFixedUpdate()
    {
        switch (owner.stats.GetMoveType())
        {
            case BulletMoveType.StraightOrParabolic:
                StraightOrParabolicMove();
                break;

            case BulletMoveType.Laser:
                LaserMove();
                break;
        }
    }

    #endregion
    #region PRIVATE

    private float EvaluateSpeedMultiplier(float progress)
    {
        if (owner.stats.GetSpeedMultiplierCurve().length == 0)
            return 1f;

        return Mathf.Max(MIN_SPEED_MULTIPLIER, owner.stats.GetSpeedMultiplierCurve().Evaluate(Mathf.Clamp01(progress)));
    }

    private void StraightOrParabolicMove()
    {
        // For straight bullet movement
        if (owner.stats.GetParabolicArcHeightMultiplier() <= 0)
        {
            Vector2 targetPos = owner.stats.GetTargetPos();

            float progress = startDistance > 0f
                ? 1f - Vector2.Distance(owner.transform.position, targetPos) / startDistance
                : 1f;

            owner.KHMoveTowards(targetPos: targetPos,
                                moveSpeed: owner.stats.GetMoveSpeed() * EvaluateSpeedMultiplier(progress) * Time.fixedDeltaTime);
        }
        else
        {
            float multiplier = EvaluateSpeedMultiplier(flightProgress);

            if (parabolicFlightDuration > 0f)
                flightProgress = Mathf.Clamp01(flightProgress + Time.fixedDeltaTime * multiplier / parabolicFlightDuration);
            else
                flightProgress = 1f;

            float t = flightProgress;

            // Actual gameplay position moves in a straight line so distance/
            // collision checks elsewhere keep working unmodified.
            Vector2 flatPos = Vector2.Lerp(launchPos, owner.stats.GetTargetPos(), t);
            Vector3 pos = owner.transform.position;
            pos.x = flatPos.x;
            pos.y = flatPos.y;
            owner.transform.position = pos;

            // Purely cosmetic hop, applied to an optional child transform only.
            if (owner.VisualRoot != null)
            {
                float distance = Vector2.Distance(launchPos, owner.stats.GetTargetPos());
                float height = owner.stats.GetParabolicArcHeightMultiplier() * distance * 4f * t * (1f - t);

                Vector3 localPos = owner.VisualRoot.localPosition;
                localPos.y = height;
                owner.VisualRoot.localPosition = localPos;
            }
        }
    }

    private void LaserMove() { }

    #endregion
    #region PUBLIC

    public void IReset()
    {
        switch (owner.stats.GetMoveType())
        {
            case BulletMoveType.StraightOrParabolic:
                owner.VisualRoot.transform.localPosition = Vector2.zero;

                launchPos = owner.transform.position;
                flightProgress = 0f;

                float distance = Vector2.Distance(launchPos, owner.stats.GetTargetPos());

                if (owner.stats.GetParabolicArcHeightMultiplier() <= 0)
                {
                    startDistance = distance;
                    break;
                }

                parabolicFlightDuration = owner.stats.GetMoveSpeed() > 0f
                    ? distance / owner.stats.GetMoveSpeed()
                    : 0f;
                break;

            case BulletMoveType.Laser:

                break;
        }
    }

    #endregion
}
