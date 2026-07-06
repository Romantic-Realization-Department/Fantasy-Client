using UnityEngine;
using UnityEngine.UI;

public class WeaponShopController : MonoBehaviour
{
    [Header("상점 UI")]
    [SerializeField]
    private Button rerollButton;

    [Header("등급별 가격")]
    [SerializeField]
    private uint dGradePrice = 500u;

    [SerializeField]
    private uint cGradePrice = 2_500u;

    [SerializeField]
    private uint bGradePrice = 12_500u;

    [SerializeField]
    private uint aGradePrice = 75_000u;

    [SerializeField]
    private uint sGradePrice = 500_000u;

    private WeaponShopSlot[] shopSlots;

    private void Awake()
    {
        shopSlots = GetComponentsInChildren<WeaponShopSlot>(true);

        foreach (WeaponShopSlot slot in shopSlots)
        {
            slot.Initialize(this);
        }

        if (rerollButton != null)
            rerollButton.onClick.AddListener(RerollAll);
    }

    private void Start()
    {
        RerollAll();
    }

    private void OnDestroy()
    {
        if (rerollButton != null)
            rerollButton.onClick.RemoveListener(RerollAll);
    }

    public void RerollAll()
    {
        foreach (WeaponShopSlot slot in shopSlots)
        {
            slot.Reroll();
        }
    }

    public uint GetPrice(WeaponID weaponID)
    {
        switch (weaponID)
        {
            case WeaponID.D1:
            case WeaponID.D2:
                return dGradePrice;

            case WeaponID.C1:
            case WeaponID.C2:
                return cGradePrice;

            case WeaponID.B1:
            case WeaponID.B2:
                return bGradePrice;

            case WeaponID.A1:
            case WeaponID.A2:
                return aGradePrice;

            case WeaponID.S1:
            case WeaponID.S2:
                return sGradePrice;

            default:
                Debug.LogError($"정의되지 않은 WeaponID입니다: {weaponID}", this);
                return uint.MaxValue;
        }
    }
}
