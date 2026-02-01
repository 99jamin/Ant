using UnityEngine;

/// <summary>
/// 장판 스킬
/// 플레이어 위치에 장판을 생성하여 범위 내 적에게 지속 데미지를 줍니다.
/// </summary>
public class AreaSkill : ActiveSkill
{
    #region Private Fields
    private string _areaPoolKey;
    #endregion

    #region Properties
    private PoolManager PoolManager => Managers.Instance.Pool;

    /// <summary>
    /// 장판 이펙트 프리팹
    /// </summary>
    private GameObject Prefab => _skillData?.skillObjectPrefab;

    /// <summary>
    /// 틱 데미지 간격 (LevelData에서 가져옴)
    /// </summary>
    private float TickInterval => CurrentLevelData?.tickInterval ?? 0.5f;

    /// <summary>
    /// 장판 지속 시간 (LevelData에서 가져옴)
    /// </summary>
    private float AreaLifetime => CurrentLevelData?.areaLifetime ?? 3f;
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (Prefab != null && PoolManager != null)
        {
            _areaPoolKey = $"AreaObject_{_skillData.skillName}";

            if (!PoolManager.HasPool(_areaPoolKey))
            {
                PoolManager.CreatePool(_areaPoolKey, Prefab, 5);
            }
        }
    }

    protected override void Activate()
    {
        SpawnAreaEffect();
    }

    protected override bool RequiresTarget()
    {
        // 장판 스킬은 타겟 없이도 발동 가능 (플레이어 위치에 생성)
        return false;
    }
    #endregion

    #region Private Methods
    private void SpawnAreaEffect()
    {
        if (PoolManager == null || !PoolManager.HasPool(_areaPoolKey))
        {
            Debug.LogError($"[AreaSkill] 풀이 생성되지 않았습니다: {_areaPoolKey}");
            return;
        }

        GameObject areaObj = PoolManager.Get(_areaPoolKey);
        areaObj.transform.position = _player.transform.position;

        if (areaObj.TryGetComponent<AreaObject>(out var areaEffect))
        {
            areaEffect.Initialize(
                ActualDamage,
                ActualAreaMultiplier,
                TickInterval,
                AreaLifetime,
                EnemyLayer,
                PoolManager,
                _areaPoolKey,
                _hitEffectPoolKey
            );
        }
    }
    #endregion
}