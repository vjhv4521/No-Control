using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource effectSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (effectSource == null)
        {
            effectSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayEffect(string clipPath)
    {
        AudioClip clip = Resources.Load<AudioClip>(clipPath);
        if (clip == null || effectSource == null)
        {
            Debug.LogError($"AudioManager：找不到音效 {clipPath}");
            return;
        }
        effectSource.PlayOneShot(clip);
    }
}