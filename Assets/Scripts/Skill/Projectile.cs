using UnityEngine;

/// <summary>
/// 투사체 기본 클래스
/// 직선, 포물선, 부메랑 등 다양한 투사체의 부모 클래스입니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour, IPoolable
{
    #region Protected Fields
    protected Rigidbody2D rb;
    protected float damage;
    protected float speed;
    protected float lifetime;
    protected float areaMultiplier = 1f;
    protected Vector2 direction;
    protected float lifetimeTimer;
    protected int pierceCount;
    protected int currentPierceCount;
    protected string poolKey;
    protected string hitEffectPoolKey;
    protected PoolManager poolManager;
    #endregion

    #region Properties
    /// <summary>
    /// 적 레이어 (Managers에서 가져옴)
    /// </summary>
    protected LayerMask TargetLayer => Managers.Instance.EnemyLayer;
    #endregion

    #region IPoolable Implementation
    public string PoolKey => poolKey;

    public virtual void OnSpawnFromPool()
    {
        lifetimeTimer = lifetime;
        currentPierceCount = pierceCount;
        rb.velocity = Vector2.zero;
    }

    public virtual void OnReturnToPool()
    {
        rb.velocity = Vector2.zero;
        damage = 0f;
    }
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
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
        this.damage = damage;
        this.direction = dir.normalized;
        this.speed = speed;
        this.pierceCount = pierce;
        this.currentPierceCount = pierce;
        this.lifetime = lifetime;
        this.lifetimeTimer = lifetime;
        this.areaMultiplier = areaMultiplier;
        this.poolManager = pool;
        this.poolKey = key;
        this.hitEffectPoolKey = hitEffectKey;

        // 크기 적용
        transform.localScale = Vector3.one * areaMultiplier;

        // 방향에 따른 회전
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    #endregion

    #region Movement
    protected virtual void Move()
    {
        rb.velocity = direction * speed;
    }
    #endregion

    #region Lifetime
    private void UpdateLifetime()
    {
        lifetimeTimer -= Time.deltaTime;

        if (lifetimeTimer <= 0f)
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
        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }

        // 히트 이펙트 스폰
        SpawnHitEffect(target.transform.position);

        if (currentPierceCount > 0)
        {
            currentPierceCount--;
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
        if (string.IsNullOrEmpty(hitEffectPoolKey)) return;
        if (poolManager == null || !poolManager.HasPool(hitEffectPoolKey)) return;

        GameObject effectObj = poolManager.Get(hitEffectPoolKey);
        effectObj.transform.position = position;

        if (effectObj.TryGetComponent<HitEffect>(out var hitEffect))
        {
            hitEffect.Initialize(poolManager, hitEffectPoolKey);
        }
    }
    #endregion

    #region Pool
    protected void ReturnToPool()
    {
        if (poolManager != null)
        {
            poolManager.Return(poolKey, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    #endregion
}
