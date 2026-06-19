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

    [Header("강화")]
    [field: SerializeField]
    public Image WeaponIconImage { get; set; }

    [field: SerializeField]
    public Image WeaponBGImage { get; set; }

    [field: SerializeField]
    public Text WeaponLevelText { get; set; }

    [field: SerializeField]
    public Text EquipInfoText { get; set; }

    [field: SerializeField]
    public Text GetInfoText { get; set; }

    [field: SerializeField]
    public GameObject[] AwakeObject { get; set; }

    [Header("대장간 변수")]
    [field: SerializeField]
    public GameObject SmithyTab { get; set; }

    [Header("합성")]
    [field: SerializeField]
    public AnvilSlot[] SynthSlots { get; set; }

    [Header("각성")]
    [field: SerializeField]
    public AnvilSlot[] AwakeSlots { get; set; }

    public void Equip() => EquipmentManager.Instance.Equip(); //버튼 추가 형

    public void UpgradeWeapon() => EquipmentManager.Instance.UpgradeWeapon(); //버튼 추가 형

    public void Synthesis() => EquipmentManager.Instance.Synthesis(); //버튼 추가 형

    public void Awakening() => EquipmentManager.Instance.Awakening(); //버튼 추가 형
}
