using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인트로 씬 UI 컨트롤러
/// 게임 시작 버튼 등 인트로 씬의 UI 이벤트를 처리합니다.
/// </summary>
public class IntroUIController : BaseUIController
{
    [Header("버튼")]
    [SerializeField] private Button _startButton;

    #region Unity Lifecycle
    private void Awake()
    {
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }
    #endregion

    #region BaseUIController
    public override void OnOpen() { }
    public override void OnClose() { }
    #endregion

    #region Button Handlers
    private void OnStartButtonClicked()
    {
        Managers.Instance.Game.LoadLobby();
    }
    #endregion
}
