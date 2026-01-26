using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    #region Events
    public event Action<BaseSkill> OnSkillAdded;
    public event Action<BaseSkill> OnSkillRemoved;
    public event Action<BaseSkill> OnSkillLevelUp;
    #endregion

    #region Serialized Fields
    [Header("설정")]
    [SerializeField] private Transform skillContainer;
    [SerializeField] private int maxSkillCount = 6;

    [Header("스킬 데이터")]
    [SerializeField] private List<SkillDataSO> allSkills; // 모든 스킬 데이터
    [SerializeField] private SkillDataSO startingSkill;   // 시작 스킬

    [Header("레벨업 선택")]
    [SerializeField] private int choiceCount = 3; // 레벨업 시 선택지 수
    #endregion

    #region Private Fields
    private Player player;
    private readonly List<BaseSkill> activeSkills = new();
    private readonly Dictionary<string, BaseSkill> skillDictionary = new();
    #endregion

    #region Public Properties
    public IReadOnlyList<BaseSkill> ActiveSkills => activeSkills;
    public int SkillCount => activeSkills.Count;
    public bool CanAddSkill => activeSkills.Count < maxSkillCount;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (skillContainer == null)
        {
            skillContainer = transform;
        }
    }

    private void Start()
    {
        player = Player.Instance;

        if (player == null)
        {
            Debug.LogError("[SkillManager] Player.Instance가 null입니다.");
            return;
        }

        // 시작 스킬 추가
        if (startingSkill != null)
        {
            AddSkill(startingSkill);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 새 스킬 추가
    /// </summary>
    /// <param name="skillData">추가할 스킬 데이터</param>
    /// <returns>생성된 스킬 (실패 시 null)</returns>
    public BaseSkill AddSkill(SkillDataSO skillData)
    {
        if (skillData == null)
        {
            Debug.LogWarning("[SkillManager] skillData가 null입니다.");
            return null;
        }

        // 이미 보유한 스킬인지 확인
        if (skillDictionary.ContainsKey(skillData.skillName))
        {
            Debug.LogWarning($"[SkillManager] 이미 보유한 스킬입니다: {skillData.skillName}");
            return null;
        }

        // 최대 스킬 수 확인
        if (!CanAddSkill)
        {
            Debug.LogWarning("[SkillManager] 최대 스킬 수에 도달했습니다.");
            return null;
        }

        // 스킬 프리팹 확인
        if (skillData.skillPrefab == null)
        {
            Debug.LogError($"[SkillManager] 스킬 프리팹이 null입니다: {skillData.skillName}");
            return null;
        }

        // 스킬 생성
        GameObject skillObj = Instantiate(skillData.skillPrefab, skillContainer);
        skillObj.name = skillData.skillName;

        // BaseSkill 컴포넌트 확인
        if (!skillObj.TryGetComponent<BaseSkill>(out var skill))
        {
            Debug.LogError($"[SkillManager] 스킬 프리팹에 BaseSkill 컴포넌트가 없습니다: {skillData.skillName}");
            Destroy(skillObj);
            return null;
        }

        // 초기화
        skill.Initialize(skillData, player);

        // 등록
        activeSkills.Add(skill);
        skillDictionary[skillData.skillName] = skill;

        OnSkillAdded?.Invoke(skill);

        Debug.Log($"[SkillManager] 스킬 추가됨: {skillData.skillName}");
        return skill;
    }

    /// <summary>
    /// 스킬 제거
    /// </summary>
    public bool RemoveSkill(string skillName)
    {
        if (!skillDictionary.TryGetValue(skillName, out var skill))
        {
            return false;
        }

        activeSkills.Remove(skill);
        skillDictionary.Remove(skillName);

        OnSkillRemoved?.Invoke(skill);

        Destroy(skill.gameObject);
        return true;
    }

    /// <summary>
    /// 스킬 레벨업
    /// </summary>
    /// <param name="skillName">레벨업할 스킬 이름</param>
    /// <returns>레벨업 성공 여부</returns>
    public bool LevelUpSkill(string skillName)
    {
        if (!skillDictionary.TryGetValue(skillName, out var skill))
        {
            Debug.LogWarning($"[SkillManager] 스킬을 찾을 수 없습니다: {skillName}");
            return false;
        }

        if (skill.IsMaxLevel)
        {
            Debug.LogWarning($"[SkillManager] 이미 최대 레벨입니다: {skillName}");
            return false;
        }

        skill.LevelUp();
        OnSkillLevelUp?.Invoke(skill);

        Debug.Log($"[SkillManager] 스킬 레벨업: {skillName} -> Lv.{skill.CurrentLevel}");
        return true;
    }

    /// <summary>
    /// 스킬 보유 여부 확인
    /// </summary>
    public bool HasSkill(string skillName)
    {
        return skillDictionary.ContainsKey(skillName);
    }

    /// <summary>
    /// 스킬 가져오기
    /// </summary>
    public BaseSkill GetSkill(string skillName)
    {
        skillDictionary.TryGetValue(skillName, out var skill);
        return skill;
    }

    /// <summary>
    /// 특정 타입의 스킬 가져오기
    /// </summary>
    public T GetSkill<T>(string skillName) where T : BaseSkill
    {
        if (skillDictionary.TryGetValue(skillName, out var skill))
        {
            return skill as T;
        }
        return null;
    }

    /// <summary>
    /// 모든 스킬 제거
    /// </summary>
    public void ClearAllSkills()
    {
        foreach (var skill in activeSkills)
        {
            if (skill != null)
            {
                Destroy(skill.gameObject);
            }
        }

        activeSkills.Clear();
        skillDictionary.Clear();
    }

    /// <summary>
    /// 스킬 추가 또는 레벨업 (이미 보유 시 레벨업)
    /// </summary>
    public BaseSkill AddOrLevelUpSkill(SkillDataSO skillData)
    {
        if (skillData == null) return null;

        if (HasSkill(skillData.skillName))
        {
            LevelUpSkill(skillData.skillName);
            return GetSkill(skillData.skillName);
        }

        return AddSkill(skillData);
    }

    /// <summary>
    /// 레벨업 시 랜덤 스킬 선택지 제공
    /// - 미보유 스킬: 새로 획득
    /// - 보유 스킬 (최대 레벨 아닌 경우): 레벨업 가능
    /// </summary>
    /// <returns>선택 가능한 스킬 데이터 목록</returns>
    public List<SkillDataSO> GetRandomSkillChoices()
    {
        List<SkillDataSO> availableSkills = new();

        foreach (var skillData in allSkills)
        {
            if (HasSkill(skillData.skillName))
            {
                // 보유 중인 스킬: 최대 레벨이 아니면 선택지에 포함
                var skill = GetSkill(skillData.skillName);
                if (!skill.IsMaxLevel)
                {
                    availableSkills.Add(skillData);
                }
            }
            else
            {
                // 미보유 스킬: 슬롯이 남아있으면 선택지에 포함
                if (CanAddSkill)
                {
                    availableSkills.Add(skillData);
                }
            }
        }

        // 셔플 후 choiceCount개 반환
        ShuffleList(availableSkills);

        int count = Mathf.Min(choiceCount, availableSkills.Count);
        return availableSkills.GetRange(0, count);
    }

    /// <summary>
    /// 선택지에서 스킬 선택 (UI에서 호출)
    /// </summary>
    public void SelectSkill(SkillDataSO skillData)
    {
        AddOrLevelUpSkill(skillData);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Fisher-Yates 셔플
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    #endregion
}