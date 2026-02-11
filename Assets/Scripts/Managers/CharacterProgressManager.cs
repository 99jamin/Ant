using System;
using UnityEngine;

/// <summary>
/// 캐릭터 강화 스탯 타입
/// </summary>
public enum StatType
{
    Health,
    MoveSpeed,
    MagnetRadius,
    Damage
}

/// <summary>
/// 캐릭터 해금 및 강화 데이터를 관리하는 매니저
/// PlayerPrefs를 통해 저장/로드합니다.
/// </summary>
public class CharacterProgressManager : MonoBehaviour
{
    #region Constants
    private const string UnlockKeyPrefix = "CharUnlock_";
    private const string StatLevelKeyPrefix = "CharStat_";
    private const int MaxStatLevel = 10;
    private const int BaseCost = 100;
    private const float BonusPerLevel = 0.05f; // 레벨당 5% 증가
    #endregion

    #region Events
    /// <summary>
    /// 캐릭터가 해금되었을 때 발행되는 이벤트
    /// </summary>
    public event Action<string> OnCharacterUnlocked;

    /// <summary>
    /// 스탯이 강화되었을 때 발행되는 이벤트 (캐릭터명, 스탯타입, 새 레벨)
    /// </summary>
    public event Action<string, StatType, int> OnStatUpgraded;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Load();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }
    #endregion

    #region Unlock Methods
    /// <summary>
    /// 캐릭터가 해금되었는지 확인합니다.
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    /// <returns>해금 여부</returns>
    public bool IsUnlocked(string characterName)
    {
        string key = GetUnlockKey(characterName);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// 캐릭터 해금을 시도합니다.
    /// </summary>
    /// <param name="character">캐릭터 데이터</param>
    /// <returns>해금 성공 여부</returns>
    public bool TryUnlock(PlayerDataSO character)
    {
        if (character == null) return false;

        string characterName = character.characterName;

        // 이미 해금된 경우
        if (IsUnlocked(characterName))
        {
            Debug.LogWarning($"[CharacterProgressManager] {characterName}은(는) 이미 해금되어 있습니다.");
            return false;
        }

        // 기본 해금 캐릭터인 경우
        if (character.isDefaultUnlocked)
        {
            SetUnlocked(characterName);
            return true;
        }

        // 비용 차감 시도
        int cost = character.unlockCost;
        if (!Managers.Instance.Currency.SpendProtein(cost))
        {
            Debug.LogWarning($"[CharacterProgressManager] 단백질 부족: 필요 {cost}");
            return false;
        }

        SetUnlocked(characterName);
        Debug.Log($"[CharacterProgressManager] {characterName} 해금 완료! (비용: {cost})");
        return true;
    }

    /// <summary>
    /// 캐릭터를 해금 상태로 설정합니다. (내부용)
    /// </summary>
    private void SetUnlocked(string characterName)
    {
        string key = GetUnlockKey(characterName);
        PlayerPrefs.SetInt(key, 1);
        OnCharacterUnlocked?.Invoke(characterName);
    }

    /// <summary>
    /// 기본 해금 캐릭터들을 초기화합니다.
    /// </summary>
    /// <param name="characters">모든 캐릭터 목록</param>
    public void InitializeDefaultUnlocks(PlayerDataSO[] characters)
    {
        if (characters == null) return;

        foreach (var character in characters)
        {
            if (character != null && character.isDefaultUnlocked && !IsUnlocked(character.characterName))
            {
                SetUnlocked(character.characterName);
                Debug.Log($"[CharacterProgressManager] 기본 캐릭터 해금: {character.characterName}");
            }
        }
    }
    #endregion

    #region Stat Upgrade Methods
    /// <summary>
    /// 특정 캐릭터의 스탯 레벨을 가져옵니다.
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    /// <param name="stat">스탯 타입</param>
    /// <returns>현재 레벨 (0-10)</returns>
    public int GetStatLevel(string characterName, StatType stat)
    {
        string key = GetStatKey(characterName, stat);
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// 스탯 강화를 시도합니다.
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    /// <param name="stat">스탯 타입</param>
    /// <returns>강화 성공 여부</returns>
    public bool TryUpgradeStat(string characterName, StatType stat)
    {
        int currentLevel = GetStatLevel(characterName, stat);

        // 최대 레벨 체크
        if (currentLevel >= MaxStatLevel)
        {
            Debug.LogWarning($"[CharacterProgressManager] {characterName}의 {stat}은(는) 이미 최대 레벨입니다.");
            return false;
        }

        // 비용 차감 시도
        int cost = GetUpgradeCost(currentLevel);
        if (!Managers.Instance.Currency.SpendProtein(cost))
        {
            Debug.LogWarning($"[CharacterProgressManager] 단백질 부족: 필요 {cost}");
            return false;
        }

        // 레벨업
        int newLevel = currentLevel + 1;
        string key = GetStatKey(characterName, stat);
        PlayerPrefs.SetInt(key, newLevel);

        OnStatUpgraded?.Invoke(characterName, stat, newLevel);
        Debug.Log($"[CharacterProgressManager] {characterName}의 {stat} 강화: Lv.{newLevel} (비용: {cost})");

        return true;
    }

    /// <summary>
    /// 강화 비용을 계산합니다.
    /// </summary>
    /// <param name="currentLevel">현재 레벨</param>
    /// <returns>다음 레벨 업그레이드 비용</returns>
    public int GetUpgradeCost(int currentLevel)
    {
        // (currentLevel + 1) * 100 = 100, 200, 300...
        return (currentLevel + 1) * BaseCost;
    }

    /// <summary>
    /// 특정 캐릭터의 스탯 배율을 계산합니다.
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    /// <param name="stat">스탯 타입</param>
    /// <returns>배율 (1.0 ~ 1.5)</returns>
    public float GetStatMultiplier(string characterName, StatType stat)
    {
        int level = GetStatLevel(characterName, stat);
        // 기본 1.0 + (레벨 * 0.05) = 최대 1.5배 (10레벨)
        return 1f + (level * BonusPerLevel);
    }

    /// <summary>
    /// 최대 강화 레벨을 반환합니다.
    /// </summary>
    public int GetMaxStatLevel() => MaxStatLevel;

    /// <summary>
    /// 레벨당 보너스 퍼센트를 반환합니다.
    /// </summary>
    public float GetBonusPerLevel() => BonusPerLevel;
    #endregion

    #region Key Helpers
    private string GetUnlockKey(string characterName)
    {
        return UnlockKeyPrefix + characterName;
    }

    private string GetStatKey(string characterName, StatType stat)
    {
        return $"{StatLevelKeyPrefix}{characterName}_{stat}";
    }
    #endregion

    #region Save/Load
    /// <summary>
    /// 데이터를 저장합니다.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.Save();
        Debug.Log("[CharacterProgressManager] 저장 완료");
    }

    /// <summary>
    /// 데이터를 로드합니다.
    /// </summary>
    public void Load()
    {
        // PlayerPrefs는 자동으로 로드되므로 별도 처리 불필요
        Debug.Log("[CharacterProgressManager] 로드 완료");
    }

    /// <summary>
    /// 특정 캐릭터의 모든 데이터를 초기화합니다. (디버그용)
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    public void ResetCharacterData(string characterName)
    {
        // 해금 초기화
        PlayerPrefs.DeleteKey(GetUnlockKey(characterName));

        // 모든 스탯 초기화
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            PlayerPrefs.DeleteKey(GetStatKey(characterName, stat));
        }

        Debug.Log($"[CharacterProgressManager] {characterName} 데이터 초기화 완료");
    }

    /// <summary>
    /// 모든 캐릭터 데이터를 초기화합니다. (디버그용)
    /// </summary>
    public void ResetAllData()
    {
        // 모든 캐릭터 관련 키를 삭제하기 위해 알려진 캐릭터 목록이 필요
        // GameManager에서 캐릭터 목록을 가져와서 처리
        var characters = Managers.Instance?.Game?.AvailableCharacters;
        if (characters != null)
        {
            foreach (var character in characters)
            {
                if (character != null)
                {
                    ResetCharacterData(character.characterName);
                }
            }
        }

        Debug.Log("[CharacterProgressManager] 모든 데이터 초기화 완료");
    }
    #endregion
}
