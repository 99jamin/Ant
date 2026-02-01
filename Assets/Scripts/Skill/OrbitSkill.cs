using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 회전 스킬 (성경형)
/// 플레이어 주변을 공전하는 오브젝트들을 관리합니다.
/// </summary>
public class OrbitSkill : ActiveSkill
{
    #region Private Fields
    private readonly List<OrbitObject> _orbitObjects = new List<OrbitObject>();
    #endregion

    #region Properties
    /// <summary>
    /// 회전 오브젝트 프리팹
    /// </summary>
    private GameObject Prefab => _skillData?.skillObjectPrefab;

    /// <summary>
    /// 회전 반경 (LevelData에서 가져옴)
    /// </summary>
    public float OrbitRadius => CurrentLevelData?.orbitRadius ?? 1.5f;

    /// <summary>
    /// 오브젝트 기본 크기 (LevelData에서 가져옴)
    /// </summary>
    private float BaseObjectSize => CurrentLevelData?.orbitObjectSize ?? 0.5f;

    /// <summary>
    /// 회전 속도 (LevelData에서 가져옴, 도/초)
    /// </summary>
    public float RotationSpeed => CurrentLevelData?.orbitSpeed ?? 90f;

    /// <summary>
    /// 오브젝트 크기 (AreaMultiplier 적용)
    /// </summary>
    public float ObjectSize => BaseObjectSize * ActualAreaMultiplier;

    /// <summary>
    /// 데미지 (글로벌 배율 적용)
    /// </summary>
    public float Damage => ActualDamage;

    /// <summary>
    /// 플레이어 위치
    /// </summary>
    public Vector3 PlayerPosition => _player.transform.position;

    /// <summary>
    /// 적 레이어 (OrbitObject에서 접근용)
    /// </summary>
    public LayerMask TargetEnemyLayer => EnemyLayer;

    /// <summary>
    /// 현재 오브젝트 개수 (LevelData에서 가져옴)
    /// </summary>
    private int CurrentObjectCount => CurrentLevelData?.orbitObjectCount ?? 1;

    /// <summary>
    /// 틱 간격 (LevelData에서 가져옴)
    /// </summary>
    private float TickInterval => CurrentLevelData?.tickInterval ?? 0.5f;
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        SpawnOrbitObjects(CurrentObjectCount);

        // 글로벌 스탯 변경 이벤트 구독
        if (_player != null)
        {
            _player.OnGlobalStatsChanged += OnGlobalStatsChanged;
        }
    }

    protected override void Activate()
    {
        // 회전 스킬은 Update에서 지속적으로 회전하므로
        // Activate에서는 별도 처리 불필요
        // 데미지는 각 OrbitObject의 OnTrigger에서 처리
    }

    protected override bool RequiresTarget()
    {
        // 회전 스킬은 타겟 없이도 발동
        return false;
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();

        int targetCount = CurrentObjectCount;
        int currentCount = _orbitObjects.Count;

        // 오브젝트 수 증가 시 추가 생성
        if (targetCount > currentCount)
        {
            for (int i = currentCount; i < targetCount; i++)
            {
                SpawnSingleOrbitObject(i, targetCount);
            }

            // 기존 오브젝트들의 각도 재배치
            UpdateAllObjectAngles(targetCount);
        }

        // 모든 오브젝트 스케일 업데이트
        foreach (var obj in _orbitObjects)
        {
            obj.UpdateScale();
        }
    }
    #endregion

    #region Private Methods
    private void SpawnOrbitObjects(int count)
    {
        if (Prefab == null)
        {
            Debug.LogWarning($"[OrbitSkill] 회전 오브젝트 프리팹이 설정되지 않았습니다: {_skillData?.skillName}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            SpawnSingleOrbitObject(i, count);
        }
    }

    private void SpawnSingleOrbitObject(int index, int totalCount)
    {
        // OrbitSkill 자식으로 생성 (풀 사용 안 함 - 개수가 적고 스킬과 생명주기 동일)
        GameObject obj = Instantiate(Prefab, transform);

        OrbitObject orbitObject = obj.GetComponent<OrbitObject>();
        if (orbitObject != null)
        {
            orbitObject.Initialize(this, index, totalCount, Managers.Instance.Pool, _hitEffectPoolKey, TickInterval);
            _orbitObjects.Add(orbitObject);
        }
        else
        {
            Debug.LogWarning($"[OrbitSkill] 프리팹에 OrbitObject 컴포넌트가 없습니다.");
            Destroy(obj);
        }
    }

    private void UpdateAllObjectAngles(int totalCount)
    {
        for (int i = 0; i < _orbitObjects.Count; i++)
        {
            _orbitObjects[i].UpdateIndex(i, totalCount);
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_player != null)
        {
            _player.OnGlobalStatsChanged -= OnGlobalStatsChanged;
        }

        // 스킬 파괴 시 모든 회전 오브젝트 정리
        foreach (var obj in _orbitObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        _orbitObjects.Clear();
    }
    #endregion

    #region Event Handlers
    private void OnGlobalStatsChanged()
    {
        // 글로벌 스탯 변경 시 모든 오브젝트 스케일 업데이트
        foreach (var obj in _orbitObjects)
        {
            if (obj != null)
            {
                obj.UpdateScale();
            }
        }
    }
    #endregion
}