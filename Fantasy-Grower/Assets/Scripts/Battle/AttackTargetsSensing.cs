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

    private readonly HashSet<Entity> _targets = new();

    private readonly HashSet<Entity> _targetListCache = new();

    public IReadOnlyCollection<Entity> GetTargets()
    {
        foreach (var target in _targets)
        {
            if (target)
                _targetListCache.Add(target);
        }
        return _targetListCache;
    }

    private IAttackEvent _attackEvent;

    private void Awake()
    {
        _attackEvent = GetComponentInParent<IAttackEvent>();
        if (_attackEvent == null)
            Debug.LogError("[AttackTargets] 부모 중 IAttackEvent 구현체가 없습니다.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Entity>(out var entity) && entity.EntityType != _type)
        {
            if (_targets.Add(entity))
            {
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
}
