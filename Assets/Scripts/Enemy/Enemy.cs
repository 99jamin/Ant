using System;
using UnityEngine;

/// <summary>
/// 적 캐릭터의 기본 동작을 담당하는 클래스
/// IDamageable: 데미지 처리, IPoolable: 오브젝트 풀링 지원
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IDamageable, IPoolable
{
    #region Animator Hashes
    private static readonly int DeadTrigger = Animator.StringToHash("Dead");
    #endregion

    #region Static Events
    /// <summary>
    /// 적이 죽었을 때 발생하는 정적 이벤트 (위치, 경험치량)
    /// </summary>
    public static event Action<Vector3, float> OnAnyEnemyDied;
    #endregion

    #region Events
    public event Action<Enemy> OnDeath;
    public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    #endregion

    #region IDamageable Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _data != null ? _data.Health : 0f;
    public bool IsDead => _currentHealth <= 0f;
    #endregion

    #region IPoolable Properties
    public string PoolKey => _poolKey;
    #endregion

    #region Public Properties
    public EnemyDataSO Data => _data;
    #endregion

    #region Serialized Fields (Hit Effect Settings)
    [Header("넉백 설정")]
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.1f;

    [Header("무적 설정")]
    [SerializeField] private float _invincibilityDuration = 0.2f;

    [Header("반짝임 설정")]
    [SerializeField] private Material _flashMaterial;
    [SerializeField] private float _hitFlashDuration = 0.1f;

    [Header("사망 설정")]
    [SerializeField] private float _deathAnimationDuration = 0.5f;
    #endregion

    #region Private Fields
    private EnemyDataSO _data;
    private string _poolKey;
    private float _currentHealth;
    private bool _isInitialized;

    // Cached Components
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    // References
    private Transform _target;

    // Hit Effect State
    private bool _isInvincible;
    private bool _isKnockedBack;
    private float _invincibilityTimer;
    private float _knockbackTimer;
    private float _hitFlashTimer;
    private Material _originalMaterial;

    // Death State
    private bool _isDying;
    private float _deathTimer;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // 사망 애니메이션 타이머
        if (_isDying)
        {
            _deathTimer -= Time.deltaTime;
            if (_deathTimer <= 0f)
            {
                ReturnToPool();
            }
            return;
        }

        if (IsDead) return;

        UpdateHitEffectTimers();
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || IsDead) return;

        // 넉백 중에는 추적하지 않음
        if (_isKnockedBack) return;

        ChaseTarget();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_isInitialized || _isDying) return;

        // 플레이어와 충돌 시 데미지
        if (collision.collider.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(_data.Damage);
        }
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 원본 Material 저장
        if (_spriteRenderer != null)
        {
            _originalMaterial = _spriteRenderer.material;
        }
    }

    /// <summary>
    /// 외부에서 데이터를 주입받아 초기화합니다.
    /// </summary>
    /// <param name="data">적의 스탯 및 외형 데이터</param>
    /// <param name="poolKey">풀 반환 시 사용할 키</param>
    public void Init(EnemyDataSO data, string poolKey)
    {
        if (data == null)
        {
            Debug.LogError("[Enemy] Init failed: data is null", this);
            return;
        }

        _data = data;
        _poolKey = poolKey;
        _currentHealth = data.Health;
        _isInitialized = true;

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (_spriteRenderer != null && _data.Sprite != null)
        {
            _spriteRenderer.sprite = _data.Sprite;
        }

        if (_animator != null && _data.AnimatorController != null)
        {
            _animator.runtimeAnimatorController = _data.AnimatorController;
        }
    }
    #endregion

    #region Movement
    private void ChaseTarget()
    {
        if (_target == null) return;

        Vector2 direction = GetDirectionToTarget();
        Move(direction);
        UpdateSpriteDirection(direction);
    }

    private Vector2 GetDirectionToTarget()
    {
        return ((Vector2)_target.position - (Vector2)transform.position).normalized;
    }

    private void Move(Vector2 direction)
    {
        _rb.velocity = direction * _data.Speed;
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
    }
    #endregion

    #region IDamageable Implementation
    public void TakeDamage(float damage)
    {
        // 기본 호출 시 넉백 방향 없이 처리
        TakeDamage(damage, Vector2.zero);
    }

    /// <summary>
    /// 데미지와 함께 넉백 효과를 적용합니다.
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    /// <param name="knockbackDirection">넉백 방향 (정규화된 벡터)</param>
    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        if (IsDead || _isInvincible) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        // 타격 효과 적용
        ApplyHitEffects(knockbackDirection);

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }
    #endregion

    #region Hit Effects
    private void ApplyHitEffects(Vector2 knockbackDirection)
    {
        ApplyKnockback(knockbackDirection);
        ApplyInvincibility();
        ApplyHitFlash();
    }

    private void ApplyKnockback(Vector2 direction)
    {
        if (direction == Vector2.zero || _knockbackForce <= 0f) return;

        _isKnockedBack = true;
        _knockbackTimer = _knockbackDuration;

        // 기존 속도 초기화 후 넉백 적용
        _rb.velocity = Vector2.zero;
        _rb.AddForce(direction.normalized * _knockbackForce, ForceMode2D.Impulse);
    }

    private void ApplyInvincibility()
    {
        _isInvincible = true;
        _invincibilityTimer = _invincibilityDuration;
    }

    private void ApplyHitFlash()
    {
        if (_flashMaterial == null) return;

        _hitFlashTimer = _hitFlashDuration;
        _spriteRenderer.material = _flashMaterial;
    }

    private void UpdateHitEffectTimers()
    {
        float deltaTime = Time.deltaTime;

        // 넉백 타이머
        if (_isKnockedBack)
        {
            _knockbackTimer -= deltaTime;
            if (_knockbackTimer <= 0f)
            {
                _isKnockedBack = false;
                _rb.velocity = Vector2.zero;
            }
        }

        // 무적 타이머
        if (_isInvincible)
        {
            _invincibilityTimer -= deltaTime;
            if (_invincibilityTimer <= 0f)
            {
                _isInvincible = false;
            }
        }

        // 반짝임 타이머
        if (_hitFlashTimer > 0f)
        {
            _hitFlashTimer -= deltaTime;
            if (_hitFlashTimer <= 0f)
            {
                _spriteRenderer.material = _originalMaterial;
            }
        }
    }
    #endregion

    #region IPoolable Implementation
    public void OnSpawnFromPool()
    {
        FindTarget();
        _isInitialized = false;
    }

    public void OnReturnToPool()
    {
        ResetState();
        ClearEvents();
    }

    private void FindTarget()
    {
        if (Player.Instance != null)
        {
            _target = Player.Instance.transform;
        }
    }

    private void ResetState()
    {
        _rb.velocity = Vector2.zero;
        _currentHealth = 0f;
        _isInitialized = false;
        _data = null;

        // 타격 효과 상태 리셋
        _isInvincible = false;
        _isKnockedBack = false;
        _invincibilityTimer = 0f;
        _knockbackTimer = 0f;
        _hitFlashTimer = 0f;

        // 사망 상태 리셋
        _isDying = false;
        _deathTimer = 0f;

        // 콜라이더 다시 활성화
        if (_collider != null)
        {
            _collider.enabled = true;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.material = _originalMaterial;
        }
    }

    private void ClearEvents()
    {
        OnDeath = null;
        OnHealthChanged = null;
    }
    #endregion

    #region Death
    private void Die()
    {
        if (_isDying) return;

        _isDying = true;
        _deathTimer = _deathAnimationDuration;

        // 이동 정지
        _rb.velocity = Vector2.zero;

        // 콜라이더 비활성화 (다른 적에게 밀리지 않도록)
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        // 플래시 Material 즉시 복원
        if (_spriteRenderer != null)
        {
            _spriteRenderer.material = _originalMaterial;
        }

        // 사망 애니메이션 트리거
        if (_animator != null)
        {
            _animator.SetTrigger(DeadTrigger);
        }

        // 경험치 드랍 이벤트 발행
        if (_data != null && _data.ExpAmount > 0f)
        {
            OnAnyEnemyDied?.Invoke(transform.position, _data.ExpAmount);
        }

        OnDeath?.Invoke(this);
    }

    private void ReturnToPool()
    {
        PoolableHelper.ReturnToPool(Managers.Instance.Pool, PoolKey, gameObject);
    }
    #endregion
}