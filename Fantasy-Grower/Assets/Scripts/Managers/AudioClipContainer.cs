using UnityEngine;

[DisallowMultipleComponent]
public class AudioClipContainer : MonoBehaviour
{
    public static AudioClipContainer Instance { get; private set; }

    [SerializeField]
    private AudioClip[] defaultHitClips;

    [SerializeField]
    private AudioClip[] playerHitClips;

    [SerializeField]
    private AudioClip[] enemyHitClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public AudioClip GetHitClip(EntityType entityType)
    {
        AudioClip hitClip = entityType switch
        {
            EntityType.Player => GetRandomClip(playerHitClips),
            EntityType.Enemy => GetRandomClip(enemyHitClips),
            _ => null,
        };

        if (hitClip != null)
        {
            return hitClip;
        }

        return GetRandomClip(defaultHitClips);
    }

    private static AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }
}
