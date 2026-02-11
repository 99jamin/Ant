using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태를 나타내는 열거형
/// </summary>
public enum GameState
{
    None,           // 초기 상태
    Intro,          // 인트로 씬
    Lobby,          // 로비 씬 (캐릭터 선택)
    BattleReady,    // 배틀 씬 로드 완료, 전투 시작 대기
    BattlePlaying,  // 전투 진행 중
    BattlePause,    // 일시정지
    BattleResult,   // 전투 종료 → 결과 화면
    Loading         // 씬 전환 중
}

/// <summary>
/// 게임 상태 FSM + 씬 전환 + TimeScale 제어를 담당하는 매니저
/// Managers 싱글톤의 서브 매니저로 동작합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region State
    private GameState _currentState = GameState.None;

    /// <summary>
    /// 현재 게임 상태
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// 게임 상태 전환 시 발행되는 이벤트 (이전 상태, 새 상태)
    /// </summary>
    public event Action<GameState, GameState> OnGameStateChanged;
    #endregion

    #region Game Settings
    [Header("레이어 설정")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private LayerMask _experienceLayer;

    /// <summary>
    /// 적 레이어 마스크
    /// </summary>
    public LayerMask EnemyLayer => _enemyLayer;

    /// <summary>
    /// 경험치 오브젝트 레이어 마스크
    /// </summary>
    public LayerMask ExperienceLayer => _experienceLayer;
    #endregion

    #region Character Selection
    [Header("캐릭터 설정")]
    [SerializeField] private PlayerDataSO[] _availableCharacters;
    [SerializeField] private PlayerDataSO _defaultCharacter;

    private PlayerDataSO _selectedCharacter;

    /// <summary>
    /// 현재 선택된 캐릭터 데이터 (미선택 시 기본 캐릭터 반환)
    /// </summary>
    public PlayerDataSO SelectedCharacter => _selectedCharacter ?? _defaultCharacter;

    /// <summary>
    /// 선택 가능한 모든 캐릭터 목록
    /// </summary>
    public PlayerDataSO[] AvailableCharacters => _availableCharacters;

    /// <summary>
    /// 캐릭터를 선택합니다.
    /// </summary>
    /// <param name="character">선택할 캐릭터 데이터</param>
    public void SelectCharacter(PlayerDataSO character)
    {
        _selectedCharacter = character;
        Debug.Log($"[GameManager] 캐릭터 선택: {character?.characterName ?? "null"}");
    }
    #endregion

    #region Scene Names
    // 씬 이름은 상수로 관리하여 오타 방지
    private const string SceneIntro = "IntroScene";
    private const string SceneLobby = "LobbyScene";
    private const string SceneBattle = "BattleScene";
    #endregion

    #region State Transition
    /// <summary>
    /// 게임 상태를 전환하고 이벤트를 발행합니다.
    /// </summary>
    private void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

        GameState prevState = _currentState;
        _currentState = newState;

        Debug.Log($"[GameManager] 상태 전환: {prevState} → {newState}");
        OnGameStateChanged?.Invoke(prevState, newState);
    }
    #endregion

    #region Scene Loading
    /// <summary>
    /// 인트로 씬으로 전환합니다.
    /// </summary>
    public void LoadIntro()
    {
        LoadSceneAsync(SceneIntro, GameState.Intro);
    }

    /// <summary>
    /// 로비 씬으로 전환합니다.
    /// </summary>
    public void LoadLobby()
    {
        LoadSceneAsync(SceneLobby, GameState.Lobby);
    }

    /// <summary>
    /// 배틀 씬으로 전환합니다. 로드 완료 시 바로 전투가 시작됩니다.
    /// </summary>
    public void LoadBattle()
    {
        LoadSceneAsync(SceneBattle, GameState.BattlePlaying);
    }

    /// <summary>
    /// 비동기 씬 전환을 수행합니다.
    /// Loading 상태로 전환 후, 로드 완료 시 목표 상태로 전환합니다.
    /// </summary>
    private async void LoadSceneAsync(string sceneName, GameState targetState)
    {
        // 씬 전환 전 풀 초기화
        Managers.Instance.Pool.ClearAllPools();

        ChangeState(GameState.Loading);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"[GameManager] 씬 로드 실패: {sceneName}");
            return;
        }

        // 로드 완료까지 대기
        while (!operation.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        ChangeState(targetState);
    }
    #endregion

    #region Battle Flow
    /// <summary>
    /// 전투를 시작합니다. BattleReady 상태에서만 호출 가능합니다.
    /// </summary>
    public void StartBattle()
    {
        if (_currentState != GameState.BattleReady)
        {
            Debug.LogWarning($"[GameManager] StartBattle 호출 불가: 현재 상태 = {_currentState}");
            return;
        }

        Time.timeScale = 1f;
        ChangeState(GameState.BattlePlaying);
    }

    /// <summary>
    /// 전투를 일시정지합니다.
    /// </summary>
    public void PauseBattle()
    {
        if (_currentState != GameState.BattlePlaying)
        {
            Debug.LogWarning($"[GameManager] PauseBattle 호출 불가: 현재 상태 = {_currentState}");
            return;
        }

        Time.timeScale = 0f;
        ChangeState(GameState.BattlePause);
    }

    /// <summary>
    /// 일시정지를 해제하고 전투를 재개합니다.
    /// </summary>
    public void ResumeBattle()
    {
        if (_currentState != GameState.BattlePause)
        {
            Debug.LogWarning($"[GameManager] ResumeBattle 호출 불가: 현재 상태 = {_currentState}");
            return;
        }

        Time.timeScale = 1f;
        ChangeState(GameState.BattlePlaying);
    }

    /// <summary>
    /// 전투를 종료하고 결과 화면으로 전환합니다.
    /// </summary>
    /// <param name="survivalTime">생존 시간 (초)</param>
    public void EndBattle(float survivalTime)
    {
        if (_currentState != GameState.BattlePlaying && _currentState != GameState.BattlePause)
        {
            Debug.LogWarning($"[GameManager] EndBattle 호출 불가: 현재 상태 = {_currentState}");
            return;
        }

        Time.timeScale = 1f;

        // 생존 시간 기반 보상 계산
        int reward = CalculateReward(survivalTime);
        Managers.Instance.Currency?.AddProtein(reward);

        Debug.Log($"[GameManager] 전투 종료 - 생존 시간: {survivalTime:F1}초, 보상: {reward} 단백질");
        ChangeState(GameState.BattleResult);
    }

    /// <summary>
    /// 생존 시간 기반 보상을 계산합니다.
    /// </summary>
    private int CalculateReward(float survivalTime)
    {
        // 기본 보상: 생존 시간(초) * 1 단백질
        return Mathf.FloorToInt(survivalTime);
    }
    #endregion
}
