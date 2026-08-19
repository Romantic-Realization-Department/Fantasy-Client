using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClipContainer audioClipContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayRandom(AudioClip[] clips, float volumeScale = 1f)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        Play(clips[Random.Range(0, clips.Length)], volumeScale);
    }

    public void PlayHit(EntityType entityType, float volumeScale = 1f)
    {
        AudioClipContainer container = GetAudioClipContainer();
        if (container == null)
        {
            return;
        }

        Play(container.GetHitClip(entityType), volumeScale);
    }

    private AudioClipContainer GetAudioClipContainer()
    {
        if (audioClipContainer != null)
        {
            return audioClipContainer;
        }

        return AudioClipContainer.Instance;
    }
}
