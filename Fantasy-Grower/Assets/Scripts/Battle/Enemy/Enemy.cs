using UnityEngine;

public class Enemy : Entity
{
    public override void Death()
    {
        Debug.Log("TestEnemy 죽음");
    }
}
