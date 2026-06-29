using UnityEngine;

public partial class GameManager
{
    //캐싱 될 플레이어 변수
    private Entity PlayerEntity;

    /// <summary>
    /// 플레이어 접근 함수
    /// 플레이어 변수가 null이면 현재 씬의 플레이어 할당
    /// </summary>
    /// <returns></returns>
    public Entity GetPlayer()
    {
        if (PlayerEntity == null)
        {
            PlayerEntity = FindAnyObjectByType<Player>();
        }
        return PlayerEntity;
    }
}
