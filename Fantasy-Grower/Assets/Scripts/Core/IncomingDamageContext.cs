public sealed class IncomingDamageContext
{
    public IncomingDamageContext(Entity target, Entity attacker, float damage)
    {
        Target = target;
        Attacker = attacker;
        Damage = damage;
    }

    public Entity Target { get; }
    public Entity Attacker { get; }
    public float Damage { get; set; }
    public bool IsCancelled { get; private set; }

    public void Cancel()
    {
        IsCancelled = true;
        Damage = 0f;
    }
}
