using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private float volume = 0.7f;
    private const float volumeStep = 0.05f;
    private const float minVolume = 0f;
    private const float maxVolume = 1f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("NewGame/Audio/BGM"); // 확장자 없이 경로만
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = volume;
        audioSource.Play();
    }

    void Update()
    {
        // [ 키로 볼륨 감소
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            volume = Mathf.Max(minVolume, volume - volumeStep);
            audioSource.volume = volume;
        }
        // ] 키로 볼륨 증가
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            volume = Mathf.Min(maxVolume, volume + volumeStep);
            audioSource.volume = volume;
        }
    }
} 