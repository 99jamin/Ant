using UnityEngine;

/// <summary>
/// 로비 씬 UI 컨트롤러
/// 캐릭터 선택과 배틀 시작을 관리합니다.
/// 스타트 버튼은 CharacterSelectUI에서 OnStartRequested 이벤트로 전달됩니다.
/// </summary>
public class LobbyUIController : BaseUIController
{
    [Header("캐릭터 선택")]
    [SerializeField] private CharacterSelectUI _characterSelectUI;

    #region Unity Lifecycle
    private void Start()
    {
        InitializeCharacterSelect();
    }

    private void OnDestroy()
    {
        if (_characterSelectUI != null)
        {
            _characterSelectUI.OnCharacterSelected -= OnCharacterSelected;
            _characterSelectUI.OnStartRequested -= OnStartRequested;
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
        _characterSelectUI.OnStartRequested += OnStartRequested;
    }

    private void OnCharacterSelected(PlayerDataSO character)
    {
        Managers.Instance?.Game?.SelectCharacter(character);
    }

    private void OnStartRequested()
    {
        Managers.Instance?.Game?.LoadBattle();
    }
    #endregion

    #region BaseUIController
    public override void OnOpen() { }
    public override void OnClose() { }
    #endregion
}
