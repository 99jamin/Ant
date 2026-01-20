using UnityEngine;

/// <summary>
/// 무한 맵 시스템을 관리하는 매니저
/// 플레이어 주변 3x3 청크를 유지하며, 플레이어 이동에 따라 동적으로 재배치합니다.
/// </summary>
public class MapManager : MonoBehaviour
{
    #region Constants
    private const int GRID_SIZE = 3;
    private const int TOTAL_CHUNKS = GRID_SIZE * GRID_SIZE;
    private const float REPOSITION_THRESHOLD_MULTIPLIER = 1.5f;
    private const float REPOSITION_DISTANCE_MULTIPLIER = 3f;
    #endregion

    #region Serialized Fields
    [Header("맵 데이터")]
    [Tooltip("맵 생성에 사용할 데이터")]
    [SerializeField] private MapDataSO mapData;

    [Header("청크 설정")]
    [Tooltip("각 청크의 크기 (Unity 단위)")]
    [SerializeField] private float chunkSize = 30f;
    #endregion

    #region Private Fields
    private GameObject[] _chunks;
    private bool _isInitialized;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        if (!_isInitialized) return;

        UpdateAllChunks();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        if (!ValidateReferences()) return;

        _chunks = new GameObject[TOTAL_CHUNKS];
        GenerateInitialChunks();
        _isInitialized = true;
    }

    private bool ValidateReferences()
    {
        if (mapData == null)
        {
            Debug.LogError("[MapManager] MapData is not assigned.", this);
            return false;
        }

        if (Player.Instance == null)
        {
            Debug.LogError("[MapManager] Player.Instance is null.", this);
            return false;
        }

        return true;
    }

    private void GenerateInitialChunks()
    {
        int index = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 position = CalculateChunkPosition(x, y);
                _chunks[index] = CreateChunk(position);
                index++;
            }
        }
    }

    private Vector3 CalculateChunkPosition(int gridX, int gridY)
    {
        return new Vector3(gridX * chunkSize, gridY * chunkSize, 0f);
    }
    #endregion

    #region Chunk Creation
    private GameObject CreateChunk(Vector3 position)
    {
        GameObject groundChunk = SpawnGroundChunk(position);
        SpawnObjectsOnChunk(groundChunk);
        return groundChunk;
    }

    private GameObject SpawnGroundChunk(Vector3 position)
    {
        GameObject prefab = mapData.GetRandomGroundPrefab();
        return Instantiate(prefab, position, Quaternion.identity, transform);
    }

    private void SpawnObjectsOnChunk(GameObject chunk)
    {
        GameObject prefab = mapData.GetRandomObjectPrefab();
        if (prefab != null)
        {
            Instantiate(prefab, chunk.transform.position, Quaternion.identity, chunk.transform);
        }
    }
    #endregion

    #region Chunk Repositioning
    private void UpdateAllChunks()
    {
        foreach (GameObject chunk in _chunks)
        {
            if (chunk != null)
            {
                RepositionChunkIfNeeded(chunk);
            }
        }
    }

    private void RepositionChunkIfNeeded(GameObject chunk)
    {
        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 chunkPos = chunk.transform.position;

        Vector3 difference = playerPos - chunkPos;
        float threshold = chunkSize * REPOSITION_THRESHOLD_MULTIPLIER;
        float repositionDistance = chunkSize * REPOSITION_DISTANCE_MULTIPLIER;

        // X축 재배치
        if (Mathf.Abs(difference.x) > threshold)
        {
            float direction = Mathf.Sign(difference.x);
            chunk.transform.position += Vector3.right * (direction * repositionDistance);
        }

        // Y축 재배치
        if (Mathf.Abs(difference.y) > threshold)
        {
            float direction = Mathf.Sign(difference.y);
            chunk.transform.position += Vector3.up * (direction * repositionDistance);
        }
    }
    #endregion

    #region Editor Visualization
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_chunks == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        foreach (GameObject chunk in _chunks)
        {
            if (chunk != null)
            {
                Vector3 center = chunk.transform.position;
                Vector3 size = new Vector3(chunkSize, chunkSize, 0.1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
#endif
    #endregion
}