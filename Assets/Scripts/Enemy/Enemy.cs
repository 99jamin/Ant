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

    #region Private Fields
    private EnemyDataSO _data;
    private string _poolKey;
    private float _currentHealth;
    private bool _isInitialized;

    // Cached Components
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    // References
    private Transform _target;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || IsDead) return;

        ChaseTarget();
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
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
        if (IsDead) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

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
        OnDeath?.Invoke(this);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        Managers.Instance.Pool.Return(PoolKey, gameObject);
    }
    #endregion
}