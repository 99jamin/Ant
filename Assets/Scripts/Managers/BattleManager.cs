using System;
using UnityEngine;

/// <summary>
/// 전투 전용 매니저 (스텁)
/// 웨이브 시스템, 보스 스폰, 생존 시간 추적 등을 담당합니다.
/// 배틀 씬에서만 존재하며, 구체적인 로직은 추후 구현합니다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    #region Fields
    private float _survivalTime;
    private bool _isRunning;
    #endregion

    #region Properties
    /// <summary>
    /// 현재 생존 시간 (초)
    /// </summary>
    public float SurvivalTime => _survivalTime;
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
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        if (!_isRunning) return;

        _survivalTime += Time.deltaTime;
        OnBattleTimeUpdated?.Invoke(_survivalTime);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 전투를 시작합니다. 생존 시간을 초기화하고 타이머를 시작합니다.
    /// </summary>
    public void StartBattle()
    {
        _survivalTime = 0f;
        _isRunning = true;

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
}
