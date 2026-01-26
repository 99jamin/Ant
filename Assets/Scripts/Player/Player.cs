using System;
using UnityEngine;

/// <summary>
/// 플레이어의 상태 데이터 및 컴포넌트를 관리하는 허브 클래스
/// HP, 레벨, 경험치 등 플레이어 상태를 담당합니다.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour, IDamageable
{
    #region Static Instance
    public static Player Instance { get; private set; }
    #endregion

    #region Events
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
    public event Action<int> OnLevelChanged;
    public event Action<float, float> OnExperienceChanged;
    public event Action OnGlobalStatsChanged;
    #endregion

    #region IDamageable Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0f;
    #endregion

    #region Public Properties
    public int Level => _level;
    public float CurrentExperience => _currentExperience;
    public float ExperienceToNextLevel => CalculateRequiredExperience(_level);
    public PlayerController Controller => _controller;
    public float MagnetRadius { get; set; }

    // 스킬 매니저에서 수정할 글로벌 배율
    public float GlobalAreaMultiplier
    {
        get => _globalAreaMultiplier;
        set
        {
            if (Mathf.Approximately(_globalAreaMultiplier, value)) return;
            _globalAreaMultiplier = value;
            OnGlobalStatsChanged?.Invoke();
        }
    }

    public float GlobalCooldownMultiplier
    {
        get => _globalCooldownMultiplier;
        set
        {
            if (Mathf.Approximately(_globalCooldownMultiplier, value)) return;
            _globalCooldownMultiplier = value;
            OnGlobalStatsChanged?.Invoke();
        }
    }

    public float GlobalDamageMultiplier
    {
        get => _globalDamageMultiplier;
        set
        {
            if (Mathf.Approximately(_globalDamageMultiplier, value)) return;
            _globalDamageMultiplier = value;
            OnGlobalStatsChanged?.Invoke();
        }
    }
    #endregion

    #region Serialized Fields
    [Header("체력 설정")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float healthPerLevel = 10f;

    [Header("레벨 설정")]
    [SerializeField] private int maxLevel = 99;
    [SerializeField] private float baseExperienceRequired = 100f;
    [SerializeField] private float experienceMultiplier = 1.2f;

    [Header("무적 시간")]
    [SerializeField] private float invincibilityDuration = 0.5f;

    [Header("경험치 자석")]
    [SerializeField] private float baseMagnetRadius = 3f;
    [SerializeField] private float magnetPullSpeed = 15f;
    [SerializeField] private LayerMask experienceLayer;
    #endregion

    #region Private Fields
    private PlayerController _controller;

    // Stats
    private float _maxHealth;
    private float _currentHealth;
    private int _level = 1;
    private float _currentExperience;

    // Global Multipliers
    private float _globalAreaMultiplier = 1f;
    private float _globalCooldownMultiplier = 1f;
    private float _globalDamageMultiplier = 1f;

    // State
    private bool _isInvincible;
    private float _invincibilityTimer;

    // Magnet
    private readonly Collider2D[] _magnetBuffer = new Collider2D[32]; // GC 방지용 버퍼
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        CacheComponents();
    }

    private void Start()
    {
        InitializeStats();
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateInvincibility();
        UpdateMagnet();
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _controller = GetComponent<PlayerController>();
    }

    private void InitializeStats()
    {
        _level = 1;
        _currentExperience = 0f;
        MagnetRadius = baseMagnetRadius;
        CalculateMaxHealth();
        _currentHealth = _maxHealth;
    }

    private void CalculateMaxHealth()
    {
        _maxHealth = baseHealth + (healthPerLevel * (_level - 1));
    }
    #endregion

    #region IDamageable Implementation
    public void TakeDamage(float damage)
    {
        if (IsDead || _isInvincible) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (IsDead)
        {
            Die();
        }
        else
        {
            StartInvincibility();
            _controller.PlayDamageFlash();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    #endregion

    #region Invincibility
    private void StartInvincibility()
    {
        _isInvincible = true;
        _invincibilityTimer = invincibilityDuration;
    }

    private void UpdateInvincibility()
    {
        if (!_isInvincible) return;

        _invincibilityTimer -= Time.deltaTime;
        if (_invincibilityTimer <= 0f)
        {
            _isInvincible = false;
        }
    }
    #endregion

    #region Experience & Level
    /// <summary>
    /// 경험치를 획득합니다.
    /// </summary>
    public void GainExperience(float amount)
    {
        if (IsDead || _level >= maxLevel) return;

        _currentExperience += amount;

        while (_currentExperience >= ExperienceToNextLevel && _level < maxLevel)
        {
            LevelUp();
        }

        OnExperienceChanged?.Invoke(_currentExperience, ExperienceToNextLevel);
    }

    private void LevelUp()
    {
        _currentExperience -= ExperienceToNextLevel;
        _level++;

        // 체력 증가 및 회복
        float previousMaxHealth = _maxHealth;
        CalculateMaxHealth();
        _currentHealth += (_maxHealth - previousMaxHealth);

        OnLevelChanged?.Invoke(_level);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private float CalculateRequiredExperience(int level)
    {
        return baseExperienceRequired * Mathf.Pow(experienceMultiplier, level - 1);
    }
    #endregion

    #region Death
    private void Die()
    {
        _controller.SetMovementEnabled(false);
        OnDeath?.Invoke();
    }

    /// <summary>
    /// 플레이어를 부활시킵니다.
    /// </summary>
    public void Revive(float healthPercent = 1f)
    {
        _currentHealth = _maxHealth * Mathf.Clamp01(healthPercent);
        _controller.SetMovementEnabled(true);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    #endregion

    #region Magnet
    private void UpdateMagnet()
    {
        if (MagnetRadius <= 0f) return;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, MagnetRadius, _magnetBuffer, experienceLayer);
        float pullDelta = magnetPullSpeed * Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            Transform target = _magnetBuffer[i].transform;
            Vector3 direction = (transform.position - target.position).normalized;
            target.position += direction * pullDelta;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 체력을 최대치로 회복합니다.
    /// </summary>
    public void FullHeal()
    {
        Heal(_maxHealth);
    }

    /// <summary>
    /// 현재 무적 상태인지 확인합니다.
    /// </summary>
    public bool IsInvincible => _isInvincible;
    #endregion
}