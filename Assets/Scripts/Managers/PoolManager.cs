using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링을 관리하는 매니저
/// 게임 내 자주 생성/파괴되는 오브젝트의 성능 최적화를 담당합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    #region Private Fields
    // Stack 사용: LIFO 방식으로 최근 반환된 오브젝트를 재사용하여 캐시 효율 향상
    private readonly Dictionary<string, Stack<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabs = new();
    private readonly Dictionary<string, Transform> _roots = new();
    #endregion

    #region Constants
    private const int DEFAULT_POOL_SIZE = 10;
    #endregion

    #region Public Methods
    /// <summary>
    /// 새로운 오브젝트 풀을 생성합니다.
    /// </summary>
    /// <param name="key">풀을 식별하는 고유 키</param>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 생성할 오브젝트 수</param>
    public void CreatePool(string key, GameObject prefab, int initialSize = DEFAULT_POOL_SIZE)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[PoolManager] CreatePool failed: key is null or empty");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] CreatePool failed: prefab is null for key '{key}'");
            return;
        }

        
        if (_pools.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] Pool '{key}' already exists.");
            return;
        }

        InitializePool(key, prefab, initialSize);
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다.
    /// IPoolable 인터페이스가 구현되어 있다면 OnSpawnFromPool이 호출됩니다.
    /// </summary>
    /// <param name="key">풀 키</param>
    /// <returns>활성화된 게임오브젝트</returns>
    public GameObject Get(string key)
    {
        if (!ValidatePoolExists(key, "Get")) return null;

        GameObject obj = _pools[key].Count > 0
            ? _pools[key].Pop()
            : CreateNewObject(key);

        ActivateObject(obj);
        return obj;
    }

    /// <summary>
    /// 오브젝트를 풀에 반환합니다.
    /// IPoolable 인터페이스가 구현되어 있다면 OnReturnToPool이 호출됩니다.
    /// </summary>
    /// <param name="key">풀 키</param>
    /// <param name="obj">반환할 오브젝트</param>
    public void Return(string key, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[PoolManager] Return failed: obj is null");
            return;
        }

        if (!ValidatePoolExists(key, "Return"))
        {
            Destroy(obj);
            return;
        }

        DeactivateObject(obj, key);
        _pools[key].Push(obj);
    }

    /// <summary>
    /// 풀이 존재하는지 확인합니다.
    /// </summary>
    public bool HasPool(string key) => _pools.ContainsKey(key);

    /// <summary>
    /// 풀의 현재 대기 중인 오브젝트 수를 반환합니다.
    /// </summary>
    public int GetPoolCount(string key) => _pools.TryGetValue(key, out var pool) ? pool.Count : 0;

    /// <summary>
    /// 특정 풀을 제거하고 모든 오브젝트를 파괴합니다.
    /// </summary>
    public void DestroyPool(string key)
    {
        if (!ValidatePoolExists(key, "DestroyPool")) return;

        // 풀의 모든 오브젝트 파괴
        while (_pools[key].Count > 0)
        {
            GameObject obj = _pools[key].Pop();
            if (obj != null) Destroy(obj);
        }

        // 루트 오브젝트 파괴
        if (_roots.TryGetValue(key, out Transform root) && root != null)
        {
            Destroy(root.gameObject);
        }

        // 딕셔너리에서 제거
        _pools.Remove(key);
        _prefabs.Remove(key);
        _roots.Remove(key);
    }

    /// <summary>
    /// 모든 풀을 제거합니다.
    /// </summary>
    public void ClearAllPools()
    {
        var keys = new List<string>(_pools.Keys);
        foreach (string key in keys)
        {
            DestroyPool(key);
        }
    }
    #endregion

    #region Private Methods
    private void InitializePool(string key, GameObject prefab, int initialSize)
    {
        // 루트 오브젝트 생성
        Transform root = CreatePoolRoot(key);
        _roots[key] = root;
        _prefabs[key] = prefab;
        _pools[key] = new Stack<GameObject>(initialSize);

        // 초기 오브젝트 생성
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject(key);
            obj.SetActive(false);
            _pools[key].Push(obj);
        }
    }

    private Transform CreatePoolRoot(string key)
    {
        GameObject root = new($"{key}_Pool");
        root.transform.SetParent(transform);
        return root.transform;
    }

    private GameObject CreateNewObject(string key)
    {
        GameObject obj = Instantiate(_prefabs[key], _roots[key]);
        obj.name = _prefabs[key].name;
        return obj;
    }

    private void ActivateObject(GameObject obj)
    {
        obj.SetActive(true);

        // IPoolable 콜백 호출
        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnSpawnFromPool();
        }
    }

    private void DeactivateObject(GameObject obj, string key)
    {
        // IPoolable 콜백 호출
        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnReturnToPool();
        }

        obj.SetActive(false);
        obj.transform.SetParent(_roots[key]);
    }

    private bool ValidatePoolExists(string key, string methodName)
    {
        if (_pools.ContainsKey(key)) return true;

        Debug.LogError($"[PoolManager] {methodName} failed: Pool '{key}' does not exist.");
        return false;
    }
    #endregion
}