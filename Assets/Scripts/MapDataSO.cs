using UnityEngine;

/// <summary>
/// 맵 생성에 필요한 프리팹 데이터를 관리하는 ScriptableObject
/// 지형 청크와 오브젝트 청크 프리팹을 저장합니다.
/// </summary>
[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 0)]
public class MapDataSO : ScriptableObject
{
    #region Serialized Fields
    [Header("배경 청크")]
    [Tooltip("지형 배경으로 사용할 타일맵 프리팹들")]
    [SerializeField] private GameObject[] groundChunkPrefabs;

    [Header("오브젝트 청크")]
    [Tooltip("장애물/오브젝트로 사용할 프리팹들")]
    [SerializeField] private GameObject[] objectChunkPrefabs;
    #endregion

    #region Properties
    public int GroundPrefabCount => groundChunkPrefabs?.Length ?? 0;
    public int ObjectPrefabCount => objectChunkPrefabs?.Length ?? 0;
    #endregion

    #region Public Methods
    /// <summary>
    /// 랜덤한 지형 프리팹을 반환합니다.
    /// </summary>
    public GameObject GetRandomGroundPrefab()
    {
        if (groundChunkPrefabs == null || groundChunkPrefabs.Length == 0)
        {
            Debug.LogError("[MapDataSO] Ground chunk prefabs are not configured.");
            return null;
        }

        int randomIndex = Random.Range(0, groundChunkPrefabs.Length);
        return groundChunkPrefabs[randomIndex];
    }

    /// <summary>
    /// 랜덤한 오브젝트 프리팹을 반환합니다.
    /// </summary>
    public GameObject GetRandomObjectPrefab()
    {
        if (objectChunkPrefabs == null || objectChunkPrefabs.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, objectChunkPrefabs.Length);
        return objectChunkPrefabs[randomIndex];
    }

    /// <summary>
    /// 인덱스로 지형 프리팹을 가져옵니다.
    /// </summary>
    public GameObject GetGroundPrefab(int index)
    {
        if (!IsValidGroundIndex(index)) return null;
        return groundChunkPrefabs[index];
    }

    /// <summary>
    /// 인덱스로 오브젝트 프리팹을 가져옵니다.
    /// </summary>
    public GameObject GetObjectPrefab(int index)
    {
        if (!IsValidObjectIndex(index)) return null;
        return objectChunkPrefabs[index];
    }
    #endregion

    #region Validation
    private bool IsValidGroundIndex(int index)
    {
        return groundChunkPrefabs != null && index >= 0 && index < groundChunkPrefabs.Length;
    }

    private bool IsValidObjectIndex(int index)
    {
        return objectChunkPrefabs != null && index >= 0 && index < objectChunkPrefabs.Length;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidatePrefabArrays();
    }

    private void ValidatePrefabArrays()
    {
        if (groundChunkPrefabs != null)
        {
            for (int i = 0; i < groundChunkPrefabs.Length; i++)
            {
                if (groundChunkPrefabs[i] == null)
                {
                    Debug.LogWarning($"[MapDataSO] Ground chunk prefab at index {i} is null.", this);
                }
            }
        }
    }
#endif
    #endregion
}