using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f;
    public int damage = 10;
    public float lifetime = 3f; // 투사체 수명
    
    private Vector2 moveDir;
    private bool isInitialized = false;

    public void Init(Vector2 direction, int dmg, float projectileSpeed = -1f)
    {
        moveDir = direction.normalized;
        damage = dmg;
        
        // projectileSpeed가 지정되면 사용, 아니면 기본값 사용
        if (projectileSpeed > 0f)
        {
            speed = projectileSpeed;
        }
        
        isInitialized = true;
        
        // 투사체를 "Projectile" Layer로 설정
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        gameObject.layer = projectileLayer;
        
        // Debug.Log($"[Projectile DEBUG] 투사체 초기화 완료: {damage} 데미지");
        
        // 투사체 회전 (발사 방향으로)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 수명 설정
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // 투사체 이동
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Projectile DEBUG] OnTriggerEnter2D 호출됨 - 대상: {other.name}, Layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");
        
        // Player Layer와의 충돌 무시 (가장 먼저 체크)
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("[Projectile DEBUG] Player Layer 무시");
            return;
        }
            
        // Weapon Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            Debug.Log("[Projectile DEBUG] Weapon Layer 무시");
            return;
        }
            
        // Projectile Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Debug.Log("[Projectile DEBUG] Projectile Layer 무시");
            return;
        }
            
        // 무기 픽업과의 충돌 무시
        if (other.GetComponent<WeaponPickup>() != null)
        {
            Debug.Log("[Projectile DEBUG] WeaponPickup 무시");
            return;
        }

        Debug.Log("[Projectile DEBUG] 충돌 처리 시작");
        // 충돌 처리 (적, 벽 등)
        HandleCollision(other);
        
        // 투사체 파괴
        Debug.Log("[Projectile DEBUG] 투사체 파괴");
        Destroy(gameObject);
    }
    
    // OnCollisionEnter2D도 추가해서 일반 콜라이더와의 충돌도 처리
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Projectile DEBUG] OnCollisionEnter2D 호출됨 - 대상: {collision.gameObject.name}, Layer: {collision.gameObject.layer} ({LayerMask.LayerToName(collision.gameObject.layer)})");
        
        Collider2D other = collision.collider;
        
        // Player Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("[Projectile DEBUG] Player Layer 무시 (Collision)");
            return;
        }
            
        // Weapon Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            Debug.Log("[Projectile DEBUG] Weapon Layer 무시 (Collision)");
            return;
        }
            
        // Projectile Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Debug.Log("[Projectile DEBUG] Projectile Layer 무시 (Collision)");
            return;
        }
            
        // 무기 픽업과의 충돌 무시
        if (other.GetComponent<WeaponPickup>() != null)
        {
            Debug.Log("[Projectile DEBUG] WeaponPickup 무시 (Collision)");
            return;
        }

        Debug.Log("[Projectile DEBUG] 충돌 처리 시작 (Collision)");
        // 충돌 처리 (적, 벽 등)
        HandleCollision(other);
        
        // 투사체 파괴
        Debug.Log("[Projectile DEBUG] 투사체 파괴 (Collision)");
        Destroy(gameObject);
    }

    void HandleCollision(Collider2D other)
    {
        Debug.Log($"[Projectile DEBUG] 충돌 감지: {other.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // 적 피격 처리
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"[Projectile DEBUG] Enemy 컴포넌트 발견: {enemy.enemyName}");
            enemy.TakeDamage(damage);
            Debug.Log($"[Projectile] ✅ 적 {enemy.enemyName}에게 {damage} 데미지를 입혔습니다!");
            return;
        }
        else
        {
            Debug.Log($"[Projectile DEBUG] ❌ Enemy 컴포넌트가 없습니다.");
        }
        
        // 기타 충돌 처리 (벽, 장애물 등)
        Debug.Log($"[Projectile] {other.name}과 충돌했습니다.");
    }
} 