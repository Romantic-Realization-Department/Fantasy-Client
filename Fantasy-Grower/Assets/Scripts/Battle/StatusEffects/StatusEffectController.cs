using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StatusEffectController : MonoBehaviour
{
    private sealed class StatusEffectInstance
    {
        public StatusEffectType Type;
        public Entity Source;
        public float RemainingDuration;
        public float DamagePerSecond;
        public EntityStatModifierHandle ModifierHandle;
    }

    private readonly List<StatusEffectInstance> effects = new();
    private Entity owner;

    public static StatusEffectController GetOrAdd(Entity entity)
    {
        if (entity == null)
            return null;

        if (!entity.TryGetComponent(out StatusEffectController controller))
            controller = entity.gameObject.AddComponent<StatusEffectController>();

        return controller;
    }

    public bool HasEffect(StatusEffectType type)
    {
        return GetStackCount(type) > 0;
    }

    public int GetStackCount(StatusEffectType type)
    {
        int count = 0;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].Type == type)
                count++;
        }

        return count;
    }

    private void Awake()
    {
        owner = GetComponent<Entity>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance effect = effects[i];
            if (effect.DamagePerSecond > 0f && owner != null && owner.Hp > 0f)
                owner.TakeDamage(effect.DamagePerSecond * deltaTime, effect.Source);

            effect.RemainingDuration -= deltaTime;
            if (effect.RemainingDuration <= 0f)
                RemoveAt(i);
        }
    }

    public void ApplyBurn(
        Entity source,
        float damagePerSecond,
        float duration,
        bool canStack,
        int maxStacks
    )
    {
        if (damagePerSecond <= 0f || duration <= 0f)
            return;

        ApplyEffect(
            StatusEffectType.Burn,
            source,
            duration,
            damagePerSecond,
            EntityStatModifier.Zero,
            canStack,
            maxStacks
        );
    }

    public void ApplyModifierEffect(
        StatusEffectType type,
        Entity source,
        EntityStatModifier modifier,
        float duration,
        bool canStack,
        int maxStacks
    )
    {
        if (duration <= 0f)
            return;

        ApplyEffect(type, source, duration, 0f, modifier, canStack, maxStacks);
    }

    public void RemoveAll(StatusEffectType type)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].Type == type)
                RemoveAt(i);
        }
    }

    private void ApplyEffect(
        StatusEffectType type,
        Entity source,
        float duration,
        float damagePerSecond,
        EntityStatModifier modifier,
        bool canStack,
        int maxStacks
    )
    {
        if (owner == null)
            return;

        if (!canStack)
            RemoveAll(type);
        else if (maxStacks > 0)
            TrimOldestStacks(type, maxStacks - 1);

        EntityStatModifierHandle handle = default;
        if (!modifier.Equals(EntityStatModifier.Zero))
            handle = owner.ApplyStatModifier(modifier);

        effects.Add(
            new StatusEffectInstance
            {
                Type = type,
                Source = source,
                RemainingDuration = duration,
                DamagePerSecond = damagePerSecond,
                ModifierHandle = handle,
            }
        );
    }

    private void TrimOldestStacks(StatusEffectType type, int targetStackCount)
    {
        while (GetStackCount(type) > targetStackCount)
        {
            int oldestIndex = -1;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Type == type)
                {
                    oldestIndex = i;
                    break;
                }
            }

            if (oldestIndex < 0)
                return;

            RemoveAt(oldestIndex);
        }
    }

    private void RemoveAt(int index)
    {
        StatusEffectInstance effect = effects[index];
        if (owner != null && effect.ModifierHandle.IsValid)
            owner.RemoveStatModifier(effect.ModifierHandle);

        effects.RemoveAt(index);
    }

    private void OnDestroy()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
            RemoveAt(i);
    }
}
