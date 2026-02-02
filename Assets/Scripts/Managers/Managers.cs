using UnityEngine;

/// <summary>
/// 게임의 모든 매니저를 통합 관리하는 싱글톤 클래스
/// DontDestroyOnLoad로 씬 전환 시에도 유지됩니다.
/// </summary>
public class Managers : MonoBehaviour
{
    #region Singleton
    private static Managers _instance;
    private static bool _isQuitting;

    public static Managers Instance
    {
        get
        {
            if (_isQuitting) return null;

            if (_instance == null)
            {
                InitializeInstance();
            }
            return _instance;
        }
    }

    private static void InitializeInstance()
    {
        _instance = FindFirstObjectByType<Managers>();

        if (_instance == null)
        {
            GameObject go = new GameObject("@Managers");
            _instance = go.AddComponent<Managers>();
            Debug.Log("[Managers] Auto-created Managers instance.");
        }
    }
    #endregion

    #region Game Settings
    [Header("레이어 설정")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask experienceLayer;

    /// <summary>
    /// 적 레이어 마스크
    /// </summary>
    public LayerMask EnemyLayer => enemyLayer;

    /// <summary>
    /// 경험치 오브젝트 레이어 마스크
    /// </summary>
    public LayerMask ExperienceLayer => experienceLayer;
    #endregion

    #region Sub Managers
    private PoolManager _pool;
    private UIManager _ui;

    /// <summary>
    /// 오브젝트 풀링을 관리하는 매니저
    /// </summary>
    public PoolManager Pool => _pool;

    /// <summary>
    /// UI 시스템을 관리하는 매니저
    /// </summary>
    public UIManager UI => _ui;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else if (_instance != this)
        {
            Debug.LogWarning("[Managers] Duplicate instance detected. Destroying...");
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
    #endregion

    #region Initialization
    private void InitializeManagers()
    {
        _pool = GetOrCreateManager<PoolManager>("PoolManager");
        _ui = GetOrCreateManager<UIManager>("UIManager");

        Debug.Log("[Managers] All managers initialized.");
    }

    private T GetOrCreateManager<T>(string managerName) where T : Component
    {
        T manager = GetComponentInChildren<T>();

        if (manager == null)
        {
            GameObject go = new GameObject(managerName);
            go.transform.SetParent(transform);
            manager = go.AddComponent<T>();
        }

        return manager;
    }
    #endregion
}