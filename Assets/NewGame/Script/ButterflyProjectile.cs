using UnityEngine;

public class ButterflyProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f;
    public int damage = 15;
    public float lifetime = 5f;
    public bool isGuided = false;
    public float guidedStrength = 2f;
    
    [Header("시각 효과")]
    public Color projectileColor = Color.magenta;
    public float rotationSpeed = 180f;
    public bool hasTrail = true;
    
    [Header("특수 효과")]
    public bool isExplosive = false;
    public float explosionRadius = 2f;
    public GameObject explosionEffect;
    
    private Vector2 moveDirection;
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private bool isInitialized = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        
        // 투사체를 "Projectile" Layer로 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        
        // 콜라이더를 Trigger로 설정
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    public void Init(Vector2 direction, int dmg = -1, float projectileSpeed = -1f, bool guided = false)
    {
        moveDirection = direction.normalized;
        isGuided = guided;
        
        if (dmg > 0) damage = dmg;
        if (projectileSpeed > 0f) speed = projectileSpeed;
        
        // 플레이어 찾기 (유도용)
        if (isGuided)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        isInitialized = true;
        
        // 투사체 회전 (발사 방향으로)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 시각 효과 적용
        ApplyVisualEffects();
        
        // 수명 설정
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // 유도 처리
        if (isGuided && target != null)
        {
            Vector2 targetDirection = (target.position - transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, targetDirection, guidedStrength * Time.deltaTime).normalized;
            
            // 회전 업데이트
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // 투사체 이동
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
        
        // 회전 효과
        if (rotationSpeed != 0)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
    
    void ApplyVisualEffects()
    {
        // 색상 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.color = projectileColor;
        }
        
        // 트레일 효과
        if (trailRenderer != null)
        {
            trailRenderer.enabled = hasTrail;
            if (hasTrail)
            {
                trailRenderer.startColor = projectileColor;
                trailRenderer.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player Layer와의 충돌만 처리
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;
        
        // 플레이어에게 데미지
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        
        // 폭발 효과
        if (isExplosive)
        {
            CreateExplosion();
        }
        
        // 투사체 제거
        DestroyProjectile();
    }
    
    void CreateExplosion()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // 범위 내 모든 플레이어에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage / 2); // 폭발 데미지는 절반
                }
            }
        }
    }
    
    void DestroyProjectile()
    {
        // 사망 이펙트 (선택사항)
        // GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        // Destroy(deathEffect, 1f);
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (isExplosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
} 