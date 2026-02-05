using UnityEngine;

/// <summary>
/// 보스 공격 패턴 타입
/// </summary>
public enum BossPatternType
{
    Charge,      // 돌진 - 플레이어 방향으로 빠르게 돌진
    AreaAttack,  // 범위 공격 - 주변 원형 범위 데미지
    Projectile,  // 투사체 - 플레이어 방향으로 투사체 발사
    Summon       // 소환 - 주변에 일반 적 소환
}

/// <summary>
/// 보스 공격 패턴 데이터를 정의하는 ScriptableObject
/// 보스별로 여러 패턴을 조합하여 사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "BossAttackPattern", menuName = "ScriptableObjects/Boss/AttackPattern", order = 1)]
public class BossAttackPatternSO : ScriptableObject
{
    #region Basic Info
    [Header("기본 정보")]
    [Tooltip("패턴 타입")]
    [SerializeField] private BossPatternType patternType;

    [Tooltip("애니메이션 트리거 이름")]
    [SerializeField] private string animationTrigger;

    [Tooltip("패턴 쿨타임 (초)")]
    [SerializeField] [Min(0.1f)] private float cooldown = 3f;

    [Tooltip("패턴 데미지")]
    [SerializeField] [Min(0f)] private float damage = 10f;

    [Tooltip("공격 후 경직 시간 (초)")]
    [SerializeField] [Min(0f)] private float recoveryDuration = 0.3f;
    #endregion

    #region Charge Settings
    [Header("돌진 설정 (Charge)")]
    [Tooltip("돌진 속도")]
    [SerializeField] [Min(1f)] private float chargeSpeed = 15f;

    [Tooltip("돌진 지속 시간 (초)")]
    [SerializeField] [Min(0.1f)] private float chargeDuration = 0.5f;

    [Tooltip("돌진 전 준비 시간 (초)")]
    [SerializeField] [Min(0f)] private float chargeWindup = 0.3f;
    #endregion

    #region Area Attack Settings
    [Header("범위 공격 설정 (AreaAttack)")]
    [Tooltip("공격 범위 반경")]
    [SerializeField] [Min(0.5f)] private float areaRadius = 3f;

    [Tooltip("공격 전 준비 시간 (초)")]
    [SerializeField] [Min(0f)] private float areaWindup = 0.5f;
    #endregion

    #region Projectile Settings
    [Header("투사체 설정 (Projectile)")]
    [Tooltip("투사체 프리팹")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("투사체 속도")]
    [SerializeField] [Min(1f)] private float projectileSpeed = 10f;

    [Tooltip("발사할 투사체 수")]
    [SerializeField] [Min(1)] private int projectileCount = 1;

    [Tooltip("다중 투사체 시 퍼짐 각도")]
    [SerializeField] [Range(0f, 360f)] private float projectileSpreadAngle = 30f;
    #endregion

    #region Summon Settings
    [Header("소환 설정 (Summon)")]
    [Tooltip("소환할 적 데이터")]
    [SerializeField] private EnemyDataSO summonEnemyData;

    [Tooltip("소환 수")]
    [SerializeField] [Min(1)] private int summonCount = 3;

    [Tooltip("소환 반경")]
    [SerializeField] [Min(1f)] private float summonRadius = 5f;
    #endregion

    #region Properties
    public BossPatternType PatternType => patternType;
    public string AnimationTrigger => animationTrigger;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float RecoveryDuration => recoveryDuration;

    // Charge
    public float ChargeSpeed => chargeSpeed;
    public float ChargeDuration => chargeDuration;
    public float ChargeWindup => chargeWindup;

    // AreaAttack
    public float AreaRadius => areaRadius;
    public float AreaWindup => areaWindup;

    // Projectile
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public int ProjectileCount => projectileCount;
    public float ProjectileSpreadAngle => projectileSpreadAngle;

    // Summon
    public EnemyDataSO SummonEnemyData => summonEnemyData;
    public int SummonCount => summonCount;
    public float SummonRadius => summonRadius;
    #endregion
}
