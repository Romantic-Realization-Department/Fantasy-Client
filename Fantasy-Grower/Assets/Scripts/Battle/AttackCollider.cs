using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackCollider : MonoBehaviour
{
    [SerializeField]
    private EntityType type;

    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<Entity>(out Entity target))
            return;

        bool shouldHit = (type != entity.EntityType);

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
