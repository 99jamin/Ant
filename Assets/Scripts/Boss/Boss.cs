using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 전용 컴포넌트
/// Enemy를 상속하여 보스 패턴 시스템을 추가합니다.
/// </summary>
public class Boss : Enemy
{
    #region Animator Hashes
    private static readonly int RunStateHash = Animator.StringToHash("Run");
    #endregion

    #region Constants
    private const string BOSS_PROJECTILE_POOL_KEY = "BossProjectile";
    #endregion

    #region Private Fields
    private BossDataSO _bossData;
    private EnemySpawner _enemySpawner;
    private readonly List<PatternState> _patternStates = new();
    private readonly Dictionary<BossAttackPatternSO, int> _triggerHashes = new();
    private bool _isExecutingPattern;

    // 현재 실행 중인 패턴 상태
    private BossAttackPatternSO _currentPattern;
    private PatternPhase _currentPhase;
    private float _phaseTimer;
    private Vector2 _chargeDirection;

    #endregion

    #region Pattern State
    private class PatternState
    {
        public BossAttackPatternSO Pattern;
        public float CooldownTimer;
    }

    private enum PatternPhase
    {
        None,
        Windup,    // 준비 동작
        Execute,   // 실행
        Recovery   // 후딜레이
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 보스 데이터로 초기화합니다.
    /// </summary>
    public override void Init(EnemyDataSO data, string poolKey)
    {
        base.Init(data, poolKey);

        if (data is BossDataSO bossData)
        {
            _bossData = bossData;
            transform.localScale = Vector3.one * bossData.Scale;
            InitializePatterns();
        }
    }

    /// <summary>
    /// EnemySpawner 참조를 주입합니다. 소환 패턴에서 적 등록에 사용됩니다.
    /// </summary>
    public void SetSpawnerReference(EnemySpawner spawner)
    {
        _enemySpawner = spawner;
    }

    private void InitializePatterns()
    {
        _patternStates.Clear();
        _triggerHashes.Clear();

        if (_bossData?.AttackPatterns == null) return;

        foreach (var pattern in _bossData.AttackPatterns)
        {
            if (pattern == null) continue;

            _patternStates.Add(new PatternState
            {
                Pattern = pattern,
                CooldownTimer = pattern.Cooldown * 0.5f // 첫 패턴은 절반 쿨타임으로 시작
            });

            // 애니메이션 트리거 해시 사전 캐싱
            if (!string.IsNullOrEmpty(pattern.AnimationTrigger))
            {
                _triggerHashes[pattern] = Animator.StringToHash(pattern.AnimationTrigger);
            }

            // 투사체 패턴의 풀을 초기화 시점에 생성
            if (pattern.PatternType == BossPatternType.Projectile
                && pattern.ProjectilePrefab != null
                && !Managers.Instance.Pool.HasPool(BOSS_PROJECTILE_POOL_KEY))
            {
                Managers.Instance.Pool.CreatePool(BOSS_PROJECTILE_POOL_KEY, pattern.ProjectilePrefab, 10);
            }
        }
    }
    #endregion

    #region Unity Lifecycle
    protected override void Update()
    {
        // 부모의 Update 호출 (히트 이펙트 타이머, 사망 처리 등)
        base.Update();

        if (!_isInitialized || IsDead) return;

        // 보스 패턴 시스템
        if (_bossData != null)
        {
            UpdatePatternCooldowns();
            UpdateCurrentPattern();
            TryExecutePattern();
        }
    }
    #endregion

    #region Pattern System
    private void UpdatePatternCooldowns()
    {
        if (_isExecutingPattern) return;

        float deltaTime = Time.deltaTime;

        foreach (var state in _patternStates)
        {
            if (state.CooldownTimer > 0f)
            {
                state.CooldownTimer -= deltaTime;
            }
        }
    }

    private void TryExecutePattern()
    {
        if (_isExecutingPattern) return;

        // 쿨타임이 끝난 패턴 중 하나 선택
        foreach (var state in _patternStates)
        {
            if (state.CooldownTimer <= 0f)
            {
                StartPattern(state);
                break;
            }
        }
    }

    private void StartPattern(PatternState state)
    {
        _currentPattern = state.Pattern;
        _isExecutingPattern = true;
        state.CooldownTimer = state.Pattern.Cooldown;

        // 패턴 실행 중 이동 억제
        _canMove = false;
        _rb.velocity = Vector2.zero;

        // 애니메이션 트리거 (캐싱된 해시 사용 + 누적 방지)
        if (_animator != null && _triggerHashes.TryGetValue(_currentPattern, out int hash))
        {
            _animator.ResetTrigger(hash);
            _animator.SetTrigger(hash);
        }

        // 패턴 타입별 초기화
        switch (_currentPattern.PatternType)
        {
            case BossPatternType.Charge:
                StartChargePattern();
                break;
            case BossPatternType.AreaAttack:
                StartAreaAttackPattern();
                break;
            case BossPatternType.Projectile:
                StartProjectilePattern();
                break;
            case BossPatternType.Summon:
                StartSummonPattern();
                break;
        }
    }

    private void UpdateCurrentPattern()
    {
        if (!_isExecutingPattern || _currentPattern == null) return;

        _phaseTimer -= Time.deltaTime;

        // Recovery 페이즈 공통 처리
        if (_currentPhase == PatternPhase.Recovery)
        {
            if (_phaseTimer <= 0f)
            {
                EndPattern();
            }
            return;
        }

        switch (_currentPattern.PatternType)
        {
            case BossPatternType.Charge:
                UpdateChargePattern();
                break;
            case BossPatternType.AreaAttack:
                UpdateAreaAttackPattern();
                break;
            case BossPatternType.Projectile:
            case BossPatternType.Summon:
                // 즉시 실행 패턴은 타이머만 체크
                if (_phaseTimer <= 0f)
                {
                    EnterRecovery();
                }
                break;
        }
    }

