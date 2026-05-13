public class GoldOre : DamageMeasurer
{
    private int _totalDamage;

    public void TakeDamage(int damage)
    {
        _totalDamage += damage;
        HitEffectObjPool.Spawn(transform.position, transform.rotation);
        InvokeOnTakeDamage(_totalDamage);
    }
}
