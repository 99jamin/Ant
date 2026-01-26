using UnityEngine;

/// <summary>
/// 오라 효과 컴포넌트 (마늘형 스킬)
/// 플레이어를 따라다니며 시각적 효과를 표시합니다.
/// 데미지 로직은 AuraSkill에서 처리합니다.
/// </summary>
public class AuraEffect : MonoBehaviour
{
    #region Serialized Fields
    [Header("디버그")]
    [SerializeField] private bool showGizmos = true;
    #endregion

    #region Private Fields
    private float _radius;
    private SpriteRenderer _spriteRenderer;
    #endregion

    #region Properties
    public float Radius => _radius;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _radius > 0 ? _radius : 0.5f);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 오라 효과 초기화
    /// </summary>
    /// <param name="radius">오라 반경</param>
    public void Initialize(float radius)
    {
        _radius = radius;
        ApplyScale(radius);
    }

    /// <summary>
    /// 오라 범위 업데이트 (레벨업 시 호출)
    /// </summary>
    /// <param name="newRadius">새로운 반경</param>
    public void UpdateRadius(float newRadius)
    {
        _radius = newRadius;
        ApplyScale(newRadius);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 스프라이트 bounds 기반으로 스케일 적용
    /// </summary>
    private void ApplyScale(float radius)
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        // 스프라이트 원본 크기 (localScale 무관)
        float spriteSize = _spriteRenderer.sprite.bounds.size.x;

        // 원하는 직경 / 스프라이트 원본 크기 = 필요한 스케일
        float scale = (radius * 2f) / spriteSize;
        transform.localScale = Vector3.one * scale;
    }
    #endregion
}
