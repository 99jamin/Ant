using UnityEngine;

/// <summary>
/// UI 요소의 베이스 클래스
/// CanvasGroup 기반으로 Open/Close를 제어합니다.
/// SetActive 대신 CanvasGroup을 사용하여 코루틴 유지 및 페이드 연출이 가능합니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseUI : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    public bool IsOpen => _canvasGroup.alpha > 0f;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// UI를 표시합니다.
    /// </summary>
    public virtual void Open()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// UI를 숨깁니다.
    /// </summary>
    public virtual void Close()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
