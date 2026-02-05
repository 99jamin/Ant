using System;
using UnityEngine;

/// <summary>
/// 전투 전용 매니저
/// 웨이브 시스템, 보스 스폰, 생존 시간 추적 등을 담당합니다.
/// 배틀 씬에서만 존재합니다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("웨이브 설정")]
    [SerializeField] private WaveDataSO waveData;

    [Header("참조")]
    [SerializeField] private EnemySpawner enemySpawner;
    #endregion

    #region Fields
    private float _survivalTime;
    private bool _isRunning;
    private int _currentWaveIndex = -1;
    private int _nextBossIndex;
    #endregion

    #region Properties
    /// <summary>
    /// 현재 생존 시간 (초)
    /// </summary>
    public float SurvivalTime => _survivalTime;

    /// <summary>
    /// 현재 웨이브 인덱스 (0부터 시작)
    /// </summary>
    public int CurrentWaveIndex => _currentWaveIndex;
    #endregion

    #region Events
    /// <summary>
    /// 전투 시간이 갱신될 때 발행되는 이벤트
    /// </summary>
    public event Action<float> OnBattleTimeUpdated;

    /// <summary>
    /// 전투가 완료(시간 초과 등)되었을 때 발행되는 이벤트
    /// </summary>
    public event Action OnBattleComplete;

    /// <summary>
    /// 웨이브가 변경될 때 발행되는 이벤트 (웨이브 인덱스)
    /// </summary>
    public event Action<int> OnWaveChanged;

    /// <summary>
    /// 보스가 스폰될 때 발행되는 이벤트 (보스 데이터)
    /// </summary>
    public event Action<BossDataSO> OnBossSpawned;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // 배틀 씬 진입 시 자동으로 전투 시작
        StartBattle();
    }

    private void Update()
    {
        if (!_isRunning) return;

        _survivalTime += Time.deltaTime;
        OnBattleTimeUpdated?.Invoke(_survivalTime);

        UpdateWave();
        UpdateBossSpawn();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 전투를 시작합니다. 생존 시간을 초기화하고 타이머를 시작합니다.
    /// </summary>
    public void StartBattle()
    {
        _survivalTime = 0f;
        _currentWaveIndex = -1;
        _nextBossIndex = 0;
        _isRunning = true;

        // 첫 웨이브 즉시 적용
        UpdateWave();

        Debug.Log("[BattleManager] 전투 시작");
    }

    /// <summary>
    /// 전투를 중지합니다. 타이머를 멈추고 GameManager에 생존 시간을 전달합니다.
    /// </summary>
    public void StopBattle()
    {
        _isRunning = false;

        Debug.Log($"[BattleManager] 전투 중지 - 생존 시간: {_survivalTime:F1}초");
        OnBattleComplete?.Invoke();
    }
    #endregion

    #region Wave System
    private void UpdateWave()
    {
        if (waveData == null || waveData.WaveEntries == null) return;

        int newWaveIndex = waveData.GetWaveIndexForTime(_survivalTime);

        if (newWaveIndex != _currentWaveIndex && newWaveIndex >= 0)
        {
            _currentWaveIndex = newWaveIndex;
            ApplyWave(waveData.WaveEntries[_currentWaveIndex]);
            OnWaveChanged?.Invoke(_currentWaveIndex);

            Debug.Log($"[BattleManager] 웨이브 {_currentWaveIndex + 1} 시작 (시간: {_survivalTime:F1}초)");
        }
    }

    private void ApplyWave(WaveEntry waveEntry)
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning("[BattleManager] EnemySpawner가 할당되지 않았습니다.");
            return;
        }

        enemySpawner.SetWaveData(waveEntry);
    }
    #endregion

    #region Boss System
    private void UpdateBossSpawn()
    {
        if (waveData == null || waveData.BossEntries == null) return;
        if (_nextBossIndex >= waveData.BossEntries.Length) return;

        BossEntry nextBoss = waveData.BossEntries[_nextBossIndex];

        if (_survivalTime >= nextBoss.SpawnTime)
        {
            SpawnBoss(nextBoss);
            _nextBossIndex++;
        }
    }

    private void SpawnBoss(BossEntry bossEntry)
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning("[BattleManager] EnemySpawner가 할당되지 않았습니다.");
            return;
        }

        if (bossEntry.BossData == null)
        {
            Debug.LogWarning("[BattleManager] BossData가 null입니다.");
            return;
        }

        enemySpawner.SpawnBoss(bossEntry.BossData);
        OnBossSpawned?.Invoke(bossEntry.BossData);

        Debug.Log($"[BattleManager] 보스 스폰: {bossEntry.BossData.EnemyName} (시간: {_survivalTime:F1}초)");
    }
    #endregion
}
