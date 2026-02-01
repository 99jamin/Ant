using UnityEngine;

/// <summary>
/// 데미지 처리 공통 유틸리티
/// 넉백 + 데미지 적용 로직을 통합합니다.
/// </summary>
public static class DamageHelper
{
    /// <summary>
    /// 대상에게 데미지를 주고 넉백을 적용합니다.
    /// Enemy인 경우 넉백 방향이 적용되고, 아닌 경우 기본 데미지만 적용됩니다.
    /// </summary>
    /// <param name="target">대상 콜라이더</param>
    /// <param name="damage">데미지량</param>
    /// <param name="knockbackDir">넉백 방향 (정규화된 벡터)</param>
    public static void DealDamageWithKnockback(Collider2D target, float damage, Vector2 knockbackDir)
    {
        if (target.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage, knockbackDir);
        }
        else if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
}
