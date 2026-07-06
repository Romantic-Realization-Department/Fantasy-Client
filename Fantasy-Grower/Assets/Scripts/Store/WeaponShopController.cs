using UnityEngine;
using UnityEngine.UI;

public class WeaponShopController : MonoBehaviour
{
    [Header("ªÛ¡° UI")]
    [SerializeField]
    private Button rerollButton;

    private WeaponShopSlot[] shopSlots;

    private void Awake()
    {
        shopSlots = GetComponentsInChildren<WeaponShopSlot>(true);

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
}
