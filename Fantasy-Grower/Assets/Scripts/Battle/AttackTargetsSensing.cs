using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적대 대상의 감지를 담당하는 컴포넌트. (추후 리팩토링하여 IAttackEvent와의 의존성 제거 필요)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AttackTargetsSensing : MonoBehaviour
{
    [SerializeField]
    private EntityType _type;

    private readonly List<Entity> _targets = new();

    private BoxCollider2D boxCollider;
    private Entity owner;
    private Vector2 originalSize;
    private Vector2 originalOffset;

    public Entity GetFirstTarget()
    {
        if (_targets.Count > 0)
            return _targets[0];
        return null;
    }

    public IReadOnlyList<Entity> GetTargets() => _targets;

    private IAttackEvent _attackEvent;

    private void Awake()
    {
        _attackEvent = GetComponentInParent<IAttackEvent>();
        if (_attackEvent == null)
            Debug.LogError("[AttackTargets] 부모 중 IAttackEvent 구현체가 없습니다.", this);

        owner = GetComponentInParent<Entity>();
        if (owner != null)
            owner.OnStatsChanged += RecalculateRange;

        CacheRangeCollider();
        RecalculateRange();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            other.TryGetComponent<Entity>(out var entity)
            && entity.EntityType != _type
            && EntityState.Instance[entity.gameObject].State != PlayerState.DEATH // 이미 죽은 상태라면 추가하지 않음
        )
        {
            if (!_targets.Contains(entity))
            {
                _targets.Add(entity);
                entity.OnDied += HandleTargetDied;

                if (_targets.Count == 1)
                    _attackEvent.StartAttacking();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Entity>(out var entity))
        {
            RemoveTarget(entity);
        }
    }

    private void HandleTargetDied(Entity entity)
    {
        if (entity.TryGetComponent(out IAttackEvent attackEvent) && attackEvent != null)
            attackEvent.StopAttacking();
        RemoveTarget(entity);
    }

    private void RemoveTarget(Entity entity)
    {
        if (_targets.Remove(entity))
        {
            entity.OnDied -= HandleTargetDied;

            if (_targets.Count == 0)
                _attackEvent.StopAttacking();
        }
    }

    private void OnDisable()
    {
        foreach (var target in _targets)
        {
            if (target)
                target.OnDied -= HandleTargetDied;
        }
        _targets.Clear();
    }

    private void OnDestroy()
    {
        if (owner != null)
            owner.OnStatsChanged -= RecalculateRange;
    }

    private bool CacheRangeCollider()
    {
        if (boxCollider != null)
            return true;

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError(
                "[AttackTargets] 사거리 패시브는 BoxCollider2D 감지 콜라이더에서만 동작합니다.",
                this
            );
            return false;
        }

        originalSize = boxCollider.size;
        originalOffset = boxCollider.offset;
        return true;
    }

    private void RecalculateRange()
    {
        if (!CacheRangeCollider())
            return;

        float attackRange =
            owner != null && owner.AttackRange > 0f ? owner.AttackRange : originalSize.x;
        float newWidth = Mathf.Max(0f, attackRange);
        float widthDelta = newWidth - originalSize.x;

        boxCollider.size = new Vector2(newWidth, originalSize.y);
        boxCollider.offset = new Vector2(originalOffset.x - widthDelta * 0.5f, originalOffset.y);
    }
}
