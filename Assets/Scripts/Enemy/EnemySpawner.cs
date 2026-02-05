using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 적을 주기적으로 스폰하는 스포너
/// 플레이어 주변 도넛 형태 영역에서 적을 생성하고,
/// 너무 멀어진 적은 플레이어 진행 방향 앞으로 재배치합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region Constants
    public const string POOL_KEY = "Enemy";
    public const string BOSS_POOL_KEY = "Boss";
    #endregion

    #region Events
    public event Action<Enemy> OnEnemySpawned;
    public event Action<Enemy> OnEnemyRelocated;
    #endregion

    #region Serialized Fields
    [Header("프리팹 설정")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("풀링 설정")]
    [SerializeField] private int poolInitialSize = 20;
    [SerializeField] private int bossPoolInitialSize = 2;

    [Header("스폰 영역 (도넛 형태)")]
    [Tooltip("플레이어로부터의 최소 스폰 거리")]
    [SerializeField] private float minSpawnRadius = 15f;
    [Tooltip("플레이어로부터의 최대 스폰 거리")]
    [SerializeField] private float maxSpawnRadius = 25f;

    [Header("재배치 설정")]
    [Tooltip("이 거리 이상 멀어지면 재배치")]
    [SerializeField] private float relocateDistance = 30f;
    [Tooltip("플레이어 진행 방향 앞에 재배치할 거리")]
    [SerializeField] private float relocateAheadDistance = 30f;
    [Tooltip("재배치 시 좌우 랜덤 오프셋 범위")]
    [SerializeField] private float relocateSpreadRange = 15f;

    [Header("장애물 체크")]
    [SerializeField] private LayerMask obstacleLayer;
    [Tooltip("스폰 위치 주변 장애물 검사 반경")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;
    #endregion

    #region Private Fields
    private Transform _playerTransform;
    private float _spawnTimer;
    private bool _isInitialized;

    // 활성 Enemy 추적
    private readonly HashSet<Enemy> _activeEnemies = new();
    private readonly List<Enemy> _enemiesToProcess = new(); // GC 방지용 재사용 버퍼

    // 플레이어 이동 방향 계산용
    private Vector3 _lastPlayerPosition;
    private Vector3 _playerMoveDirection;

    // 웨이브 데이터 (BattleManager에서 주입)
    private EnemyDataSO[] _enemyTypes;
    private float _spawnInterval = 1f;
    private int _spawnCount = 1;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        UpdatePlayerDirection();
        UpdateSpawnTimer();
        UpdateEnemyRelocation();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        if (!ValidateConfiguration()) return;

        InitializePool();
        FindPlayer();

        _isInitialized = _playerTransform != null;

        if (_isInitialized)
        {
            _lastPlayerPosition = _playerTransform.position;
            _playerMoveDirection = Vector3.right; // 기본 방향
        }
    }

    private bool ValidateConfiguration()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab is not assigned.", this);
            return false;
        }

        if (minSpawnRadius >= maxSpawnRadius)
        {
            Debug.LogError("[EnemySpawner] minSpawnRadius must be less than maxSpawnRadius.", this);
            return false;
        }

        return true;
    }

    private void InitializePool()
    {
        Managers.Instance.Pool.CreatePool(POOL_KEY, enemyPrefab, poolInitialSize);

        if (bossPrefab != null)
        {
            Managers.Instance.Pool.CreatePool(BOSS_POOL_KEY, bossPrefab, bossPoolInitialSize);
        }
    }

    private void FindPlayer()
    {
        if (Player.Instance != null)
        {
            _playerTransform = Player.Instance.transform;
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] Player.Instance is null.", this);
        }
    }
    #endregion

    #region Player Direction
    private void UpdatePlayerDirection()
    {
        Vector3 currentPosition = _playerTransform.position;
        Vector3 movement = currentPosition - _lastPlayerPosition;

        // 이동이 있을 때만 방향 업데이트
        if (movement.sqrMagnitude > 0.001f)
        {
            _playerMoveDirection = movement.normalized;
        }

        _lastPlayerPosition = currentPosition;
    }
    #endregion

    #region Spawn Logic
    private void UpdateSpawnTimer()
    {
        // 웨이브 데이터가 없으면 스폰하지 않음
        if (_enemyTypes == null || _enemyTypes.Length == 0) return;

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= _spawnInterval)
        {
            TrySpawnEnemy();
            _spawnTimer = 0f;
        }
    }

    private void TrySpawnEnemy()
    {
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 spawnPosition = CalculateRandomSpawnPosition();

            if (IsPositionBlocked(spawnPosition)) continue;

            SpawnEnemyAt(spawnPosition);
        }
    }

    private void SpawnEnemyAt(Vector3 position)
    {
        EnemyDataSO selectedData = GetRandomEnemyData();
        GameObject enemyObj = Managers.Instance.Pool.Get(POOL_KEY);

        enemyObj.transform.position = position;

        if (enemyObj.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.Init(selectedData, POOL_KEY);
            RegisterEnemy(enemy);
            OnEnemySpawned?.Invoke(enemy);
        }
    }

    private EnemyDataSO GetRandomEnemyData()
    {
        int randomIndex = Random.Range(0, _enemyTypes.Length);
        return _enemyTypes[randomIndex];
    }
    #endregion

    #region Wave System
    /// <summary>
    /// 웨이브 데이터를 적용합니다. BattleManager에서 호출됩니다.
    /// </summary>
    public void SetWaveData(WaveEntry waveEntry)
    {
        if (waveEntry == null) return;

        _enemyTypes = waveEntry.EnemyTypes;
        _spawnInterval = waveEntry.SpawnInterval;
        _spawnCount = waveEntry.SpawnCount;

        // 웨이브 전환 시 스폰 타이머 리셋
        _spawnTimer = 0f;

        Debug.Log($"[EnemySpawner] 웨이브 적용 - 적 종류: {_enemyTypes.Length}, 스폰 간격: {_spawnInterval}초, 스폰 수: {_spawnCount}");
    }

    /// <summary>
    /// 보스를 스폰합니다. BattleManager에서 호출됩니다.
    /// </summary>
    public void SpawnBoss(BossDataSO bossData)
    {
        if (bossData == null)
        {
            Debug.LogWarning("[EnemySpawner] SpawnBoss 실패: BossData가 null입니다.");
            return;
        }

        if (!Managers.Instance.Pool.HasPool(BOSS_POOL_KEY))
        {
            Debug.LogWarning("[EnemySpawner] SpawnBoss 실패: 보스 풀이 초기화되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = CalculateRandomSpawnPosition();

        GameObject bossObj = Managers.Instance.Pool.Get(BOSS_POOL_KEY);
        bossObj.transform.position = spawnPosition;

        // Boss 컴포넌트로 초기화
        if (bossObj.TryGetComponent<Boss>(out var boss))
        {
            boss.Init(bossData, BOSS_POOL_KEY);
            boss.SetSpawnerReference(this);
            RegisterEnemy(boss);
            OnEnemySpawned?.Invoke(boss);
        }
        else
        {
            Debug.LogError("[EnemySpawner] 보스 프리팹에 Boss 컴포넌트가 없습니다.");
        }
    }
    #endregion

    #region Enemy Tracking
    /// <summary>
    /// 적을 활성 목록에 등록합니다. (외부 소환 시에도 호출 가능)
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        _activeEnemies.Add(enemy);
        enemy.OnDeath += HandleEnemyDeath;
    }

    private void UnregisterEnemy(Enemy enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        _activeEnemies.Remove(enemy);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        UnregisterEnemy(enemy);
    }
    #endregion

    #region Enemy Relocation
    private void UpdateEnemyRelocation()
    {
        if (_activeEnemies.Count == 0) return;

        // HashSet을 List로 복사하여 순회 중 수정 가능하도록 함
        _enemiesToProcess.Clear();
        foreach (var enemy in _activeEnemies)
        {
            _enemiesToProcess.Add(enemy);
        }

        foreach (Enemy enemy in _enemiesToProcess)
        {
            if (enemy == null || enemy.IsDead) continue;

            if (ShouldRelocateEnemy(enemy))
            {
                RelocateEnemy(enemy);
            }
        }
    }

    private bool ShouldRelocateEnemy(Enemy enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, _playerTransform.position);
        return distance > relocateDistance;
    }

    private void RelocateEnemy(Enemy enemy)
    {
        Vector3 newPosition = CalculateRelocationPosition();

        // 장애물 체크
        if (IsPositionBlocked(newPosition))
        {
            // 장애물이 있으면 스폰 영역 내 랜덤 위치로 대체
            newPosition = CalculateRandomSpawnPosition();
        }

        enemy.transform.position = newPosition;
        OnEnemyRelocated?.Invoke(enemy);
    }

    private Vector3 CalculateRelocationPosition()
    {
        // 플레이어 진행 방향 앞쪽
        Vector3 aheadPosition = _playerTransform.position + _playerMoveDirection * relocateAheadDistance;

        // 좌우 랜덤 오프셋 (진행 방향의 수직 방향)
        Vector3 perpendicular = new Vector3(-_playerMoveDirection.y, _playerMoveDirection.x, 0f);
        float randomOffset = Random.Range(-relocateSpreadRange, relocateSpreadRange);

        return aheadPosition + perpendicular * randomOffset;
    }
    #endregion

    #region Position Calculation
    private Vector3 CalculateRandomSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );

        return (Vector2)_playerTransform.position + offset;
    }

    private bool IsPositionBlocked(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, obstacleCheckRadius, obstacleLayer);
        return hit != null;
    }
    #endregion

    #region Editor Visualization
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform center = _playerTransform != null ? _playerTransform : transform;

        // 스폰 영역 - 최소 반경 (초록색)
        Gizmos.color = Color.green;
        DrawCircle(center.position, minSpawnRadius);

        // 스폰 영역 - 최대 반경 (빨간색)
        Gizmos.color = Color.red;
        DrawCircle(center.position, maxSpawnRadius);

        // 재배치 거리 (파란색)
        Gizmos.color = Color.blue;
        DrawCircle(center.position, relocateDistance);

        // 플레이어 진행 방향 및 재배치 위치 (노란색)
        if (_playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 aheadPos = center.position + _playerMoveDirection * relocateAheadDistance;
            Gizmos.DrawLine(center.position, aheadPos);
            Gizmos.DrawWireSphere(aheadPos, relocateSpreadRange);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments = 64)
    {
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = Mathf.Deg2Rad * (i * angleStep);
            float angle2 = Mathf.Deg2Rad * ((i + 1) * angleStep);

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0f) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0f) * radius;

            Gizmos.DrawLine(point1, point2);
        }
    }
#endif
    #endregion
}