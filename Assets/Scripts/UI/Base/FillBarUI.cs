using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fill 바 UI의 공통 베이스 클래스
/// fillImage의 fillAmount를 Lerp로 부드럽게 보간하여 표시합니다.
/// </summary>
public class FillBarUI : BaseUI
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float lerpSpeed = 5f;

    private float _targetFillAmount;

    protected override void Awake()
    {
        base.Awake();
        _targetFillAmount = fillImage.fillAmount;
    }

    private void Update()
    {
        if (Mathf.Approximately(fillImage.fillAmount, _targetFillAmount)) return;

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            _targetFillAmount,
            Time.unscaledDeltaTime * lerpSpeed
        );
    }

    /// <summary>
    /// 바 갱신
    /// </summary>
    public void UpdateValue(float current, float max)
    {
        if (max <= 0f) return;
        _targetFillAmount = current / max;
    }

    /// <summary>
    /// 보간 없이 즉시 바 갱신 (초기화 용도)
    /// </summary>
    public void SetValueImmediate(float current, float max)
    {
        if (max <= 0f) return;
        _targetFillAmount = current / max;
        fillImage.fillAmount = _targetFillAmount;
    }
}
