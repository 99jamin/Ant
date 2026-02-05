using UnityEngine;

/// <summary>
/// 보스 데이터를 정의하는 ScriptableObject
/// EnemyDataSO를 상속하여 보스 전용 필드를 추가합니다.
/// </summary>
[CreateAssetMenu(fileName = "BossData", menuName = "ScriptableObjects/Boss/BossData", order = 0)]
public class BossDataSO : EnemyDataSO
{
    #region Boss Settings
    [Header("보스 설정")]
    [Tooltip("보스 크기 배율")]
    [SerializeField] [Min(1f)] private float scale = 2f;

    [Tooltip("보스 공격 패턴 목록")]
    [SerializeField] private BossAttackPatternSO[] attackPatterns;
    #endregion

    #region Properties
    /// <summary>
    /// 보스 크기 배율
    /// </summary>
    public float Scale => scale;

    /// <summary>
    /// 보스 공격 패턴 목록
    /// </summary>
    public BossAttackPatternSO[] AttackPatterns => attackPatterns;

    /// <summary>
    /// 보스 여부 (항상 true)
    /// </summary>
    public bool IsBoss => true;
    #endregion
}
