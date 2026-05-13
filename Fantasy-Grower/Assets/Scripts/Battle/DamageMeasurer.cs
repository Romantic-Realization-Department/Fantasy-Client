using System;
using UnityEngine;

public class DamageMeasurer : MonoBehaviour
{
    public event Action<int> OnTakeDamage;

    protected void InvokeOnTakeDamage(int totalDamage)
    {
        OnTakeDamage?.Invoke(totalDamage);
    }
}
