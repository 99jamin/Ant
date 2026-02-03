using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배틀씬 UI 컨트롤러
/// Player 이벤트를 구독하여 HUD/팝업 UI를 갱신합니다.
/// </summary>
public class BattleUIController : BaseUIController
{
    #region Serialized Fields
    [Header("HUD")]
    [SerializeField] private HpBarUI hpBarUI;
    [SerializeField] private ExpBarUI expBarUI;
    [SerializeField] private TimerUI timerUI;

    [Header("팝업")]
    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private MenuUI menuUI;
    [SerializeField] private ResultUI resultUI;

    [Header("참조")]
    [SerializeField] private SkillManager skillManager;
    #endregion

    #region Private Fields
    private Player _player;
    private BaseUI _currentPopup;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        _player = Player.Instance;

        if (_player == null)
        {
            Debug.LogError("[BattleUIController] Player.Instance가 null입니다.");
            return;
        }

        SubscribeEvents();
        InitializePopups();
        InitializeHud();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }
    #endregion

    #region BaseUIController
    public override void OnOpen()
    {
        // 씬 진입 시 초기화 (필요 시 확장)
    }

    public override void OnClose()
    {
        // 씬 퇴장 시 정리 (필요 시 확장)
    }
    #endregion

    #region Initialization
    private void SubscribeEvents()
    {
        _player.OnHealthChanged += HandleHealthChanged;
        _player.OnExperienceChanged += HandleExperienceChanged;
        _player.OnLevelChanged += HandleLevelChanged;
        _player.OnDeath += HandleDeath;
    }

    private void UnsubscribeEvents()
    {
        if (_player == null) return;

        _player.OnHealthChanged -= HandleHealthChanged;
        _player.OnExperienceChanged -= HandleExperienceChanged;
        _player.OnLevelChanged -= HandleLevelChanged;
        _player.OnDeath -= HandleDeath;
    }

    private void InitializePopups()
    {
        // 팝업 초기 상태: 닫힘
        levelUpUI.Close();
        menuUI.Close();
        resultUI.Close();

        // 레벨업 UI 스킬 선택 콜백 등록
        levelUpUI.OnSkillSelected += HandleSkillSelected;

        // 메뉴 UI Resume 콜백 등록
        menuUI.Initialize(ClosePopup);
    }

    private void InitializeHud()
    {
        // 초기 HP/EXP 바 표시
        if (_player != null)
        {
            hpBarUI.SetValueImmediate(_player.CurrentHealth, _player.MaxHealth);
            expBarUI.SetValueImmediate(_player.CurrentExperience, _player.ExperienceToNextLevel);
        }
    }
    #endregion

    #region Event Handlers
    private void HandleHealthChanged(float current, float max)
    {
        hpBarUI.UpdateValue(current, max);
    }

    private void HandleExperienceChanged(float current, float max)
    {
        expBarUI.UpdateValue(current, max);
    }

    private void HandleLevelChanged(int level)
    {
        // 레벨업 시 EXP바를 즉시 0으로 리셋하여 초과 경험치가 자연스럽게 차오르도록 함
        expBarUI.SetValueImmediate(0f, 1f);

        var choices = skillManager.GetRandomSkillChoices();

        if (choices.Count > 0)
        {
            OpenPopup(levelUpUI);
            levelUpUI.Show(choices);
        }
    }

    private void HandleDeath()
    {
        // 현재 팝업 닫기 (Resume 없이)
        if (_currentPopup != null)
        {
            _currentPopup.Close();
            _currentPopup = null;
        }

        // 생존 시간 및 점수 계산
        float survivalTime = timerUI.ElapsedTime;
        int score = Mathf.FloorToInt(survivalTime);

        // GameManager에 전투 종료 알림
        Managers.Instance.Game.EndBattle(survivalTime);

        // 결과 UI 표시
        resultUI.Show(survivalTime, score);
    }

    private void HandleSkillSelected(SkillDataSO skillData)
    {
        ClosePopup();
        skillManager.SelectSkill(skillData);
    }
    #endregion

    #region Popup Management
    /// <summary>
    /// 팝업을 열고 게임을 일시정지합니다.
    /// </summary>
    public void OpenPopup(BaseUI popup)
    {
        if (_currentPopup != null)
        {
            _currentPopup.Close();
        }

        _currentPopup = popup;
        _currentPopup.Open();
        Managers.Instance.Game.PauseBattle();
    }

    /// <summary>
    /// 현재 팝업을 닫고 게임을 재개합니다.
    /// </summary>
    public void ClosePopup()
    {
        if (_currentPopup != null)
        {
            _currentPopup.Close();
            _currentPopup = null;
        }

        Managers.Instance.Game.ResumeBattle();
    }

    /// <summary>
    /// 메뉴 팝업을 토글합니다. (외부에서 호출용)
    /// </summary>
    public void ToggleMenu()
    {
        if (_currentPopup == menuUI)
        {
            ClosePopup();
            return;
        }

        // 레벨업 등 필수 팝업이 열려있으면 메뉴 진입 차단
        if (_currentPopup != null) return;

        OpenPopup(menuUI);
    }
    #endregion
}
