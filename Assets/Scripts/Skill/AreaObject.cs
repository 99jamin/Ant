using UnityEngine;

/// <summary>
/// 장판 오브젝트
/// 일정 범위 내 적에게 틱 데미지를 주고, 지속 시간 후 풀에 반환됩니다.
/// </summary>
public class AreaObject : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("디버그")]
    [SerializeField] private bool showGizmos = true;
    #endregion

    #region Private Fields
    private string _poolKey;
    private PoolManager _poolManager;
    private SpriteRenderer _spriteRenderer;

    private float _damage;
    private float _radius;
    private float _tickInterval;
    private float _duration;
    private LayerMask _enemyLayer;
    private string _hitEffectPoolKey;

    private float _durationTimer;
    private float _tickTimer;

    // GC 방지용 버퍼
    private readonly Collider2D[] _hitBuffer = new Collider2D[32];
    #endregion

    #region Properties
    public string PoolKey => _poolKey;
    #endregion

    #region Public Methods
    /// <summary>
    /// 장판 효과 초기화
    /// </summary>
    /// <param name="damage">틱당 데미지</param>
    /// <param name="radius">범위 반경</param>
    /// <param name="tickInterval">틱 간격</param>
    /// <param name="duration">지속 시간</param>
    /// <param name="enemyLayer">적 레이어</param>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="poolKey">풀 키</param>
    /// <param name="hitEffectPoolKey">히트 이펙트 풀 키 (선택)</param>
    public void Initialize(
        float damage,
        float radius,
        float tickInterval,
        float duration,
        LayerMask enemyLayer,
        PoolManager poolManager,
        string poolKey,
        string hitEffectPoolKey = null)
    {
        _damage = damage;
        _radius = radius;
        _tickInterval = tickInterval;
        _duration = duration;
        _enemyLayer = enemyLayer;
        _poolManager = poolManager;
        _poolKey = poolKey;
        _hitEffectPoolKey = hitEffectPoolKey;

        ApplyScale(radius);
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateDuration();
        UpdateTick();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, _radius > 0 ? _radius : 0.5f);
    }
    #endregion

    #region IPoolable Implementation
    public void OnSpawnFromPool()
    {
        _durationTimer = _duration;
        _tickTimer = 0f; // 스폰 즉시 첫 틱 발동
    }

    public void OnReturnToPool()
    {
        _damage = 0f;
        _radius = 0f;
        transform.localScale = Vector3.one;
    }
    #endregion

    #region Private Methods
    private void ApplyScale(float radius)
    {
        SpriteScaleHelper.ApplyRadiusScale(transform, _spriteRenderer, radius);
    }

    private void UpdateDuration()
    {
        _durationTimer -= Time.deltaTime;

        if (_durationTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void UpdateTick()
    {
        _tickTimer -= Time.deltaTime;

        if (_tickTimer <= 0f)
        {
            DealTickDamage();
            _tickTimer = _tickInterval;
        }
    }

    private void DealTickDamage()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _radius,
            _hitBuffer,
            _enemyLayer
        );

        Vector2 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            Vector2 knockbackDir = ((Vector2)_hitBuffer[i].transform.position - center).normalized;
            DamageHelper.DealDamageWithKnockback(_hitBuffer[i], _damage, knockbackDir);
            SpawnHitEffect(_hitBuffer[i].transform.position);
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        PoolableHelper.SpawnHitEffect(_poolManager, _hitEffectPoolKey, position);
    }

    private void ReturnToPool()
    {
        PoolableHelper.ReturnToPool(_poolManager, _poolKey, gameObject);
    }
    #endregion
}
