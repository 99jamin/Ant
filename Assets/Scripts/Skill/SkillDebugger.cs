using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 테스트용 디버그 컴포넌트
/// 인스펙터에서 스킬 활성화/비활성화, 레벨 조정이 가능합니다.
/// </summary>
public class SkillDebugger : MonoBehaviour
{
    #region Serialized Fields
    [Header("참조")]
    [SerializeField] private SkillManager skillManager;

    [Header("스킬 추가")]
    [SerializeField] private SkillDataSO skillToAdd;
    [SerializeField] private bool addSkill;

    [Header("활성 스킬 조작 (아래 목록의 인덱스 입력)")]
    [SerializeField] private int targetSkillIndex;
    [SerializeField] private bool levelUp;
    [SerializeField] private bool levelDown;
    [SerializeField] private int setLevelTo = 1;
    [SerializeField] private bool applySetLevel;
    [SerializeField] private bool removeTargetSkill;

    [Header("전체 스킬")]
    [SerializeField] private bool removeAllSkills;

    [Header("현재 활성 스킬 목록")]
    [SerializeField] private List<SkillDebugInfo> activeSkillsInfo = new();
    #endregion

    #region Unity Lifecycle
    private void OnValidate()
    {
        // 스킬 추가
        if (addSkill)
        {
            addSkill = false;
            if (Application.isPlaying && skillManager != null && skillToAdd != null)
            {
                skillManager.AddSkill(skillToAdd);
                RefreshActiveSkillsInfo();
            }
        }

        // 타겟 스킬 가져오기
        BaseSkill targetSkill = GetSkillByIndex(targetSkillIndex);

        // 레벨업
        if (levelUp)
        {
            levelUp = false;
            if (Application.isPlaying && targetSkill != null)
            {
                targetSkill.LevelUp();
                RefreshActiveSkillsInfo();
            }
        }

        // 레벨다운
        if (levelDown)
        {
            levelDown = false;
            if (Application.isPlaying && targetSkill != null)
            {
                targetSkill.LevelDown();
                RefreshActiveSkillsInfo();
            }
        }

        // 레벨 직접 설정
        if (applySetLevel)
        {
            applySetLevel = false;
            if (Application.isPlaying && targetSkill != null)
            {
                targetSkill.SetLevel(setLevelTo);
                RefreshActiveSkillsInfo();
            }
        }

        // 타겟 스킬 제거
        if (removeTargetSkill)
        {
            removeTargetSkill = false;
            if (Application.isPlaying && skillManager != null && targetSkill != null)
            {
                skillManager.RemoveSkill(targetSkill.SkillData.skillName);
                RefreshActiveSkillsInfo();
            }
        }

        // 전체 스킬 제거
        if (removeAllSkills)
        {
            removeAllSkills = false;
            if (Application.isPlaying && skillManager != null)
            {
                skillManager.ClearAllSkills();
                RefreshActiveSkillsInfo();
            }
        }
    }

    private void Start()
    {
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<SkillManager>();
        }

        if (skillManager != null)
        {
            skillManager.OnSkillAdded += OnSkillChanged;
            skillManager.OnSkillRemoved += OnSkillChanged;
            skillManager.OnSkillLevelUp += OnSkillChanged;
        }

        RefreshActiveSkillsInfo();
    }

    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillAdded -= OnSkillChanged;
            skillManager.OnSkillRemoved -= OnSkillChanged;
            skillManager.OnSkillLevelUp -= OnSkillChanged;
        }
    }
    #endregion

    #region Private Methods
    private BaseSkill GetSkillByIndex(int index)
    {
        if (skillManager == null) return null;

        var skills = skillManager.ActiveSkills;
        if (index < 0 || index >= skills.Count) return null;

        return skills[index];
    }

    private void OnSkillChanged(BaseSkill skill)
    {
        RefreshActiveSkillsInfo();
    }

    private void RefreshActiveSkillsInfo()
    {
        activeSkillsInfo.Clear();

        if (skillManager == null) return;

        int index = 0;
        foreach (var skill in skillManager.ActiveSkills)
        {
            if (skill == null) continue;

            activeSkillsInfo.Add(new SkillDebugInfo
            {
                index = index,
                skillName = skill.SkillData?.skillName ?? "Unknown",
                currentLevel = skill.CurrentLevel,
                maxLevel = skill.MaxLevel
            });
            index++;
        }
    }
    #endregion

    #region Context Menu (에디터 우클릭 메뉴)
    [ContextMenu("Refresh Skill List")]
    private void ContextRefreshSkillList()
    {
        RefreshActiveSkillsInfo();
    }

    [ContextMenu("Add All Skills (Level 1)")]
    private void ContextAddAllSkills()
    {
        if (!Application.isPlaying || skillManager == null) return;

        // SkillManager의 allSkills에 접근해야 하는데 private이므로
        // 대신 skillToAdd를 사용하도록 안내
        Debug.Log("[SkillDebugger] skillToAdd에 스킬을 설정한 후 addSkill 체크박스를 사용하세요.");
    }
    #endregion
}

/// <summary>
/// 디버그용 스킬 정보 표시 클래스
/// </summary>
[Serializable]
public class SkillDebugInfo
{
    [Tooltip("이 인덱스를 Target Skill Index에 입력")]
    public int index;
    public string skillName;
    public int currentLevel;
    public int maxLevel;
}