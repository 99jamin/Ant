using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MapDataSO mapData;
    [SerializeField] private float chunkSize = 30f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    private GameObject[] _chunks = new GameObject[9];
    
    void Start()
    {
        InitializeMap();
    }
    
    private void InitializeMap()
    {
        int index = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 pos = new Vector3(x * chunkSize, y * chunkSize, 0);
               _chunks[index] = SpawnChunk(pos);
                index++;
            }
        }
    }
    
    private GameObject SpawnChunk(Vector3 position)
    {
        var randomIndex = Random.Range(0, mapData.groundChunkPrefabs.Length);
        var tilemap =  Instantiate(mapData.groundChunkPrefabs[randomIndex], position, Quaternion.identity, transform);
        SpawnObjects(tilemap);
        
        return tilemap;
    }

    private void SpawnObjects(GameObject chunk)
    {
        var randomIndex = Random.Range(0, mapData.objectChunkPrefabs.Length);
        Instantiate(mapData.objectChunkPrefabs[randomIndex], chunk.transform.position, Quaternion.identity, chunk.transform);
    }
    
    
    private void LateUpdate()
    {
        foreach (GameObject chunk in _chunks)
        {
            UpdateChunkPosition(chunk);
        }
    }
    
    private void UpdateChunkPosition(GameObject chunk)
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 chunkPos = chunk.transform.position;

        float diffX = playerPos.x - chunkPos.x;
        float diffY = playerPos.y - chunkPos.y;
        float limit = chunkSize * 1.5f;

        // X축 재배치
        if (Mathf.Abs(diffX) > limit)
        {
            float dirX = diffX > 0 ? 1 : -1;
            chunk.transform.position += Vector3.right * (dirX * chunkSize * 3);
        }

        // Y축 재배치
        if (Mathf.Abs(diffY) > limit)
        {
            float dirY = diffY > 0 ? 1 : -1;
            chunk.transform.position += Vector3.up * (dirY * chunkSize * 3);
        }
    }
}
