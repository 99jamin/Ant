using UnityEngine;

/// <summary>
/// 씬별 UI 컨트롤러의 베이스 클래스
/// UIManager에 자신을 등록/해제하며, 씬 진입/퇴장 시 초기화/정리를 담당합니다.
/// </summary>
public abstract class BaseUIController : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        // Managers 또는 UIManager가 아직 초기화되지 않았을 수 있음
        if (Managers.Instance != null && Managers.Instance.UI != null)
        {
            Managers.Instance.UI.RegisterController(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (Managers.Instance != null && Managers.Instance.UI != null)
        {
            Managers.Instance.UI.UnregisterController(this);
        }
    }

    /// <summary>
    /// 씬 진입 시 초기화
    /// </summary>
    public abstract void OnOpen();

    /// <summary>
    /// 씬 퇴장 시 정리
    /// </summary>
    public abstract void OnClose();
}
