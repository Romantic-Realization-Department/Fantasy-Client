using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RewardIndicator : MonoBehaviour
{
    private TMP_Text _rewardText;

    [SerializeField]
    private GoodsType[] _usingGoods;

    private readonly char[] _rewardTextChar = new char[128];

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
        Span<char> charSpan = _rewardTextChar;
        int offset = 0;

        foreach (GoodsType usingGoods in _usingGoods)
        {
            ReadOnlySpan<char> tempSpan = UIHelper.GetSpriteTag(usingGoods).AsSpan();

            // Sprite 태그 추가 가능 여부 확인
            if (offset + tempSpan.Length > charSpan.Length)
            {
                Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                break;
            }

            // Sprite 태그 추가
            tempSpan.CopyTo(charSpan[offset..]);
            offset += tempSpan.Length;

            // 값 추가 가능 여부 확인
            if (DungeonManager.Instance is IDungeonRewardProvider dungeonReward)
            {
                if (
                    !dungeonReward
                        .GetReward()[usingGoods]
                        .TryFormat(charSpan[offset..], out int charsWritten, "N0")
                )
                {
                    Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                    break;
                }
                offset += charsWritten;
            }

            // 줄바꿈 가능 여부 확인
            if (offset + 1 > charSpan.Length)
            {
                Debug.LogWarning("RewardText: 버퍼 범위를 초과하여 텍스트가 잘렸습니다.");
                break;
            }

            charSpan[offset] = '\n';
            offset += 1;
        }

        // 마지막 줄바꿈 제외
        int finalLength = Mathf.Max(0, offset - 1);
        _rewardText.SetCharArray(_rewardTextChar, 0, finalLength);
    }
}
