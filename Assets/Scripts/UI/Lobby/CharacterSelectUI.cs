using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 선택 UI를 관리하는 컴포넌트
/// 여러 캐릭터 슬롯을 관리하고 선택 이벤트를 처리합니다.
/// 잠긴 캐릭터: 실루엣 + "???" 표시 / 해금된 캐릭터: 정상 표시
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    #region Events
    /// <summary>
    /// 해금된 캐릭터가 선택되었을 때 발행되는 이벤트
    /// </summary>
    public event Action<PlayerDataSO> OnCharacterSelected;

    /// <summary>
    /// 스타트 버튼이 클릭되었을 때 발행되는 이벤트
    /// </summary>
    public event Action OnStartRequested;
    #endregion

    #region Serialized Fields
    [Header("슬롯 설정")]
    [SerializeField] private CharacterSlotUI[] _characterSlots;

    [Header("상세 정보 구역")]
    [SerializeField] private Image _illustrationImage;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("실루엣")]
    [SerializeField] private Material _silhouetteMaterial;

    [Header("강화 UI")]
    [SerializeField] private CharacterUpgradeUI _upgradeUI;

    [Header("해금된 캐릭터 버튼 패널")]
    [SerializeField] private GameObject _unlockedButtonPanel;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _startButton;

    [Header("잠긴 캐릭터 버튼 패널")]
    [SerializeField] private GameObject _lockedButtonPanel;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private TMP_Text _unlockCostText;
    #endregion

    #region Private Fields
    private PlayerDataSO _currentSelected;
    private CharacterProgressManager _progressManager;
    #endregion

    #region Public Properties
    /// <summary>
    /// 현재 선택된 캐릭터
    /// </summary>
    public PlayerDataSO CurrentSelected => _currentSelected;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        _progressManager = Managers.Instance?.CharacterProgress;

        // 버튼 이벤트 연결
        if (_unlockButton != null)
        {
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClicked);
        }

        // 해금 이벤트 구독
        if (_progressManager != null)
        {
            _progressManager.OnCharacterUnlocked += OnCharacterUnlocked;
        }

        // 재화 변동 이벤트 구독 (해금 버튼 interactable 실시간 갱신)
        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged += OnProteinChanged;
        }
    }

    private void OnDestroy()
    {
        if (_progressManager != null)
        {
            _progressManager.OnCharacterUnlocked -= OnCharacterUnlocked;
        }

        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged -= OnProteinChanged;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 캐릭터 목록으로 UI를 초기화합니다.
    /// </summary>
    /// <param name="characters">표시할 캐릭터 목록</param>
    public void Initialize(PlayerDataSO[] characters)
    {
        if (characters == null || _characterSlots == null) return;

        // Start()보다 먼저 호출될 수 있으므로 여기서도 초기화
        if (_progressManager == null)
        {
            _progressManager = Managers.Instance?.CharacterProgress;
        }

        // 기본 해금 캐릭터 초기화
        _progressManager?.InitializeDefaultUnlocks(characters);

        for (int i = 0; i < _characterSlots.Length; i++)
        {
            if (i < characters.Length && characters[i] != null)
            {
                _characterSlots[i].gameObject.SetActive(true);
                _characterSlots[i].Bind(characters[i], OnSlotClicked);

                // 해금 상태 업데이트
                UpdateSlotLockStatus(_characterSlots[i], characters[i]);
            }
            else
            {
                // 캐릭터 수보다 슬롯이 많으면 비활성화
                _characterSlots[i].gameObject.SetActive(false);
            }
        }

        // 첫 번째 해금된 캐릭터 자동 선택
        PlayerDataSO firstUnlocked = FindFirstUnlockedCharacter(characters);
        if (firstUnlocked != null)
        {
            SelectCharacter(firstUnlocked);
        }
        else if (characters.Length > 0 && characters[0] != null)
        {
            // 해금된 캐릭터가 없으면 첫 번째 캐릭터 선택 (해금 안내용)
            SelectCharacter(characters[0]);
        }
    }

    /// <summary>
    /// 특정 캐릭터를 선택합니다.
    /// </summary>
    public void SelectCharacter(PlayerDataSO character)
    {
        if (character == null) return;

        _currentSelected = character;

        // 모든 슬롯의 선택 상태 업데이트
        foreach (var slot in _characterSlots)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.SetSelected(slot.Data == character);
            }
        }

        // 상세 정보 구역 업데이트
        UpdateDetailPanel(character);

        // 해금된 캐릭터만 선택 이벤트 발행
        if (IsCharacterUnlocked(character))
        {
            OnCharacterSelected?.Invoke(character);
        }
    }
    #endregion

    #region Detail Panel
    /// <summary>
    /// 상세 정보 구역을 업데이트합니다.
    /// 잠긴 캐릭터: 이름/설명 "???", 일러스트 실루엣
    /// 해금된 캐릭터: 정상 표시
    /// </summary>
    private void UpdateDetailPanel(PlayerDataSO character)
    {
        bool isUnlocked = IsCharacterUnlocked(character);

        if (_illustrationImage != null)
        {
            // illustration 우선, 없으면 icon 사용
            Sprite displaySprite = character.illustration != null ? character.illustration : character.icon;
            _illustrationImage.sprite = displaySprite;
            _illustrationImage.enabled = displaySprite != null;

            // null 할당 = Unity 기본 Material로 리셋 (씬 전환 시 안전)
            _illustrationImage.material = isUnlocked ? null : _silhouetteMaterial;
        }

        if (_characterNameText != null)
        {
            _characterNameText.text = isUnlocked ? character.characterName : "???";
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = isUnlocked ? character.description : "???";
        }

        // 버튼 패널 전환
        UpdateButtonPanel(character, isUnlocked);
    }

    /// <summary>
    /// 해금 상태에 따라 버튼 패널을 전환합니다.
    /// 해금됨: [업그레이드] + [스타트] / 잠김: [해금]
    /// </summary>
    private void UpdateButtonPanel(PlayerDataSO character, bool isUnlocked)
    {
        // 해금된 캐릭터 버튼 패널
        if (_unlockedButtonPanel != null)
        {
            _unlockedButtonPanel.SetActive(isUnlocked);
        }

        // 잠긴 캐릭터 버튼 패널
        if (_lockedButtonPanel != null)
        {
            _lockedButtonPanel.SetActive(!isUnlocked);
        }

        // 해금 비용 표시 및 버튼 활성화 상태
        if (!isUnlocked)
        {
            if (_unlockCostText != null)
            {
                _unlockCostText.text = "UNLOCK!\nCOST : " + character.unlockCost.ToString("N0");
            }

            if (_unlockButton != null)
            {
                bool canAfford = Managers.Instance?.Currency?.HasEnoughProtein(character.unlockCost) ?? false;
                _unlockButton.interactable = canAfford;
            }
        }
    }
    #endregion

    #region Private Methods
    private void OnSlotClicked(PlayerDataSO character)
    {
        SelectCharacter(character);
    }

    /// <summary>
    /// 첫 번째 해금된 캐릭터를 찾습니다.
    /// </summary>
    private PlayerDataSO FindFirstUnlockedCharacter(PlayerDataSO[] characters)
    {
        foreach (var character in characters)
        {
            if (character != null && IsCharacterUnlocked(character))
            {
                return character;
            }
        }
        return null;
    }

    /// <summary>
    /// 캐릭터가 해금되었는지 확인합니다.
    /// </summary>
    private bool IsCharacterUnlocked(PlayerDataSO character)
    {
        if (character == null) return false;

        // 기본 해금 캐릭터 체크
        if (character.isDefaultUnlocked) return true;

        // CharacterProgressManager에서 확인
        return _progressManager?.IsUnlocked(character.characterName) ?? false;
    }

    /// <summary>
    /// 슬롯의 잠금 상태를 업데이트합니다.
    /// </summary>
    private void UpdateSlotLockStatus(CharacterSlotUI slot, PlayerDataSO character)
    {
        bool isUnlocked = IsCharacterUnlocked(character);
        slot.UpdateLockStatus(isUnlocked);
    }

    /// <summary>
    /// 모든 슬롯의 잠금 상태를 갱신합니다.
    /// </summary>
    private void RefreshAllSlotLockStatus()
    {
        foreach (var slot in _characterSlots)
        {
            if (slot.gameObject.activeSelf && slot.Data != null)
            {
                UpdateSlotLockStatus(slot, slot.Data);
            }
        }
    }
    #endregion

    #region Button Handlers
    /// <summary>
    /// 해금 버튼 클릭 처리
    /// </summary>
    private void OnUnlockButtonClicked()
    {
        if (_currentSelected == null || _progressManager == null) return;

        if (_progressManager.TryUnlock(_currentSelected))
        {
            // 해금 성공 - UI 갱신
            RefreshAllSlotLockStatus();
            UpdateDetailPanel(_currentSelected);

            // 해금된 캐릭터 선택 이벤트 발행
            OnCharacterSelected?.Invoke(_currentSelected);
        }
    }

    /// <summary>
    /// 업그레이드 버튼 클릭 처리 → 팝업 오픈
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (_currentSelected == null || _upgradeUI == null) return;

        _upgradeUI.Show(_currentSelected);
    }

    /// <summary>
    /// 스타트 버튼 클릭 처리
    /// </summary>
    private void OnStartButtonClicked()
    {
        if (_currentSelected == null) return;

        // 해금된 캐릭터만 배틀 시작 가능
        if (!IsCharacterUnlocked(_currentSelected)) return;

        OnStartRequested?.Invoke();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 캐릭터 해금 이벤트 핸들러
    /// </summary>
    private void OnCharacterUnlocked(string characterName)
    {
        // 현재 선택된 캐릭터가 해금된 경우 UI 갱신
        if (_currentSelected != null && _currentSelected.characterName == characterName)
        {
            UpdateDetailPanel(_currentSelected);
        }

        RefreshAllSlotLockStatus();
    }

    /// <summary>
    /// 재화 변동 이벤트 핸들러 (해금 버튼 interactable 실시간 갱신)
    /// </summary>
    private void OnProteinChanged(int newAmount)
    {
        if (_currentSelected == null) return;

        // 잠긴 캐릭터의 해금 버튼 활성화 상태 갱신
        if (!IsCharacterUnlocked(_currentSelected) && _unlockButton != null)
        {
            bool canAfford = Managers.Instance?.Currency?.HasEnoughProtein(_currentSelected.unlockCost) ?? false;
            _unlockButton.interactable = canAfford;
        }
    }
    #endregion
}
