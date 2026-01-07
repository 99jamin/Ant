using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData")]
public class MapDataSO : ScriptableObject
{
    [Header("배경 청크")]
    public GameObject[] groundChunkPrefabs; 
    
    [Header("오브젝트 청크")]
    public GameObject[] objectChunkPrefabs; 
}