using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackCollider : MonoBehaviour
{
    public EntityType type;

    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity target;
        if (!collision.gameObject.TryGetComponent<Entity>(out target))
            return;

        bool shouldHit =
            (type == EntityType.Player && target is Enemy)
            || (type == EntityType.Enemy && target is Player);

        if (!shouldHit)
            return;

        var (damage, _) = DamageCalculator.Calculate(
            entity.AttackPower,
            target.DamageReduction,
            entity.CriticalPercentage
        );
        target.TakeDamage(damage);
    }
}

public enum EntityType
{
    Player,
    Enemy,
}
