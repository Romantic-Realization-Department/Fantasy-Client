using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackCollider : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy target;
        if (collision.gameObject.TryGetComponent<Enemy>(out target))
        {
            target.TakeDamage(player.AttackPower);
        }
    }
}
