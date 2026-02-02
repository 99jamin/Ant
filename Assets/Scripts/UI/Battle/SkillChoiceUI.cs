using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 선택지 버튼 프리팹에 부착되는 컴포넌트
/// 아이콘, 이름, 설명을 표시하고 클릭 시 콜백을 호출합니다.
/// </summary>
public class SkillChoiceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    private Action<SkillDataSO> _onSelected;
    private SkillDataSO _skillData;

    /// <summary>
    /// 스킬 데이터를 바인딩합니다.
    /// </summary>
    /// <param name="skillData">스킬 데이터</param>
    /// <param name="nextLevel">다음 레벨 (1부터 시작)</param>
    /// <param name="onSelected">선택 콜백</param>
    public void Bind(SkillDataSO skillData, int nextLevel, Action<SkillDataSO> onSelected)
    {
        _skillData = skillData;
        _onSelected = onSelected;

        icon.sprite = skillData.icon;
        nameText.text = skillData.skillName;
        descriptionText.text = skillData.levels[nextLevel - 1].levelDescription;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _onSelected?.Invoke(_skillData);
    }
}
