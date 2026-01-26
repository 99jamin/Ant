using UnityEngine;

/// <summary>
/// 패시브 스킬의 기본 클래스
/// 플레이어의 스탯을 조정하는 역할을 담당합니다.
/// </summary>
public abstract class PassiveSkill : BaseSkill
{
    #region Protected Fields
    protected bool isApplied;
    #endregion

    #region Unity Lifecycle
    protected virtual void OnEnable()
    {
        if (player != null && !isApplied)
        {
            ApplyEffect();
        }
    }

    protected virtual void OnDisable()
    {
        if (isApplied)
        {
            RemoveEffect();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 효과 적용
    /// </summary>
    public void ApplyEffect()
    {
        if (isApplied) return;

        OnApplyEffect();
        isApplied = true;
    }

    /// <summary>
    /// 효과 제거
    /// </summary>
    public void RemoveEffect()
    {
        if (!isApplied) return;

        OnRemoveEffect();
        isApplied = false;
    }
    #endregion

    #region Protected Abstract Methods
    /// <summary>
    /// 효과 적용 로직 (자식 클래스에서 구현)
    /// </summary>
    protected abstract void OnApplyEffect();

    /// <summary>
    /// 효과 제거 로직 (자식 클래스에서 구현)
    /// </summary>
    protected abstract void OnRemoveEffect();
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        ApplyEffect();
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();

        // 레벨업 시 효과 재적용
        RemoveEffect();
        ApplyEffect();
    }
    #endregion
}