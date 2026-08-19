using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RewardIndicator : MonoBehaviour
{
    private TMP_Text _rewardText;

    private readonly char[] _rewardTextChar = new char[256];

    private void Awake()
    {
        _rewardText = GetComponent<TMP_Text>();
    }

    // 활성화 되었을 때 현재 던전 보상을 표기합니다.
    private void OnEnable()
    {
        IndicateReward();
    }

    private void IndicateReward()
    {
        if (DungeonManager.Instance is not IDungeonRewardRecorder rewardProvider)
        {
            _rewardText.text = string.Empty;
            return;
        }

        Span<char> charSpan = _rewardTextChar;
        int offset = 0;

        foreach (RewardDisplayItem reward in rewardProvider.GetRewardItems())
        {
            if (
                !charSpan.TryAppend(ref offset, "<sprite name=\"".AsSpan())
                || !charSpan.TryAppend(ref offset, reward.IconName.AsSpan())
                || !charSpan.TryAppend(ref offset, "\"> : ".AsSpan())
            )
            {
                Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                break;
            }

            if (!reward.Amount.TryFormat(charSpan[offset..], out int charsWritten, "N0"))
            {
                Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                break;
            }

            offset += charsWritten;

            if (!charSpan.TryAppend(ref offset, "\n".AsSpan()))
            {
                Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                break;
            }
        }

        // 마지막 줄바꿈 제외
        int finalLength = Mathf.Max(0, offset - 1);
        _rewardText.SetCharArray(_rewardTextChar, 0, finalLength);
    }
}

public static class RewardIconNames
{
    public const string Gold = nameof(GoodsType.Gold) + "_Icon";
    public const string XP = nameof(GoodsType.XP) + "_Icon";
    public const string Mithril = nameof(GoodsType.Mithril) + "_Icon";
    public const string UpgradeScroll = nameof(GoodsType.UpgradeScroll) + "_Icon";

    public const string WeaponS1 = nameof(WeaponID.S1) + "_Icon";
    public const string WeaponS2 = nameof(WeaponID.S2) + "_Icon";
    public const string WeaponA1 = nameof(WeaponID.A1) + "_Icon";
    public const string WeaponA2 = nameof(WeaponID.A2) + "_Icon";
    public const string WeaponB1 = nameof(WeaponID.B1) + "_Icon";
    public const string WeaponB2 = nameof(WeaponID.B2) + "_Icon";
    public const string WeaponC1 = nameof(WeaponID.C1) + "_Icon";
    public const string WeaponC2 = nameof(WeaponID.C2) + "_Icon";
    public const string WeaponD1 = nameof(WeaponID.D1) + "_Icon";
    public const string WeaponD2 = nameof(WeaponID.D2) + "_Icon";
}

public readonly struct RewardDisplayItem
{
    public readonly string IconName;
    public readonly uint Amount;

    public RewardDisplayItem(string iconName, uint amount)
    {
        IconName = iconName;
        Amount = amount;
    }
}

public static class RewardDisplayItemFactory
{
    public static RewardDisplayItem Goods(GoodsType type, uint amount)
    {
        return new RewardDisplayItem(GetGoodsIconName(type), amount);
    }

    public static RewardDisplayItem Weapon(WeaponID id, uint amount)
    {
        Career career =
            GameManager.InstanceOrNull != null
                ? GameManager.InstanceOrNull.SelectedJob
                : Career.Warrior;
        return new RewardDisplayItem(GetWeaponIconName(career, id), amount);
    }

    private static string GetGoodsIconName(GoodsType type)
    {
        return type switch
        {
            GoodsType.Gold => RewardIconNames.Gold,
            GoodsType.XP => RewardIconNames.XP,
            GoodsType.Mithril => RewardIconNames.Mithril,
            GoodsType.UpgradeScroll => RewardIconNames.UpgradeScroll,
            _ => string.Empty,
        };
    }

    private static string GetWeaponIconName(Career career, WeaponID id)
    {
        string weaponIconName = GetWeaponIconName(id);
        return string.IsNullOrEmpty(weaponIconName) ? string.Empty : $"{career}_{weaponIconName}";
    }

    private static string GetWeaponIconName(WeaponID id)
    {
        return id switch
        {
            WeaponID.S1 => RewardIconNames.WeaponS1,
            WeaponID.S2 => RewardIconNames.WeaponS2,
            WeaponID.A1 => RewardIconNames.WeaponA1,
            WeaponID.A2 => RewardIconNames.WeaponA2,
            WeaponID.B1 => RewardIconNames.WeaponB1,
            WeaponID.B2 => RewardIconNames.WeaponB2,
            WeaponID.C1 => RewardIconNames.WeaponC1,
            WeaponID.C2 => RewardIconNames.WeaponC2,
            WeaponID.D1 => RewardIconNames.WeaponD1,
            WeaponID.D2 => RewardIconNames.WeaponD2,
            _ => string.Empty,
        };
    }
}
