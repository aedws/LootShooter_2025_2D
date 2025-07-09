using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ButterflyPhase
{
    Phase1,     // 100%~70%
    Phase2,     // 69%~30%
    Phase3,     // 29%~10%
    Phase4,     // 10% 이하
    Phase5      // 0% 도달 시 (부활)
}

public enum ButterflyPattern
{
    // Phase 1
    BeamSweep,              // 좌상단→우상단 빔 스윕
    GuidedProjectiles1,     // 유도하지 않는 투사체 30개
    RandomDownProjectiles,  // 하향 랜덤 투사체
    RandomExplosion,        // 랜덤 폭발 1개
    
    // Phase 2
    RandomAngleProjectiles, // 중앙에서 랜덤 각도 투사체 25개
    TeleportCharge,         // 텔레포트 돌진
    GuidedProjectiles2,     // 유도하지 않는 투사체 30개 (Phase 2)
    RandomDownProjectiles2, // 하향 랜덤 투사체 (Phase 2)
    
    // Phase 3
    LeftToRightProjectiles, // 좌상단→우하단 투사체 45개
    RightToLeftProjectiles, // 우상단→좌하단 투사체 45개
    LeftBeamAttack,         // 좌측 광역 빔
    RightBeamAttack,        // 우측 광역 빔
    CircularHovering,       // 원형 호버링 투사체
    
    // Phase 4
    RandomProjectilesWithExplosions, // 랜덤 투사체 + 폭발 2개
    PlayerDirectedProjectiles,       // 플레이어 방향 투사체 + 폭발 2개
    VerticalTeleportCharge,          // Y축 텔레포트 돌진
    LeftWideBeam,                    // 좌측 광역 빔 (2초)
    RightWideBeam,                   // 우측 광역 빔 (2초)
    
    // Phase 5
    RevivalSequence                  // 부활 시퀀스
}

public class ButterflyBoss : MonoBehaviour
{
    [Header("나비 보스 기본 정보")]
    public string bossName = "나비 보스";
    public int maxHealth = 1000;
    public float moveSpeed = 5f;
    public float flightSpeed = 8f;
    
    [Header("페이즈 시스템")]
    public ButterflyPhase currentPhase = ButterflyPhase.Phase1;
    public float phase1Threshold = 0.7f;  // 70%
    public float phase2Threshold = 0.3f;  // 30%
    public float phase3Threshold = 0.1f;  // 10%
    
    [Header("비행 시스템")]
    public float hoverHeight = 3f;
    public float flightSmoothness = 5f;
    public float wingFlapSpeed = 2f;
    public bool isFlying = true;
    
    [Header("투사체 프리팹")]
    public GameObject projectilePrefab1; // 투사체 프리팹_1
    public GameObject projectilePrefab2; // 투사체 프리팹_2
    public GameObject beamPrefab;        // 빔 프리팹
    public GameObject explosionPrefab;   // 폭발 프리팹
    public GameObject warningPrefab;     // 경고 표시 프리팹
    
    [Header("패턴 설정")]
    public float patternCooldown = 2f;
    public float centerRestTime1 = 1f;   // 1,2페이즈 중앙 휴식 시간
    public float centerRestTime2 = 0.3f; // 3,4페이즈 중앙 휴식 시간
    
    [Header("화면 설정")]
    public float screenWidth = 1920f;
    public float screenHeight = 1080f;
    public float screenMargin = 100f;
    
    [Header("특수 효과")]
    public GameObject phaseTransitionEffect;
    public AudioClip wingFlapSound;
    public AudioClip beamSound;
    public AudioClip explosionSound;
    
    // 컴포넌트 참조
    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    private Camera mainCamera;
    
    // 상태 관리
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isInPhase5 = false;
    private bool hasRevived = false;
    private float lastAttackTime = -1f;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    // 패턴 관리
    private List<ButterflyPattern> availablePatterns = new List<ButterflyPattern>();
    private ButterflyPattern lastUsedPattern;
    private int phase5PatternIndex = 0;
    
