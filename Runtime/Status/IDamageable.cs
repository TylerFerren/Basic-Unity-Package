using UnityEngine;

public interface IDamageable
{
    void Damage(float damage);
    float MaxValue { get; set; }
    float CurrentValue { get; set; }

}
