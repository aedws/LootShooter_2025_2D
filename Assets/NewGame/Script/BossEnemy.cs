using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BossPhase
{
    Phase1,     // 첫 번째 페이즈 (기본 공격)
    Phase2,     // 두 번째 페이즈 (강화된 공격)
    Phase3      // 최종 페이즈 (절망 모드)
}

public enum BossAttackType
{
    MeleeAttack,        // 근접 공격
    RangedAttack,       // 원거리 공격
    ChargeAttack,       // 돌진 공격
    SummonMinions,      // 하수인 소환
    AreaAttack,         // 범위 공격
    SpecialAbility      // 특수 능력
}

[System.Serializable]
public class BossAttack
{
    public string attackName = "기본 공격";
    public BossAttackType attackType = BossAttackType.MeleeAttack;
    public float damage = 20f;
    public float cooldown = 2f;
    public float range = 3f;
    public float windupTime = 0.5f; // 선행 동작 시간
    public bool isUnblockable = false; // 방어 불가능한 공격
    public GameObject attackEffect; // 공격 이펙트
    public AudioClip attackSound; // 공격 사운드
    [TextArea(2, 4)]
    public string description = "공격 설명";
}

public class BossEnemy : MonoBehaviour
{
    [Header("보스 기본 정보")]
    public string bossName = "보스 몬스터";
    public int maxHealth = 500;
    public float moveSpeed = 2f;
    public float detectionRange = 20f;
    
    [Header("페이즈 시스템")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase1HealthThreshold = 0.7f; // 70% 체력에서 페이즈 2
    public float phase2HealthThreshold = 0.3f; // 30% 체력에서 페이즈 3
    public float phase3HealthThreshold = 0.1f; // 10% 체력에서 절망 모드 (선택사항)
    
    [Header("공격 시스템")]
    public List<BossAttack> phase1Attacks = new List<BossAttack>();
    public List<BossAttack> phase2Attacks = new List<BossAttack>();
    public List<BossAttack> phase3Attacks = new List<BossAttack>();
    
    [Header("공격 패턴 (선택사항)")]
    public BossAttackPattern[] attackPatterns;
    public bool useAttackPatterns = false;
    
    [Header("하수인 소환")]
    public GameObject[] minionPrefabs;
    public int maxMinions = 3;
    public float minionSpawnCooldown = 10f;
    
    [Header("보스 특수 능력")]
    public bool canTeleport = false;
    public float teleportCooldown = 15f;
    public float teleportRange = 10f;
    
    public bool canShield = false;
    public float shieldDuration = 5f;
    public float shieldCooldown = 20f;
    
    [Header("보상 시스템")]
    public int expReward = 100;
    public GameObject[] guaranteedDrops; // 확정 드롭
    public GameObject[] rareDrops; // 희귀 드롭
    public float rareDropChance = 0.1f;
    
    [Header("UI 및 이펙트")]
    public GameObject bossHealthBar;
    public GameObject phaseTransitionEffect;
    public AudioClip phaseTransitionSound;
    public AudioClip bossDeathSound;
    
    // 컴포넌트 참조
    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    
    // 상태 관리
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isShielded = false;
    private bool facingRight = true;
    private float lastAttackTime = -1f;
    private float lastMinionSpawnTime = -1f;
    private float lastTeleportTime = -1f;
    private float lastShieldTime = -1f;
    
    // 하수인 관리
    private List<GameObject> activeMinions = new List<GameObject>();
    
    // 현재 페이즈의 공격 목록
    private List<BossAttack> currentPhaseAttacks = new List<BossAttack>();
    
    // 이벤트
    public System.Action<BossPhase> OnPhaseChanged;
    public System.Action OnBossDeath;
    public System.Action<int> OnBossDamaged;
    
    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
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
        
        // 초기 페이즈 설정
        UpdatePhaseAttacks();
        
        // 보스 헬스바 표시
        ShowBossHealthBar();
        
        // 보스 등장 이펙트
        StartCoroutine(BossEntranceEffect());
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        // 페이즈 체크
        CheckPhaseTransition();
        
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // 공격 가능한지 확인
            if (!isAttacking && Time.time - lastAttackTime > GetCurrentAttackCooldown())
            {
                TryAttack();
            }
            else if (!isAttacking)
            {
                // 공격 쿨다운 중일 때는 플레이어 추적
                MoveTowardsPlayer();
            }
            
            // 특수 능력 사용
            UseSpecialAbilities();
        }
        
