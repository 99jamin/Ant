using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 씬 UI 컨트롤러
/// 캐릭터 선택, 배틀 시작 등 로비 씬의 UI 이벤트를 처리합니다.
/// </summary>
public class LobbyUIController : BaseUIController
{
    [Header("버튼")]
    [SerializeField] private Button _battleStartButton;

    #region Unity Lifecycle
    private void Awake()
    {
        if (_battleStartButton != null)
        {
            _battleStartButton.onClick.AddListener(OnBattleStartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_battleStartButton != null)
        {
            _battleStartButton.onClick.RemoveListener(OnBattleStartButtonClicked);
        }
    }
    #endregion

    #region BaseUIController
    public override void OnOpen() { }
    public override void OnClose() { }
    #endregion

    #region Button Handlers
    private void OnBattleStartButtonClicked()
    {
        Managers.Instance.Game.LoadBattle();
    }
    #endregion
}
