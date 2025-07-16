using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour
{
    [Header("기본 설정")]
    public string enemyName = "Basic Enemy";
    public int damage = 10;
    public float moveSpeed = 3.5f;
    
    [Header("추적 설정")]
    public float detectionRange = 25f;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.8f;
    public float acceleration = 8f; // 가속도
    public float maxSpeed = 4f; // 최대 속도
    public float separationDistance = 2f; // 다른 몬스터와의 최소 거리
    
    [Header("경험치/보상")]
    public int expValue = 10;
    public GameObject[] dropItems; // 드롭할 아이템들
    public float dropChance = 0.3f;
    
    [Header("컴포넌트 참조")]
    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    
    [Header("상태 관리")]
    private float lastAttackTime = -1f;
    private bool isDead = false;
    private bool facingRight = true;
    
    [Header("디버그")]
    public bool showGizmos = true;
    public bool useGoogleSheetsData = true; // 구글 시트 데이터 사용 여부
    public bool debugMode = false; // 디버그 모드
    
    // 웨이브 이동 관련 변수
    private float targetX;
    private float fixedY = 5f;
    private bool leftToRight;
    private bool reached = false;
    public void SetMoveTarget(float x, bool leftToRight)
    {
        this.targetX = x;
        this.leftToRight = leftToRight;
        this.fixedY = 5f;
        reached = false;
    }
    
    void Awake()
    {
        // 컴포넌트 찾기
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
        // 구글 시트 데이터 적용
        if (useGoogleSheetsData)
        {
            ApplyGoogleSheetsData();
        }
        
        // Health 이벤트 연결
        if (health != null)
        {
            health.OnDeath += OnDeath;
            health.OnDamaged += OnDamaged;
            // Debug.Log($"[Enemy DEBUG] Health 이벤트 연결 완료: {enemyName}");
        }
        else
        {
            // Debug.LogError($"[Enemy DEBUG] ❌ Health 컴포넌트가 없습니다: {enemyName}");
        }
        
        // 기본 레이어 설정
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        gameObject.layer = enemyLayer;
        
        // 물리 설정 (몬스터끼리 밀어내기)
        if (rb != null)
        {
            rb.linearDamping = 3f; // 공기 저항 증가로 더 안정적인 움직임
            rb.angularDamping = 10f; // 회전 저항 증가
        }
        
        // Physics 설정 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            // Debug.LogWarning($"[Enemy] {enemyName}의 콜라이더가 Trigger로 설정되어 있습니다. 물리적 분리를 위해 일반 콜라이더로 설정하는 것을 권장합니다.");
        }
        
        // Debug.Log($"[Enemy DEBUG] {enemyName} 초기화 완료");
    }
    
    /// <summary>
    /// 구글 시트에서 몬스터 정보를 로드하고 적용합니다.
    /// </summary>
    private void ApplyGoogleSheetsData()
    {
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 구글 시트 데이터 적용 시작");
        
        // ItemDropManager를 통해 드랍 테이블 데이터 가져오기
        if (ItemDropManager.Instance != null && ItemDropManager.Instance.IsDropTableLoaded())
        {
            if (debugMode)
                Debug.Log($"[Enemy] {enemyName} ItemDropManager에서 몬스터 정보 검색 중...");
            
            // DropTableData에 GetMonsterInfo 메서드가 있다고 가정
            // 실제로는 ItemDropManager에서 몬스터 정보를 가져오는 메서드를 추가해야 함
            MonsterInfo monsterInfo = GetMonsterInfoFromDropManager(enemyName);
            
            if (monsterInfo != null)
            {
                if (debugMode)
                    Debug.Log($"[Enemy] {enemyName} 몬스터 정보 찾음 - 체력: {monsterInfo.MaxHealth}, 공격력: {monsterInfo.Damage}");
                
                // 몬스터 스탯 적용
                ApplyMonsterStats(monsterInfo);
                
                if (debugMode)
                    Debug.Log($"[Enemy] {enemyName} 구글 시트 데이터 적용 완료");
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[Enemy] {enemyName}의 구글 시트 데이터를 찾을 수 없습니다. (enemyName: {enemyName})");
                    
                    // 사용 가능한 몬스터 ID 목록 출력
                    var allMonsterInfos = ItemDropManager.Instance.GetAllMonsterInfos();
                    Debug.Log($"[Enemy] 사용 가능한 몬스터 ID 목록: {string.Join(", ", allMonsterInfos.Select(m => m.MonsterID))}");
                }
            }
        }
        else
        {
            if (debugMode)
                Debug.LogWarning($"[Enemy] {enemyName} 구글 시트 데이터가 로드되지 않았습니다. 기본값을 사용합니다.");
        }
    }
    
    /// <summary>
    /// ItemDropManager에서 몬스터 정보를 가져옵니다.
    /// </summary>
    private MonsterInfo GetMonsterInfoFromDropManager(string monsterID)
    {
        if (ItemDropManager.Instance != null)
        {
            return ItemDropManager.Instance.GetMonsterInfo(monsterID);
        }
        return null;
    }
    
    /// <summary>
    /// 몬스터 스탯을 적용합니다.
    /// </summary>
    private void ApplyMonsterStats(MonsterInfo monsterInfo)
    {
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 스탯 적용 시작 - 체력: {monsterInfo.MaxHealth}");
        
        // 기본 스탯 적용
        if (health != null)
        {
            int oldHealth = health.maxHealth;
            health.SetMaxHealth(monsterInfo.MaxHealth);
            if (debugMode)
                Debug.Log($"[Enemy] {enemyName} 체력 변경: {oldHealth} → {health.maxHealth}");
        }
        else
        {
            if (debugMode)
                Debug.LogWarning($"[Enemy] {enemyName} Health 컴포넌트가 없습니다!");
        }
        
        // 전투 스탯 적용
        int oldDamage = damage;
        damage = monsterInfo.Damage;
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 공격력 변경: {oldDamage} → {damage}");
        
        float oldMoveSpeed = moveSpeed;
        moveSpeed = monsterInfo.MoveSpeed;
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 이동속도 변경: {oldMoveSpeed} → {moveSpeed}");
        
        attackRange = monsterInfo.AttackRange;
        attackCooldown = monsterInfo.AttackCooldown;
        detectionRange = monsterInfo.DetectionRange;
        
        // AI 스탯 적용
        acceleration = monsterInfo.Acceleration;
        maxSpeed = monsterInfo.MaxSpeed;
        separationDistance = monsterInfo.SeparationDistance;
        
        // 경험치 보상 적용
        int oldExpValue = expValue;
        expValue = monsterInfo.ExpReward;
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 경험치 보상 변경: {oldExpValue} → {expValue}");
        
        if (debugMode)
            Debug.Log($"[Enemy] {enemyName} 스탯 적용 완료");
    }
    
    // Layer Collision Matrix로 몬스터끼리 충돌 방지하므로 분리 코드 불필요
    
    void Update()
    {
        if (isDead) return;
        // 웨이브 이동 패턴: 목표 X까지 직선 이동, 도달 시 정지
        if (!reached && targetX != 0f)
        {
            float step = moveSpeed * Time.deltaTime;
            float nextX = Mathf.MoveTowards(transform.position.x, targetX, step);
            transform.position = new Vector3(nextX, fixedY, transform.position.z);
            if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            {
                reached = true;
            }
            return;
        }
        
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 탐지 범위 내에 있으면
        if (distanceToPlayer <= detectionRange)
        {
            // 공격 범위 내에 있으면 공격
            if (distanceToPlayer <= attackRange)
            {
                TryAttack();
            }
            else
            {
                // 플레이어 추적 - 더 적극적으로
                MoveTowardsPlayer();
            }
        }
        else
        {
            // 탐지 범위 밖이면 정지
            rb.linearVelocity = Vector2.zero;
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 플레이어가 움직이고 있으면 약간 앞쪽을 예측해서 이동
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.linearVelocity.magnitude > 0.5f)
        {
            // 플레이어의 이동 방향을 고려한 예측 위치
            Vector3 predictedPosition = player.position + (Vector3)playerRb.linearVelocity * 0.5f;
            direction = (predictedPosition - transform.position).normalized;
        }
        
        // Layer Collision Matrix로 몬스터끼리 충돌하지 않으므로 회피 불필요
        
        // 가속도를 사용한 부드러운 이동
        Vector2 targetVelocity = direction * moveSpeed;
        Vector2 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange = Vector2.ClampMagnitude(velocityChange, acceleration * Time.deltaTime);
        rb.linearVelocity += velocityChange;
        
        // 최대 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        
        // 스프라이트 방향 조정
        if (direction.x > 0 && !facingRight)
            Flip();
        else if (direction.x < 0 && facingRight)
            Flip();
    }
    
    // Layer Collision Matrix 사용으로 회피 시스템 불필요
    
    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
        
        // 공격 중일 때는 이동 속도 줄이기 (완전 정지하지 않음)
        rb.linearVelocity *= 0.3f;
    }
    
    void Attack()
    {
        // Debug.Log($"[Enemy] {enemyName}이(가) 플레이어를 공격했습니다!");
        
        // 플레이어에게 데미지 주기
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
        
        // 공격 애니메이션 트리거
        if (animator != null)
            animator.SetTrigger("Attack");
    }
    
    void OnDamaged(int damage)
    {
        // Debug.Log($"[Enemy DEBUG] {enemyName} 피격! 데미지: {damage}");
        
        // 피격 이펙트
        if (animator != null)
            animator.SetTrigger("Hit");
            
        // 시각적 피격 피드백 - 색깔 변화
        StartCoroutine(HitFlash());
        
        // 데미지 텍스트 표시
        ShowDamageText(damage);
        
        // 넉백 제거 - 몬스터가 너무 쉽게 밀려나지 않도록 함
        // (필요시 매우 약한 넉백을 적용할 수 있음)
        /*
        if (player != null && rb != null)
        {
            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            rb.AddForce(knockbackDirection * 0.5f, ForceMode2D.Impulse);
        }
        */
    }
    
    void ShowDamageText(int damage)
    {
        // 데미지 텍스트 생성 (몬스터 스프라이트 위쪽)
        Vector3 textPosition = transform.position + Vector3.up * 1f;
        
        // 크리티컬 히트 판정 (데미지가 기본 데미지의 1.5배 이상이면 크리티컬로 간주)
        bool isCritical = damage >= GetComponent<Health>()?.maxHealth * 0.15f;
        
        DamageTextManager.ShowDamage(textPosition, damage, isCritical, false, false);
    }
    
    void OnDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        // Debug.Log($"[Enemy] {enemyName}이(가) 죽었습니다!");
        
        // 경험치 지급 (나중에 GameManager에서 처리)
        // GameManager.Instance?.AddExperience(expValue);
        
        // 아이템 드롭
        DropItems();
        
        // 콜라이더 비활성화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        
        // 즉시 제거 (애니메이션 없이)
        Destroy(gameObject);
    }
    
    void DropItems()
    {
        // 새로운 ItemDropManager 사용
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.DropItemsFromEnemy(enemyName, transform.position);
        }
        else
        {
            // 기존 시스템 (백업)
        if (dropItems.Length > 0 && Random.value <= dropChance)
        {
            GameObject itemToDrop = dropItems[Random.Range(0, dropItems.Length)];
            if (itemToDrop != null)
            {
                Vector3 dropPosition = transform.position + Random.insideUnitSphere * 0.5f;
                dropPosition.z = 0;
                Instantiate(itemToDrop, dropPosition, Quaternion.identity);
                }
            }
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
            spriteRenderer.flipX = !facingRight;
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            // 이동 애니메이션
            bool isMoving = rb.linearVelocity.magnitude > 0.1f;
            animator.SetBool("isRunning", isMoving && !isDead);
        }
    }
    
    // 외부에서 데미지를 받는 함수 (투사체 등에서 호출)
    public void TakeDamage(int damage)
    {
        // Debug.Log($"[Enemy DEBUG] {enemyName}.TakeDamage({damage}) 호출됨");
        
        if (health != null && !isDead)
        {
            // Debug.Log($"[Enemy DEBUG] Health 컴포넌트 존재, 죽지 않음. 현재 체력: {health.currentHealth}");
            health.TakeDamage(damage);
        }
        else
        {
            // Debug.Log($"[Enemy DEBUG] ❌ Health가 null이거나 이미 죽음. Health: {health != null}, isDead: {isDead}");
        }
    }
    
    // 시각적 피격 효과
    System.Collections.IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red; // 빨간색으로 변경
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor; // 원래 색으로 복구
        }
    }
    
    public bool IsAlive()
    {
        return !isDead && health != null && health.IsAlive();
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 플레이어까지의 선
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                // 넉백 효과(옵션)
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    float dir = (playerRb.transform.position.x < transform.position.x) ? -1f : 1f;
                    playerRb.AddForce(new Vector2(dir * 10f, 5f), ForceMode2D.Impulse);
                }
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    float dir = (playerRb.transform.position.x < transform.position.x) ? -1f : 1f;
                    playerRb.AddForce(new Vector2(dir * 10f, 5f), ForceMode2D.Impulse);
                }
            }
        }
    }
} 