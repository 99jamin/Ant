using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 선택 UI를 관리하는 컴포넌트
/// 여러 캐릭터 슬롯을 관리하고 선택 이벤트를 처리합니다.
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    #region Events
    /// <summary>
    /// 캐릭터가 선택되었을 때 발행되는 이벤트
    /// </summary>
    public event Action<PlayerDataSO> OnCharacterSelected;
    #endregion

    #region Serialized Fields
    [Header("슬롯 설정")]
    [SerializeField] private CharacterSlotUI[] _characterSlots;

    [Header("상세 정보 구역")]
    [SerializeField] private Image _illustrationImage;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _descriptionText;
    #endregion

    #region Private Fields
    private PlayerDataSO _currentSelected;
    #endregion

    #region Public Properties
    /// <summary>
    /// 현재 선택된 캐릭터
    /// </summary>
    public PlayerDataSO CurrentSelected => _currentSelected;
    #endregion

    #region Public Methods
    /// <summary>
    /// 캐릭터 목록으로 UI를 초기화합니다.
    /// </summary>
    /// <param name="characters">표시할 캐릭터 목록</param>
    public void Initialize(PlayerDataSO[] characters)
    {
        if (characters == null || _characterSlots == null) return;

        for (int i = 0; i < _characterSlots.Length; i++)
        {
            if (i < characters.Length && characters[i] != null)
            {
                _characterSlots[i].gameObject.SetActive(true);
                _characterSlots[i].Bind(characters[i], OnSlotClicked);
            }
            else
            {
                // 캐릭터 수보다 슬롯이 많으면 비활성화
                _characterSlots[i].gameObject.SetActive(false);
            }
        }

        // 첫 번째 캐릭터 자동 선택
        if (characters.Length > 0 && characters[0] != null)
        {
            SelectCharacter(characters[0]);
        }
    }

    /// <summary>
    /// 특정 캐릭터를 선택합니다.
    /// </summary>
    public void SelectCharacter(PlayerDataSO character)
    {
        if (character == null) return;

        _currentSelected = character;

        // 모든 슬롯의 선택 상태 업데이트
        foreach (var slot in _characterSlots)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.SetSelected(slot.Data == character);
            }
        }

        // 상세 정보 구역 업데이트
        UpdateDetailPanel(character);

        OnCharacterSelected?.Invoke(character);
    }

    /// <summary>
    /// 상세 정보 구역을 업데이트합니다.
    /// </summary>
    private void UpdateDetailPanel(PlayerDataSO character)
    {
        if (_illustrationImage != null)
        {
            // illustration 우선, 없으면 icon 사용
            Sprite displaySprite = character.illustration != null ? character.illustration : character.icon;
            _illustrationImage.sprite = displaySprite;
            _illustrationImage.enabled = displaySprite != null;
        }

        if (_characterNameText != null)
        {
            _characterNameText.text = character.characterName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = character.description;
        }
    }
    #endregion

    #region Private Methods
    private void OnSlotClicked(PlayerDataSO character)
    {
        SelectCharacter(character);
    }
    #endregion
}
