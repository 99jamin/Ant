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

    // 충돌 쿨타임 관리 (적별로 쿨타임 적용)
    private readonly Dictionary<Collider2D, float> _hitCooldowns = new Dictionary<Collider2D, float>();
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
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        // 스프라이트 원본 크기 기반 스케일 적용
        float spriteSize = _spriteRenderer.sprite.bounds.size.x;
        float desiredSize = _parentSkill.ObjectSize;
        float scale = desiredSize / spriteSize;

        transform.localScale = Vector3.one * scale;
    }

    private void TryDealDamage(Collider2D other)
    {
        if (_parentSkill == null) return;

        // 적 레이어 확인
        if (((1 << other.gameObject.layer) & _parentSkill.EnemyLayer) == 0) return;

        // 쿨타임 확인
        if (_hitCooldowns.TryGetValue(other, out float cooldown) && cooldown > 0f) return;

        // 데미지 적용
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(_parentSkill.Damage);
            _parentSkill.SpawnHitEffectAt(other.transform.position);

            // 쿨타임 설정
            _hitCooldowns[other] = HitCooldown;
        }
    }

    private void UpdateHitCooldowns()
    {
        // 쿨타임 감소 (GC 방지를 위해 리스트 대신 직접 순회)
        var keys = new List<Collider2D>(_hitCooldowns.Keys);
        foreach (var key in keys)
        {
            _hitCooldowns[key] -= Time.deltaTime;

            // 쿨타임 완료된 항목 제거
            if (_hitCooldowns[key] <= 0f)
            {
                _hitCooldowns.Remove(key);
            }
        }
    }
    #endregion
}