    // 이벤트
    public System.Action<ButterflyPhase> OnPhaseChanged;
    public System.Action OnBossDeath;
    public System.Action OnBossRevival;
    public System.Action<int> OnBossDamaged;
    
    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    
    void Start()
    {
        // Health 컴포넌트 설정
        if (health != null)
        {
            health.SetMaxHealth(maxHealth);
            health.OnDeath += OnDeath;
            health.OnDamaged += OnDamaged;
        }
        
        // 보스 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        // 중력 비활성화 (비행 보스)
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        
        // 초기 위치 설정
        targetPosition = GetScreenCenterTop();
        transform.position = targetPosition;
        
        // 초기 페이즈 설정
        UpdatePhasePatterns();
        
        // 보스 등장 이펙트
        StartCoroutine(BossEntranceEffect());
    }
    
    void Update()
    {
        if (isDead) return;
        
        // 페이즈 체크
        CheckPhaseTransition();
        
        // 비행 이동
        UpdateFlight();
        
        // 공격 패턴 실행
        if (!isAttacking && Time.time - lastAttackTime > patternCooldown)
        {
            ExecuteRandomPattern();
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    void CheckPhaseTransition()
    {
        if (health == null || isInPhase5) return;
        
        float healthPercentage = health.GetHealthPercentage();
        ButterflyPhase newPhase = currentPhase;
        
        if (healthPercentage <= phase3Threshold && currentPhase == ButterflyPhase.Phase3)
        {
            newPhase = ButterflyPhase.Phase4;
        }
        else if (healthPercentage <= phase2Threshold && currentPhase == ButterflyPhase.Phase2)
        {
            newPhase = ButterflyPhase.Phase3;
        }
        else if (healthPercentage <= phase1Threshold && currentPhase == ButterflyPhase.Phase1)
        {
            newPhase = ButterflyPhase.Phase2;
        }
        
        if (newPhase != currentPhase)
        {
            TransitionToPhase(newPhase);
        }
    }
    
    void TransitionToPhase(ButterflyPhase newPhase)
    {
        ButterflyPhase oldPhase = currentPhase;
        currentPhase = newPhase;
        
        // 페이즈 전환 이펙트
        StartCoroutine(PhaseTransitionEffect());
        
        // 새로운 페이즈의 패턴 목록 업데이트
        UpdatePhasePatterns();
        
        // 이벤트 발생
        OnPhaseChanged?.Invoke(newPhase);
        
        // Debug.Log($"[ButterflyBoss] 페이즈 전환: {oldPhase} → {newPhase}");
    }
    
    void UpdatePhasePatterns()
    {
        availablePatterns.Clear();
        
        switch (currentPhase)
        {
            case ButterflyPhase.Phase1:
                availablePatterns.AddRange(new ButterflyPattern[]
                {
                    ButterflyPattern.BeamSweep,
                    ButterflyPattern.GuidedProjectiles1,
                    ButterflyPattern.RandomDownProjectiles,
                    ButterflyPattern.RandomExplosion
                });
                break;
                
            case ButterflyPhase.Phase2:
                availablePatterns.AddRange(new ButterflyPattern[]
                {
                    ButterflyPattern.RandomAngleProjectiles,
                    ButterflyPattern.TeleportCharge,
                    ButterflyPattern.GuidedProjectiles2,
                    ButterflyPattern.RandomDownProjectiles2
                });
                break;
                
            case ButterflyPhase.Phase3:
                availablePatterns.AddRange(new ButterflyPattern[]
                {
                    ButterflyPattern.LeftToRightProjectiles,
                    ButterflyPattern.RightToLeftProjectiles,
                    ButterflyPattern.LeftBeamAttack,
                    ButterflyPattern.RightBeamAttack,
                    ButterflyPattern.CircularHovering
                });
                break;
                
            case ButterflyPhase.Phase4:
                availablePatterns.AddRange(new ButterflyPattern[]
                {
                    ButterflyPattern.RandomProjectilesWithExplosions,
                    ButterflyPattern.PlayerDirectedProjectiles,
                    ButterflyPattern.VerticalTeleportCharge,
                    ButterflyPattern.LeftWideBeam,
                    ButterflyPattern.RightWideBeam
                });
                break;
        }
    }
    
    void ExecuteRandomPattern()
    {
        if (isInPhase5)
        {
            // Phase 5는 순서대로 실행
            ExecutePhase5Pattern();
            return;
        }
        
        if (availablePatterns.Count == 0) return;
        
        // 이전에 사용한 패턴 제외하고 랜덤 선택
        List<ButterflyPattern> selectablePatterns = new List<ButterflyPattern>(availablePatterns);
        if (lastUsedPattern != ButterflyPattern.RevivalSequence)
        {
            selectablePatterns.Remove(lastUsedPattern);
        }
        
        if (selectablePatterns.Count == 0)
        {
            selectablePatterns = new List<ButterflyPattern>(availablePatterns);
        }
        
        ButterflyPattern selectedPattern = selectablePatterns[Random.Range(0, selectablePatterns.Count)];
        lastUsedPattern = selectedPattern;
        
        StartCoroutine(ExecutePattern(selectedPattern));
    }
    
    void ExecutePhase5Pattern()
    {
        switch (phase5PatternIndex)
        {
            case 0:
                StartCoroutine(RevivalRedEffect());
                break;
            case 1:
                StartCoroutine(RevivalHeal());
                break;
            case 2:
                StartCoroutine(RevivalHoverToCenter());
                break;
        }
        
        phase5PatternIndex++;
        if (phase5PatternIndex >= 3)
        {
            // Phase 4로 복귀
            isInPhase5 = false;
            currentPhase = ButterflyPhase.Phase4;
            UpdatePhasePatterns();
        }
    }
    
    IEnumerator ExecutePattern(ButterflyPattern pattern)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        switch (pattern)
        {
            // Phase 1 Patterns
            case ButterflyPattern.BeamSweep:
                yield return StartCoroutine(BeamSweepPattern());
                break;
            case ButterflyPattern.GuidedProjectiles1:
                yield return StartCoroutine(GuidedProjectilesPattern(30, projectilePrefab1));
                break;
            case ButterflyPattern.RandomDownProjectiles:
                yield return StartCoroutine(RandomDownProjectilesPattern());
                break;
            case ButterflyPattern.RandomExplosion:
                yield return StartCoroutine(RandomExplosionPattern(1));
                break;
                
            // Phase 2 Patterns
            case ButterflyPattern.RandomAngleProjectiles:
                yield return StartCoroutine(RandomAngleProjectilesPattern(25));
                break;
            case ButterflyPattern.TeleportCharge:
                yield return StartCoroutine(TeleportChargePattern());
                break;
            case ButterflyPattern.GuidedProjectiles2:
                yield return StartCoroutine(GuidedProjectilesPattern(30, projectilePrefab1));
                break;
            case ButterflyPattern.RandomDownProjectiles2:
                yield return StartCoroutine(RandomDownProjectilesPattern());
                break;
                
            // Phase 3 Patterns
            case ButterflyPattern.LeftToRightProjectiles:
                yield return StartCoroutine(LeftToRightProjectilesPattern());
                break;
            case ButterflyPattern.RightToLeftProjectiles:
                yield return StartCoroutine(RightToLeftProjectilesPattern());
                break;
            case ButterflyPattern.LeftBeamAttack:
                yield return StartCoroutine(LeftBeamAttackPattern());
                break;
            case ButterflyPattern.RightBeamAttack:
                yield return StartCoroutine(RightBeamAttackPattern());
                break;
            case ButterflyPattern.CircularHovering:
                yield return StartCoroutine(CircularHoveringPattern());
                break;
                
            // Phase 4 Patterns
            case ButterflyPattern.RandomProjectilesWithExplosions:
                yield return StartCoroutine(RandomProjectilesWithExplosionsPattern());
                break;
            case ButterflyPattern.PlayerDirectedProjectiles:
                yield return StartCoroutine(PlayerDirectedProjectilesPattern());
                break;
            case ButterflyPattern.VerticalTeleportCharge:
                yield return StartCoroutine(VerticalTeleportChargePattern());
                break;
            case ButterflyPattern.LeftWideBeam:
                yield return StartCoroutine(LeftWideBeamPattern());
                break;
            case ButterflyPattern.RightWideBeam:
                yield return StartCoroutine(RightWideBeamPattern());
                break;
        }
        
        // 패턴 완료 후 중앙 휴식
        yield return StartCoroutine(RestAtCenter());
        
        isAttacking = false;
    }
    
    // Phase 1: 빔 스윕 패턴
    IEnumerator BeamSweepPattern()
    {
        Vector3 startPos = GetScreenPosition(-0.8f, 0.8f); // 좌측 상단
        Vector3 endPos = GetScreenPosition(0.8f, 0.8f);   // 우측 상단
        
        transform.position = startPos;
        
        float sweepTime = 3f;
        float timer = 0f;
        
        while (timer < sweepTime)
        {
            float t = timer / sweepTime;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            transform.position = currentPos;
            
            // 빔 발사
            FireBeamDownward();
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    // Phase 1: 유도하지 않는 투사체 패턴
    IEnumerator GuidedProjectilesPattern(int count, GameObject prefab)
    {
        Vector3 centerTop = GetScreenCenterTop();
        transform.position = centerTop;
        
        for (int i = 0; i < count; i++)
        {
            if (player != null)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                FireProjectile(direction, prefab);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Phase 1: 하향 랜덤 투사체 패턴
    IEnumerator RandomDownProjectilesPattern()
    {
        Vector3 centerTop = GetScreenCenterTop();
        transform.position = centerTop;
        
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(-45f, 45f); // 하향 45도 범위
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.down;
            FireProjectile(direction, projectilePrefab2);
            
            yield return new WaitForSeconds(0.15f);
        }
    }
    
    // Phase 1: 랜덤 폭발 패턴
    IEnumerator RandomExplosionPattern(int count)
    {
        Vector3 centerTop = GetScreenCenterTop();
        transform.position = centerTop;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = GetRandomScreenPosition();
            
            // 경고 표시
            ShowWarning(randomPos);
            yield return new WaitForSeconds(1f);
            
            // 폭발
            CreateExplosion(randomPos);
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    // Phase 2: 랜덤 각도 투사체 패턴
    IEnumerator RandomAngleProjectilesPattern(int count)
    {
        Vector3 center = GetScreenCenter();
        transform.position = center;
        
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            FireProjectile(direction, projectilePrefab1);
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Phase 2: 텔레포트 돌진 패턴
    IEnumerator TeleportChargePattern()
    {
        if (player == null) yield break;
        
        // 플레이어 X축 기준으로 화면 밖 텔레포트
        float playerX = player.position.x;
        Vector3 teleportPos = new Vector3(playerX, GetScreenPosition(0, 1.2f).y, 0);
        transform.position = teleportPos;
        
        // 돌진
        Vector3 chargeDirection = Vector3.left;
        float chargeSpeed = flightSpeed * 2f;
        float chargeTime = 2f;
        
        float timer = 0f;
        while (timer < chargeTime)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 화면 밖으로 이탈 후 돌아오기
        yield return new WaitForSeconds(1f);
        transform.position = GetScreenCenterTop();
    }
    
    // Phase 3: 좌상단→우하단 투사체 패턴
    IEnumerator LeftToRightProjectilesPattern()
    {
        Vector3 startPos = GetScreenPosition(-0.8f, 0.8f); // 좌측 상단
        transform.position = startPos;
        
        int count = 45;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(-30f, 30f); // 우하단 방향 랜덤 각도
            Vector2 direction = Quaternion.Euler(0, 0, angle) * new Vector2(1f, -1f).normalized;
            FireProjectile(direction, projectilePrefab1);
            
            yield return new WaitForSeconds(0.08f);
        }
    }
    
    // Phase 3: 우상단→좌하단 투사체 패턴
    IEnumerator RightToLeftProjectilesPattern()
    {
        Vector3 startPos = GetScreenPosition(0.8f, 0.8f); // 우측 상단
        transform.position = startPos;
        
        int count = 45;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(-30f, 30f); // 좌하단 방향 랜덤 각도
            Vector2 direction = Quaternion.Euler(0, 0, angle) * new Vector2(-1f, -1f).normalized;
            FireProjectile(direction, projectilePrefab1);
            
            yield return new WaitForSeconds(0.08f);
        }
    }
    
    // Phase 3: 좌측 광역 빔 패턴
    IEnumerator LeftBeamAttackPattern()
    {
        Vector3 leftCenter = GetScreenPosition(-0.8f, 0f);
        transform.position = leftCenter;
        
        // 경고 표시
        ShowBeamWarning(leftCenter, true);
        yield return new WaitForSeconds(1f);
        
        // 빔 발사 (1초간)
        FireWideBeam(leftCenter, true, 1f);
        yield return new WaitForSeconds(1f);
    }
    
    // Phase 3: 우측 광역 빔 패턴
    IEnumerator RightBeamAttackPattern()
    {
        Vector3 rightCenter = GetScreenPosition(0.8f, 0f);
        transform.position = rightCenter;
        
        // 경고 표시
        ShowBeamWarning(rightCenter, false);
        yield return new WaitForSeconds(1f);
        
        // 빔 발사 (1초간)
        FireWideBeam(rightCenter, false, 1f);
        yield return new WaitForSeconds(1f);
    }
    
    // Phase 3: 원형 호버링 패턴
    IEnumerator CircularHoveringPattern()
    {
        if (player == null) yield break;
        
        Vector3 playerPos = player.position;
        float radius = 3f;
        float hoverTime = 3f;
        float timer = 0f;
        
        while (timer < hoverTime)
        {
            float angle = (timer / hoverTime) * 360f;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;
            transform.position = playerPos + offset;
            
            // 5개 투사체 순차 발사
            if (Mathf.FloorToInt(timer * 2f) % 2 == 0)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                FireProjectile(direction, projectilePrefab1);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    // Phase 4: 랜덤 투사체 + 폭발 패턴
    IEnumerator RandomProjectilesWithExplosionsPattern()
    {
        Vector3 center = GetScreenCenter();
        transform.position = center;
        
        // 투사체 발사
        for (int i = 0; i < 25; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            FireProjectile(direction, projectilePrefab1);
            yield return new WaitForSeconds(0.1f);
        }
        
        // 동시 폭발 2개
        Vector3 explosion1 = GetRandomScreenPosition();
        Vector3 explosion2 = GetRandomScreenPosition();
        
        ShowWarning(explosion1);
        ShowWarning(explosion2);
        yield return new WaitForSeconds(1f);
        
        CreateExplosion(explosion1);
        CreateExplosion(explosion2);
    }
    
    // Phase 4: 플레이어 방향 투사체 + 폭발 패턴
    IEnumerator PlayerDirectedProjectilesPattern()
    {
        Vector3 center = GetScreenCenter();
        transform.position = center;
        
        // 플레이어 방향 투사체 발사
        for (int i = 0; i < 10; i++)
        {
            if (player != null)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                FireProjectile(direction, projectilePrefab2);
            }
            yield return new WaitForSeconds(0.2f);
        }
        
        // 동시 폭발 2개
        Vector3 explosion1 = GetRandomScreenPosition();
        Vector3 explosion2 = GetRandomScreenPosition();
        
        ShowWarning(explosion1);
        ShowWarning(explosion2);
        yield return new WaitForSeconds(1f);
        
        CreateExplosion(explosion1);
        CreateExplosion(explosion2);
    }
    
    // Phase 4: Y축 텔레포트 돌진 패턴
    IEnumerator VerticalTeleportChargePattern()
    {
        if (player == null) yield break;
        
        // 플레이어 Y축 기준으로 화면 밖 텔레포트
        float playerY = player.position.y;
        Vector3 teleportPos = new Vector3(GetScreenPosition(1.2f, 0).x, playerY, 0);
        transform.position = teleportPos;
        
        // 돌진
        Vector3 chargeDirection = Vector3.left;
        float chargeSpeed = flightSpeed * 2f;
        float chargeTime = 2f;
        
        float timer = 0f;
        while (timer < chargeTime)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 화면 밖으로 이탈 후 돌아오기
        yield return new WaitForSeconds(1f);
        transform.position = GetScreenCenterTop();
    }
    
    // Phase 4: 좌측 광역 빔 패턴 (2초)
    IEnumerator LeftWideBeamPattern()
    {
        Vector3 leftCenter = GetScreenPosition(-0.8f, 0f);
        transform.position = leftCenter;
        
        // 경고 표시
        ShowBeamWarning(leftCenter, true);
        yield return new WaitForSeconds(1f);
        
        // 빔 발사 (2초간)
        FireWideBeam(leftCenter, true, 2f);
        yield return new WaitForSeconds(2f);
    }
    
    // Phase 4: 우측 광역 빔 패턴 (2초)
    IEnumerator RightWideBeamPattern()
    {
        Vector3 rightCenter = GetScreenPosition(0.8f, 0f);
        transform.position = rightCenter;
        
        // 경고 표시
        ShowBeamWarning(rightCenter, false);
        yield return new WaitForSeconds(1f);
        
        // 빔 발사 (2초간)
        FireWideBeam(rightCenter, false, 2f);
        yield return new WaitForSeconds(2f);
    }
    
    // Phase 5: 부활 시퀀스
    IEnumerator RevivalRedEffect()
    {
        Vector3 center = GetScreenCenter();
        transform.position = center;
        
        // 붉어짐 효과
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(2f);
            spriteRenderer.color = originalColor;
        }
    }
    
    IEnumerator RevivalHeal()
    {
        // HP 15%로 천천히 재생
        if (health != null)
        {
            int targetHealth = Mathf.RoundToInt(maxHealth * 0.15f);
            int currentHealth = health.currentHealth;
            
            while (health.currentHealth < targetHealth)
            {
                health.Heal(1);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    IEnumerator RevivalHoverToCenter()
    {
        // 화면 우측으로 호버링
        Vector3 rightPos = GetScreenPosition(0.8f, 0f);
        transform.position = rightPos;
        yield return new WaitForSeconds(1f);
        
        // 화면 중앙 상단으로 이동
        Vector3 centerTop = GetScreenCenterTop();
        transform.position = centerTop;
    }
    
    // 중앙 휴식
    IEnumerator RestAtCenter()
    {
        Vector3 center = GetScreenCenter();
        transform.position = center;
        
        float restTime = (currentPhase == ButterflyPhase.Phase1 || currentPhase == ButterflyPhase.Phase2) 
            ? centerRestTime1 : centerRestTime2;
        
        yield return new WaitForSeconds(restTime);
    }
    
    // 비행 업데이트
    void UpdateFlight()
    {
        if (!isFlying) return;
        
        // 부드러운 비행 이동
        Vector3 targetPos = targetPosition;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / flightSmoothness);
        
        // 날개짓 애니메이션
        if (animator != null)
        {
            animator.SetFloat("WingFlapSpeed", wingFlapSpeed);
        }
    }
    
    // 화면 위치 계산
    Vector3 GetScreenPosition(float xPercent, float yPercent)
    {
        if (mainCamera == null) return Vector3.zero;
        
        float x = (xPercent * screenWidth / 2f) - screenMargin;
        float y = (yPercent * screenHeight / 2f) - screenMargin;
        
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f + xPercent * 0.5f, 0.5f + yPercent * 0.5f, 10f));
        return worldPos;
    }
    
    Vector3 GetScreenCenter()
    {
        return GetScreenPosition(0f, 0f);
    }
    
    Vector3 GetScreenCenterTop()
    {
        return GetScreenPosition(0f, 0.8f);
    }
    
    Vector3 GetRandomScreenPosition()
    {
        float x = Random.Range(-0.8f, 0.8f);
        float y = Random.Range(-0.8f, 0.8f);
        return GetScreenPosition(x, y);
    }
    
    // 투사체 발사
    void FireProjectile(Vector2 direction, GameObject prefab)
    {
        if (prefab == null) return;
        
        GameObject projectile = Instantiate(prefab, transform.position, Quaternion.identity);
        
        // ButterflyProjectile 컴포넌트 확인
        ButterflyProjectile butterflyProj = projectile.GetComponent<ButterflyProjectile>();
        if (butterflyProj != null)
        {
            butterflyProj.Init(direction, 20, 15f);
        }
        else
        {
            // 기존 Projectile 컴포넌트 확인
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(direction, 20, 15f);
            }
        }
    }
    
    // 빔 발사
    void FireBeamDownward()
    {
        if (beamPrefab == null) return;
        
        GameObject beam = Instantiate(beamPrefab, transform.position, Quaternion.identity);
        
        // ButterflyBeam 컴포넌트 설정
        ButterflyBeam butterflyBeam = beam.GetComponent<ButterflyBeam>();
        if (butterflyBeam != null)
        {
            butterflyBeam.isWideBeam = false;
            butterflyBeam.isLeftBeam = false;
            butterflyBeam.isRightBeam = false;
        }
    }
    
    // 광역 빔 발사
    void FireWideBeam(Vector3 position, bool isLeft, float duration)
    {
        if (beamPrefab == null) return;
        
        GameObject beam = Instantiate(beamPrefab, position, Quaternion.identity);
        
        // ButterflyBeam 컴포넌트 설정
        ButterflyBeam butterflyBeam = beam.GetComponent<ButterflyBeam>();
        if (butterflyBeam != null)
        {
            butterflyBeam.isWideBeam = true;
            butterflyBeam.isLeftBeam = isLeft;
            butterflyBeam.isRightBeam = !isLeft;
            butterflyBeam.lifetime = duration;
        }
    }
    
    // 폭발 생성
    void CreateExplosion(Vector3 position)
    {
        if (explosionPrefab == null) return;
        
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        
        // ButterflyExplosion 컴포넌트 확인
        ButterflyExplosion butterflyExplosion = explosion.GetComponent<ButterflyExplosion>();
        if (butterflyExplosion != null)
        {
            // ButterflyExplosion은 자체적으로 수명 관리
        }
        else
        {
            // 기존 폭발 프리팹은 수동으로 제거
            Destroy(explosion, 2f);
        }
    }
    
    // 경고 표시
    void ShowWarning(Vector3 position)
    {
        if (warningPrefab == null) return;
        
        GameObject warning = Instantiate(warningPrefab, position, Quaternion.identity);
        
        // ButterflyWarning 컴포넌트 확인
        ButterflyWarning butterflyWarning = warning.GetComponent<ButterflyWarning>();
        if (butterflyWarning != null)
        {
            // ButterflyWarning은 자체적으로 수명 관리
        }
        else
        {
            // 기존 경고 프리팹은 수동으로 제거
            Destroy(warning, 1f);
        }
    }
    
    // 빔 경고 표시
    void ShowBeamWarning(Vector3 position, bool isLeft)
    {
        if (warningPrefab == null) return;
        
        GameObject warning = Instantiate(warningPrefab, position, Quaternion.identity);
        
        // ButterflyWarning 컴포넌트 설정
        ButterflyWarning butterflyWarning = warning.GetComponent<ButterflyWarning>();
        if (butterflyWarning != null)
        {
            butterflyWarning.isExplosionWarning = false;
            butterflyWarning.isBeamWarning = true;
            butterflyWarning.isChargeWarning = false;
        }
        else
        {
            // 기존 경고 프리팹은 수동으로 제거
            Destroy(warning, 1f);
        }
    }
    
    // 애니메이션 업데이트
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isFlying", isFlying);
        }
    }
    
    // 데미지 처리
    void OnDamaged(int damage)
    {
        OnBossDamaged?.Invoke(damage);
        
        // 피격 이펙트
        if (animator != null)
            animator.SetTrigger("Hit");
        
        StartCoroutine(HitFlash());
    }
    
    // 사망 처리
    void OnDeath()
    {
        if (isInPhase5) return; // 이미 Phase 5면 무시
        
        isDead = true;
        
        if (currentPhase == ButterflyPhase.Phase4 && !hasRevived)
        {
            // Phase 5로 전환 (부활)
            isInPhase5 = true;
            hasRevived = true;
            isDead = false;
            phase5PatternIndex = 0;
            
            OnBossRevival?.Invoke();
            // Debug.Log("[ButterflyBoss] Phase 5 시작 - 부활 시퀀스");
        }
        else
        {
            // 완전한 사망
            OnBossDeath?.Invoke();
            StartCoroutine(DelayedDestroy());
        }
    }
    
    // 피격 플래시 효과
    IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    // 페이즈 전환 이펙트
    IEnumerator PhaseTransitionEffect()
    {
        if (phaseTransitionEffect != null)
        {
            Instantiate(phaseTransitionEffect, transform.position, Quaternion.identity);
        }
        
        // 화면 흔들림
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 originalPosition = mainCamera.transform.position;
            float shakeTime = 1f;
            float shakeIntensity = 0.5f;
            
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
    }
    
    // 보스 등장 이펙트
    IEnumerator BossEntranceEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            
            float fadeTime = 2f;
            float timer = 0f;
            
            while (timer < fadeTime)
            {
                float alpha = Mathf.Lerp(0f, 1f, timer / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            
            spriteRenderer.color = originalColor;
        }
    }
    
    // 지연 파괴
    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
    
    // 공개 메서드들
    public bool IsAlive()
    {
        return !isDead && health != null && health.IsAlive();
    }
    
    public ButterflyPhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public float GetHealthPercentage()
    {
        return health != null ? health.GetHealthPercentage() : 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        // 화면 범위 표시
        Gizmos.color = Color.yellow;
        Vector3 center = GetScreenCenter();
        Gizmos.DrawWireCube(center, new Vector3(screenWidth, screenHeight, 1f));
        
        // 플레이어까지의 선
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
} 