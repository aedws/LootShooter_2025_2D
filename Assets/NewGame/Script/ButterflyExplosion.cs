using UnityEngine;
using System.Collections;

public class ButterflyExplosion : MonoBehaviour
{
    [Header("폭발 설정")]
    public float explosionRadius = 3f;
    public int explosionDamage = 30;
    public float explosionForce = 10f;
    public float explosionDuration = 1f;
    
    [Header("시각 효과")]
    public Color explosionColor = new Color(1f, 0.5f, 0f, 1f); // orange
    public float maxScale = 3f;
    public float scaleSpeed = 5f;
    public bool useParticles = true;
    public GameObject particleEffect;
    
    [Header("오디오")]
    public AudioClip explosionSound;
    public float volume = 0.8f;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D explosionCollider;
    private AudioSource audioSource;
    private float startTime;
    private Vector3 originalScale;
    private bool hasExploded = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        explosionCollider = GetComponent<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // 폭발을 "Projectile" Layer로 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        
        // 콜라이더를 Trigger로 설정
        if (explosionCollider != null)
        {
            explosionCollider.isTrigger = true;
            explosionCollider.radius = explosionRadius;
        }
        
        originalScale = transform.localScale;
    }
    
    void Start()
    {
        startTime = Time.time;
        
        // 초기 설정
        SetupExplosion();
        
        // 폭발 효과 시작
        StartCoroutine(ExplosionSequence());
        
        // 오디오 재생
        PlayExplosionSound();
    }
    
    void SetupExplosion()
    {
        if (spriteRenderer != null)
        {
            // 폭발 스프라이트 생성 (없는 경우)
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateExplosionSprite();
            }
            
            // 초기 색상 설정
            spriteRenderer.color = explosionColor;
        }
        
        // 파티클 효과
        if (useParticles && particleEffect != null)
        {
            Instantiate(particleEffect, transform.position, Quaternion.identity);
        }
    }
    
    IEnumerator ExplosionSequence()
    {
        // 폭발 데미지 처리
        DealExplosionDamage();
        
        // 폭발 시각 효과
        float timer = 0f;
        while (timer < explosionDuration)
        {
            float normalizedTime = timer / explosionDuration;
            
            // 스케일 효과
            float scale = Mathf.Lerp(0.1f, maxScale, normalizedTime);
            transform.localScale = originalScale * scale;
            
            // 색상 변화
            if (spriteRenderer != null)
            {
                Color currentColor = Color.Lerp(explosionColor, Color.white, normalizedTime);
                float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 폭발 완료 후 제거
        Destroy(gameObject);
    }
    
    void DealExplosionDamage()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // 범위 내 모든 플레이어에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage);
                }
                
                // 물리적 밀어내기 (Rigidbody2D가 있는 경우)
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 direction = (hit.transform.position - transform.position).normalized;
                    playerRb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    
    void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            if (audioSource != null)
            {
                audioSource.clip = explosionSound;
                audioSource.volume = volume;
                audioSource.Play();
            }
            else
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);
            }
        }
    }
    
    // 정적 메서드 - 다른 스크립트에서 호출 가능
    public static GameObject CreateExplosion(Vector3 position, float radius = 3f, int damage = 30)
    {
        GameObject explosion = new GameObject("ButterflyExplosion");
        explosion.transform.position = position;
        
        ButterflyExplosion explosionComponent = explosion.AddComponent<ButterflyExplosion>();
        explosionComponent.explosionRadius = radius;
        explosionComponent.explosionDamage = damage;
        
        // 필요한 컴포넌트들 추가
        explosion.AddComponent<SpriteRenderer>();
        explosion.AddComponent<CircleCollider2D>();
        explosion.AddComponent<AudioSource>();
        
        return explosion;
    }
    
    // 폭발 스프라이트 생성
    Sprite CreateExplosionSprite()
    {
        int size = 128;
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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player Layer와의 충돌만 처리
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;
        
        // 플레이어에게 데미지
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(explosionDamage);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 폭발 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 