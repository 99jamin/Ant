using UnityEngine;

/// <summary>
/// 오라 오브젝트 (마늘형 스킬)
/// 플레이어를 따라다니며 범위 내 적에게 지속 데미지를 줍니다.
/// </summary>
public class AuraObject : MonoBehaviour
{
    #region Serialized Fields
    [Header("디버그")]
    [SerializeField] private bool showGizmos = true;
    #endregion

    #region Private Fields
    private float _radius;
    private float _damage;
    private float _tickInterval;
    private LayerMask _enemyLayer;
    private PoolManager _poolManager;
    private string _hitEffectPoolKey;
    private SpriteRenderer _spriteRenderer;

    private float _tickTimer;

    // GC 방지용 버퍼
    private readonly Collider2D[] _hitBuffer = new Collider2D[32];
    #endregion

    #region Properties
    public float Radius => _radius;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_damage <= 0f) return;

        _tickTimer -= Time.deltaTime;

        if (_tickTimer <= 0f)
        {
            DealTickDamage();
            _tickTimer = _tickInterval;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _radius > 0 ? _radius : 0.5f);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 오라 오브젝트 초기화
    /// </summary>
    /// <param name="radius">오라 반경</param>
    /// <param name="damage">틱당 데미지</param>
    /// <param name="tickInterval">틱 간격</param>
    /// <param name="enemyLayer">적 레이어</param>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="hitEffectPoolKey">히트 이펙트 풀 키</param>
    public void Initialize(float radius, float damage, float tickInterval, LayerMask enemyLayer,
        PoolManager poolManager, string hitEffectPoolKey)
    {
        _radius = radius;
        _damage = damage;
        _tickInterval = tickInterval;
        _enemyLayer = enemyLayer;
        _poolManager = poolManager;
        _hitEffectPoolKey = hitEffectPoolKey;
        _tickTimer = 0f; // 즉시 첫 틱 발동

        ApplyScale(radius);
    }

    /// <summary>
    /// 오라 설정 업데이트 (레벨업/글로벌 스탯 변경 시 호출)
    /// </summary>
    public void UpdateStats(float newRadius, float newDamage, float newTickInterval)
    {
        _radius = newRadius;
        _damage = newDamage;
        _tickInterval = newTickInterval;
        ApplyScale(newRadius);
    }
    #endregion

    #region Private Methods
    private void ApplyScale(float radius)
    {
        SpriteScaleHelper.ApplyRadiusScale(transform, _spriteRenderer, radius);
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
            PoolableHelper.SpawnHitEffect(_poolManager, _hitEffectPoolKey, _hitBuffer[i].transform.position);
        }
    }
    #endregion
}
