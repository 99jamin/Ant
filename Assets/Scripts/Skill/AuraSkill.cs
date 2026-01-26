using UnityEngine;

/// <summary>
/// 오라 스킬 (마늘형)
/// 플레이어 주변에 영구적인 오라를 생성하여 범위 내 적에게 지속 데미지를 줍니다.
/// </summary>
public class AuraSkill : ActiveSkill
{
    #region Serialized Fields
    [Header("오라 프리팹")]
    [SerializeField] private GameObject auraEffectPrefab;
    #endregion

    #region Private Fields
    private AuraEffect _auraEffect;

    // GC 방지용 버퍼
    private readonly Collider2D[] _hitBuffer = new Collider2D[32];
    #endregion

    #region Properties
    /// <summary>
    /// 현재 오라 반경 (글로벌 범위 배율 적용)
    /// </summary>
    private float CurrentRadius => ActualAreaMultiplier;
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        SpawnAuraEffect();

        // 글로벌 스탯 변경 이벤트 구독
        if (player != null)
        {
            player.OnGlobalStatsChanged += OnGlobalStatsChanged;
        }
    }

    protected override void Activate()
    {
        DealAuraDamage();
    }

    protected override bool RequiresTarget()
    {
        // 오라 스킬은 타겟 없이도 발동 가능
        return false;
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();

        // 레벨업 시 오라 범위 업데이트
        if (_auraEffect != null)
        {
            _auraEffect.UpdateRadius(CurrentRadius);
        }
    }
    #endregion

    #region Private Methods
    private void SpawnAuraEffect()
    {
        if (auraEffectPrefab == null)
        {
            Debug.LogWarning($"[AuraSkill] 오라 프리팹이 설정되지 않았습니다: {skillData?.skillName}");
            return;
        }

        // 플레이어 자식으로 생성 (자동으로 따라다님)
        GameObject auraObj = Instantiate(auraEffectPrefab, player.transform);
        auraObj.transform.localPosition = Vector3.zero;

        _auraEffect = auraObj.GetComponent<AuraEffect>();
        if (_auraEffect != null)
        {
            _auraEffect.Initialize(CurrentRadius);
        }
    }

    private void DealAuraDamage()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            player.transform.position,
            CurrentRadius,
            _hitBuffer,
            EnemyLayer
        );

        for (int i = 0; i < count; i++)
        {
            if (_hitBuffer[i].TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(ActualDamage);
                SpawnHitEffect(_hitBuffer[i].transform.position);
            }
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (player != null)
        {
            player.OnGlobalStatsChanged -= OnGlobalStatsChanged;
        }

        // 스킬이 파괴될 때 오라도 함께 정리
        if (_auraEffect != null)
        {
            Destroy(_auraEffect.gameObject);
        }
    }
    #endregion

    #region Event Handlers
    private void OnGlobalStatsChanged()
    {
        // 글로벌 스탯 변경 시 오라 범위 업데이트
        if (_auraEffect != null)
        {
            _auraEffect.UpdateRadius(CurrentRadius);
        }
    }
    #endregion
}
