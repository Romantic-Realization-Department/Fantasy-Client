using System;
using System.Collections.Generic;
using UnityEngine;

public class ForwardColleagueSensor : MonoBehaviour
{
    private readonly HashSet<Entity> _entities = new(); // 범위 내에 존재하는 동료

    [SerializeField]
    private Entity _myEntity; // 자신의 Entity객체

    public event Action OnBlocked;
    public event Action OnUnBlocked;

    private void Reset()
    {
        if (!_myEntity)
            _myEntity = GetComponentInParent<Entity>();
    }

    private void Awake()
    {
        if (!_myEntity)
            _myEntity = GetComponentInParent<Entity>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out Entity entity)
            && entity != _myEntity // 자기 자신을 감지했으면 추가하지 않음
            && entity.EntityType == _myEntity.EntityType // 적군이면 추가하지 않음
            && EntityState.Instance[entity.gameObject].State != PlayerState.DEATH // 이미 죽은 상태라면 추가하지 않음
        )
        {
            Add(entity);
        }
    }

    private void OnEntityDead(Entity entity)
    {
        Remove(entity);
    }

    private void Add(Entity entity)
    {
        if (_entities.Add(entity))
        {
            entity.OnDied += OnEntityDead;

            if (_entities.Count == 1)
                OnBlocked?.Invoke();
        }
    }

    private void Remove(Entity entity)
    {
        if (_entities.Remove(entity))
        {
            entity.OnDied -= OnEntityDead;

            if (_entities.Count == 0)
                OnUnBlocked?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out Entity entity)
            && entity != _myEntity
            && entity.EntityType == _myEntity.EntityType
        )
        {
            Remove(entity);
        }
    }

    private void OnDisable()
    {
        foreach (var entity in _entities)
        {
            if (entity)
                entity.OnDied -= OnEntityDead;
        }

        _entities.Clear();
    }

    private void OnValidate()
    {
        if (!_myEntity)
        {
            Debug.LogWarning("Entity가 할당되지 않았습니다!!!", this);
        }
    }
}
