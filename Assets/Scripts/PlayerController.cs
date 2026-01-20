using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 이동 및 애니메이션을 담당하는 컨트롤러
/// New Input System을 사용하여 입력을 처리합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IDamageable
{
    #region Events
    public event Action OnPlayerDeath;
    public event Action<float, float> OnHealthChanged;
    #endregion

    #region IDamageable Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => _currentHealth <= 0f;
    #endregion

    #region Serialized Fields
    [Header("이동 설정")]
    [Tooltip("플레이어 이동 속도")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("체력 설정")]
    [Tooltip("최대 체력")]
    [SerializeField] private float maxHealth = 100f;
    #endregion

    #region Private Fields
    // Animator Parameter Hash (성능 최적화)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    // Cached Components
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    // State
    private Vector2 _moveInput;
    private float _currentHealth;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
        ConfigureRigidbody();
    }

    private void Start()
    {
        InitializeHealth();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

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

    private void InitializeHealth()
    {
        _currentHealth = maxHealth;
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
        _rb.velocity = _moveInput * moveSpeed;
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
    #endregion

    #region Animation
    private void UpdateAnimation()
    {
        float currentSpeed = _moveInput.magnitude;
        _animator.SetFloat(SpeedHash, currentSpeed);
    }
    #endregion

    #region IDamageable Implementation
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void Die()
    {
        _rb.velocity = Vector2.zero;
        OnPlayerDeath?.Invoke();
    }
    #endregion
}