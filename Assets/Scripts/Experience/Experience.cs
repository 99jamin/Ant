using UnityEngine;

/// <summary>
/// 경험치 오브젝트
/// 적이 죽을 때 스폰되며, 플레이어와 충돌 시 경험치를 부여합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Experience : MonoBehaviour, IPoolable
{
    #region IPoolable Properties
    public string PoolKey => _poolKey;
    #endregion

    #region Private Fields
    private string _poolKey;
    private float _expAmount;
    private PoolManager _poolManager;
    #endregion

    #region Public Methods
    /// <summary>
    /// 경험치 오브젝트를 초기화합니다.
    /// </summary>
    /// <param name="expAmount">경험치 양</param>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="poolKey">풀 키</param>
    public void Initialize(float expAmount, PoolManager poolManager, string poolKey)
    {
        _expAmount = expAmount;
        _poolManager = poolManager;
        _poolKey = poolKey;
    }
    #endregion

    #region Collision
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            player.GainExperience(_expAmount);
            ReturnToPool();
        }
    }
    #endregion

    #region IPoolable Implementation
    public void OnSpawnFromPool()
    {
        // 스폰 시 초기화 (Initialize에서 처리)
    }

    public void OnReturnToPool()
    {
        _expAmount = 0f;
    }
    #endregion

    #region Private Methods
    private void ReturnToPool()
    {
        if (_poolManager != null && !string.IsNullOrEmpty(_poolKey))
        {
            _poolManager.Return(_poolKey, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}