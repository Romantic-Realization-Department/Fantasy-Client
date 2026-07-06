using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WeaponShopSlot : MonoBehaviour
{
    [Header("상점 UI")]
    [Tooltip("등급 색상을 적용할 버튼 배경 Image")]
    [SerializeField]
    private Image buttonBackground;

    [Tooltip("자식 WeaponIMG")]
    [SerializeField]
    private Image weaponImage;

    [Tooltip("자식 TierText")]
    [SerializeField]
    private TMP_Text tierText;

    [Tooltip("자식 PriceText")]
    [SerializeField]
    private TMP_Text priceText;

    private Button purchaseButton;
    private SO_Goods gold;
    private uint currentPrice;
    private bool isPurchased;

    public WeaponID CurrentWeaponID { get; private set; }

    private void Awake()
    {
        purchaseButton = GetComponent<Button>();
        purchaseButton.onClick.AddListener(Purchase);
    }

    private void OnDestroy()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(Purchase);

        if (gold != null)
            gold.OnValueChange -= OnGoldValueChanged;
    }

    /// <summary>
    /// 무기를 새로 뽑고 슬롯을 초기화합니다.
    /// </summary>
    public void Reroll()
    {
        if (EquipmentManager.Instance == null)
        {
            Debug.LogError("EquipmentManager가 씬에 없습니다.", this);
            return;
        }

        if (
            buttonBackground == null
            || weaponImage == null
            || tierText == null
            || priceText == null
        )
        {
            Debug.LogError($"{name}: WeaponShopSlot의 UI가 연결되지 않았습니다.", this);
            return;
        }

        if (!TryInitializeGold())
        {
            purchaseButton.interactable = false;
            return;
        }

        CurrentWeaponID = RollWeaponID();
        currentPrice = GetPrice(CurrentWeaponID);
        isPurchased = false;

        EquipmentManager equipmentManager = EquipmentManager.Instance;

        // 버튼 배경에 등급 색상 적용
        buttonBackground.color = equipmentManager.GetColor(CurrentWeaponID);

        // WeaponIMG에 무기 아이콘 적용
        weaponImage.sprite = equipmentManager.GetIcon(CurrentWeaponID);

        weaponImage.preserveAspect = true;

        // D1, C2, A1 등의 문자열 표시
        tierText.text = CurrentWeaponID.ToString();

        // 500, 2,500, 12,500 형식으로 표시
        priceText.text = currentPrice.ToString("N0");

        RefreshPurchaseState();
    }

    /// <summary>
    /// 구매 버튼 클릭 시 실행됩니다.
    /// </summary>
    private void Purchase()
    {
        if (!purchaseButton.interactable)
            return;

        if (EquipmentManager.Instance == null || !TryInitializeGold())
        {
            Debug.LogError("구매에 필요한 Manager가 씬에 없습니다.", this);
            return;
        }

        if (isPurchased || gold.Get() < currentPrice)
        {
            RefreshPurchaseState();
            return;
        }

        // 골드 차감에 성공했을 때만 구매 처리
        if (!gold.Decrease(currentPrice))
        {
            RefreshPurchaseState();
            return;
        }

        isPurchased = true;
        RefreshPurchaseState();

        // 현재 슬롯 무기를 정확히 한 개 지급
        EquipmentManager.Instance.GetItem(CurrentWeaponID, 1u);
    }

    private bool TryInitializeGold()
    {
        if (gold != null)
            return true;

        if (GoodsManager.Instance == null)
        {
            Debug.LogError("GoodsManager가 씬에 없습니다.", this);
            return false;
        }

        gold = GoodsManager.Instance.GetGoods(GoodsType.Gold);

        if (gold == null)
        {
            Debug.LogError("GoodsManager에 Gold SO_Goods가 등록되지 않았습니다.", this);
            return false;
        }

        gold.OnValueChange += OnGoldValueChanged;
        return true;
    }

    private void OnGoldValueChanged(uint value)
    {
        RefreshPurchaseState();
    }

    private void RefreshPurchaseState()
    {
        purchaseButton.interactable = !isPurchased && gold != null && gold.Get() >= currentPrice;
    }

    private uint GetPrice(WeaponID weaponID)
    {
        switch (weaponID)
        {
            case WeaponID.D1:
            case WeaponID.D2:
                return 500u;

            case WeaponID.C1:
            case WeaponID.C2:
                return 2_500u;

            case WeaponID.B1:
            case WeaponID.B2:
                return 12_500u;

            case WeaponID.A1:
            case WeaponID.A2:
                return 75_000u;

            case WeaponID.S1:
            case WeaponID.S2:
                return 500_000u;

            default:
                Debug.LogError($"정의되지 않은 WeaponID입니다: {weaponID}", this);
                return uint.MaxValue;
        }
    }

    private WeaponID RollWeaponID()
    {
        float roll = Random.Range(0f, 100f);

        // D 등급: 70%
        if (roll < 70f)
            return RandomWeapon(WeaponID.D1, WeaponID.D2);

        // C 등급: 25%
        if (roll < 95f)
            return RandomWeapon(WeaponID.C1, WeaponID.C2);

        // B 등급: 4.5%
        if (roll < 99.5f)
            return RandomWeapon(WeaponID.B1, WeaponID.B2);

        // A 등급: 0.49%
        if (roll < 99.99f)
            return RandomWeapon(WeaponID.A1, WeaponID.A2);

        // S 등급: 0.01%
        return RandomWeapon(WeaponID.S1, WeaponID.S2);
    }

    private WeaponID RandomWeapon(WeaponID first, WeaponID second)
    {
        return Random.value < 0.5f ? first : second;
    }
}
