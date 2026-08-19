using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class XPRatioText : MonoBehaviour
{
    private TMP_Text xpText;

    private void Awake()
    {
        xpText = GetComponent<TMP_Text>();
    }

    public void OnValueChanged(float value)
    {
        xpText.SetText("{0:1}%", value * 100f);
    }
}
