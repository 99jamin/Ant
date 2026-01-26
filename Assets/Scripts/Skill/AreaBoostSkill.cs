using UnityEngine;

/// <summary>
/// 스킬 범위 증가 패시브 스킬
/// 플레이어의 GlobalAreaMultiplier를 증가시킵니다.
/// </summary>
public class AreaBoostSkill : PassiveSkill
{
    #region Private Fields
    private float _appliedBonus;
    #endregion

    #region Properties
    /// <summary>
    /// 현재 레벨의 범위 증가량
    /// </summary>
    private float AreaBonus => CurrentLevelData?.areaMultiplier ?? 0f;
    #endregion

    #region Overrides
    protected override void OnApplyEffect()
    {
        if (player == null) return;

        _appliedBonus = AreaBonus;
        player.GlobalAreaMultiplier += _appliedBonus;
    }

    protected override void OnRemoveEffect()
    {
        if (player == null) return;

        player.GlobalAreaMultiplier -= _appliedBonus;
        _appliedBonus = 0f;
    }
    #endregion
}