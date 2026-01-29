using UnityEngine;

/// <summary>
/// 경험치 오브젝트 스폰 및 풀링을 관리하는 매니저
/// 씬에 배치하여 사용합니다.
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    #region Constants
    private const string EXP_POOL_KEY = "Experience";
    #endregion

    #region Serialized Fields
    [Header("경험치 설정")]
    [SerializeField] private GameObject _expPrefab;
    [SerializeField] private int _poolInitialSize = 50;
    #endregion

    #region Private Fields
    private PoolManager _poolManager;
    private bool _isInitialized;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Enemy.OnAnyEnemyDied += SpawnExperience;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemyDied -= SpawnExperience;
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        _poolManager = Managers.Instance.Pool;

        if (_poolManager == null)
        {
            Debug.LogError("[ExperienceManager] PoolManager를 찾을 수 없습니다.");
            return;
        }

        CreatePool();
        _isInitialized = true;
    }

    private void CreatePool()
    {
        if (_expPrefab == null)
        {
            Debug.LogWarning("[ExperienceManager] EXP 프리팹이 설정되지 않았습니다.");
            return;
        }

        if (!_poolManager.HasPool(EXP_POOL_KEY))
        {
            _poolManager.CreatePool(EXP_POOL_KEY, _expPrefab, _poolInitialSize);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 경험치 오브젝트를 스폰합니다. (이벤트 핸들러)
    /// </summary>
    /// <param name="position">스폰 위치</param>
    /// <param name="expAmount">경험치 양</param>
    private void SpawnExperience(Vector3 position, float expAmount)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[ExperienceManager] 초기화되지 않았습니다.");
            return;
        }

        if (!_poolManager.HasPool(EXP_POOL_KEY))
        {
            Debug.LogWarning("[ExperienceManager] EXP 풀이 존재하지 않습니다.");
            return;
        }

        GameObject expObj = _poolManager.Get(EXP_POOL_KEY);
        expObj.transform.position = position;

        if (expObj.TryGetComponent<Experience>(out var exp))
        {
            exp.Initialize(expAmount, _poolManager, EXP_POOL_KEY);
        }
    }
    #endregion
}