using UnityEngine;

/// <summary>
/// UI 시스템의 중앙 관리자
/// 씬별 UIController의 등록/해제를 관리합니다.
/// 가볍게 라우터 역할만 수행하며, 씬별 로직은 컨트롤러가 담당합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 현재 활성 씬의 UI 컨트롤러
    /// </summary>
    public BaseUIController CurrentController { get; private set; }

    /// <summary>
    /// 씬 컨트롤러를 등록합니다.
    /// </summary>
    public void RegisterController(BaseUIController controller)
    {
        if (CurrentController != null && CurrentController != controller)
        {
            Debug.LogWarning("[UIManager] 기존 컨트롤러를 해제하고 새 컨트롤러를 등록합니다.");
            CurrentController.OnClose();
        }

        CurrentController = controller;
        CurrentController.OnOpen();
    }

    /// <summary>
    /// 씬 컨트롤러를 해제합니다.
    /// </summary>
    public void UnregisterController(BaseUIController controller)
    {
        if (CurrentController != controller) return;

        CurrentController.OnClose();
        CurrentController = null;
    }
}
