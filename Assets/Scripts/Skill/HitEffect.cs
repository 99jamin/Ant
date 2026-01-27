using UnityEngine;

/// <summary>
/// 히트 이펙트 컴포넌트
/// 애니메이션 또는 파티클 재생 후 자동으로 풀에 반환됩니다.
/// </summary>
public class HitEffect : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("이펙트 설정")]
    [SerializeField] private float duration = 0.5f;
    #endregion

    #region Private Fields
    private string _poolKey;
    private PoolManager _poolManager;
    private float _timer;
    private Animator _animator;
    private ParticleSystem _particleSystem;
    #endregion

    #region Properties
    public string PoolKey => _poolKey;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            ReturnToPool();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 이펙트 초기화
    /// </summary>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="poolKey">풀 키</param>
    public void Initialize(PoolManager poolManager, string poolKey)
    {
        _poolManager = poolManager;
        _poolKey = poolKey;
    }
    #endregion

    #region IPoolable Implementation
    public void OnSpawnFromPool()
    {
        // 애니메이터 재생 시간 계산
        if (_animator != null)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _timer = stateInfo.length > 0 ? stateInfo.length : duration;
        }
        // 파티클 재생 시간 계산
        else if (_particleSystem != null)
        {
            _timer = _particleSystem.main.duration;
            _particleSystem.Play();
        }
        // 둘 다 없으면 기본 duration 사용
        else
        {
            _timer = duration;
        }
    }

    public void OnReturnToPool()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Stop();
            _particleSystem.Clear();
        }
    }
    #endregion

    #region Private Methods
    private void ReturnToPool()
    {
        PoolableHelper.ReturnToPool(_poolManager, _poolKey, gameObject);
    }
    #endregion
}