    private void EnterRecovery()
    {
        _currentPhase = PatternPhase.Recovery;
        _phaseTimer = _currentPattern.RecoveryDuration;
        _rb.velocity = Vector2.zero;
    }

    private void EndPattern()
    {
        // 남은 트리거 정리
        if (_animator != null && _currentPattern != null
            && _triggerHashes.TryGetValue(_currentPattern, out int hash))
        {
            _animator.ResetTrigger(hash);
        }

        // Run 상태로 강제 복귀
        if (_animator != null)
        {
            _animator.Play(RunStateHash, 0, 0f);
        }

        // 이동 재개
        _canMove = true;

        _isExecutingPattern = false;
        _currentPattern = null;
        _currentPhase = PatternPhase.None;
    }
    #endregion

    #region Charge Pattern
    private void StartChargePattern()
    {
        _currentPhase = PatternPhase.Windup;
        _phaseTimer = _currentPattern.ChargeWindup;

        // 돌진 방향 저장 (현재 플레이어 방향)
        if (_target != null)
        {
            _chargeDirection = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        }
    }

    private void UpdateChargePattern()
    {
        switch (_currentPhase)
        {
            case PatternPhase.Windup:
                if (_phaseTimer <= 0f)
                {
                    _currentPhase = PatternPhase.Execute;
                    _phaseTimer = _currentPattern.ChargeDuration;
                }
                break;

            case PatternPhase.Execute:
                // 돌진 이동
                _rb.velocity = _chargeDirection * _currentPattern.ChargeSpeed;

                if (_phaseTimer <= 0f)
                {
                    _rb.velocity = Vector2.zero;
                    EnterRecovery();
                }
                break;
        }
    }
    #endregion

    #region Area Attack Pattern
    private void StartAreaAttackPattern()
    {
        _currentPhase = PatternPhase.Windup;
        _phaseTimer = _currentPattern.AreaWindup;
    }

    private void UpdateAreaAttackPattern()
    {
        switch (_currentPhase)
        {
            case PatternPhase.Windup:
                if (_phaseTimer <= 0f)
                {
                    ExecuteAreaAttack();
                    EnterRecovery();
                }
                break;
        }
    }

    private void ExecuteAreaAttack()
    {
        if (_target == null) return;

        float sqrDistance = ((Vector2)_target.position - (Vector2)transform.position).sqrMagnitude;
        float radius = _currentPattern.AreaRadius;

        if (sqrDistance <= radius * radius)
        {
            if (_target.TryGetComponent<Player>(out var player))
            {
                player.TakeDamage(_currentPattern.Damage);
            }
        }
    }
    #endregion

    #region Projectile Pattern
    private void StartProjectilePattern()
    {
        _currentPhase = PatternPhase.Execute;
        _phaseTimer = 0.1f; // 짧은 후딜레이

        ExecuteProjectileAttack();
    }

    private void ExecuteProjectileAttack()
    {
        if (_currentPattern.ProjectilePrefab == null || _target == null) return;

        Vector2 baseDirection = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        int count = _currentPattern.ProjectileCount;
        float spreadAngle = _currentPattern.ProjectileSpreadAngle;

        // 시작 각도 계산 (중앙 정렬)
        float startAngle = -spreadAngle * (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + spreadAngle * i;
            Vector2 direction = RotateVector(baseDirection, angle);

            SpawnProjectile(direction);
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projObj = Managers.Instance.Pool.Get(BOSS_PROJECTILE_POOL_KEY);
        projObj.transform.position = transform.position;

        if (projObj.TryGetComponent<BossProjectile>(out var projectile))
        {
            projectile.Init(direction, _currentPattern.ProjectileSpeed, _currentPattern.Damage, BOSS_PROJECTILE_POOL_KEY);
        }
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
    #endregion

    #region Summon Pattern
    private void StartSummonPattern()
    {
        _currentPhase = PatternPhase.Execute;
        _phaseTimer = 0.1f;

        ExecuteSummon();
    }

    private void ExecuteSummon()
    {
        if (_currentPattern.SummonEnemyData == null) return;

        for (int i = 0; i < _currentPattern.SummonCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * _currentPattern.SummonRadius;
            Vector3 spawnPos = transform.position + (Vector3)randomOffset;

            GameObject enemyObj = Managers.Instance.Pool.Get(EnemySpawner.POOL_KEY);
            enemyObj.transform.position = spawnPos;

            if (enemyObj.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.Init(_currentPattern.SummonEnemyData, EnemySpawner.POOL_KEY);

                // 소환된 적을 스포너에 등록하여 추적/재배치 대상에 포함
                if (_enemySpawner != null)
                {
                    _enemySpawner.RegisterEnemy(enemy);
                }
            }
        }
    }
    #endregion

    #region IPoolable Override
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();

        // 보스 상태 초기화
        transform.localScale = Vector3.one;
        _bossData = null;
        _enemySpawner = null;
        _patternStates.Clear();
        _triggerHashes.Clear();
        _isExecutingPattern = false;
        _currentPattern = null;
        _currentPhase = PatternPhase.None;
    }
    #endregion
}
