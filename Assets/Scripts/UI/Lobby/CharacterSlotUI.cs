using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 선택 UI에서 개별 캐릭터 슬롯을 담당하는 컴포넌트
/// </summary>
public class CharacterSlotUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("UI 요소")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Animator _iconAnimator;
    [SerializeField] private Button _selectButton;

    [Header("선택 효과")]
    [SerializeField] private GameObject _selectedIndicator;
    #endregion

    #region Private Fields
    private PlayerDataSO _data;
    private Action<PlayerDataSO> _onSelected;
    #endregion

    #region Public Properties
    public PlayerDataSO Data => _data;
    #endregion

    #region Public Methods
    /// <summary>
    /// 슬롯에 캐릭터 데이터를 바인딩합니다.
    /// </summary>
    /// <param name="data">캐릭터 데이터</param>
    /// <param name="onSelected">선택 시 콜백</param>
    public void Bind(PlayerDataSO data, Action<PlayerDataSO> onSelected)
    {
        _data = data;
        _onSelected = onSelected;

        // 아이콘 업데이트
        if (_iconImage != null && data.icon != null)
        {
            _iconImage.sprite = data.icon;
        }

        // 애니메이터 설정
        if (_iconAnimator != null && data.lobbyAnimatorController != null)
        {
            _iconAnimator.runtimeAnimatorController = data.lobbyAnimatorController;
            _iconAnimator.enabled = false; // 선택 전까지 비활성화
        }

        // 버튼 이벤트 설정
        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(OnClick);
        }

        SetSelected(false);
    }

    /// <summary>
    /// 선택 상태를 설정합니다.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (_selectedIndicator != null)
        {
            _selectedIndicator.SetActive(isSelected);
        }

        // 선택 시 애니메이션 재생
        if (_iconAnimator != null && _data?.lobbyAnimatorController != null)
        {
            _iconAnimator.enabled = isSelected;

            if (isSelected)
            {
                // 애니메이션 처음부터 재생
                _iconAnimator.Play(0, 0, 0f);
            }
        }

        // 애니메이터가 없거나 비선택 시 정적 아이콘 표시
        if (!isSelected && _iconImage != null && _data?.icon != null)
        {
            _iconImage.sprite = _data.icon;
        }
    }
    #endregion

    #region Private Methods
    private void OnClick()
    {
        _onSelected?.Invoke(_data);
    }
    #endregion
}
