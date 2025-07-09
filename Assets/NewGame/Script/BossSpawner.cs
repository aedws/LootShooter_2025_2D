using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BossSpawnCondition
{
    public enum SpawnTrigger
    {
        WaveCount,      // 특정 웨이브에서 스폰
        TimeElapsed,    // 특정 시간 후 스폰
        PlayerLevel,    // 플레이어 레벨 기준
        ManualTrigger   // 수동 트리거
    }
    
    public SpawnTrigger trigger = SpawnTrigger.WaveCount;
    public int triggerValue = 10; // 웨이브 수, 시간(초), 레벨 등
    public bool hasSpawned = false;
}

public class BossSpawner : MonoBehaviour
{
    [Header("보스 설정")]
    public GameObject[] bossPrefabs;
    public BossSpawnCondition[] spawnConditions;
    
    [Header("스폰 위치")]
    public Transform[] bossSpawnPoints;
    public float spawnDistance = 20f; // 플레이어로부터의 거리
    
    [Header("보스 전투 설정")]
    public bool pauseNormalSpawners = true; // 보스 전투 중 일반 스폰 중지
    public bool resumeAfterBossDeath = true; // 보스 사망 후 일반 스폰 재개
    public float bossFightTimeout = 300f; // 보스 전투 타임아웃 (5분)
    
    [Header("보스 전투 UI")]
    public GameObject bossHealthBarPrefab;
    public Transform uiParent; // UI 부모 Transform
    
    [Header("보스 전투 이벤트")]
    public AudioClip bossEntranceSound;
    public AudioClip bossDeathSound;
    public GameObject bossEntranceEffect;
    
    // 상태 관리
    private ButterflyBoss currentBoss;
    private bool isBossFightActive = false;
    private float bossFightStartTime;
    private List<EnemySpawner> normalSpawners = new List<EnemySpawner>();
    private GameObject currentBossHealthBar;
    
    // 이벤트
    public System.Action<ButterflyBoss> OnBossSpawned;
    public System.Action<ButterflyBoss> OnBossDefeated;
    public System.Action OnBossFightStarted;
    public System.Action OnBossFightEnded;
    
    void Start()
    {
        // 일반 스폰어들 찾기
        FindNormalSpawners();
        
        // 보스 스폰 조건 초기화
        InitializeSpawnConditions();
    }
    
    void Update()
    {
        if (!isBossFightActive)
        {
            // 보스 스폰 조건 체크
            CheckBossSpawnConditions();
        }
        else
        {
            // 보스 전투 타임아웃 체크
            CheckBossFightTimeout();
        }
    }
    
    void FindNormalSpawners()
    {
        normalSpawners.Clear();
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        
        foreach (EnemySpawner spawner in spawners)
        {
            // 보스 스폰어는 제외
            if (spawner.gameObject != gameObject)
            {
                normalSpawners.Add(spawner);
            }
        }
    }
    
    void InitializeSpawnConditions()
    {
        foreach (BossSpawnCondition condition in spawnConditions)
        {
            condition.hasSpawned = false;
        }
    }
    
    void CheckBossSpawnConditions()
    {
        foreach (BossSpawnCondition condition in spawnConditions)
        {
            if (condition.hasSpawned) continue;
            
            bool shouldSpawn = false;
            
            switch (condition.trigger)
            {
                case BossSpawnCondition.SpawnTrigger.WaveCount:
                    shouldSpawn = CheckWaveCountCondition(condition.triggerValue);
                    break;
                case BossSpawnCondition.SpawnTrigger.TimeElapsed:
                    shouldSpawn = CheckTimeElapsedCondition(condition.triggerValue);
                    break;
                case BossSpawnCondition.SpawnTrigger.PlayerLevel:
                    shouldSpawn = CheckPlayerLevelCondition(condition.triggerValue);
                    break;
                case BossSpawnCondition.SpawnTrigger.ManualTrigger:
                    // 수동 트리거는 외부에서 호출
                    continue;
            }
            
            if (shouldSpawn)
            {
                SpawnBoss(condition);
                break; // 한 번에 하나의 보스만 스폰
            }
        }
    }
    
    bool CheckWaveCountCondition(int targetWave)
    {
        // 웨이브 카운트 체크 (GameManager나 EnemySpawner에서 가져오기)
        // 임시로 시간 기반으로 구현
        return Time.time > targetWave * 30f; // 30초마다 웨이브로 가정
    }
    
    bool CheckTimeElapsedCondition(int targetTime)
    {
        return Time.time > targetTime;
    }
    
    bool CheckPlayerLevelCondition(int targetLevel)
    {
        // 플레이어 레벨 체크 (나중에 GameManager에서 구현)
        return false; // 임시로 false 반환
    }
    
    public void SpawnBoss(BossSpawnCondition condition = null)
    {
        if (isBossFightActive || bossPrefabs.Length == 0) return;
        
        // 보스 프리팹 선택
        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];
        
        // 스폰 위치 결정
        Vector3 spawnPosition = GetBossSpawnPosition();
        
        // 보스 생성
        GameObject bossObject = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        currentBoss = bossObject.GetComponent<ButterflyBoss>();
        
