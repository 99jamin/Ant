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
    #region Serialized Fields
    [Header("기본 정보")]
    [SerializeField] private string _skillName;
    [SerializeField] [TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private GameObject _skillPrefab;

    [Header("스킬 설정")]
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private FireDirectionType _fireDirection;

    [Header("히트 이펙트 설정")]
    [SerializeField] private GameObject _hitEffectPrefab;

    [Header("장판(Area) 스킬 설정")]
    [SerializeField] private float _tickInterval = 0.5f;

    [Header("레벨 데이터")]
    [SerializeField] private List<SkillLevelData> _levels;
    #endregion

    #region Properties
    public string skillName => _skillName;
    public string description => _description;
    public Sprite icon => _icon;
    public GameObject skillPrefab => _skillPrefab;
    public float detectRange => _detectRange;
    public float spreadAngle => _spreadAngle;
    public FireDirectionType fireDirection => _fireDirection;
    public GameObject hitEffectPrefab => _hitEffectPrefab;
    public float tickInterval => _tickInterval;
    public IReadOnlyList<SkillLevelData> levels => _levels;
    #endregion
}

/// <summary>
/// 레벨별 스킬 스탯 데이터
/// </summary>
[System.Serializable]
public class SkillLevelData
{
    [Header("기본 스탯")]
    public float damage;
    public float cooldown;
    public float speed;         // 투사체 속도 또는 포물선 발사력
    public float lifetime = 5f; // 투사체 지속 시간

    [Header("투사체")]
    public int projectileCount = 1;
    public int pierceCount;     // 관통 수 (0이면 관통 없음)
    public float areaMultiplier = 1f;

    [Header("설명")]
    [TextArea] public string levelDescription;
}
