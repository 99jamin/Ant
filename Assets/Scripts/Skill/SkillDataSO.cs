using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 발사 방향 타입
/// </summary>
public enum FireDirectionType
{
    TowardEnemy,    // 가장 가까운 적 방향
    HorizontalBoth, // 좌우 양방향
    PlayerFacing,   // 플레이어가 바라보는 방향
    Random          // 랜덤 방향
}

/// <summary>
/// 스킬 데이터를 담는 ScriptableObject
/// 스킬의 기본 정보와 레벨별 성장 데이터를 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class SkillDataSO : ScriptableObject
{
    #region Basic Info
    [Header("기본 정보")]
    [SerializeField] private string _skillName;
    [SerializeField] [TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    #endregion

    #region Prefabs
    [Header("프리팹")]
    [Tooltip("스킬 프리팹 (스킬 매니저에서 생성)")]
    [SerializeField] private GameObject _skillPrefab;

    [Tooltip("스킬 오브젝트 프리팹 (투사체, 장판, 오라, 회전 오브젝트 등)")]
    [SerializeField] private GameObject _skillObjectPrefab;

    [Tooltip("히트 이펙트 프리팹")]
    [SerializeField] private GameObject _hitEffectPrefab;
    #endregion

    #region Projectile Settings
    [Header("투사체 설정 (Projectile 스킬용)")]
    [Tooltip("투사체 퍼짐 각도")]
    [SerializeField] private float _spreadAngle = 15f;

    [Tooltip("발사 방향 타입")]
    [SerializeField] private FireDirectionType _fireDirection = FireDirectionType.TowardEnemy;
    #endregion

    #region Level Data
    [Header("레벨 데이터")]
    [SerializeField] private List<SkillLevelData> _levels;
    #endregion

    #region Properties
    public string skillName => _skillName;
    public string description => _description;
    public Sprite icon => _icon;
    public GameObject skillPrefab => _skillPrefab;
    public GameObject skillObjectPrefab => _skillObjectPrefab;
    public GameObject hitEffectPrefab => _hitEffectPrefab;
    public float spreadAngle => _spreadAngle;
    public FireDirectionType fireDirection => _fireDirection;
    public IReadOnlyList<SkillLevelData> levels => _levels;
    #endregion
}

/// <summary>
/// 레벨별 스킬 스탯 데이터
/// 모든 스킬 타입의 레벨별 수치를 통합 관리합니다.
/// 사용하지 않는 필드는 0 또는 기본값으로 두면 됩니다.
/// </summary>
[System.Serializable]
public class SkillLevelData
{
    [Header("공통")]
    public float damage;
    public float cooldown;
    public float areaMultiplier = 1f;
    [Tooltip("적 탐지 범위")]
    public float detectRange = 10f;

    [Header("투사체 (Projectile)")]
    [Tooltip("투사체 속도")]
    public float projectileSpeed = 10f;
    [Tooltip("투사체 지속 시간")]
    public float projectileLifetime = 5f;
    [Tooltip("투사체 수")]
    public int projectileCount = 1;
    [Tooltip("관통 수 (0이면 관통 없음)")]
    public int pierceCount;

    [Header("회전 (Orbit)")]
    [Tooltip("회전 반경")]
    public float orbitRadius = 1.5f;
    [Tooltip("오브젝트 크기")]
    public float orbitObjectSize = 0.5f;
    [Tooltip("공전 속도 (도/초)")]
    public float orbitSpeed = 90f;
    [Tooltip("회전 오브젝트 수")]
    public int orbitObjectCount = 1;

    [Header("지속 데미지 (Area, Orbit, Aura 공통)")]
    [Tooltip("틱 데미지 간격")]
    public float tickInterval = 0.5f;
    [Tooltip("장판 지속 시간")]
    public float areaLifetime = 3f;

    [Header("설명")]
    [TextArea] public string levelDescription;
}