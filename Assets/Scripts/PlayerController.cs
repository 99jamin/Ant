using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 물리적 이동 및 입력 처리를 담당하는 컨트롤러
/// New Input System을 사용하여 입력을 처리하고, Rigidbody2D를 제어합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("이동 설정")]
    [Tooltip("플레이어 기본 이동 속도")]
    [SerializeField] private float baseMoveSpeed = 5f;
    #endregion

    #region Private Fields
    // Animator Parameter Hash (성능 최적화)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    // Cached Components
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    // State
    private Vector2 _moveInput;
    private float _currentMoveSpeed;
    private bool _isMovementEnabled = true;
    #endregion

    #region Public Properties
    public Vector2 MoveInput => _moveInput;
    public Vector2 Velocity => _rb.velocity;
    public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
    public bool FacingRight => !_spriteRenderer.flipX;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
        ConfigureRigidbody();
        _currentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!_isMovementEnabled)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        Move();
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void ConfigureRigidbody()
    {
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// New Input System에서 호출되는 이동 입력 콜백
    /// </summary>
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    #endregion

    #region Movement
    private void Move()
    {
        _rb.velocity = _moveInput * _currentMoveSpeed;
        UpdateSpriteDirection();
    }

    private void UpdateSpriteDirection()
    {
        _spriteRenderer.flipX = _moveInput.x switch
        {
            < 0 => true,
            > 0 => false,
            _ => _spriteRenderer.flipX
        };
    }

    /// <summary>
    /// 이동 활성화/비활성화
    /// </summary>
    public void SetMovementEnabled(bool isEnabled)
    {
        _isMovementEnabled = isEnabled;

        if (!isEnabled)
        {
            _rb.velocity = Vector2.zero;
            _animator.SetBool(IsDeadHash, true);
        }
        else
        {
            _animator.SetBool(IsDeadHash, false);
        }
    }

    /// <summary>
    /// 이동 속도 배율 적용 (버프/디버프용)
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        _currentMoveSpeed = baseMoveSpeed * multiplier;
    }

    /// <summary>
    /// 이동 속도 초기화
    /// </summary>
    public void ResetSpeed()
    {
        _currentMoveSpeed = baseMoveSpeed;
    }
    #endregion

    #region Animation
    private void UpdateAnimation()
    {
        float currentSpeed = _moveInput.magnitude;
        _animator.SetFloat(SpeedHash, currentSpeed);
    }

    /// <summary>
    /// 애니메이터 트리거 실행
    /// </summary>
    public void TriggerAnimation(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// 애니메이터 bool 파라미터 설정
    /// </summary>
    public void SetAnimationBool(string paramName, bool value)
    {
        _animator.SetBool(paramName, value);
    }
    #endregion
}