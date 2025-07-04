using UnityEngine;
using System.Collections;

public class ButterflyBeam : MonoBehaviour
{
    [Header("빔 설정")]
    public float beamWidth = 2f;
    public float beamLength = 20f;
    public int damage = 25;
    public float damageInterval = 0.1f; // 데미지 주는 간격
    public float lifetime = 3f;
    
    [Header("시각 효과")]
    public Color beamColor = new Color(1f, 0.75f, 0.8f, 1f); // pink
    public float pulseSpeed = 2f;
    public bool hasParticles = true;
    public GameObject particleEffect;
    
    [Header("빔 타입")]
    public bool isWideBeam = false;
    public bool isLeftBeam = false;
    public bool isRightBeam = false;
    public float wideBeamWidth = 5f;
    
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D beamCollider;
    private float lastDamageTime;
    private float startTime;
    private Camera mainCamera;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        beamCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        
        // 빔을 "Projectile" Layer로 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        
        // 콜라이더를 Trigger로 설정
        if (beamCollider != null)
        {
            beamCollider.isTrigger = true;
        }
    }
    
    void Start()
    {
        startTime = Time.time;
        lastDamageTime = Time.time;
        
        // 빔 설정
        SetupBeam();
        
        // 시각 효과 적용
        ApplyVisualEffects();
        
        // 수명 설정
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // 빔 펄스 효과
        UpdatePulseEffect();
        
        // 데미지 처리
        if (Time.time - lastDamageTime >= damageInterval)
        {
            DealDamageToPlayer();
            lastDamageTime = Time.time;
        }
    }
    
    void SetupBeam()
    {
        if (spriteRenderer != null)
        {
            // 빔 크기 설정
            float width = isWideBeam ? wideBeamWidth : beamWidth;
            spriteRenderer.size = new Vector2(width, beamLength);
        }
        
        if (beamCollider != null)
        {
            // 콜라이더 크기 설정
            float width = isWideBeam ? wideBeamWidth : beamWidth;
            beamCollider.size = new Vector2(width, beamLength);
        }
        
        // 빔 방향 설정
        if (isLeftBeam)
        {
            // 좌측 빔: 우측으로 향함
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (isRightBeam)
        {
            // 우측 빔: 좌측으로 향함
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            // 하향 빔: 아래로 향함
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
    }
    
    void ApplyVisualEffects()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = beamColor;
        }
        
        // 파티클 효과
        if (hasParticles && particleEffect != null)
        {
            Instantiate(particleEffect, transform.position, transform.rotation, transform);
        }
    }
    
    void UpdatePulseEffect()
    {
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
            Color currentColor = beamColor;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, pulse);
        }
    }
    
    void DealDamageToPlayer()
    {
        // 빔 범위 내 플레이어 찾기
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, 
            new Vector2(beamWidth, beamLength), transform.rotation.eulerAngles.z);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
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
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Player Layer와의 충돌만 처리
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;
        
        // 지속 데미지
        if (Time.time - lastDamageTime >= damageInterval)
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            lastDamageTime = Time.time;
        }
    }
    
    // 빔 경고 표시 (사전 경고용)
    public static GameObject CreateBeamWarning(Vector3 position, bool isLeft, float warningTime = 1f)
    {
        GameObject warning = new GameObject("BeamWarning");
        warning.transform.position = position;
        
        // 경고 스프라이트 생성
        SpriteRenderer warningSprite = warning.AddComponent<SpriteRenderer>();
        warningSprite.sprite = CreateWarningSprite();
        warningSprite.color = new Color(1f, 0f, 0f, 0.5f);
        
        // 경고 크기 설정
        float width = 5f;
        float length = 20f;
        warningSprite.size = new Vector2(width, length);
        
        // 경고 방향 설정
        if (isLeft)
        {
            warning.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            warning.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        
        // 깜박임 효과
        MonoBehaviour warningMono = warning.AddComponent<MonoBehaviour>();
        warningMono.StartCoroutine(WarningBlink(warning, warningTime));
        
        return warning;
    }
    
    static Sprite CreateWarningSprite()
    {
        // 간단한 경고 스프라이트 생성
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.red;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    static IEnumerator WarningBlink(GameObject warning, float duration)
    {
        SpriteRenderer sprite = warning.GetComponent<SpriteRenderer>();
        float timer = 0f;
        
        while (timer < duration)
        {
            float alpha = Mathf.Sin(timer * 10f) * 0.5f + 0.5f;
            sprite.color = new Color(1f, 0f, 0f, alpha);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Destroy(warning);
    }
    
    void OnDrawGizmosSelected()
    {
        // 빔 범위 표시
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(beamWidth, beamLength, 1f));
    }
} 