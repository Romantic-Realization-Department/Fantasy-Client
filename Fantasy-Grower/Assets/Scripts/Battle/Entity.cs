using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Hp { get; set; }
    public int Attack { get; set; }
    public float CriticalPercentage { get; set; } = 0f;

}
