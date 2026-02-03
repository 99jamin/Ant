using TMPro;
using UnityEngine;

/// <summary>
/// 경과 시간 표시 UI
/// mm:ss 포맷으로 배틀 경과 시간을 표시합니다.
/// 초 단위가 변경될 때만 텍스트를 갱신하여 불필요한 연산을 방지합니다.
/// </summary>
public class TimerUI : BaseUI
{
    [SerializeField] private TMP_Text timerText;

    private float _elapsedTime;
    private int _lastDisplayedSeconds = -1;

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        int totalSeconds = (int)_elapsedTime;

        if (totalSeconds == _lastDisplayedSeconds) return;
        _lastDisplayedSeconds = totalSeconds;

        timerText.SetText("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }

    /// <summary>
    /// 타이머를 초기화합니다.
    /// </summary>
    public void ResetTimer()
    {
        _elapsedTime = 0f;
        _lastDisplayedSeconds = -1;
    }

    /// <summary>
    /// 현재 경과 시간(초)을 반환합니다.
    /// </summary>
    public float ElapsedTime => _elapsedTime;
}
