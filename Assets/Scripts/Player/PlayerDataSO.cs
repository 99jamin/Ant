using UnityEngine;

/// <summary>
/// 플레이어블 캐릭터의 기본 데이터를 정의하는 ScriptableObject
/// 스탯, 시작 스킬, 외형 정보를 담습니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    #region Basic Info
    [Header("기본 정보")]
    [Tooltip("캐릭터 이름")]
    public string characterName;

    [Tooltip("캐릭터 선택 슬롯에 표시될 아이콘 (애니메이션 미선택 시 사용)")]
    public Sprite icon;

    [Tooltip("로비 상세 정보에 표시될 일러스트")]
    public Sprite illustration;

    [Tooltip("로비 슬롯용 애니메이터 컨트롤러 (Idle 애니메이션)")]
    public RuntimeAnimatorController lobbyAnimatorController;

    [TextArea(2, 4)]
    [Tooltip("캐릭터 설명")]
    public string description;
    #endregion

    #region Appearance (추후 적용)
    [Header("외형 (추후 적용)")]
    [Tooltip("캐릭터 스프라이트")]
    public Sprite sprite;

    [Tooltip("캐릭터 애니메이터 컨트롤러")]
    public RuntimeAnimatorController animatorController;
    #endregion

    #region Stats Multipliers
    [Header("스탯 배율")]
    [Tooltip("기본 체력 배율 (1.0 = 100%)")]
    [Range(0.1f, 3f)]
    public float healthMultiplier = 1f;

    [Tooltip("레벨당 체력 증가량 배율 (1.0 = 100%)")]
    [Range(0.1f, 3f)]
    public float healthPerLevelMultiplier = 1f;

    [Tooltip("이동 속도 배율 (1.0 = 100%)")]
    [Range(0.5f, 2f)]
    public float moveSpeedMultiplier = 1f;

    [Tooltip("경험치 자석 반경 배율 (1.0 = 100%)")]
    [Range(0.5f, 2f)]
    public float magnetRadiusMultiplier = 1f;

    [Tooltip("공격력 배율 (1.0 = 100%)")]
    [Range(0.5f, 3f)]
    public float damageMultiplier = 1f;
    #endregion

    #region Starting Skill
    [Header("시작 스킬")]
    [Tooltip("게임 시작 시 자동으로 부여되는 스킬")]
    public SkillDataSO startingSkill;
    #endregion

    #region Unlock Settings
    [Header("해금 설정")]
    [Tooltip("해금에 필요한 단백질 (0 = 기본 해금)")]
    public int unlockCost = 0;

    [Tooltip("기본 해금 캐릭터 여부 (게임 시작 시 자동 해금)")]
    public bool isDefaultUnlocked = false;
    #endregion
}
