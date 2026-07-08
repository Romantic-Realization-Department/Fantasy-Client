using System;
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
    private readonly List<Action> runtimeCleanups = new();
    private readonly List<Entity> targetBuffer = new();
    private readonly List<Entity> waveTargetBuffer = new();
    private readonly HashSet<int> usedOncePerDungeonSlots = new();

    private Player player;
    private SkillTreeComponent skillTreeComponent;
    private AttackTargetsSensing attackTargetsSensing;
    private float[] cooldownEndTimes;

    private void Awake()
    {
        player = GetComponent<Player>();
        skillTreeComponent = GetComponent<SkillTreeComponent>();
        attackTargetsSensing = GetComponentInChildren<AttackTargetsSensing>();
    }

    private void Start()
    {
        EnsureCooldownBuffer();
    }

    private void OnEnable()
    {
        AddressableStageFeatureBase.OnAnyStageChanged += HandleStageChanged;
        ResetOncePerDungeonUsages();
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
        AddressableStageFeatureBase.OnAnyStageChanged -= HandleStageChanged;
        ClearRuntimeCleanups();
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

        if (skill.UsableOncePerDungeon && usedOncePerDungeonSlots.Contains(slotIndex))
            return false;

        var context = new ActiveSkillContext(this, skillTreeComponent, player, skill, slotIndex);

        if (!skill.TryUseSkill(context))
            return false;

        if (skill.UsableOncePerDungeon)
            usedOncePerDungeonSlots.Add(slotIndex);

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

    public bool IsOncePerDungeonSkillUsed(int slotIndex)
    {
        return usedOncePerDungeonSlots.Contains(slotIndex);
    }

    public void ReduceAllCooldowns(float seconds)
    {
        if (seconds <= 0f || cooldownEndTimes == null)
            return;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
            ReduceCooldown(i, seconds);
    }

    public void ReduceAllCooldownsPercent(float percent)
    {
        if (percent <= 0f || cooldownEndTimes == null)
            return;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
        {
            ActiveSkillData skill = GetEquippedSkill(i);
            if (skill != null)
            {
                float totalCooldown = GetModifiedCooldown(skill);
                ReduceCooldown(i, totalCooldown * percent);
            }
        }
    }

    public void ResetCooldownsExcept(int excludedSlotIndex)
    {
        if (cooldownEndTimes == null)
            return;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
        {
            if (i != excludedSlotIndex)
                cooldownEndTimes[i] = Time.time;
        }
    }

    public void ResetOncePerDungeonUsages()
    {
        usedOncePerDungeonSlots.Clear();
    }

    private void HandleStageChanged(int stageIndex)
    {
        ResetOncePerDungeonUsages();
    }

    public bool TryCollectTargets(
        ActiveSkillTargetMode targetMode,
        int maxTargets,
        List<Entity> results,
        float extensionRange = 0f
    )
    {
        if (results == null)
            return false;

        results.Clear();

        switch (targetMode)
        {
            case ActiveSkillTargetMode.Self:
                if (player != null)
                    results.Add(player);
                break;
            case ActiveSkillTargetMode.AllEnemies:
                WaveController.TryCollectActiveEnemies(waveTargetBuffer);
                AddTargets(waveTargetBuffer, maxTargets, results);
                break;
            case ActiveSkillTargetMode.DetectedTargets:
            default:
                if (extensionRange > 0f && player != null)
                {
                    // 인식 사거리 + 연장 사거리 범위 내에서 가장 가까운 K마리를 O(N*K)로 수집
                    float attackAreaMultiplier =
                        skillTreeComponent != null
                            ? skillTreeComponent.GetAttackAreaMultiplier()
                            : 1f;
                    float totalRange =
                        (player.AttackRange > 0f ? player.AttackRange : 0f)
                        + extensionRange * attackAreaMultiplier;
                    WaveController.TryCollectActiveEnemies(waveTargetBuffer);
                    CollectNearestInRange(
                        waveTargetBuffer,
                        player.transform.position,
                        totalRange,
                        maxTargets,
                        results
                    );
                }
                else if (attackTargetsSensing != null)
                {
                    AddTargets(attackTargetsSensing.GetTargets(), maxTargets, results);
                }
                break;
        }

        return results.Count > 0;
    }

    public IReadOnlyList<Entity> CollectTargets(
        ActiveSkillTargetMode targetMode,
        int maxTargets,
        float extensionRange = 0f
    )
    {
        TryCollectTargets(targetMode, maxTargets, targetBuffer, extensionRange);
        return targetBuffer;
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

    public void RegisterRuntimeCleanup(Action cleanup)
    {
        if (cleanup == null || runtimeCleanups.Contains(cleanup))
            return;

        runtimeCleanups.Add(cleanup);
    }

    public void UnregisterRuntimeCleanup(Action cleanup)
    {
        if (cleanup == null)
            return;

        runtimeCleanups.Remove(cleanup);
    }

    private IEnumerator RemoveTemporaryModifierAfterDelay(
        EntityStatModifierHandle handle,
        float duration
    )
    {
        yield return YieldInstructionCache.WaitForSeconds(duration);
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

    private void ClearRuntimeCleanups()
    {
        for (int i = runtimeCleanups.Count - 1; i >= 0; i--)
            runtimeCleanups[i].Invoke();

        runtimeCleanups.Clear();
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

    private static void AddTargets(
        IReadOnlyList<Entity> source,
        int maxTargets,
        List<Entity> results
    )
    {
        if (source == null)
            return;

        int targetLimit = maxTargets > 0 ? maxTargets : int.MaxValue;
        for (int i = 0; i < source.Count && results.Count < targetLimit; i++)
        {
            Entity target = source[i];
            if (target != null && target.Hp > 0f)
                results.Add(target);
        }
    }

    /// <summary>
    /// 지정한 원점으로부터 totalRange 이내의 생존 적 중 가장 가까운 K마리를 수집합니다.
    /// O(N * K) 삽입 정렬 방식: 람다 캡처·Sort·Sqrt 호출 없이 제곱 거리(sqrMagnitude)만 비교합니다.
    /// </summary>
    private static void CollectNearestInRange(
        List<Entity> candidates,
        Vector3 origin,
        float totalRange,
        int maxTargets,
        List<Entity> results
    )
    {
        float rangeSqr = totalRange * totalRange;
        int k = maxTargets > 0 ? maxTargets : int.MaxValue;

        // results는 호출 전 Clear된 상태이므로 재활용됨
        for (int i = 0; i < candidates.Count; i++)
        {
            Entity candidate = candidates[i];
            if (candidate == null || candidate.Hp <= 0f)
                continue;

            float sqrDist = (candidate.transform.position - origin).sqrMagnitude;
            if (sqrDist > rangeSqr)
                continue;

            // results에 삽입할 위치를 찾는다 (가까운 순서 유지)
            int insertIndex = results.Count;
            for (int j = results.Count - 1; j >= 0; j--)
            {
                float existingSqrDist = (results[j].transform.position - origin).sqrMagnitude;
                if (sqrDist < existingSqrDist)
                    insertIndex = j;
                else
                    break;
            }

            if (insertIndex < k)
            {
                results.Insert(insertIndex, candidate);

                if (results.Count > k)
                    results.RemoveAt(results.Count - 1);
            }
        }
    }
}
