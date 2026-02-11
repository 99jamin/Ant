using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 결과 팝업 UI
/// 생존 시간, 획득 점수를 표시하고 로비로 돌아갑니다.
/// </summary>
public class ResultUI : BaseUI
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text survivalTimeText;
    [SerializeField] private TMP_Text scoreText;

    [Header("버튼")]
    [SerializeField] private Button confirmButton;

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 결과 화면을 표시합니다.
    /// </summary>
    /// <param name="survivalTime">생존 시간 (초)</param>
    /// <param name="score">획득 점수</param>
    public void Show(float survivalTime, int score)
    {
        int minutes = (int)(survivalTime / 60);
        int seconds = (int)(survivalTime % 60);

        survivalTimeText.SetText("생존 시간 : {0:00}:{1:00}", minutes, seconds);
        scoreText.SetText("획득 단백질 : {0}", score);

        Open();
    }
    #endregion

    #region Button Handlers
    private void OnConfirmButtonClicked()
    {
        Close();
        Managers.Instance.Game.LoadLobby();
    }
    #endregion
}
