using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [SerializeField]
    private AudioClip[] bgmClips;

    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private bool playOnEnable = true;

    private AudioClip lastUseClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
        }

        bgmSource.loop = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        if (playOnEnable && bgmSource != null && !bgmSource.isPlaying)
        {
            RandomPlay();
        }
    }

    private void Update()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            RandomPlay();
        }
    }

    private void RandomPlay()
    {
        if (bgmClips == null || bgmClips.Length == 0 || bgmSource == null)
        {
            return;
        }

        AudioClip nextClip = bgmClips[Random.Range(0, bgmClips.Length)];

        if (bgmClips.Length > 1)
        {
            while (nextClip == lastUseClip)
            {
                nextClip = bgmClips[Random.Range(0, bgmClips.Length)];
            }
        }

        bgmSource.clip = nextClip;
        bgmSource.Play();
        lastUseClip = nextClip;
    }
}