        // 하수인 관리
        ManageMinions();
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    void CheckPhaseTransition()
    {
        if (health == null) return;
        
        float healthPercentage = health.GetHealthPercentage();
        BossPhase newPhase = currentPhase;
        
        if (healthPercentage <= phase2HealthThreshold && currentPhase == BossPhase.Phase1)
        {
            newPhase = BossPhase.Phase2;
        }
        else if (healthPercentage <= phase3HealthThreshold && currentPhase == BossPhase.Phase2)
        {
            newPhase = BossPhase.Phase3;
        }
        
        if (newPhase != currentPhase)
        {
            TransitionToPhase(newPhase);
        }
    }
    
    void TransitionToPhase(BossPhase newPhase)
    {
        BossPhase oldPhase = currentPhase;
        currentPhase = newPhase;
        
        // 페이즈 전환 이펙트
        StartCoroutine(PhaseTransitionEffect());
        
        // 새로운 페이즈의 공격 목록 업데이트
        UpdatePhaseAttacks();
        
        // 페이즈별 특수 효과
        ApplyPhaseEffects(newPhase);
        
        // 이벤트 발생
        OnPhaseChanged?.Invoke(newPhase);
        
        // Debug.Log($"[Boss] {bossName} 페이즈 전환: {oldPhase} → {newPhase}");
    }
    
