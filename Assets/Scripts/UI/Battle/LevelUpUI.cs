using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨업 스킬 선택 팝업 UI
/// 풀링 방식으로 SkillChoiceUI 인스턴스를 재사용합니다.
/// </summary>
public class LevelUpUI : BaseUI
{
    [Header("스킬 선택지")]
    [SerializeField] private SkillChoiceUI choicePrefab;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private int poolSize = 4;

    /// <summary>
    /// 스킬 선택 시 호출되는 콜백
    /// </summary>
    public event Action<SkillDataSO> OnSkillSelected;

    private readonly List<SkillChoiceUI> _pool = new();

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SkillChoiceUI instance = Instantiate(choicePrefab, choiceContainer);
            instance.gameObject.SetActive(false);
            _pool.Add(instance);
        }
    }

    /// <summary>
    /// 스킬 선택지를 표시합니다.
    /// </summary>
    public void Show(List<(SkillDataSO skillData, int nextLevel)> choices)
    {
        HideAllChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            SkillChoiceUI choice = GetOrExpandPool(i);
            choice.Bind(choices[i].skillData, choices[i].nextLevel, OnChoiceSelected);
            choice.gameObject.SetActive(true);
        }

        Open();
    }

    private SkillChoiceUI GetOrExpandPool(int index)
    {
        if (index < _pool.Count) return _pool[index];

        // 풀이 부족할 경우 확장
        SkillChoiceUI instance = Instantiate(choicePrefab, choiceContainer);
        _pool.Add(instance);
        return instance;
    }

    private void OnChoiceSelected(SkillDataSO skillData)
    {
        OnSkillSelected?.Invoke(skillData);
    }

    public override void Close()
    {
        base.Close();
        HideAllChoices();
    }

    private void HideAllChoices()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].gameObject.SetActive(false);
        }
    }
}
