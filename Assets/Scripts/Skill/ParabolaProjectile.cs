using UnityEngine;

/// <summary>
/// 도끼형 포물선 투사체
/// 위로 올라갔다가 중력에 의해 떨어지는 궤적을 그립니다.
/// </summary>
public class ParabolaProjectile : Projectile
{
    #region Private Fields
    private Vector2 _velocity;
    private float _horizontalDirection;

    // 포물선 설정 (SO에서 주입)
    private float _gravity;
    private float _horizontalSpeed;
    private float _rotationSpeed;
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

        // speed를 수직 발사력으로 사용
        _velocity = new Vector2(spreadHorizontal, speed);
    }

    protected override void FixedUpdate()
    {
        MoveParabola();
    }

    protected override void Move()
    {
        // 기본 Move 사용 안 함
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 포물선 설정 초기화 (ProjectileSkill에서 호출)
    /// </summary>
    public void InitializeParabolaSettings(float gravity, float horizontalSpeed, float rotationSpeed)
    {
        _gravity = gravity;
        _horizontalSpeed = horizontalSpeed;
        _rotationSpeed = rotationSpeed;
    }
    #endregion

    #region Parabola Movement
    private void MoveParabola()
    {
        // 중력 적용
        _velocity.y -= _gravity * Time.fixedDeltaTime;

        // 위치 업데이트
        transform.position += (Vector3)(_velocity * Time.fixedDeltaTime);

        // 회전 (도끼가 빙글빙글 도는 효과)
        float rotationDirection = -_horizontalDirection;
        transform.Rotate(0f, 0f, _rotationSpeed * rotationDirection * Time.fixedDeltaTime);
    }
    #endregion
}