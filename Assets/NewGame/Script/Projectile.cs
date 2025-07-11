using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f;
    public int damage = 10;
    public float lifetime = 3f; // 투사체 수명
    
    [Header("특수 효과")]
    private bool hasTracerEffect = false;
    private bool isCriticalHit = false;
    private bool isExplosive = false;
    private float explosionRadius = 0f;
    private System.Action<Vector3, float> onExplosionCallback;
    
    [Header("관통 효과")]
    private int remainingPierces = 0;
    private float pierceDamageReduction = 0f;
    private int currentDamage;
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();
    
    [Header("폭발 시각 효과")]
    public GameObject explosionEffectPrefab; // 폭발 이펙트 프리팹
    public Color explosionColor = new Color(1f, 0.5f, 0f, 1f); // 폭발 색상 (주황색)
    public float explosionDuration = 0.5f; // 폭발 지속 시간
    
    private Vector2 moveDir;
    private bool isInitialized = false;
    
    // 시각적 효과용 컴포넌트들
    private TrailRenderer trailRenderer;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // 컴포넌트 캐싱
        trailRenderer = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 direction, int dmg, float projectileSpeed = -1f)
    {
        moveDir = direction.normalized;
        damage = dmg;
        currentDamage = dmg; // 관통용 현재 데미지
        
        // projectileSpeed가 지정되면 사용, 아니면 기본값 사용
        if (projectileSpeed > 0f)
        {
            speed = projectileSpeed;
        }
        
        isInitialized = true;
        
        // 투사체를 "Projectile" Layer로 설정
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        gameObject.layer = projectileLayer;
        
        // 콜라이더를 Trigger로 설정하여 충돌 감지 향상
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            // Debug.Log($"🔧 [PROJECTILE] 콜라이더를 Trigger로 설정했습니다.");
        }
        
        // Debug.Log($"[Projectile DEBUG] 투사체 초기화 완료: {damage} 데미지");
        
        // 투사체 회전 (발사 방향으로)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 수명 설정
        if (remainingPierces <= 0) // 관통 효과가 없으면 기본 수명 적용
        {
            Destroy(gameObject, lifetime);
        }
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // 투사체 이동
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
        
        // 예광탄 효과 업데이트
        UpdateTracerEffect();
    }

    // 관통 효과 설정
    public void SetPiercing(int pierceCount, float damageReduction)
    {
        remainingPierces = pierceCount;
        pierceDamageReduction = damageReduction;
        
        // 관통 투사체는 더 오래 살아있음
        lifetime *= 2f;
        // Debug.Log($"💎 [PROJECTILE] 관통 효과 설정: {pierceCount}회, 데미지 감소율: {damageReduction * 100}%");
    }

    // 폭발 효과 설정
    public void SetExplosive(float radius, System.Action<Vector3, float> explosionCallback)
    {
        isExplosive = true;
        explosionRadius = radius;
        onExplosionCallback = explosionCallback;
        // Debug.Log($"💥 [PROJECTILE] 폭발 효과 설정: 반경 {radius}");
    }

    // 예광탄 효과 설정
    public void SetTracer(bool hasTracer)
    {
        hasTracerEffect = hasTracer;
        ApplyTracerEffect();
        // Debug.Log($"✨ [PROJECTILE] 예광탄 효과: {hasTracer}");
    }

    // 크리티컬 효과 설정
    public void SetCritical(bool isCritical)
    {
        isCriticalHit = isCritical;
        ApplyCriticalEffect();
        // Debug.Log($"🎯 [PROJECTILE] 크리티컬 효과: {isCritical}");
    }

    private void ApplyTracerEffect()
    {
        if (!hasTracerEffect) return;
        
        // Trail Renderer 설정
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.time = 0.3f;
            trailRenderer.startWidth = 0.1f;
            trailRenderer.endWidth = 0.05f;
            trailRenderer.material = null; // 기본 재질 사용
            
            // 예광탄 색상 (밝은 황색)
            Color tracerColor = new Color(1f, 1f, 0.5f, 0.8f);
            // trailRenderer.startColor = tracerColor;
            // trailRenderer.endColor = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 0f);
        }
        
        // 투사체 색상 변경
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 0.7f, 1f);
        }
    }

    private void ApplyCriticalEffect()
    {
        if (!isCriticalHit) return;
        
        // 크리티컬 시각 효과 (빨간색 강조)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
        
        // 크리티컬은 약간 더 크게
        transform.localScale = Vector3.one * 1.2f;
    }

    private void UpdateTracerEffect()
    {
        // 예광탄 깜박임 효과
        if (hasTracerEffect && spriteRenderer != null)
        {
            float flicker = Mathf.Sin(Time.time * 20f) * 0.2f + 0.8f;
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, flicker);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"[Projectile DEBUG] OnTriggerEnter2D 호출됨 - 대상: {other.name}, Layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");
        
        // Player Layer와의 충돌 무시 (가장 먼저 체크)
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Debug.Log("[Projectile DEBUG] Player Layer 무시");
            return;
        }
            
        // Weapon Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            // Debug.Log("[Projectile DEBUG] Weapon Layer 무시");
            return;
        }
            
        // Projectile Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            // Debug.Log("[Projectile DEBUG] Projectile Layer 무시");
            return;
        }
            
        // 무기 픽업과의 충돌 무시
        if (other.GetComponent<WeaponPickup>() != null)
        {
            // Debug.Log("[Projectile DEBUG] WeaponPickup 무시");
            return;
        }

        // Debug.Log("[Projectile DEBUG] 충돌 처리 시작");
        // 충돌 처리 (적, 벽 등)
        bool shouldDestroy = HandleCollision(other);
        
        // 관통 효과가 없거나 파괴해야 하는 경우에만 파괴
        if (shouldDestroy)
        {
            // Debug.Log("[Projectile DEBUG] 투사체 파괴");
            DestroyProjectile();
        }
    }
    
    // OnCollisionEnter2D도 추가해서 일반 콜라이더와의 충돌도 처리
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log($"[Projectile DEBUG] OnCollisionEnter2D 호출됨 - 대상: {collision.gameObject.name}, Layer: {collision.gameObject.layer} ({LayerMask.LayerToName(collision.gameObject.layer)})");
        
        Collider2D other = collision.collider;
        
        // Player Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Debug.Log("[Projectile DEBUG] Player Layer 무시 (Collision)");
            return;
        }
            
        // Weapon Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            // Debug.Log("[Projectile DEBUG] Weapon Layer 무시 (Collision)");
            return;
        }
            
        // Projectile Layer와의 충돌 무시
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            // Debug.Log("[Projectile DEBUG] Projectile Layer 무시 (Collision)");
            return;
        }
            
        // 무기 픽업과의 충돌 무시
        if (other.GetComponent<WeaponPickup>() != null)
        {
            // Debug.Log("[Projectile DEBUG] WeaponPickup 무시 (Collision)");
            return;
        }

        // Debug.Log("[Projectile DEBUG] 충돌 처리 시작 (Collision)");
        // 충돌 처리 (적, 벽 등)
        bool shouldDestroy = HandleCollision(other);
        
        // 관통 효과가 없거나 파괴해야 하는 경우에만 파괴
        if (shouldDestroy)
        {
            // Debug.Log("[Projectile DEBUG] 투사체 파괴 (Collision)");
            DestroyProjectile();
        }
    }

    bool HandleCollision(Collider2D other)
    {
        // Ground 충돌 체크를 가장 먼저 하고 명확하게 로그 출력
        // Debug.Log($"🌍 [GROUND CHECK] 충돌 오브젝트: {other.name}, Layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");
        
        // Ground와의 충돌 처리
        bool isGroundCollision = false;
        
        // 1. Default Layer(0)와의 충돌 체크
        if (other.gameObject.layer == 0)
        {
            // Debug.Log("🟢 [GROUND CHECK] Default Layer(0) 감지!");
            isGroundCollision = true;
        }
        
        // 2. Ground Layer가 존재하는 경우 체크
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1 && other.gameObject.layer == groundLayer)
        {
            // Debug.Log("🟢 [GROUND CHECK] Ground Layer 감지!");
            isGroundCollision = true;
        }
        
        // 3. Ground 태그나 이름으로 추가 확인
        if (other.gameObject.name.ToLower().Contains("ground") || 
            other.gameObject.name.ToLower().Contains("wall") ||
            other.gameObject.name.ToLower().Contains("platform") ||
            other.gameObject.name.ToLower().Contains("tilemap") ||
            other.gameObject.name.ToLower().Contains("layer1") ||
            other.gameObject.name.ToLower().Contains("layer"))
        {
            // Debug.Log($"🟢 [GROUND CHECK] 이름 기반 Ground 감지: {other.gameObject.name}");
            isGroundCollision = true;
        }
        
        if (isGroundCollision)
        {
            // Debug.Log($"💥 [PROJECTILE] Ground/Wall과 충돌하여 투사체를 파괴합니다: {other.name}");
            return true; // 지형과 충돌하면 항상 파괴
        }
        
        // 적 피격 처리
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 이미 맞춘 적인지 확인 (관통용)
            if (hitTargets.Contains(other))
            {
                return false; // 이미 맞춘 적은 다시 맞추지 않음
            }
            
            // 크리티컬 히트 계산 (10% 확률)
            bool isCritical = Random.value < 0.1f;
            int finalDamage = isCritical ? Mathf.RoundToInt(currentDamage * 1.5f) : currentDamage;
            
            // 적 피격 처리
            enemy.TakeDamage(finalDamage);
            hitTargets.Add(other);
            
            // 크리티컬 히트 정보를 Enemy에 전달 (데미지 텍스트 표시용)
            if (isCritical)
            {
                // Enemy의 OnDamaged 이벤트에 크리티컬 정보 전달
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    // 크리티컬 히트 표시를 위해 별도 처리
                    ShowCriticalDamageText(enemy.transform.position, finalDamage);
                }
            }
            
            // Debug.Log($"⚔️ [PROJECTILE] 적 {enemy.enemyName}에게 {finalDamage} 데미지! {(isCritical ? "(크리티컬!)" : "")}");
            
            // 폭발 효과 처리 (적 사망 시)
            if (isExplosive && !enemy.IsAlive())
            {
                TriggerExplosion(transform.position);
            }
            
            // 관통 처리
            if (remainingPierces > 0)
            {
                remainingPierces--;
                
                // 관통할 때마다 데미지 감소
                currentDamage = Mathf.RoundToInt(currentDamage * (1f - pierceDamageReduction));
                
                // Debug.Log($"💎 [PROJECTILE] 관통! 남은 관통: {remainingPierces}, 현재 데미지: {currentDamage}");
                
                // 아직 관통 가능하면 계속 진행
                if (remainingPierces > 0 && currentDamage > 0)
                {
                    return false; // 파괴하지 않음
                }
            }
            
            return true; // 관통이 끝났거나 관통 효과가 없으면 파괴
        }
        
        // 기타 충돌 처리
        // Debug.Log($"❓ [PROJECTILE] 알 수 없는 충돌: {other.name}");
        return true; // 알 수 없는 충돌은 파괴
    }

    private void TriggerExplosion(Vector3 explosionCenter)
    {
        if (!isExplosive || explosionRadius <= 0f) return;
        
        // Debug.Log($"💥 [PROJECTILE] 폭발 발생! 위치: {explosionCenter}, 반경: {explosionRadius}");
        
        // 폭발 시각 효과 생성
        CreateExplosionVisualEffect(explosionCenter);
        
        // 폭발 콜백 호출
        onExplosionCallback?.Invoke(explosionCenter, explosionRadius);
        
        // 폭발 반경 내 적들에게 추가 데미지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius);
        
        foreach (var hitCollider in hitEnemies)
        {
            var enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !hitTargets.Contains(hitCollider))
            {
                int explosionDamage = Mathf.RoundToInt(currentDamage * 0.5f); // 폭발 데미지는 50%
                enemy.TakeDamage(explosionDamage);
                // Debug.Log($"💥 [EXPLOSION] 폭발 데미지 {explosionDamage}를 {enemy.enemyName}에게!");
            }
        }
    }
    
    private void CreateExplosionVisualEffect(Vector3 position)
    {
        // 프리팹이 있으면 프리팹 사용
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            Destroy(explosion, explosionDuration);
        }
        else
        {
            // 프리팹이 없으면 동적으로 생성
            CreateDynamicExplosionEffect(position);
        }
    }
    
    private void CreateDynamicExplosionEffect(Vector3 position)
    {
        // 폭발 게임오브젝트 생성
        GameObject explosion = new GameObject("ExplosionEffect");
        explosion.transform.position = position;
        
        // 스프라이트 렌더러 추가
        SpriteRenderer spriteRenderer = explosion.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateExplosionSprite();
        spriteRenderer.color = explosionColor;
        spriteRenderer.sortingOrder = 10; // 다른 오브젝트 위에 표시
        
        // 폭발 애니메이션 코루틴 시작
        StartCoroutine(ExplosionAnimation(explosion, spriteRenderer));
    }
    
    private Sprite CreateExplosionSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 0f;
                
                if (distance <= radius)
                {
                    // 중심에서 멀어질수록 투명해짐
                    alpha = 1f - (distance / radius);
                    
                    // 가장자리 부드럽게
                    if (distance > radius - 4f)
                    {
                        alpha = Mathf.Lerp(1f, 0f, (distance - (radius - 4f)) / 4f);
                    }
                    
                    // 폭발 패턴 (불규칙한 모양)
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    alpha *= noise;
                }
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private System.Collections.IEnumerator ExplosionAnimation(GameObject explosion, SpriteRenderer spriteRenderer)
    {
        Vector3 originalScale = explosion.transform.localScale;
        float timer = 0f;
        
        while (timer < explosionDuration)
        {
            float normalizedTime = timer / explosionDuration;
            
            // 스케일 효과 (점점 커짐)
            float scale = Mathf.Lerp(0.1f, explosionRadius * 2f, normalizedTime);
            explosion.transform.localScale = originalScale * scale;
            
            // 색상 변화 (점점 밝아졌다가 어두워짐)
            Color currentColor = Color.Lerp(explosionColor, Color.white, Mathf.Sin(normalizedTime * Mathf.PI));
            float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Destroy(explosion);
    }

    private void DestroyProjectile()
    {
        // 폭발 효과가 있고 아직 폭발하지 않았다면 폭발 시킴
        if (isExplosive)
        {
            TriggerExplosion(transform.position);
        }
        
        Destroy(gameObject);
    }
    
    private void ShowCriticalDamageText(Vector3 position, int damage)
    {
        // 크리티컬 데미지 텍스트 생성
        Vector3 textPosition = position + Vector3.up * 1f;
        DamageTextManager.ShowDamage(textPosition, damage, true, false, false);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 반경 시각화
        if (isExplosive && explosionRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
} 