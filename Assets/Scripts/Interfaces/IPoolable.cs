/// <summary>
/// 오브젝트 풀에서 관리되는 객체가 구현하는 인터페이스
/// </summary>
public interface IPoolable
{
    string PoolKey { get; }

    /// <summary>
    /// 풀에서 꺼내질 때 호출됩니다.
    /// </summary>
    void OnSpawnFromPool();

    /// <summary>
    /// 풀로 반환될 때 호출됩니다.
    /// </summary>
    void OnReturnToPool();
}