        if (currentBoss != null)
        {
            // 보스 이벤트 연결
            currentBoss.OnBossDeath += OnBossDeath;
            
            // 보스 전투 시작
            StartBossFight();
            
            // 스폰 조건 마킹
            if (condition != null)
            {
                condition.hasSpawned = true;
            }
            
            // 이벤트 발생
            OnBossSpawned?.Invoke(currentBoss);
            
            // Debug.Log($"[BossSpawner] 보스 스폰: {currentBoss.bossName} at {spawnPosition}");
        }
        else
        {
            Debug.LogError("[BossSpawner] 보스 프리팹에 ButterflyBoss 컴포넌트가 없습니다!");
            Destroy(bossObject);
        }
    }
    
    Vector3 GetBossSpawnPosition()
    {
        // 지정된 스폰 포인트가 있으면 사용
        if (bossSpawnPoints != null && bossSpawnPoints.Length > 0)
        {
            Transform spawnPoint = bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)];
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }
        
        // 플레이어 위치 기반 스폰
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 spawnPosition = player.position + (Vector3)(randomDirection * spawnDistance);
            
            // 지면 높이 조정
            spawnPosition.y = GetGroundY();
            
            return spawnPosition;
        }
        
        // 기본 위치
        return transform.position + Vector3.right * spawnDistance;
    }
    
    float GetGroundY()
    {
        // 지면 높이 찾기 (레이캐스트 사용)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("Ground"));
        return hit.collider != null ? hit.point.y : 0f;
    }
    
    void StartBossFight()
    {
        isBossFightActive = true;
        bossFightStartTime = Time.time;
        
        // 일반 스폰어들 중지
        if (pauseNormalSpawners)
        {
            PauseNormalSpawners();
        }
        
        // 보스 헬스바 생성
        CreateBossHealthBar();
        
        // 보스 등장 이펙트
        PlayBossEntranceEffect();
        
        // 이벤트 발생
        OnBossFightStarted?.Invoke();
        
        // Debug.Log("[BossSpawner] 보스 전투 시작!");
    }
    
    void PauseNormalSpawners()
    {
        foreach (EnemySpawner spawner in normalSpawners)
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
            }
        }
    }
    
    void ResumeNormalSpawners()
    {
        foreach (EnemySpawner spawner in normalSpawners)
        {
            if (spawner != null)
            {
                spawner.ResumeSpawning();
            }
        }
    }
    
    void CreateBossHealthBar()
    {
        if (bossHealthBarPrefab == null) return;
        
        Transform parent = uiParent != null ? uiParent : FindFirstObjectByType<Canvas>()?.transform;
        if (parent != null)
        {
            currentBossHealthBar = Instantiate(bossHealthBarPrefab, parent);
        }
    }
    
    void PlayBossEntranceEffect()
    {
        // 보스 등장 사운드
        if (bossEntranceSound != null)
        {
            AudioSource.PlayClipAtPoint(bossEntranceSound, currentBoss.transform.position);
        }
        
        // 보스 등장 이펙트
        if (bossEntranceEffect != null)
        {
            Instantiate(bossEntranceEffect, currentBoss.transform.position, Quaternion.identity);
        }
        
        // 화면 흔들림 효과
        StartCoroutine(ScreenShakeEffect());
    }
    
    System.Collections.IEnumerator ScreenShakeEffect()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;
        
        Vector3 originalPosition = mainCamera.transform.position;
        float shakeTime = 2f;
        float shakeIntensity = 1f;
        
        float timer = 0f;
        while (timer < shakeTime)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            mainCamera.transform.position = originalPosition + shakeOffset;
            timer += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = originalPosition;
    }
    
    void CheckBossFightTimeout()
    {
        if (Time.time - bossFightStartTime > bossFightTimeout)
        {
            Debug.LogWarning("[BossSpawner] 보스 전투 타임아웃! 보스가 강제로 사라집니다.");
            ForceEndBossFight();
        }
    }
    
    void OnBossDeath()
    {
        if (currentBoss != null)
        {
            // 보스 사망 사운드
            if (bossDeathSound != null)
            {
                AudioSource.PlayClipAtPoint(bossDeathSound, currentBoss.transform.position);
            }
            
            // 보스 사망 이벤트
            OnBossDefeated?.Invoke(currentBoss);
            
            // Debug.Log($"[BossSpawner] 보스 {currentBoss.bossName} 처치!");
        }
        
        EndBossFight();
    }
    
    void EndBossFight()
    {
        isBossFightActive = false;
        
        // 일반 스폰어들 재개
        if (resumeAfterBossDeath)
        {
            ResumeNormalSpawners();
        }
        
        // 보스 헬스바 제거
        if (currentBossHealthBar != null)
        {
            Destroy(currentBossHealthBar);
            currentBossHealthBar = null;
        }
        
        // 보스 참조 정리
        currentBoss = null;
        
        // 이벤트 발생
        OnBossFightEnded?.Invoke();
        
        // Debug.Log("[BossSpawner] 보스 전투 종료!");
    }
    
    void ForceEndBossFight()
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss.gameObject);
        }
        
        EndBossFight();
    }
    
    // 수동 보스 스폰 (디버그용)
    [ContextMenu("Spawn Boss Manually")]
    public void SpawnBossManually()
    {
        SpawnBoss();
    }
    
    // 보스 전투 상태 확인
    public bool IsBossFightActive()
    {
        return isBossFightActive;
    }
    
    public ButterflyBoss GetCurrentBoss()
    {
        return currentBoss;
    }
    
    public float GetBossFightTime()
    {
        return isBossFightActive ? Time.time - bossFightStartTime : 0f;
    }
    
    void OnDestroy()
    {
        // 이벤트 연결 해제
        if (currentBoss != null)
        {
            currentBoss.OnBossDeath -= OnBossDeath;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 보스 스폰 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnDistance);
        
        // 스폰 포인트 표시
        if (bossSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in bossSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                }
            }
        }
    }
} 