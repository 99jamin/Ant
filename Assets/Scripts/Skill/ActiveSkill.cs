using UnityEngine;

/// <summary>
/// 액티브 스킬의 기본 클래스
/// 쿨타임 관리, 자동 발동, 적 탐지 등을 담당합니다.
/// </summary>
public abstract class ActiveSkill : BaseSkill
{
    #region Protected Fields
    protected float _cooldownTimer;
    protected Transform _currentTarget;
    protected string _hitEffectPoolKey;

    // GC 방지용 버퍼
    private readonly Collider2D[] _detectBuffer = new Collider2D[32];
    #endregion

    #region Properties
    /// <summary>
    /// 적 탐지 범위 (레벨 데이터에서 가져옴)
    /// </summary>
    protected float DetectRange => CurrentLevelData?.detectRange ?? 10f;

    /// <summary>
    /// 적 레이어 (Managers에서 가져옴)
    /// </summary>
    protected LayerMask EnemyLayer => Managers.Instance.EnemyLayer;

    /// <summary>
    /// 글로벌 배율이 적용된 실제 쿨타임
    /// </summary>
    public float ActualCooldown
    {
        get
        {
            if (CurrentLevelData == null) return 1f;
            return CurrentLevelData.cooldown * _player.GlobalCooldownMultiplier;
        }
    }

    /// <summary>
    /// 글로벌 배율이 적용된 실제 데미지
    /// </summary>
    public float ActualDamage
    {
        get
        {
            if (CurrentLevelData == null) return 0f;
            return CurrentLevelData.damage * _player.GlobalDamageMultiplier;
        }
    }

    /// <summary>
    /// 글로벌 배율이 적용된 실제 범위 배율
    /// </summary>
    public float ActualAreaMultiplier
    {
        get
        {
            if (CurrentLevelData == null) return 1f;
            return CurrentLevelData.areaMultiplier * _player.GlobalAreaMultiplier;
        }
    }

    public bool CanActivate => _cooldownTimer <= 0f;
    public float CooldownProgress => 1f - (_cooldownTimer / ActualCooldown);
    #endregion

    #region Unity Lifecycle
    protected virtual void Update()
    {
        if (_player == null || _player.IsDead) return;

        UpdateCooldown();

        if (CanActivate)
        {
            TryActivate();
        }
    }
    #endregion

    #region Cooldown
    private void UpdateCooldown()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    protected void StartCooldown()
    {
        _cooldownTimer = ActualCooldown;
    }
    #endregion

    #region Activation
    /// <summary>
    /// 스킬 발동 시도
    /// </summary>
    protected virtual void TryActivate()
    {
        _currentTarget = FindNearestEnemy();

        if (_currentTarget != null || !RequiresTarget())
        {
            Activate();
            StartCooldown();
        }
    }

    /// <summary>
    /// 스킬 발동 (자식 클래스에서 구현)
    /// </summary>
    protected abstract void Activate();

    /// <summary>
    /// 타겟이 필요한 스킬인지 여부 (기본: 필요함)
    /// </summary>
    protected virtual bool RequiresTarget() => true;
    #endregion

    #region Target Detection
    /// <summary>
    /// 가장 가까운 적을 찾습니다.
    /// </summary>
    protected Transform FindNearestEnemy()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            _player.transform.position,
            DetectRange,
            _detectBuffer,
            EnemyLayer
        );

        if (count == 0) return null;

        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 playerPos = _player.transform.position;

        for (int i = 0; i < count; i++)
        {
            float distance = Vector3.SqrMagnitude(_detectBuffer[i].transform.position - playerPos);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = _detectBuffer[i].transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 범위 내 모든 적을 찾습니다.
    /// </summary>
    protected int FindEnemiesInRange(float range, Collider2D[] results)
    {
        return Physics2D.OverlapCircleNonAlloc(
            _player.transform.position,
            range,
            results,
            EnemyLayer
        );
    }
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _cooldownTimer = 0f;

        RegisterHitEffectPool();
    }
    #endregion

    #region Hit Effect Pool
    private PoolManager PoolManager => Managers.Instance.Pool;

    private void RegisterHitEffectPool()
    {
        if (_skillData == null || _skillData.hitEffectPrefab == null) return;
        if (PoolManager == null) return;

        _hitEffectPoolKey = $"HitEffect_{_skillData.skillName}";

        if (!PoolManager.HasPool(_hitEffectPoolKey))
        {
            PoolManager.CreatePool(_hitEffectPoolKey, _skillData.hitEffectPrefab, 10);
        }
    }
    #endregion
}
