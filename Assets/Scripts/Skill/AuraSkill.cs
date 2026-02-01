using UnityEngine;

/// <summary>
/// 오라 스킬 (마늘형)
/// 플레이어 주변에 영구적인 오라를 생성하여 범위 내 적에게 지속 데미지를 줍니다.
/// 데미지 처리는 AuraObject에서 담당합니다.
/// </summary>
public class AuraSkill : ActiveSkill
{
    #region Private Fields
    private AuraObject _auraObject;
    #endregion

    #region Properties
    /// <summary>
    /// 오라 오브젝트 프리팹
    /// </summary>
    private GameObject Prefab => _skillData?.skillObjectPrefab;

    /// <summary>
    /// 현재 오라 반경 (글로벌 범위 배율 적용)
    /// </summary>
    private float CurrentRadius => ActualAreaMultiplier;

    /// <summary>
    /// 현재 틱 간격
    /// </summary>
    private float TickInterval => CurrentLevelData?.tickInterval ?? 0.5f;
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        SpawnAuraObject();

        // 글로벌 스탯 변경 이벤트 구독
        if (_player != null)
        {
            _player.OnGlobalStatsChanged += OnGlobalStatsChanged;
        }
    }

    protected override void Activate()
    {
        // 데미지 처리는 AuraObject에서 tickInterval 기반으로 자체 처리
    }

    protected override bool RequiresTarget()
    {
        // 오라 스킬은 타겟 없이도 발동 가능
        return false;
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        UpdateAuraStats();
    }
    #endregion

    #region Private Methods
    private void SpawnAuraObject()
    {
        if (Prefab == null)
        {
            Debug.LogWarning($"[AuraSkill] 오라 프리팹이 설정되지 않았습니다: {_skillData?.skillName}");
            return;
        }

        // AuraSkill 자식으로 생성
        GameObject auraObj = Instantiate(Prefab, transform);
        auraObj.transform.localPosition = Vector3.zero;

        _auraObject = auraObj.GetComponent<AuraObject>();
        if (_auraObject != null)
        {
            _auraObject.Initialize(
                CurrentRadius,
                ActualDamage,
                TickInterval,
                EnemyLayer,
                Managers.Instance.Pool,
                _hitEffectPoolKey
            );
        }
    }

    private void UpdateAuraStats()
    {
        if (_auraObject != null)
        {
            _auraObject.UpdateStats(CurrentRadius, ActualDamage, TickInterval);
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_player != null)
        {
            _player.OnGlobalStatsChanged -= OnGlobalStatsChanged;
        }

        // 스킬이 파괴될 때 오라도 함께 정리
        if (_auraObject != null)
        {
            Destroy(_auraObject.gameObject);
        }
    }
    #endregion

    #region Event Handlers
    private void OnGlobalStatsChanged()
    {
        UpdateAuraStats();
    }
    #endregion
}
