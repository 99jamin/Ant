using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 회전 오브젝트 (성경형 스킬)
/// 플레이어 주변을 공전하며 적과 충돌 시 데미지를 줍니다.
/// </summary>
public class OrbitObject : MonoBehaviour
{
    #region Private Fields
    private OrbitSkill _parentSkill;
    private SpriteRenderer _spriteRenderer;
    private int _index;
    private float _currentAngle;

    // 충돌 쿨타임 관리 (적별로 만료 시간 저장)
    private readonly Dictionary<Collider2D, float> _hitExpirationTimes = new Dictionary<Collider2D, float>();
    private readonly List<Collider2D> _expiredColliders = new List<Collider2D>(); // GC 방지용 재사용 버퍼
    private const float HitCooldown = 0.5f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateRotation();
        UpdateHitCooldowns();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 회전 오브젝트 초기화
    /// </summary>
    /// <param name="parentSkill">부모 스킬</param>
    /// <param name="index">오브젝트 인덱스 (배치 각도 계산용)</param>
    /// <param name="totalCount">전체 오브젝트 수</param>
    public void Initialize(OrbitSkill parentSkill, int index, int totalCount)
    {
        _parentSkill = parentSkill;
        _index = index;

        // 초기 각도 설정 (등간격 배치)
        _currentAngle = 360f / totalCount * index;

        // 즉시 위치 업데이트
        UpdatePosition();
        ApplyScale();
    }

    /// <summary>
    /// 오브젝트 수 변경 시 각도 재설정
    /// </summary>
    public void UpdateIndex(int index, int totalCount)
    {
        _index = index;
        _currentAngle = 360f / totalCount * index;
    }

    /// <summary>
    /// 스케일 업데이트 (레벨업 시 호출)
    /// </summary>
    public void UpdateScale()
    {
        ApplyScale();
    }
    #endregion

    #region Private Methods
    private void UpdateRotation()
    {
        if (_parentSkill == null) return;

        // 회전 속도 적용
        _currentAngle += _parentSkill.RotationSpeed * Time.deltaTime;

        // 360도 넘으면 초기화 (부동소수점 오차 방지)
        if (_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_parentSkill == null) return;

        float radius = _parentSkill.OrbitRadius;
        float angleRad = _currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(angleRad) * radius,
            Mathf.Sin(angleRad) * radius,
            0f
        );

        transform.position = _parentSkill.PlayerPosition + offset;
    }

    private void ApplyScale()
    {
        if (_parentSkill == null) return;
        SpriteScaleHelper.ApplySizeScale(transform, _spriteRenderer, _parentSkill.ObjectSize);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (_parentSkill == null) return;

        // 적 레이어 확인
        if (((1 << other.gameObject.layer) & _parentSkill.TargetEnemyLayer) == 0) return;

        // 쿨타임 확인 (만료 시간과 현재 시간 비교)
        float currentTime = Time.time;
        if (_hitExpirationTimes.TryGetValue(other, out float expirationTime) && currentTime < expirationTime)
            return;

        // 데미지 적용
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            // 회전 오브젝트에서 적 방향으로 넉백
            if (other.TryGetComponent<Enemy>(out var enemy))
            {
                Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                enemy.TakeDamage(_parentSkill.Damage, knockbackDir);
            }
            else
            {
                target.TakeDamage(_parentSkill.Damage);
            }
            _parentSkill.SpawnHitEffectAt(other.transform.position);

            // 만료 시간 설정 (현재 시간 + 쿨타임)
            _hitExpirationTimes[other] = currentTime + HitCooldown;
        }
    }

    private void UpdateHitCooldowns()
    {
        if (_hitExpirationTimes.Count == 0) return;

        float currentTime = Time.time;
        _expiredColliders.Clear();

        // 만료된 항목 수집 (값 수정 없이 비교만)
        foreach (var kvp in _hitExpirationTimes)
        {
            if (currentTime >= kvp.Value)
            {
                _expiredColliders.Add(kvp.Key);
            }
        }

        // 만료된 항목 제거
        foreach (var collider in _expiredColliders)
        {
            _hitExpirationTimes.Remove(collider);
        }
    }
    #endregion
}