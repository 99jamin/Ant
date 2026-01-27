using UnityEngine;

/// <summary>
/// 투사체 발사 스킬
/// 다양한 방향으로 투사체를 발사합니다.
/// </summary>
public class ProjectileSkill : ActiveSkill
{
    #region Serialized Fields
    [Header("투사체 프리팹")]
    [SerializeField] private GameObject projectilePrefab;
    #endregion

    #region Private Fields
    private string _poolKey;
    #endregion

    #region Properties
    private PoolManager PoolManager => Managers.Instance.Pool;

    /// <summary>
    /// 투사체 퍼짐 각도 (SO에서 가져옴)
    /// </summary>
    private float SpreadAngle => _skillData?.spreadAngle ?? 15f;

    /// <summary>
    /// 발사 방향 타입 (SO에서 가져옴)
    /// </summary>
    private FireDirectionType FireDirection => _skillData?.fireDirection ?? FireDirectionType.TowardEnemy;
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (projectilePrefab != null && PoolManager != null)
        {
            _poolKey = $"Projectile_{_skillData.skillName}";

            if (!PoolManager.HasPool(_poolKey))
            {
                PoolManager.CreatePool(_poolKey, projectilePrefab, 20);
            }
        }
    }

    protected override void Activate()
    {
        int count = CurrentLevelData?.projectileCount ?? 1;

        switch (FireDirection)
        {
            case FireDirectionType.TowardEnemy:
                if (_currentTarget == null) return;
                FireProjectiles((_currentTarget.position - _player.transform.position).normalized, count);
                break;

            case FireDirectionType.HorizontalBoth:
                FireProjectiles(Vector2.right, count);
                FireProjectiles(Vector2.left, count);
                break;

            case FireDirectionType.PlayerFacing:
                Vector2 facingDir = _player.Controller.FacingRight ? Vector2.right : Vector2.left;
                FireProjectiles(facingDir, count);
                break;

            case FireDirectionType.Random:
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                FireProjectiles(randomDir, count);
                break;
        }
    }

    protected override bool RequiresTarget()
    {
        return FireDirection == FireDirectionType.TowardEnemy;
    }
    #endregion

    #region Fire Logic
    private void FireProjectiles(Vector2 baseDirection, int count)
    {
        if (count <= 0) return;

        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            // 첫 번째는 기본 궤적, 이후는 점점 더 퍼짐
            float spreadOffset;
            if (i == 0)
            {
                spreadOffset = 0f;
            }
            else
            {
                int level = (i + 1) / 2;
                float sign = (i % 2 == 1) ? 1f : -1f;
                spreadOffset = SpreadAngle * level * sign;
            }

            float angle = baseAngle + spreadOffset;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            SpawnProjectile(direction);
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projObj;

        if (PoolManager != null && PoolManager.HasPool(_poolKey))
        {
            projObj = PoolManager.Get(_poolKey);
        }
        else
        {
            Debug.LogError($"[ProjectileSkill] 풀이 생성되지 않았습니다: {_poolKey}");
            projObj = Instantiate(projectilePrefab);
        }

        projObj.transform.position = _player.transform.position;

        if (projObj.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.Initialize(
                ActualDamage,
                direction,
                CurrentLevelData?.speed ?? 10f,
                CurrentLevelData?.pierceCount ?? 0,
                CurrentLevelData?.lifetime ?? 5f,
                ActualAreaMultiplier,
                PoolManager,
                _poolKey,
                _hitEffectPoolKey
            );
        }
    }
    #endregion
}
