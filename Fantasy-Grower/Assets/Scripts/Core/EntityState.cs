using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStateLifetime : MonoBehaviour
{
    private EntityState _entityState;

    public void Init(EntityState entityState) => _entityState = entityState;

    private void OnDestroy()
    {
        _entityState.Remove(gameObject);
    }
}

public class EntityStateData
{
    private PlayerState _state; // PlayerState는 SPUM 에셋에 포함된 SPUM_Prefabs.cs에 정의된 enum입니다.
    public PlayerState State
    {
        get => _state;
        set
        {
            _state = value;
            OnStateChanged?.Invoke(_state);
        }
    }

    public event Action<PlayerState> OnStateChanged;
}

public class EntityState
{
    public static EntityState Instance { get; } = new(); // 싱글톤 인스턴스

    private readonly Dictionary<GameObject, EntityStateData> _entityStats = new(); // 엔티티 별 상태 데이터를 저장하는 딕셔너리

    public EntityStateData this[GameObject go]
    {
        get
        {
            if (!_entityStats.TryGetValue(go, out EntityStateData data))
            {
                _entityStats[go] = data = new();

                var lifetime = go.AddComponent<EntityStateLifetime>(); // 엔티티 상태의 수명 관리를 위한 컴포넌트 추가(엔티티 삭제 시 메모리 누수 방지)
                lifetime.Init(this);
            }

            return data;
        }
    }

    public bool Remove(GameObject go)
    {
        return _entityStats.Remove(go);
    }
}
