using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일시정지 메뉴 팝업 UI
/// Resume 버튼을 통해 게임을 재개합니다.
/// </summary>
public class MenuUI : BaseUI
{
    [SerializeField] private Button resumeButton;

    /// <summary>
    /// Resume 버튼 클릭 시 호출할 콜백을 등록합니다.
    /// </summary>
    public void Initialize(System.Action onResume)
    {
        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(() => onResume?.Invoke());
    }
}
