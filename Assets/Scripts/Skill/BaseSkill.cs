using UnityEngine;

/// <summary>
/// 모든 스킬의 기본 클래스
/// 스킬 데이터 참조, 레벨 관리, 초기화 등 공통 기능을 담당합니다.
/// </summary>
public abstract class BaseSkill : MonoBehaviour
{
    #region Protected Fields
    protected SkillDataSO _skillData;
    protected Player _player;
    protected int _currentLevel;
    #endregion

    #region Public Properties
    public SkillDataSO SkillData => _skillData;
    public int CurrentLevel => _currentLevel;
    public int MaxLevel => _skillData != null ? _skillData.levels.Count : 0;
    public bool IsMaxLevel => _currentLevel >= MaxLevel;

    /// <summary>
    /// 현재 레벨의 스킬 데이터
    /// </summary>
    public SkillLevelData CurrentLevelData
    {
        get
        {
            if (_skillData == null || _skillData.levels == null || _skillData.levels.Count == 0)
                return null;

            int index = Mathf.Clamp(_currentLevel - 1, 0, _skillData.levels.Count - 1);
            return _skillData.levels[index];
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 스킬 초기화
    /// </summary>
    /// <param name="data">스킬 데이터 SO</param>
    /// <param name="owner">스킬 소유자 (플레이어)</param>
    public virtual void Initialize(SkillDataSO data, Player owner)
    {
        _skillData = data;
        _player = owner;
        _currentLevel = 1;

        OnInitialize();
    }

    /// <summary>
    /// 스킬 레벨업
    /// </summary>
    /// <returns>레벨업 성공 여부</returns>
    public virtual bool LevelUp()
    {
        if (IsMaxLevel) return false;

        _currentLevel++;
        OnLevelUp();
        return true;
    }

    /// <summary>
    /// 스킬 레벨다운
    /// </summary>
    /// <returns>레벨다운 성공 여부</returns>
    public virtual bool LevelDown()
    {
        if (_currentLevel <= 1) return false;

        _currentLevel--;
        OnLevelChanged();
        return true;
    }

    /// <summary>
    /// 스킬 레벨 직접 설정 (디버그용)
    /// </summary>
    /// <param name="level">설정할 레벨</param>
    public virtual void SetLevel(int level)
    {
        int newLevel = Mathf.Clamp(level, 1, MaxLevel);
        if (newLevel == _currentLevel) return;

        _currentLevel = newLevel;
        OnLevelChanged();
    }
    #endregion

    #region Protected Virtual Methods
    /// <summary>
    /// 초기화 시 호출되는 콜백 (자식 클래스에서 오버라이드)
    /// </summary>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// 레벨업 시 호출되는 콜백 (자식 클래스에서 오버라이드)
    /// </summary>
    protected virtual void OnLevelUp() { }

    /// <summary>
    /// 레벨 변경 시 호출되는 콜백 (레벨다운, SetLevel 등)
    /// 기본 구현은 OnLevelUp과 동일
    /// </summary>
    protected virtual void OnLevelChanged()
    {
        OnLevelUp();
    }
    #endregion
}