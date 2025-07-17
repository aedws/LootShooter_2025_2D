using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private static BGMPlayer _instance;
    public static BGMPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BGMPlayer>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BGMPlayer");
                    _instance = go.AddComponent<BGMPlayer>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private AudioSource audioSource;
    private float volume = 0.5f; // 기본 볼륨을 50%로 변경
    private const float volumeStep = 0.05f;
    private const float minVolume = 0f;
    private const float maxVolume = 1f;
    private const string VOLUME_KEY = "BGMVolume"; // PlayerPrefs 키

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolume(); // 저장된 볼륨 불러오기
            InitializeAudio();
        }
        else if (_instance != this)
        {
            // 이미 BGMPlayer가 존재하면 새로 생성된 것을 파괴
            Destroy(gameObject);
            return;
        }
    }

    void InitializeAudio()
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
            SaveVolume(); // 볼륨 변경 시 자동 저장
        }
        // ] 키로 볼륨 증가
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            volume = Mathf.Min(maxVolume, volume + volumeStep);
            audioSource.volume = volume;
            SaveVolume(); // 볼륨 변경 시 자동 저장
        }
    }

    // 볼륨 저장
    public void SaveVolume()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    // 볼륨 불러오기
    public void LoadVolume()
    {
        if (PlayerPrefs.HasKey(VOLUME_KEY))
        {
            volume = PlayerPrefs.GetFloat(VOLUME_KEY);
            volume = Mathf.Clamp(volume, minVolume, maxVolume); // 범위 제한
        }
        else
        {
            // 저장된 볼륨이 없으면 기본값 사용
            volume = 0.5f;
            SaveVolume(); // 기본값 저장
        }
    }

    // BGM 재시작 메서드 (필요시 사용)
    public void RestartBGM()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }

    // BGM 정지 메서드 (필요시 사용)
    public void StopBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // 현재 볼륨 반환
    public float GetVolume()
    {
        return volume;
    }

    // 볼륨 설정
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp(newVolume, minVolume, maxVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        SaveVolume(); // 볼륨 변경 시 자동 저장
    }

    // 볼륨 초기화 (기본값으로 리셋)
    public void ResetVolume()
    {
        volume = 0.5f;
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        SaveVolume();
    }
} 