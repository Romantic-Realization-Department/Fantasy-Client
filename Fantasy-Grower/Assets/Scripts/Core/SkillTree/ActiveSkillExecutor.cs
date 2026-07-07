using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어에게 장착된 액티브 스킬을 실행하고 쿨다운과 일시적 스탯 보정을 관리합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(SkillTreeComponent))]
public sealed class ActiveSkillExecutor : MonoBehaviour
{
    [Header("입력(컴퓨터 환경 전용)")]
    [SerializeField]
    private bool enableKeyboardInput = true;

    [SerializeField]
    private KeyCode[] slotKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
    };

    private readonly List<EntityStatModifierHandle> temporaryModifierHandles = new();

    private Player player;
    private SkillTreeComponent skillTreeComponent;
    private float[] cooldownEndTimes;

    private void Awake()
    {
        player = GetComponent<Player>();
        skillTreeComponent = GetComponent<SkillTreeComponent>();
    }

    private void Start()
    {
        EnsureCooldownBuffer();
    }

    private void Update()
    {
        if (!enableKeyboardInput)
            return;

        int slotCount = Mathf.Min(slotKeys.Length, GetActiveSlotCount());
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
                TryUseSkill(i);
        }
    }

    private void OnDisable()
    {
        ClearTemporaryModifiers();
    }

    public bool TryUseSkill(int slotIndex)
    {
        ActiveSkillData skill = GetEquippedSkill(slotIndex);
        if (skill == null)
            return false;

        EnsureCooldownBuffer();
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length)
            return false;

        if (GetCooldownRemaining(slotIndex) > 0f)
            return false;

        var context = new ActiveSkillContext(this, skillTreeComponent, player, skill, slotIndex);

        if (!skill.TryUseSkill(context))
            return false;

        float cooldown = GetModifiedCooldown(skill);
        if (cooldown > 0f)
            cooldownEndTimes[slotIndex] = Time.time + cooldown;

        return true;
    }

    public float GetCooldownRemaining(int slotIndex)
    {
        if (cooldownEndTimes == null || slotIndex < 0 || slotIndex >= cooldownEndTimes.Length)
            return 0f;

        return Mathf.Max(0f, cooldownEndTimes[slotIndex] - Time.time);
    }

    public ActiveSkillData GetEquippedSkill(int slotIndex)
    {
        if (skillTreeComponent == null)
            return null;

        return skillTreeComponent.GetEquippedActive(slotIndex);
    }

    public float GetCooldownRatio(int slotIndex)
    {
        ActiveSkillData skill = GetEquippedSkill(slotIndex);
        float cooldown = GetModifiedCooldown(skill);
        if (skill == null || cooldown <= 0f)
            return 0f;

        return Mathf.Clamp01(GetCooldownRemaining(slotIndex) / cooldown);
    }

    public void ReduceAllCooldowns(float seconds)
    {
        if (seconds <= 0f || cooldownEndTimes == null)
            return;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
            ReduceCooldown(i, seconds);
    }

    public void ReduceCooldown(int slotIndex, float seconds)
    {
        if (
            seconds <= 0f
            || cooldownEndTimes == null
            || slotIndex < 0
            || slotIndex >= cooldownEndTimes.Length
        )
        {
            return;
        }

        cooldownEndTimes[slotIndex] = Mathf.Max(Time.time, cooldownEndTimes[slotIndex] - seconds);
    }

    public float GetActiveSkillDamageMultiplier()
    {
        return GetActiveSkillDamageMultiplier(null);
    }

    public float GetActiveSkillDamageMultiplier(ActiveSkillData skill)
    {
        return skillTreeComponent != null
            ? skillTreeComponent.GetActiveSkillDamageMultiplier(skill)
            : 1f;
    }

    private float GetModifiedCooldown(ActiveSkillData skill)
    {
        if (skill == null || skill.Cooldown <= 0f)
            return 0f;

        float multiplier =
            skillTreeComponent != null ? skillTreeComponent.GetActiveSkillCooldownMultiplier() : 1f;
        return skill.Cooldown * multiplier;
    }

    public EntityStatModifierHandle ApplyTemporaryModifier(
        EntityStatModifier modifier,
        float duration
    )
    {
        EntityStatModifierHandle handle = player.ApplyStatModifier(modifier);
        temporaryModifierHandles.Add(handle);

        if (duration > 0f)
            StartCoroutine(RemoveTemporaryModifierAfterDelay(handle, duration));

        return handle;
    }

    private IEnumerator RemoveTemporaryModifierAfterDelay(
        EntityStatModifierHandle handle,
        float duration
    )
    {
        yield return new WaitForSeconds(duration);
        RemoveTemporaryModifier(handle);
    }

    private void RemoveTemporaryModifier(EntityStatModifierHandle handle)
    {
        if (!handle.IsValid)
            return;

        player.RemoveStatModifier(handle);
        temporaryModifierHandles.Remove(handle);
    }

    private void ClearTemporaryModifiers()
    {
        StopAllCoroutines();

        foreach (EntityStatModifierHandle handle in temporaryModifierHandles)
            player.RemoveStatModifier(handle);

        temporaryModifierHandles.Clear();
    }

    private void EnsureCooldownBuffer()
    {
        int slotCount = GetActiveSlotCount();
        if (cooldownEndTimes != null && cooldownEndTimes.Length == slotCount)
            return;

        cooldownEndTimes = new float[slotCount];
    }

    private int GetActiveSlotCount()
    {
        return skillTreeComponent != null ? skillTreeComponent.EquippedActiveCount : 0;
    }
}
