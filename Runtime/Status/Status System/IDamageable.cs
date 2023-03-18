public interface IDamageable
{
    void Damage(float amount);
    public bool IsAlive();
    float MaxValue { get; set; }
    float CurrentValue { get; set; }
}
