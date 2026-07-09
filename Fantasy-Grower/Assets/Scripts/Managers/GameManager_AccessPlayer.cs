using System;
using UnityEngine;

public partial class GameManager
{
    private Entity PlayerEntity;

    public event Action<Entity> OnPlayerChanged;

    public Entity GetPlayer()
    {
        if (PlayerEntity == null)
        {
            Player foundPlayer = FindAnyObjectByType<Player>();
            if (foundPlayer != null)
                SetPlayer(foundPlayer);
        }

        return PlayerEntity;
    }

    public void SetPlayer(Entity player)
    {
        if (PlayerEntity == player)
            return;

        PlayerEntity = player;
        OnPlayerChanged?.Invoke(PlayerEntity);
    }
}
