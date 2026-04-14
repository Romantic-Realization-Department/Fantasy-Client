using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SynthSlot : MonoBehaviour, IPointerClickHandler
{
    public Image SlotImage;
    public Image BGImage;
    public Text WeaponCountText; //TMP·Î ¹Ù²ð ¿¹Á¤
    public bool isSelectSlot;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelectSlot) { }
    }
}
