using KH;
using UnityEngine;

public class Projectile2 : MonoBehaviour
{
    #region FIELDS
    private const float MIN_SPEED_MULTIPLIER = 0.1f;
    private const float HIT_DISTANCE = 0.01f;
    private const float MIN_INITIAL_DISTANCE = 0.0001f;

    // INSPECTOR
    [SerializeField] private Transform target;

    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector2 targetPos;
    [SerializeField] private float progress_01; // 0 - 1

    [Space(20)]

    [SerializeField] private AnimationCurve speedMultiplierCurve = AnimationCurve.Constant(0, 1, 1);

    [Header("Position Offset")]
    [Tooltip("Sideways offset over progress. Keep it 0 at both ends so the projectile starts and lands on the straight path.")]
    [SerializeField] private AnimationCurve posOffsetCurve = AnimationCurve.Constant(0, 1, 0);
    [SerializeField] private float posOffsetAmplitude = 1f; // world units at curve value 1. Negative flips the side

    private Vector2 basePos;   // logical position (no offset)
    private float initialDistance;
    private float traveledDistance;
    #endregion

    private void Update()
    {
        UpdateTargetPos();

        Move();

        DestroyOnHit();
    }

    private void UpdateTargetPos()
    {
        if (target != null)
            targetPos = target.position;
    }

    private void DestroyOnHit()
    {
        // Check the LOGICAL position, so the offset can never block a hit
        if (Kh.SqrDistanceIsLessThan(basePos, targetPos, HIT_DISTANCE))
            Destroy(gameObject);
    }

    private void Move()
    {
        // Speed multiplier
        float speedMultiplier = Mathf.Max(speedMultiplierCurve.Evaluate(progress_01), MIN_SPEED_MULTIPLIER);
        float step = moveSpeed * speedMultiplier * Time.deltaTime;

        // Move the logical position straight toward the REAL target
        Vector2 before = basePos;
        basePos = Vector2.MoveTowards(basePos, targetPos, step);
        traveledDistance += Vector2.Distance(before, basePos);

        progress_01 = initialDistance > MIN_INITIAL_DISTANCE
            ? Mathf.Clamp01(traveledDistance / initialDistance)
            : 1f;

        // Visual offset, perpendicular to the travel direction
        Vector2 dir = (targetPos - basePos).normalized; // zero vector if already at target -> no offset
        Vector2 perpendicular = new(-dir.y, dir.x);
        float offset = posOffsetCurve.Evaluate(progress_01) * posOffsetAmplitude;

        transform.position = basePos + perpendicular * offset;
    }

    public Projectile2 Init(Transform target, float moveSpeed)
    {
        return Setup(target, target.position, moveSpeed);
    }

    public Projectile2 Init(Vector2 targetPos, float moveSpeed)
    {
        return Setup(null, targetPos, moveSpeed);
    }

    private Projectile2 Setup(Transform target, Vector2 pos, float moveSpeed)
    {
        this.target = target;
        this.moveSpeed = moveSpeed;
        targetPos = pos;
        basePos = transform.position;
        initialDistance = Vector2.Distance(basePos, pos);
        traveledDistance = 0f;
        progress_01 = 0f;
        return this;
    }
}