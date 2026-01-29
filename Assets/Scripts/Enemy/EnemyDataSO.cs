using UnityEngine;

/// <summary>
/// 적의 기본 데이터를 정의하는 ScriptableObject
/// 스폰 시 Enemy에 주입되어 스탯과 외형을 결정합니다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyDataSO : ScriptableObject
{
    #region Basic Info
    [Header("기본 정보")]
    [Tooltip("적의 고유 이름")]
    [SerializeField] private string enemyName;

    [Tooltip("적의 외형 스프라이트")]
    [SerializeField] private Sprite sprite;

    [Tooltip("적의 애니메이션 컨트롤러")]
    [SerializeField] private RuntimeAnimatorController animatorController;
    #endregion

    #region Stats
    [Header("능력치")]
    [Tooltip("최대 체력")]
    [SerializeField] [Min(1f)] private float health = 10f;

    [Tooltip("이동 속도")]
    [SerializeField] [Min(0f)] private float speed = 3f;

    [Tooltip("공격력")]
    [SerializeField] [Min(0f)] private float damage = 1f;

    [Tooltip("처치 시 획득 점수")]
    [SerializeField] [Min(0f)] private float scorePoints = 10f;

    [Tooltip("처치 시 드랍하는 경험치")]
    [SerializeField] [Min(0f)] private float expAmount = 10f;
    #endregion

    #region Properties (Read-Only)
    public string EnemyName => enemyName;
    public Sprite Sprite => sprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
    public float Health => health;
    public float Speed => speed;
    public float Damage => damage;
    public float ScorePoints => scorePoints;
    public float ExpAmount => expAmount;
    #endregion

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(enemyName))
        {
            enemyName = name;
        }
    }
#endif
    #endregion
}