    void UpdatePhaseAttacks()
    {
        currentPhaseAttacks.Clear();
        
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                currentPhaseAttacks.AddRange(phase1Attacks);
                break;
            case BossPhase.Phase2:
                currentPhaseAttacks.AddRange(phase2Attacks);
                break;
            case BossPhase.Phase3:
                currentPhaseAttacks.AddRange(phase3Attacks);
                break;
        }
    }
    
    void ApplyPhaseEffects(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.Phase2:
                // 페이즈 2: 공격력 증가, 이동속도 증가
                moveSpeed *= 1.2f;
                break;
            case BossPhase.Phase3:
                // 페이즈 3: 절망 모드 - 모든 능력 강화
                moveSpeed *= 1.5f;
                break;
        }
    }
    
    void TryAttack()
    {
        // 공격 패턴 사용 여부 확인
        if (useAttackPatterns && attackPatterns != null && attackPatterns.Length > 0)
        {
            // 현재 페이즈에 맞는 패턴 선택
            BossAttackPattern selectedPattern = SelectAttackPattern();
            if (selectedPattern != null)
            {
                StartCoroutine(ExecuteAttackPattern(selectedPattern));
                return;
            }
        }
        
        // 기본 공격 시스템 사용
        if (currentPhaseAttacks.Count == 0) return;
        
        // 현재 상황에 맞는 공격 선택
        BossAttack selectedAttack = SelectBestAttack();
        if (selectedAttack != null)
        {
            StartCoroutine(ExecuteAttack(selectedAttack));
        }
    }
    
    BossAttackPattern SelectAttackPattern()
    {
        List<BossAttackPattern> availablePatterns = new List<BossAttackPattern>();
        
        foreach (BossAttackPattern pattern in attackPatterns)
        {
            if (pattern != null && pattern.targetPhase == currentPhase)
            {
                availablePatterns.Add(pattern);
            }
        }
        
        if (availablePatterns.Count > 0)
        {
            // 랜덤 선택 (나중에 AI 로직으로 개선 가능)
            return availablePatterns[Random.Range(0, availablePatterns.Count)];
        }
        
        return null;
    }
    
    BossAttack SelectBestAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 거리에 따라 적합한 공격 필터링
        List<BossAttack> availableAttacks = new List<BossAttack>();
        
        foreach (BossAttack attack in currentPhaseAttacks)
        {
            if (attack.range >= distanceToPlayer)
            {
                availableAttacks.Add(attack);
            }
        }
        
        if (availableAttacks.Count == 0)
        {
            // 범위 공격이나 특수 능력 사용
            foreach (BossAttack attack in currentPhaseAttacks)
            {
                if (attack.attackType == BossAttackType.AreaAttack || 
                    attack.attackType == BossAttackType.SpecialAbility)
                {
                    availableAttacks.Add(attack);
                }
            }
        }
        
        if (availableAttacks.Count > 0)
        {
            // 랜덤 선택 (나중에 AI 로직으로 개선 가능)
            return availableAttacks[Random.Range(0, availableAttacks.Count)];
        }
        
        return null;
    }
    
    IEnumerator ExecuteAttackPattern(BossAttackPattern pattern)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 패턴 실행
        yield return StartCoroutine(pattern.ExecutePattern(this));
        
        // 패턴 완료
        isAttacking = false;
    }
    
    IEnumerator ExecuteAttack(BossAttack attack)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 선행 동작 (공격 준비)
        if (attack.windupTime > 0)
        {
            // 공격 준비 애니메이션
            if (animator != null)
                animator.SetTrigger("AttackWindup");
            
            yield return new WaitForSeconds(attack.windupTime);
        }
        
        // 공격 실행
        switch (attack.attackType)
        {
            case BossAttackType.MeleeAttack:
                ExecuteMeleeAttack(attack);
                break;
            case BossAttackType.RangedAttack:
                ExecuteRangedAttack(attack);
                break;
            case BossAttackType.ChargeAttack:
                yield return StartCoroutine(ExecuteChargeAttack(attack));
                break;
            case BossAttackType.SummonMinions:
                ExecuteSummonMinions(attack);
                break;
            case BossAttackType.AreaAttack:
                ExecuteAreaAttack(attack);
                break;
            case BossAttackType.SpecialAbility:
                ExecuteSpecialAbility(attack);
                break;
        }
        
        // 공격 완료
        isAttacking = false;
        
        // 공격 이펙트 재생
        if (attack.attackEffect != null)
        {
            Instantiate(attack.attackEffect, transform.position, Quaternion.identity);
        }
        
        // 공격 사운드 재생
        if (attack.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attack.attackSound, transform.position);
        }
    }
    
    void ExecuteMeleeAttack(BossAttack attack)
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attack.range)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)attack.damage);
            }
        }
        
        if (animator != null)
            animator.SetTrigger("Attack");
    }
    
    void ExecuteRangedAttack(BossAttack attack)
    {
        if (player == null) return;
        
        // 원거리 투사체 발사 (나중에 구현)
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 투사체 생성 로직 (Weapon 시스템과 연동)
        // GameObject projectile = Instantiate(attack.projectilePrefab, transform.position, Quaternion.identity);
        // projectile.GetComponent<Projectile>().Init(direction, (int)attack.damage);
        
        if (animator != null)
            animator.SetTrigger("RangedAttack");
    }
    
    IEnumerator ExecuteChargeAttack(BossAttack attack)
    {
        if (player == null) yield break;
        
        Vector2 chargeDirection = (player.position - transform.position).normalized;
        float chargeSpeed = moveSpeed * 2f;
        float chargeDuration = 1f;
        
        // 돌진 시작
        if (animator != null)
            animator.SetTrigger("Charge");
        
        float chargeTimer = 0f;
        while (chargeTimer < chargeDuration)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        
        // 돌진 종료
        rb.linearVelocity = Vector2.zero;
    }
    
    void ExecuteSummonMinions(BossAttack attack)
    {
        if (activeMinions.Count >= maxMinions) return;
        
        int minionsToSpawn = Mathf.Min(2, maxMinions - activeMinions.Count);
        
        for (int i = 0; i < minionsToSpawn; i++)
        {
            if (minionPrefabs.Length > 0)
            {
                Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 3f;
                spawnPosition.z = 0;
                
                GameObject minion = Instantiate(minionPrefabs[Random.Range(0, minionPrefabs.Length)], 
                                              spawnPosition, Quaternion.identity);
                activeMinions.Add(minion);
            }
        }
        
        if (animator != null)
            animator.SetTrigger("Summon");
    }
    
    void ExecuteAreaAttack(BossAttack attack)
    {
        // 범위 내 모든 플레이어에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attack.range);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)attack.damage);
                }
            }
        }
        
        if (animator != null)
            animator.SetTrigger("AreaAttack");
    }
    
    void ExecuteSpecialAbility(BossAttack attack)
    {
        // 특수 능력 실행 (텔레포트, 실드 등)
        if (canTeleport && Time.time - lastTeleportTime > teleportCooldown)
        {
            StartCoroutine(Teleport());
        }
        else if (canShield && Time.time - lastShieldTime > shieldCooldown)
        {
            StartCoroutine(ActivateShield());
        }
        
        if (animator != null)
            animator.SetTrigger("SpecialAbility");
    }
    
    void UseSpecialAbilities()
    {
        // 텔레포트 사용
        if (canTeleport && Time.time - lastTeleportTime > teleportCooldown)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > teleportRange)
            {
                StartCoroutine(Teleport());
            }
        }
        
        // 실드 사용
        if (canShield && Time.time - lastShieldTime > shieldCooldown && health.GetHealthPercentage() < 0.3f)
        {
            StartCoroutine(ActivateShield());
        }
    }
    
    IEnumerator Teleport()
    {
        lastTeleportTime = Time.time;
        
        // 텔레포트 이펙트
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // 플레이어 근처로 텔레포트
        if (player != null)
        {
            Vector2 teleportDirection = Random.insideUnitCircle.normalized;
            Vector3 teleportPosition = player.position + (Vector3)(teleportDirection * 5f);
            transform.position = teleportPosition;
        }
        
        // 텔레포트 완료
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }
    
    IEnumerator ActivateShield()
    {
        lastShieldTime = Time.time;
        isShielded = true;
        
        // 실드 이펙트
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.cyan;
        }
        
        yield return new WaitForSeconds(shieldDuration);
        
        // 실드 해제
        isShielded = false;
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = originalColor;
        }
    }
    
    void MoveTowardsPlayer()
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 가속도를 사용한 부드러운 이동
        Vector2 targetVelocity = direction * moveSpeed;
        Vector2 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange = Vector2.ClampMagnitude(velocityChange, 5f * Time.deltaTime);
        rb.linearVelocity += velocityChange;
        
        // 스프라이트 방향 조정
        if (direction.x > 0 && !facingRight)
            Flip();
        else if (direction.x < 0 && facingRight)
            Flip();
    }
    
    void ManageMinions()
    {
        // 죽은 하수인 제거
        activeMinions.RemoveAll(minion => minion == null);
        
        // 하수인 소환 쿨다운 체크
        if (Time.time - lastMinionSpawnTime > minionSpawnCooldown && activeMinions.Count < maxMinions)
        {
            ExecuteSummonMinions(new BossAttack { attackType = BossAttackType.SummonMinions });
            lastMinionSpawnTime = Time.time;
        }
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            bool isMoving = rb.linearVelocity.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving && !isAttacking);
            animator.SetBool("isShielded", isShielded);
        }
    }
    
    void OnDamaged(int damage)
    {
        OnBossDamaged?.Invoke(damage);
        
        // 피격 이펙트
        if (animator != null)
            animator.SetTrigger("Hit");
        
        // 실드 상태면 데미지 무시
        if (isShielded)
        {
            return;
        }
        
        // 피격 시각 효과
        StartCoroutine(HitFlash());
    }
    
    void OnDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        // 보스 사망 이벤트
        OnBossDeath?.Invoke();
        
        // 보상 지급
        GiveRewards();
        
        // 하수인들 제거
        foreach (GameObject minion in activeMinions)
        {
            if (minion != null)
                Destroy(minion);
        }
        
        // 보스 헬스바 숨기기
        HideBossHealthBar();
        
        // 사망 사운드 재생
        if (bossDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(bossDeathSound, transform.position);
        }
        
        // 사망 애니메이션
        if (animator != null)
            animator.SetTrigger("Death");
        
        // 지연 후 제거
        StartCoroutine(DelayedDestroy());
    }
    
    void GiveRewards()
    {
        // 새로운 ItemDropManager 사용
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.DropItemsFromEnemy(bossName, transform.position);
        }
        else
        {
            // 기존 시스템 (백업)
            // 확정 드롭
            foreach (GameObject drop in guaranteedDrops)
            {
                if (drop != null)
                {
                    Vector3 dropPosition = transform.position + Random.insideUnitSphere * 2f;
                    dropPosition.z = 0;
                    Instantiate(drop, dropPosition, Quaternion.identity);
                }
            }
            
            // 희귀 드롭
            if (Random.value <= rareDropChance)
            {
                if (rareDrops.Length > 0)
                {
                    GameObject rareDrop = rareDrops[Random.Range(0, rareDrops.Length)];
                    if (rareDrop != null)
                    {
                        Vector3 dropPosition = transform.position + Random.insideUnitSphere * 2f;
                        dropPosition.z = 0;
                        Instantiate(rareDrop, dropPosition, Quaternion.identity);
                    }
                }
            }
        }
    }
    
    void ShowBossHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(true);
        }
    }
    
    void HideBossHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(false);
        }
    }
    
    IEnumerator BossEntranceEffect()
    {
        // 보스 등장 이펙트
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
    
    IEnumerator PhaseTransitionEffect()
    {
        // 페이즈 전환 이펙트
        if (phaseTransitionEffect != null)
        {
            Instantiate(phaseTransitionEffect, transform.position, Quaternion.identity);
        }
        
        if (phaseTransitionSound != null)
        {
            AudioSource.PlayClipAtPoint(phaseTransitionSound, transform.position);
        }
        
        // 화면 흔들림 효과
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
    
    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(3f); // 사망 애니메이션 대기
        Destroy(gameObject);
    }
    
    float GetCurrentAttackCooldown()
    {
        // 페이즈에 따른 공격 쿨다운 조정
        float baseCooldown = 2f;
        
        switch (currentPhase)
        {
            case BossPhase.Phase2:
                return baseCooldown * 0.8f;
            case BossPhase.Phase3:
                return baseCooldown * 0.6f;
            default:
                return baseCooldown;
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
            spriteRenderer.flipX = !facingRight;
    }
    
    public bool IsAlive()
    {
        return !isDead && health != null && health.IsAlive();
    }
    
    public BossPhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public float GetHealthPercentage()
    {
        return health != null ? health.GetHealthPercentage() : 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        // 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 텔레포트 범위
        if (canTeleport)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, teleportRange);
        }
        
        // 플레이어까지의 선
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
} 