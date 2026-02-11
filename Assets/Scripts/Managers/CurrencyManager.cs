using System;
using UnityEngine;

/// <summary>
/// 게임 재화(단백질)를 관리하는 매니저
/// 재화의 획득, 소비, 저장/로드를 담당합니다.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    #region Constants
    private const string SaveKey = "Protein";
    #endregion

    #region Events
    /// <summary>
    /// 단백질 보유량 변동 시 발행되는 이벤트
    /// </summary>
    public event Action<int> OnProteinChanged;
    #endregion

    #region Private Fields
    private int _protein;
    #endregion

    #region Public Properties
    /// <summary>
    /// 현재 보유 단백질
    /// </summary>
    public int Protein => _protein;
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

    #region Public Methods
    /// <summary>
    /// 단백질을 추가합니다.
    /// </summary>
    /// <param name="amount">추가할 양 (양수만 허용)</param>
    public void AddProtein(int amount)
    {
        if (amount <= 0) return;

        _protein += amount;
        OnProteinChanged?.Invoke(_protein);

        Debug.Log($"[CurrencyManager] 단백질 획득: +{amount} (보유: {_protein})");
    }

    /// <summary>
    /// 단백질을 소비합니다.
    /// </summary>
    /// <param name="amount">소비할 양</param>
    /// <returns>소비 성공 여부</returns>
    public bool SpendProtein(int amount)
    {
        if (amount <= 0) return false;

        if (_protein < amount)
        {
            Debug.LogWarning($"[CurrencyManager] 단백질 부족: 필요 {amount}, 보유 {_protein}");
            return false;
        }

        _protein -= amount;
        OnProteinChanged?.Invoke(_protein);

        Debug.Log($"[CurrencyManager] 단백질 소비: -{amount} (보유: {_protein})");
        return true;
    }

    /// <summary>
    /// 단백질이 충분한지 확인합니다.
    /// </summary>
    /// <param name="amount">필요한 양</param>
    /// <returns>충분 여부</returns>
    public bool HasEnoughProtein(int amount)
    {
        return _protein >= amount;
    }

    /// <summary>
    /// 데이터를 저장합니다.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(SaveKey, _protein);
        PlayerPrefs.Save();

        Debug.Log($"[CurrencyManager] 저장 완료: 단백질 {_protein}");
    }

    /// <summary>
    /// 데이터를 로드합니다.
    /// </summary>
    public void Load()
    {
        _protein = PlayerPrefs.GetInt(SaveKey, 0);

        Debug.Log($"[CurrencyManager] 로드 완료: 단백질 {_protein}");
    }

    /// <summary>
    /// 모든 데이터를 초기화합니다. (디버그용)
    /// </summary>
    public void ResetData()
    {
        _protein = 0;
        PlayerPrefs.DeleteKey(SaveKey);
        OnProteinChanged?.Invoke(_protein);

        Debug.Log("[CurrencyManager] 데이터 초기화 완료");
    }
    #endregion
}
