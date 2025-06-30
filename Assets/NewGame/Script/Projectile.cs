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
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        
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
        // Player Layer와의 충돌 무시 (가장 먼저 체크)
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;
            
        // Weapon Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
            return;
            
        // Projectile Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
            return;
            
        // 무기 픽업과의 충돌 무시
        if (other.GetComponent<WeaponPickup>() != null)
            return;

        // 충돌 처리 (적, 벽 등)
        HandleCollision(other);
        
        // 투사체 파괴
        Destroy(gameObject);
    }

    void HandleCollision(Collider2D other)
    {
        // 여기에 적 피격 처리 등을 추가할 수 있습니다
        
        // 예시: 적 컴포넌트가 있다면 데미지 적용
        // var enemy = other.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     enemy.TakeDamage(damage);
        // }
    }
} 