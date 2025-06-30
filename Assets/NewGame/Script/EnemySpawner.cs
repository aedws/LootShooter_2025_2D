using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject[] enemyPrefabs; // 스폰할 적 프리팹들
    public Transform[] spawnPoints; // 스폰 위치들
    public float spawnInterval = 2f; // 스폰 간격
    public int maxEnemies = 10; // 최대 적 수
    
    [Header("웨이브 시스템")]
    public bool useWaveSystem = true;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 10f;
    public float waveMultiplier = 1.2f; // 웨이브마다 적 증가 배수
    
    [Header("스폰 범위 (자동 스폰포인트)")]
    public bool useAutoSpawnPoints = true;
    public float spawnRadius = 15f; // 플레이어 주변 스폰 반경
    public int autoSpawnPointCount = 8; // 자동 생성할 스폰포인트 수
    
    [Header("현재 상태")]
    public int currentWave = 1;
    public int enemiesRemaining = 0;
    public bool isSpawning = false;
    
    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    
    void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // 자동 스폰포인트 생성
        if (useAutoSpawnPoints)
        {
            CreateAutoSpawnPoints();
        }
        
        // 스폰 시작
        if (useWaveSystem)
        {
            StartWave();
        }
        else
        {
            StartContinuousSpawn();
        }
    }
    
    void CreateAutoSpawnPoints()
    {
        spawnPoints = new Transform[autoSpawnPointCount];
        
        for (int i = 0; i < autoSpawnPointCount; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.SetParent(transform);
            
            // 원형으로 배치
            float angle = (360f / autoSpawnPointCount) * i;
            Vector3 position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius,
                0
            );
            spawnPoint.transform.localPosition = position;
            spawnPoints[i] = spawnPoint.transform;
        }
    }
    
    void StartWave()
    {
        if (isSpawning) return;
        
        int enemiesToSpawn = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(waveMultiplier, currentWave - 1));
        Debug.Log($"[EnemySpawner] Wave {currentWave} 시작! 적 {enemiesToSpawn}마리 스폰 예정");
        
        spawnCoroutine = StartCoroutine(SpawnWave(enemiesToSpawn));
    }
    
    void StartContinuousSpawn()
    {
        if (isSpawning) return;
        
        spawnCoroutine = StartCoroutine(ContinuousSpawn());
    }
    
    IEnumerator SpawnWave(int enemiesToSpawn)
    {
        isSpawning = true;
        enemiesRemaining = enemiesToSpawn;
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                // 최대치에 도달하면 적이 죽을 때까지 대기
                yield return new WaitUntil(() => activeEnemies.Count < maxEnemies);
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        isSpawning = false;
        
        // 모든 적이 죽을 때까지 대기
        yield return new WaitUntil(() => activeEnemies.Count == 0);
        
        // 다음 웨이브 준비
        currentWave++;
        yield return new WaitForSeconds(timeBetweenWaves);
        StartWave();
    }
    
    IEnumerator ContinuousSpawn()
    {
        isSpawning = true;
        
        while (true)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] 적 프리팹이나 스폰포인트가 설정되지 않았습니다!");
            return;
        }
        
        // 랜덤 적 선택
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // 스폰 위치 결정
        Vector3 spawnPosition = GetSpawnPosition();
        
        // 적 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Health 컴포넌트가 있으면 죽음 이벤트 연결
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += () => OnEnemyDeath(enemy);
        }
        
        activeEnemies.Add(enemy);
        
        if (useWaveSystem)
            enemiesRemaining--;
        
        Debug.Log($"[EnemySpawner] 적 스폰: {enemy.name} at {spawnPosition}");
    }
    
    Vector3 GetSpawnPosition()
    {
        if (player == null)
        {
            // 플레이어가 없으면 랜덤 스폰포인트 사용
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }
        
        // 플레이어 위치 기준으로 스폰포인트 조정
        Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPos = player.position + selectedSpawnPoint.localPosition;
        
        // 화면 밖에서 스폰되도록 보정
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(spawnPos);
            
            // 화면 안에 있으면 화면 밖으로 밀어내기
            if (screenPos.x > 0 && screenPos.x < Screen.width && 
                screenPos.y > 0 && screenPos.y < Screen.height)
            {
                Vector3 direction = (spawnPos - player.position).normalized;
                spawnPos = player.position + direction * spawnRadius;
            }
        }
        
        return spawnPos;
    }
    
    void OnEnemyDeath(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            isSpawning = false;
        }
    }
    
    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            if (useWaveSystem)
                StartWave();
            else
                StartContinuousSpawn();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 스폰 반경 표시
        Gizmos.color = Color.yellow;
        if (player != null)
        {
            Gizmos.DrawWireSphere(player.position, spawnRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        
        // 스폰포인트들 표시
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Vector3 pos = player != null ? player.position + spawnPoint.localPosition : spawnPoint.position;
                    Gizmos.DrawWireSphere(pos, 0.5f);
                }
            }
        }
    }
} 