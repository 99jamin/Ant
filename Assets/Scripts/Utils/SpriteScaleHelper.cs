using UnityEngine;

/// <summary>
/// 스프라이트 스케일 관련 공통 유틸리티 메서드
/// </summary>
public static class SpriteScaleHelper
{
    /// <summary>
    /// 스프라이트 bounds 기반으로 원하는 직경에 맞게 스케일을 적용합니다.
    /// </summary>
    /// <param name="transform">스케일을 적용할 Transform</param>
    /// <param name="spriteRenderer">스프라이트 렌더러</param>
    /// <param name="radius">원하는 반경</param>
    public static void ApplyRadiusScale(Transform transform, SpriteRenderer spriteRenderer, float radius)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // 스프라이트 원본 크기 (localScale 무관)
        float spriteSize = spriteRenderer.sprite.bounds.size.x;

        // 원하는 직경 / 스프라이트 원본 크기 = 필요한 스케일
        float scale = (radius * 2f) / spriteSize;
        transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// 스프라이트 bounds 기반으로 원하는 크기에 맞게 스케일을 적용합니다.
    /// </summary>
    /// <param name="transform">스케일을 적용할 Transform</param>
    /// <param name="spriteRenderer">스프라이트 렌더러</param>
    /// <param name="desiredSize">원하는 크기</param>
    public static void ApplySizeScale(Transform transform, SpriteRenderer spriteRenderer, float desiredSize)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        float spriteSize = spriteRenderer.sprite.bounds.size.x;
        float scale = desiredSize / spriteSize;
        transform.localScale = Vector3.one * scale;
    }
}
