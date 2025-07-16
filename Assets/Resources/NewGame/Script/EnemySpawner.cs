using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject[] enemyPrefabs; // 스폰할 적 프리팹들
    public float spawnInterval = 2f; // 스폰 간격
    public int maxEnemies = 10; // 최대 적 수
    
    [Header("웨이브 시스템")]
    public bool useWaveSystem = true;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 10f;
    public float waveMultiplier = 1.2f; // 웨이브마다 적 증가 배수
    
    [Header("스폰 포인트")]
    public Transform[] spawnPoints; // 지정된 스폰 위치들
    public float spawnDistance = 15f; // 화면 밖 스폰 거리 (백업용)
    public Vector2 spawnAreaSize = new Vector2(20f, 8f); // 스폰 영역 크기 (백업용)
    
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
    

    
    void StartWave()
    {
        isSpawning = true;
        currentWave++;
        
        int enemiesToSpawn = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(waveMultiplier, currentWave - 1));
        // Debug.Log($"[EnemySpawner] Wave {currentWave} 시작! 적 {enemiesToSpawn}마리 스폰 예정");
        
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
        if (enemyPrefabs.Length == 0)
        {
            // Debug.LogWarning("[EnemySpawner] 적 프리팹이 설정되지 않았습니다!");
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
        
        // Debug.Log($"[EnemySpawner] 적 스폰: {enemy.name} at {spawnPosition}");
    }
    
    Vector3 GetSpawnPosition()
    {
        // 지정된 스폰 포인트가 있으면 그중에서 선택
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // 사용 가능한 스폰 포인트 찾기 (다른 몬스터가 너무 가깝지 않은 곳)
            Transform bestSpawnPoint = GetAvailableSpawnPoint();
            if (bestSpawnPoint != null)
            {
                return bestSpawnPoint.position;
            }
        }
        
        // 스폰 포인트가 없거나 모두 사용 중이면 기존 방식 사용
        if (player == null)
        {
            return transform.position + new Vector3(spawnDistance, GetGroundY(), 0);
        }
        
        return GetForwardGroundSpawnPosition();
    }
    
    Transform GetAvailableSpawnPoint()
    {
        // 모든 스폰 포인트를 랜덤하게 섞어서 확인
        System.Collections.Generic.List<Transform> availablePoints = new System.Collections.Generic.List<Transform>(spawnPoints);
        
        // 랜덤하게 섞기
        for (int i = 0; i < availablePoints.Count; i++)
        {
            Transform temp = availablePoints[i];
            int randomIndex = Random.Range(i, availablePoints.Count);
            availablePoints[i] = availablePoints[randomIndex];
            availablePoints[randomIndex] = temp;
        }
        
        // 각 스폰 포인트에서 가장 가까운 적과의 거리 확인
        foreach (Transform spawnPoint in availablePoints)
        {
            if (spawnPoint == null) continue;
            
            bool isAvailable = true;
            float minDistance = 2f; // 최소 거리 2유닛
            
            // 활성 적들과의 거리 확인
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector3.Distance(spawnPoint.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    isAvailable = false;
                    break;
                }
            }
            
            if (isAvailable)
            {
                return spawnPoint;
            }
        }
        
        // 모든 포인트가 사용 중이면 가장 멀리 떨어진 포인트 반환
        Transform farthestPoint = null;
        float maxMinDistance = 0f;
        
        foreach (Transform spawnPoint in availablePoints)
        {
            if (spawnPoint == null) continue;
            
            float minDistanceToEnemies = float.MaxValue;
            
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector3.Distance(spawnPoint.position, enemy.transform.position);
                if (distance < minDistanceToEnemies)
                {
                    minDistanceToEnemies = distance;
                }
            }
            
            if (minDistanceToEnemies > maxMinDistance)
            {
                maxMinDistance = minDistanceToEnemies;
                farthestPoint = spawnPoint;
            }
        }
        
        return farthestPoint;
    }
    
    Vector3 GetForwardGroundSpawnPosition()
    {
        // 플레이어 앞쪽(오른쪽)에서 바닥에 붙어서 스폰
        Camera mainCam = Camera.main;
        Vector3 spawnPos = player.position;
        
        // X축: 화면 오른쪽 밖에서 스폰
        if (mainCam != null)
        {
            float cameraWidth = mainCam.orthographicSize * mainCam.aspect;
            spawnPos.x += cameraWidth + 2f; // 화면 오른쪽 밖
        }
        else
        {
            spawnPos.x += spawnDistance; // 카메라가 없으면 기본 거리
        }
        
        // Y축: 바닥에 붙어서 스폰
        spawnPos.y = GetGroundY();
        
        return spawnPos;
    }
    
    float GetGroundY()
    {
        // 바닥 Y 좌표 계산
        // 여기서는 간단히 플레이어 Y 좌표를 기준으로 함
        // 실제 게임에서는 Raycast를 사용해서 바닥을 찾을 수도 있음
        
        if (player != null)
        {
            // 플레이어와 같은 높이에서 스폰 (플레이어가 바닥에 있다고 가정)
            return player.position.y;
        }
        
        // 플레이어가 없으면 기본 높이 (0)
        return 0f;
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
    
    void OnDestroy()
    {
        // 활성 적들 정리
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
    }
    
    void OnDrawGizmosSelected()
    {
        // 지정된 스폰 포인트들 표시
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] == null) continue;
                
                // 스폰 포인트 번호에 따라 색상 변경
                Gizmos.color = Color.HSVToRGB((float)i / spawnPoints.Length, 0.8f, 1f);
                Gizmos.DrawWireSphere(spawnPoints[i].position, 1f);
                Gizmos.DrawSphere(spawnPoints[i].position, 0.3f);
                
                // 스폰 포인트를 더 잘 보이게 표시
                Gizmos.color = Color.white;
                Gizmos.DrawRay(spawnPoints[i].position, Vector3.up * 2f);
            }
        }
        
        // 플레이어가 없으면 백업 스폰 지역만 표시
        if (player == null) 
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position + Vector3.right * spawnDistance, 1f);
            return;
        }
        
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        
        Vector3 playerPos = player.position;
        float cameraHeight = mainCam.orthographicSize;
        float cameraWidth = mainCam.orthographicSize * mainCam.aspect;
        
        // 백업 스폰 지역 (스폰 포인트가 없을 때만 사용)
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Gizmos.color = Color.yellow;
            float groundY = GetGroundY();
            Vector3 spawnPoint = new Vector3(playerPos.x + cameraWidth + 2f, groundY, 0);
            Gizmos.DrawWireSphere(spawnPoint, 1f);
            
            // 바닥 라인 표시
            Gizmos.color = Color.green;
            Vector3 groundStart = new Vector3(playerPos.x - cameraWidth - 3f, groundY, 0);
            Vector3 groundEnd = new Vector3(playerPos.x + cameraWidth + 5f, groundY, 0);
            Gizmos.DrawLine(groundStart, groundEnd);
        }
        
        // 카메라 영역 표시
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(playerPos, new Vector3(cameraWidth * 2, cameraHeight * 2, 0));
        
        // 진행 방향 화살표
        Gizmos.color = Color.cyan;
        Vector3 arrowStart = playerPos + Vector3.right * 2f;
        Vector3 arrowEnd = arrowStart + Vector3.right * 3f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd + Vector3.left * 0.5f + Vector3.up * 0.5f);
        Gizmos.DrawLine(arrowEnd, arrowEnd + Vector3.left * 0.5f + Vector3.down * 0.5f);
    }
} 