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

    [Header("캐릭터 선택")]
    [SerializeField] private CharacterSelectUI _characterSelectUI;

    #region Unity Lifecycle
    private void Awake()
    {
        if (_battleStartButton != null)
        {
            _battleStartButton.onClick.AddListener(OnBattleStartButtonClicked);
        }
    }

    private void Start()
    {
        InitializeCharacterSelect();
    }

    private void OnDestroy()
    {
        if (_battleStartButton != null)
        {
            _battleStartButton.onClick.RemoveListener(OnBattleStartButtonClicked);
        }

        if (_characterSelectUI != null)
        {
            _characterSelectUI.OnCharacterSelected -= OnCharacterSelected;
        }
    }
    #endregion

    #region Character Selection
    private void InitializeCharacterSelect()
    {
        if (_characterSelectUI == null) return;

        GameManager gameManager = Managers.Instance?.Game;
        if (gameManager == null)
        {
            Debug.LogError("[LobbyUIController] GameManager를 찾을 수 없습니다.");
            return;
        }

        PlayerDataSO[] characters = gameManager.AvailableCharacters;
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("[LobbyUIController] 선택 가능한 캐릭터가 없습니다.");
            return;
        }

        _characterSelectUI.Initialize(characters);
        _characterSelectUI.OnCharacterSelected += OnCharacterSelected;
    }

    private void OnCharacterSelected(PlayerDataSO character)
    {
        Managers.Instance?.Game?.SelectCharacter(character);
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
