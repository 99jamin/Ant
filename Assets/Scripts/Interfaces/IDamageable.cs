/// <summary>
/// 데미지를 받을 수 있는 객체가 구현하는 인터페이스
/// </summary>
public interface IDamageable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }

    void TakeDamage(float damage);
    void Heal(float amount);
}