using UnityEngine;
using TMPro;

/// <summary>
/// 재화(단백질) 보유량을 표시하는 UI 컴포넌트
/// </summary>
public class CurrencyUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("UI 요소")]
    [SerializeField] private TMP_Text _proteinText;

    [Header("포맷 설정")]
    [Tooltip("숫자 표시 포맷 (예: N0 = 1,000)")]
    [SerializeField] private string _format = "N0";
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // 초기 값 표시
        UpdateUI(Managers.Instance?.Currency?.Protein ?? 0);

        // 이벤트 구독
        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged += UpdateUI;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (Managers.Instance?.Currency != null)
        {
            Managers.Instance.Currency.OnProteinChanged -= UpdateUI;
        }
    }
    #endregion

    #region Private Methods
    private void UpdateUI(int amount)
    {
        if (_proteinText != null)
        {
            _proteinText.text = "PROTEIN : " + amount.ToString(_format);
        }
    }
    #endregion
}
