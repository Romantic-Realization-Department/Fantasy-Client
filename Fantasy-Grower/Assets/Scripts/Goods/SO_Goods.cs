using UnityEngine;

/// <summary>
/// Parent class for goods
/// </summary>
public class SO_Goods : ScriptableObject
{
    /// <summary>
    /// The number of goods
    /// </summary>
    [SerializeField] protected uint value;

    /// <summary>
    /// Get the number of goods
    /// </summary>
    /// <returns>The number of goods</returns>
    public virtual uint Get() => value;

    /// <summary>
    /// Increase goods by the value of 'amount'
    /// </summary>
    public virtual void Increase(uint amount) => value += amount;

    /// <summary>
    /// Decrease goods by the value of 'amount'
    /// </summary>
    public virtual void Decrease(uint amount) => value -= amount;
}
