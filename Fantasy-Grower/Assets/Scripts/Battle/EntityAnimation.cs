using DG.Tweening;
using UnityEngine;

public abstract class EntityAnimation : MonoBehaviour
{
    protected readonly int ATTACK_SPEED = Animator.StringToHash("AttackSpeed");

    protected EntityState _entityState = EntityState.Instance;

    [Header("References")]
    [SerializeField]
    protected SPUM_Prefabs _prefabs;

    [SerializeField]
    protected Entity _entity;

    protected SpriteRenderer[] _spriteRenderers;

    [Header("Effect")]
    [SerializeField]
    protected Color _takeDamageColor;
    protected Tweener _takeDamageTweener;

    protected virtual void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _prefabs.OverrideControllerInit();
        _entityState[gameObject].OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                OnIdle();
                break;
            case PlayerState.MOVE:
                OnMove();
                break;
            case PlayerState.ATTACK:
                OnAttack();
                break;
            case PlayerState.DAMAGED:
                OnDamaged();
                break;
            case PlayerState.DEBUFF:
                OnDebuff();
                break;
            case PlayerState.DEATH:
                OnDeath();
                break;
            case PlayerState.OTHER:
                OnOther();
                break;
        }
    }

    #region 상태 변경 로직

    protected virtual void OnIdle() { }

    protected virtual void OnMove() { }

    protected virtual void OnAttack() { }

    protected virtual void OnDamaged()
    {
        if (_takeDamageTweener != null && _takeDamageTweener.IsPlaying())
            _takeDamageTweener.Complete();

        _takeDamageTweener = _spriteRenderers
            .DOColor(_takeDamageColor, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }

    protected virtual void OnDebuff() { }

    protected virtual void OnDeath() { }

    protected virtual void OnOther() { }

    #endregion

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
