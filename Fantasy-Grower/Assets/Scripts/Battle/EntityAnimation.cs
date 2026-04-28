using UnityEngine;

public abstract class EntityAnimation : MonoBehaviour
{
    protected readonly int ATTACK_SPEED = Animator.StringToHash("AttackSpeed");

    protected EntityState _entityState = EntityState.Instance;

    [SerializeField]
    protected SPUM_Prefabs _prefabs;

    [SerializeField]
    protected Entity _entity;

    protected virtual void Awake()
    {
        _entityState[gameObject].OnStateChanged += OnStateChanged;
    }

    protected abstract void OnStateChanged(PlayerState state);

    protected virtual void OnDestroy()
    {
        _entityState[gameObject].OnStateChanged -= OnStateChanged;
    }

    protected virtual void OnValidate()
    {
        if (_prefabs == null)
        {
            Debug.LogWarning("[경고] SPUM_Prefabs가 비어 있습니다!");
        }
        if (_entity == null)
        {
            Debug.LogWarning("[경고] Entity가 비어 있습니다!");
        }
    }
}
