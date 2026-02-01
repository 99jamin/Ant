using UnityEngine;

/// <summary>
/// 투사체 오브젝트 기본 클래스
/// 직선, 포물선, 부메랑 등 다양한 투사체의 부모 클래스입니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileObject : MonoBehaviour, IPoolable
{
    #region Protected Fields
    protected Rigidbody2D _rb;
    protected float _damage;
    protected float _speed;
    protected float _lifetime;
    protected float _areaMultiplier = 1f;
    protected Vector2 _direction;
    protected float _lifetimeTimer;
    protected int _pierceCount;
    protected int _currentPierceCount;
    protected string _poolKey;
    protected string _hitEffectPoolKey;
    protected PoolManager _poolManager;
    #endregion

    #region Properties
    /// <summary>
    /// 적 레이어 (Managers에서 가져옴)
    /// </summary>
    protected LayerMask TargetLayer => Managers.Instance.EnemyLayer;
    #endregion

    #region IPoolable Implementation
    public string PoolKey => _poolKey;

    public virtual void OnSpawnFromPool()
    {
        _lifetimeTimer = _lifetime;
        _currentPierceCount = _pierceCount;
        _rb.velocity = Vector2.zero;
    }

    public virtual void OnReturnToPool()
    {
        _rb.velocity = Vector2.zero;
        _damage = 0f;
    }
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        ConfigureRigidbody();
    }

    protected virtual void Update()
    {
        UpdateLifetime();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsTargetLayer(other.gameObject.layer)) return;

        OnHitTarget(other);
    }
    #endregion

    #region Initialization
    private void ConfigureRigidbody()
    {
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    /// <summary>
    /// 투사체 초기화
    /// </summary>
    public virtual void Initialize(
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
        _damage = damage;
        _direction = dir.normalized;
        _speed = speed;
        _pierceCount = pierce;
        _currentPierceCount = pierce;
        _lifetime = lifetime;
        _lifetimeTimer = lifetime;
        _areaMultiplier = areaMultiplier;
        _poolManager = pool;
        _poolKey = key;
        _hitEffectPoolKey = hitEffectKey;

        // 크기 적용
        transform.localScale = Vector3.one * _areaMultiplier;

        // 방향에 따른 회전
        float angle = Mathf.Atan2(-_direction.y, -_direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    #endregion

    #region Movement
    protected virtual void Move()
    {
        _rb.velocity = _direction * _speed;
    }
    #endregion

    #region Lifetime
    private void UpdateLifetime()
    {
        _lifetimeTimer -= Time.deltaTime;

        if (_lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }
    #endregion

    #region Hit Detection
    private bool IsTargetLayer(int layer)
    {
        return (TargetLayer.value & (1 << layer)) != 0;
    }

    protected virtual void OnHitTarget(Collider2D target)
    {
        DamageHelper.DealDamageWithKnockback(target, _damage, _direction);

        // 히트 이펙트 스폰
        SpawnHitEffect(target.transform.position);

        if (_currentPierceCount > 0)
        {
            _currentPierceCount--;
        }
        else
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 히트 이펙트를 스폰합니다.
    /// </summary>
    protected void SpawnHitEffect(Vector3 position)
    {
        PoolableHelper.SpawnHitEffect(_poolManager, _hitEffectPoolKey, position);
    }
    #endregion

    #region Pool
    protected void ReturnToPool()
    {
        PoolableHelper.ReturnToPool(_poolManager, _poolKey, gameObject);
    }
    #endregion
}
