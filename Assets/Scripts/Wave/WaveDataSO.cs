using System;
using UnityEngine;

/// <summary>
/// 웨이브 시스템 전체 데이터를 정의하는 ScriptableObject
/// 시간대별 적 스폰 설정과 보스 스폰 타이밍을 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 2)]
public class WaveDataSO : ScriptableObject
{
    [Header("웨이브 설정")]
    [Tooltip("시간대별 웨이브 엔트리 (startTime 오름차순으로 정렬)")]
    [SerializeField] private WaveEntry[] waveEntries;

    [Header("보스 설정")]
    [Tooltip("보스 스폰 엔트리")]
    [SerializeField] private BossEntry[] bossEntries;

    public WaveEntry[] WaveEntries => waveEntries;
    public BossEntry[] BossEntries => bossEntries;

    /// <summary>
    /// 주어진 시간에 해당하는 웨이브 엔트리를 반환합니다.
    /// </summary>
    public WaveEntry GetWaveEntryForTime(float time)
    {
        if (waveEntries == null || waveEntries.Length == 0) return null;

        // 첫 웨이브 시작 시간 이전이면 null 반환
        if (time < waveEntries[0].StartTime) return null;

        WaveEntry result = waveEntries[0];

        for (int i = 1; i < waveEntries.Length; i++)
        {
            if (waveEntries[i].StartTime <= time)
            {
                result = waveEntries[i];
            }
            else
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 주어진 시간에 해당하는 웨이브 인덱스를 반환합니다.
    /// </summary>
    public int GetWaveIndexForTime(float time)
    {
        if (waveEntries == null || waveEntries.Length == 0) return -1;

        // 첫 웨이브 시작 시간 이전이면 -1 반환
        if (time < waveEntries[0].StartTime) return -1;

        int result = 0;

        for (int i = 1; i < waveEntries.Length; i++)
        {
            if (waveEntries[i].StartTime <= time)
            {
                result = i;
            }
            else
            {
                break;
            }
        }

        return result;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 웨이브 엔트리를 startTime 기준으로 정렬
        if (waveEntries != null && waveEntries.Length > 1)
        {
            Array.Sort(waveEntries, (a, b) => a.StartTime.CompareTo(b.StartTime));
        }

        // 보스 엔트리를 spawnTime 기준으로 정렬
        if (bossEntries != null && bossEntries.Length > 1)
        {
            Array.Sort(bossEntries, (a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
        }
    }
#endif
}

/// <summary>
/// 시간대별 적 스폰 설정
/// </summary>
[Serializable]
public class WaveEntry
{
    [Tooltip("이 웨이브가 시작되는 시간 (초)")]
    [SerializeField] private float startTime;

    [Tooltip("이 웨이브에서 스폰할 적 종류들")]
    [SerializeField] private EnemyDataSO[] enemyTypes;

    [Tooltip("적 스폰 간격 (초)")]
    [SerializeField] [Min(0.1f)] private float spawnInterval = 1f;

    [Tooltip("한 번에 스폰할 적 수")]
    [SerializeField] [Min(1)] private int spawnCount = 1;

    public float StartTime => startTime;
    public EnemyDataSO[] EnemyTypes => enemyTypes;
    public float SpawnInterval => spawnInterval;
    public int SpawnCount => spawnCount;
}

/// <summary>
/// 보스 스폰 설정
/// </summary>
[Serializable]
public class BossEntry
{
    [Tooltip("보스가 스폰되는 시간 (초)")]
    [SerializeField] private float spawnTime;

    [Tooltip("보스 데이터 (BossDataSO)")]
    [SerializeField] private BossDataSO bossData;

    public float SpawnTime => spawnTime;
    public BossDataSO BossData => bossData;
}
