using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance != null)
            Instance = null;
    }

    [Header("장비 탭")]
    [field: SerializeField]
    public GameObject WeaponInfoObject;

    [field: SerializeField]
    public Image WeaponIconImage { get; private set; }

    [field: SerializeField]
    public Image WeaponBGImage { get; private set; }

    [field: SerializeField]
    public Text WeaponLevelText { get; private set; }

    [field: SerializeField]
    public Text EquipInfoText { get; private set; }

    [field: SerializeField]
    public Text GetInfoText { get; private set; }

    [field: SerializeField]
    public GameObject[] AwakeObject { get; private set; }

    [Header("강화")]
    [field: SerializeField]
    public Image UpgradeWeaponIconImage { get; private set; }

    [field: SerializeField]
    public Image UpgradeWeaponBGImage { get; private set; }

    [field: SerializeField]
    public Text UpgradeWeaponLevelText { get; private set; }

    [field: SerializeField]
    public Text UpgradeWeaponLevelUpText { get; private set; }

    [field: SerializeField]
    public Text UpgradeEquipInfoText { get; private set; }

    [field: SerializeField]
    public Text UpgradeGetInfoText { get; private set; }

    [field: SerializeField]
    public GameObject[] UpgradeAwakeObject { get; private set; }

    [Header("대장간 변수")]
    [field: SerializeField]
    public GameObject SmithyTab { get; private set; }

    [Header("합성")]
    [field: SerializeField]
    public AnvilSlot[] SynthSlots { get; private set; }

    [Header("각성")]
    [field: SerializeField]
    public AnvilSlot[] AwakeSlots { get; private set; }

    public void Equip() => EquipmentManager.Instance.Equip(); //버튼 추가 형

    public void UpgradeWeapon() => EquipmentManager.Instance.UpgradeWeapon(); //버튼 추가 형

    public void Synthesis() => EquipmentManager.Instance.Synthesis(); //버튼 추가 형

    public void Awakening() => EquipmentManager.Instance.Awakening(); //버튼 추가 형
}
