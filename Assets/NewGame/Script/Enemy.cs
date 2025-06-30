using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("기본 설정")]
    public string enemyName = "Basic Enemy";
    public int damage = 10;
    public float moveSpeed = 2f;
    
    [Header("추적 설정")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    
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
        // Health 이벤트 연결
        if (health != null)
        {
            health.OnDeath += OnDeath;
            health.OnDamaged += OnDamaged;
        }
        
        // 기본 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
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
                // 플레이어 추적
                MoveTowardsPlayer();
            }
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        
        // 스프라이트 방향 조정
        if (direction.x > 0 && !facingRight)
            Flip();
        else if (direction.x < 0 && facingRight)
            Flip();
    }
    
    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
        
        // 공격 중일 때는 이동 멈춤
        rb.linearVelocity = Vector2.zero;
    }
    
    void Attack()
    {
        Debug.Log($"[Enemy] {enemyName}이(가) 플레이어를 공격했습니다!");
        
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
        // 피격 이펙트
        if (animator != null)
            animator.SetTrigger("Hit");
            
        // 피격 시 잠시 밀려나기
        if (player != null && rb != null)
        {
            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            rb.AddForce(knockbackDirection * 3f, ForceMode2D.Impulse);
        }
    }
    
    void OnDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        Debug.Log($"[Enemy] {enemyName}이(가) 죽었습니다!");
        
        // 죽음 애니메이션
        if (animator != null)
            animator.SetBool("Dead", true);
        
        // 경험치 지급 (나중에 GameManager에서 처리)
        // GameManager.Instance?.AddExperience(expValue);
        
        // 아이템 드롭
        DropItems();
        
        // 콜라이더 비활성화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        
        // 일정 시간 후 제거
        Destroy(gameObject, 2f);
    }
    
    void DropItems()
    {
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
        if (health != null && !isDead)
        {
            health.TakeDamage(damage);
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
} 