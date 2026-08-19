using System;
using UnityEngine;

/// <summary>
/// Parent class for goods
/// </summary>
public abstract class SO_Goods : ScriptableObject
{
    /// <summary>
    /// The number of goods
    /// </summary>
    [SerializeField]
    protected uint value;

    protected abstract string GoodsName { get; }

    /// <summary>
    /// Get the number of goods
    /// </summary>
    /// <returns>The number of goods</returns>
    public virtual uint Get() => value;

    /// <summary>
    /// Increase goods by the value of 'amount'
    /// </summary>
    public virtual void Increase(uint amount)
    {
        value += amount;
        OnValueChange?.Invoke(value);
    }

    /// <summary>
    /// Decrease goods by the value of 'amount'
    /// </summary>
    public virtual bool Decrease(uint amount)
    {
        if (amount > value)
        {
            Debug.LogWarning($"{GoodsName}은(는) {amount - value}만큼 부족합니다!!!");
            return false;
        }

        value -= amount;
        OnValueChange?.Invoke(value);
        return true;
    }

    /// <summary>
    /// 자원의 값이 변동했을 때 발동되는 이벤트입니다.
    /// </summary>
    public event Action<uint> OnValueChange;
}
