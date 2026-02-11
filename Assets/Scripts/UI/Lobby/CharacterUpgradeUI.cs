using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 스탯 업그레이드 슬롯 데이터
/// </summary>
[Serializable]
public class StatUpgradeSlot
{
    [Header("슬롯 데이터")]
    [Tooltip("스탯 이름 (예: 체력) — 보너스 텍스트에 합쳐서 표시")]
    public string statName;

    [Tooltip("스탯 아이콘")]
    public Sprite icon;

    [Header("UI 요소")]
    [Tooltip("강화 이름 텍스트")]
    public TMP_Text statNameText;

    [Tooltip("스탯 아이콘 이미지")]
    public Image iconImage;

    [Tooltip("스탯 이름 + 보너스 표시 (예: 체력 +5%)")]
    public TMP_Text bonusText;

    [Tooltip("현재 레벨 표시 (예: Lv.5/10)")]
    public TMP_Text currentLevelText;

    [Tooltip("업그레이드 비용 표시")]
    public TMP_Text costText;

    [Tooltip("업그레이드 버튼")]
    public Button upgradeButton;
}

/// <summary>
/// 캐릭터 강화 UI 팝업 (BaseUI 기반)
/// 각 스탯(HP, 이속, 자석반경, 공격력)별 강화 슬롯을 관리합니다.
/// </summary>
public class CharacterUpgradeUI : BaseUI
{
    #region Serialized Fields
    [Header("스탯 업그레이드 슬롯")]
    [SerializeField] private StatUpgradeSlot _healthSlot;
    [SerializeField] private StatUpgradeSlot _moveSpeedSlot;
    [SerializeField] private StatUpgradeSlot _magnetSlot;
    [SerializeField] private StatUpgradeSlot _damageSlot;

    [Header("팝업 제어")]
    [SerializeField] private Button _closeButton;
    #endregion

    #region Private Fields
    private PlayerDataSO _currentCharacter;
    private CharacterProgressManager _progressManager;
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        SetupButtonListeners();

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }
    }

    private void Start()
    {
        _progressManager = Managers.Instance?.CharacterProgress;

        // 강화 이벤트 구독
        if (_progressManager != null)
        {
            _progressManager.OnStatUpgraded += OnStatUpgraded;
        }

        // 재화 변동 이벤트 구독 (버튼 활성화 상태 갱신용)
        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged += OnProteinChanged;
        }

        // 초기 상태: 닫힘
        Close();
    }

    private void OnDestroy()
    {
        if (_progressManager != null)
        {
            _progressManager.OnStatUpgraded -= OnStatUpgraded;
        }

        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged -= OnProteinChanged;
        }
    }
    #endregion

    #region Initialization
    private void SetupButtonListeners()
    {
        _healthSlot?.upgradeButton?.onClick.AddListener(() => OnUpgradeClicked(StatType.Health));
        _moveSpeedSlot?.upgradeButton?.onClick.AddListener(() => OnUpgradeClicked(StatType.MoveSpeed));
        _magnetSlot?.upgradeButton?.onClick.AddListener(() => OnUpgradeClicked(StatType.MagnetRadius));
        _damageSlot?.upgradeButton?.onClick.AddListener(() => OnUpgradeClicked(StatType.Damage));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 캐릭터 데이터를 바인딩하고 팝업을 엽니다.
    /// </summary>
    /// <param name="character">캐릭터 데이터</param>
    public void Show(PlayerDataSO character)
    {
        Bind(character);
        Open();
    }

    /// <summary>
    /// 캐릭터 데이터를 바인딩하고 모든 슬롯을 갱신합니다.
    /// </summary>
    /// <param name="character">캐릭터 데이터</param>
    public void Bind(PlayerDataSO character)
    {
        _currentCharacter = character;

        if (_progressManager == null)
        {
            _progressManager = Managers.Instance?.CharacterProgress;
        }

        RefreshAllSlots();
    }

    /// <summary>
    /// 모든 슬롯을 갱신합니다.
    /// </summary>
    public void RefreshAllSlots()
    {
        if (_currentCharacter == null || _progressManager == null) return;

        RefreshSlot(_healthSlot, StatType.Health);
        RefreshSlot(_moveSpeedSlot, StatType.MoveSpeed);
        RefreshSlot(_magnetSlot, StatType.MagnetRadius);
        RefreshSlot(_damageSlot, StatType.Damage);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 개별 슬롯을 갱신합니다.
    /// </summary>
    private void RefreshSlot(StatUpgradeSlot slot, StatType statType)
    {
        if (slot == null || _currentCharacter == null || _progressManager == null) return;

        string characterName = _currentCharacter.characterName;
        int currentLevel = _progressManager.GetStatLevel(characterName, statType);
        int maxLevel = _progressManager.GetMaxStatLevel();
        float bonusPerLevel = _progressManager.GetBonusPerLevel();

        // 아이콘 설정
        if (slot.iconImage != null && slot.icon != null)
        {
            slot.iconImage.sprite = slot.icon;
        }

        // 스탯 이름 + 보너스 (예: 체력 +5%)
        if (slot.bonusText != null)
        {
            int bonusPercent = Mathf.RoundToInt(currentLevel * bonusPerLevel * 100f);
            slot.bonusText.text = $"{slot.statName} +{bonusPercent}%";
        }

        // 현재 레벨 (예: Lv.5/10)
        if (slot.currentLevelText != null)
        {
            slot.currentLevelText.text = $"Lv.{currentLevel}/{maxLevel}";
        }

        // 업그레이드 비용 및 버튼 상태
        bool isMaxLevel = currentLevel >= maxLevel;
        if (slot.costText != null)
        {
            if (isMaxLevel)
            {
                slot.costText.text = "MAX";
            }
            else
            {
                int cost = _progressManager.GetUpgradeCost(currentLevel);
                slot.costText.text = "COST : " + cost.ToString("N0");
            }
        }

        // 버튼 활성화 상태
        if (slot.upgradeButton != null)
        {
            if (isMaxLevel)
            {
                slot.upgradeButton.interactable = false;
            }
            else
            {
                int cost = _progressManager.GetUpgradeCost(currentLevel);
                bool canAfford = Managers.Instance?.Currency?.HasEnoughProtein(cost) ?? false;
                slot.upgradeButton.interactable = canAfford;
            }
        }
    }

    /// <summary>
    /// 업그레이드 버튼 클릭 처리
    /// </summary>
    private void OnUpgradeClicked(StatType statType)
    {
        if (_currentCharacter == null || _progressManager == null) return;

        if (_progressManager.TryUpgradeStat(_currentCharacter.characterName, statType))
        {
            // 성공 시 UI 갱신
            RefreshAllSlots();
        }
    }

    /// <summary>
    /// 스탯 강화 이벤트 핸들러
    /// </summary>
    private void OnStatUpgraded(string characterName, StatType statType, int newLevel)
    {
        // 현재 바인딩된 캐릭터의 강화인 경우에만 갱신
        if (_currentCharacter != null && _currentCharacter.characterName == characterName)
        {
            RefreshAllSlots();
        }
    }

    /// <summary>
    /// 재화 변동 이벤트 핸들러
    /// </summary>
    private void OnProteinChanged(int newAmount)
    {
        // 팝업이 열린 상태에서만 갱신
        if (IsOpen)
        {
            RefreshAllSlots();
        }
    }
    #endregion
}
