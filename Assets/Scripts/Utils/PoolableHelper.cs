using UnityEngine;

/// <summary>
/// 풀링 관련 공통 유틸리티 메서드
/// SpawnHitEffect, ReturnToPool 등 반복되는 로직을 통합합니다.
/// </summary>
public static class PoolableHelper
{
    /// <summary>
    /// 히트 이펙트를 지정된 위치에 스폰합니다.
    /// </summary>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="hitEffectPoolKey">히트 이펙트 풀 키</param>
    /// <param name="position">스폰 위치</param>
    public static void SpawnHitEffect(PoolManager poolManager, string hitEffectPoolKey, Vector3 position)
    {
        if (string.IsNullOrEmpty(hitEffectPoolKey)) return;
        if (poolManager == null || !poolManager.HasPool(hitEffectPoolKey)) return;

        GameObject effectObj = poolManager.Get(hitEffectPoolKey);
        effectObj.transform.position = position;

        if (effectObj.TryGetComponent<HitEffect>(out var hitEffect))
        {
            hitEffect.Initialize(poolManager, hitEffectPoolKey);
        }
    }

    /// <summary>
    /// 오브젝트를 풀에 반환합니다. 풀 매니저가 없으면 비활성화합니다.
    /// </summary>
    /// <param name="poolManager">풀 매니저</param>
    /// <param name="poolKey">풀 키</param>
    /// <param name="gameObject">반환할 게임오브젝트</param>
    public static void ReturnToPool(PoolManager poolManager, string poolKey, GameObject gameObject)
    {
        if (poolManager != null)
        {
            poolManager.Return(poolKey, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
