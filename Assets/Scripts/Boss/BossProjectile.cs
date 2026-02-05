using UnityEngine;

/// <summary>
/// 보스 투사체
/// 플레이어에게 데미지를 주고 사라집니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("설정")]
    [SerializeField] private float lifetime = 5f;
    #endregion

    #region Private Fields
    private Rigidbody2D _rb;
    private float _damage;
    private string _poolKey;
    private float _lifetimeTimer;
    #endregion

    #region IPoolable
    public string PoolKey => _poolKey;

    public void OnSpawnFromPool()
    {
        _lifetimeTimer = lifetime;
    }

    public void OnReturnToPool()
    {
        _rb.velocity = Vector2.zero;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _lifetimeTimer -= Time.deltaTime;

        if (_lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(_damage);
            ReturnToPool();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 투사체를 초기화합니다.
    /// </summary>
    public void Init(Vector2 direction, float speed, float damage, string poolKey)
    {
        _damage = damage;
        _poolKey = poolKey;
        _lifetimeTimer = lifetime;

        _rb.velocity = direction.normalized * speed;

        // 투사체 회전 (진행 방향을 바라보도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    #endregion

    #region Private Methods
    private void ReturnToPool()
    {
        if (!string.IsNullOrEmpty(_poolKey) && Managers.Instance != null)
        {
            Managers.Instance.Pool.Return(_poolKey, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
