using UnityEngine;

public class TestEnemy : Enemy
{
    public override void Death()
    {
        Debug.Log("TestEnemy 죽음");
    }
}
