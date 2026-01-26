using UnityEngine;

/// <summary>
/// 도끼형 포물선 투사체
/// 위로 올라갔다가 중력에 의해 떨어지는 궤적을 그립니다.
/// </summary>
public class ParabolaProjectile : Projectile
{
    #region Serialized Fields
    [Header("포물선 설정")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float horizontalSpeed = 2f;
    [SerializeField] private float rotationSpeed = 360f;
    #endregion

    #region Private Fields
    private Vector2 velocity;
    private float horizontalDirection;
    #endregion

    #region Overrides
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        velocity = Vector2.zero;
    }

    public override void Initialize(
        float damage,
        Vector2 dir,
        float speed,
        int pierce,
        float lifetime,
        float areaMultiplier,
        PoolManager pool,
        string key,
        string hitEffectKey = null)
    {
        base.Initialize(damage, dir, speed, pierce, lifetime, areaMultiplier, pool, key, hitEffectKey);

        horizontalDirection = dir.x >= 0 ? 1f : -1f;

        // dir.y를 활용하여 각 투사체마다 고유한 퍼짐 값 생성
        // Abs를 사용하면 +/-가 같은 값이 되므로 dir.y를 직접 사용
        float spreadMultiplier = 1f + (dir.y + 0.5f) * 2f;
        float spreadHorizontal = horizontalDirection * horizontalSpeed * spreadMultiplier;

        // speed를 수직 발사력으로 사용
        velocity = new Vector2(spreadHorizontal, speed);
    }

    protected override void FixedUpdate()
    {
        MoveAxe();
    }

    protected override void Move()
    {
        // 기본 Move 사용 안 함
    }
    #endregion

    #region Axe Movement
    private void MoveAxe()
    {
        // 중력 적용
        velocity.y -= gravity * Time.fixedDeltaTime;

        // 위치 업데이트
        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);

        // 회전 (도끼가 빙글빙글 도는 효과)
        float rotationDirection = -horizontalDirection; // 발사 방향 반대로 회전
        transform.Rotate(0f, 0f, rotationSpeed * rotationDirection * Time.fixedDeltaTime);
    }
    #endregion
}
