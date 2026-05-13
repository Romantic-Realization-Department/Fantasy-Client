using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TotalDamageCalculator : MonoBehaviour
{
    private TMP_Text _totalDamaageText;
    private DamageMeasurer _damage;

    private void Awake()
    {
        _totalDamaageText = GetComponent<TMP_Text>();
        _damage = FindAnyObjectByType<DamageMeasurer>();
        _damage.OnTakeDamage += OnTakeDamage;
    }

    private void OnTakeDamage(int totalDamage)
    {
        _totalDamaageText.text = totalDamage.ToString();
        transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);
    }

    private void OnDestroy()
    {
        _damage.OnTakeDamage -= OnTakeDamage;
    }
}
