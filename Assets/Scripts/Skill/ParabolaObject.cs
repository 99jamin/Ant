using UnityEngine;

/// <summary>
/// 포물선 투사체 오브젝트
/// 위로 올라갔다가 중력에 의해 떨어지는 궤적을 그립니다.
/// </summary>
public class ParabolaObject : ProjectileObject
{
    #region Serialized Fields
    [Header("포물선 설정")]
    [SerializeField] private float _gravity = 20f;
    [SerializeField] private float _horizontalSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 360f;
    #endregion

    #region Private Fields
    private Vector2 _velocity;
    private float _horizontalDirection;
    private float _verticalSpeed;
    #endregion

    #region Overrides
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        _velocity = Vector2.zero;
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

        _horizontalDirection = dir.x >= 0 ? 1f : -1f;

        // dir.y를 활용하여 각 투사체마다 고유한 퍼짐 값 생성
        float spreadMultiplier = 1f + (dir.y + 0.5f) * 2f;
        float spreadHorizontal = _horizontalDirection * _horizontalSpeed * spreadMultiplier;

        // speed를 수직 발사력으로 사용하여 초기 속도 설정
        _verticalSpeed = speed;
        _velocity = new Vector2(spreadHorizontal, _verticalSpeed);
    }

    /// <summary>
    /// 포물선 이동: 중력을 적용하여 위치를 업데이트합니다.
    /// </summary>
    protected override void Move()
    {
        // 중력 적용
        _velocity.y -= _gravity * Time.fixedDeltaTime;

        // Rigidbody를 통한 이동 대신 직접 위치 업데이트
        _rb.velocity = Vector2.zero;
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);

        // 회전 (도끼가 빙글빙글 도는 효과)
        float rotationDirection = -_horizontalDirection;
        transform.Rotate(0f, 0f, _rotationSpeed * rotationDirection * Time.fixedDeltaTime);
    }
    #endregion
}
