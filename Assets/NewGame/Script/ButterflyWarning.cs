using UnityEngine;
using System.Collections;

public class ButterflyWarning : MonoBehaviour
{
    [Header("경고 설정")]
    public float warningDuration = 1f;
    public float blinkSpeed = 5f;
    public Color warningColor = Color.red;
    public float warningAlpha = 0.7f;
    
    [Header("경고 타입")]
    public bool isExplosionWarning = true;
    public bool isBeamWarning = false;
    public bool isChargeWarning = false;
    
    [Header("시각 효과")]
    public float scaleSpeed = 2f;
    public bool usePulse = true;
    public float pulseSpeed = 3f;
    
    private SpriteRenderer spriteRenderer;
    private float startTime;
    private Vector3 originalScale;
    private bool isDestroying = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // 경고를 "UI" Layer로 설정 (플레이어와 충돌하지 않도록)
        gameObject.layer = LayerMask.NameToLayer("UI");
    }
    
    void Start()
    {
        startTime = Time.time;
        
        // 초기 설정
        SetupWarning();
        
        // 자동 제거
        StartCoroutine(DestroyAfterDuration());
    }
    
    void Update()
    {
        if (isDestroying) return;
        
        // 깜박임 효과
        UpdateBlinkEffect();
        
        // 펄스 효과
        if (usePulse)
        {
            UpdatePulseEffect();
        }
        
        // 스케일 효과
        UpdateScaleEffect();
    }
    
    void SetupWarning()
    {
        if (spriteRenderer != null)
        {
            // 경고 색상 설정
            spriteRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, warningAlpha);
            
            // 경고 스프라이트 생성 (없는 경우)
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateWarningSprite();
            }
        }
        
        // 경고 타입별 설정
        if (isExplosionWarning)
        {
            SetupExplosionWarning();
        }
        else if (isBeamWarning)
        {
            SetupBeamWarning();
        }
        else if (isChargeWarning)
        {
            SetupChargeWarning();
        }
    }
    
    void SetupExplosionWarning()
    {
        // 폭발 경고: 원형
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = CreateCircleWarningSprite();
        }
        
        // 원형 콜라이더 추가 (선택사항)
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 1f;
    }
    
    void SetupBeamWarning()
    {
        // 빔 경고: 직사각형
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = CreateBeamWarningSprite();
            spriteRenderer.size = new Vector2(5f, 20f);
        }
        
        // 박스 콜라이더 추가 (선택사항)
        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(5f, 20f);
    }
    
    void SetupChargeWarning()
    {
        // 돌진 경고: 화살표 모양
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = CreateArrowWarningSprite();
        }
    }
    
    void UpdateBlinkEffect()
    {
        if (spriteRenderer != null)
        {
            float blink = Mathf.Sin((Time.time - startTime) * blinkSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0.2f, warningAlpha, blink);
            spriteRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
        }
    }
    
    void UpdatePulseEffect()
    {
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * 0.3f + 0.7f;
            transform.localScale = originalScale * pulse;
        }
    }
    
    void UpdateScaleEffect()
    {
        float elapsed = Time.time - startTime;
        float normalizedTime = elapsed / warningDuration;
        
        if (isExplosionWarning)
        {
            // 폭발 경고: 점점 커짐
            float scale = Mathf.Lerp(0.5f, 1.5f, normalizedTime);
            transform.localScale = originalScale * scale;
        }
        else if (isBeamWarning)
        {
            // 빔 경고: 세로로 늘어남
            float scaleY = Mathf.Lerp(0.5f, 1.2f, normalizedTime);
            transform.localScale = new Vector3(originalScale.x, originalScale.y * scaleY, originalScale.z);
        }
    }
    
    IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(warningDuration);
        
        // 페이드 아웃
        isDestroying = true;
        float fadeTime = 0.3f;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(warningAlpha, 0f, timer / fadeTime);
                spriteRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // 정적 메서드들 - 다른 스크립트에서 호출 가능
    public static GameObject CreateExplosionWarning(Vector3 position, float duration = 1f)
    {
        GameObject warning = new GameObject("ExplosionWarning");
        warning.transform.position = position;
        
        ButterflyWarning warningComponent = warning.AddComponent<ButterflyWarning>();
        warningComponent.isExplosionWarning = true;
        warningComponent.isBeamWarning = false;
        warningComponent.isChargeWarning = false;
        warningComponent.warningDuration = duration;
        
        return warning;
    }
    
    public static GameObject CreateBeamWarning(Vector3 position, bool isLeft, float duration = 1f)
    {
        GameObject warning = new GameObject("BeamWarning");
        warning.transform.position = position;
        
        ButterflyWarning warningComponent = warning.AddComponent<ButterflyWarning>();
        warningComponent.isExplosionWarning = false;
        warningComponent.isBeamWarning = true;
        warningComponent.isChargeWarning = false;
        warningComponent.warningDuration = duration;
        
        // 빔 방향 설정
        if (isLeft)
        {
            warning.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            warning.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        
        return warning;
    }
    
    public static GameObject CreateChargeWarning(Vector3 position, Vector3 direction, float duration = 1f)
    {
        GameObject warning = new GameObject("ChargeWarning");
        warning.transform.position = position;
        
        ButterflyWarning warningComponent = warning.AddComponent<ButterflyWarning>();
        warningComponent.isExplosionWarning = false;
        warningComponent.isBeamWarning = false;
        warningComponent.isChargeWarning = true;
        warningComponent.warningDuration = duration;
        
        // 돌진 방향으로 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        warning.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        return warning;
    }
    
    // 경고 스프라이트 생성 메서드들
    Sprite CreateWarningSprite()
    {
        return CreateCircleWarningSprite();
    }
    
    static Sprite CreateCircleWarningSprite()
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
                float alpha = distance <= radius ? 1f : 0f;
                
                // 가장자리 부드럽게
                if (distance > radius - 2f)
                {
                    alpha = Mathf.Lerp(1f, 0f, (distance - (radius - 2f)) / 2f);
                }
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    static Sprite CreateBeamWarningSprite()
    {
        int width = 32;
        int height = 128;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float alpha = 1f;
                
                // 가장자리 부드럽게
                if (x < 2 || x > width - 3)
                {
                    alpha = Mathf.Lerp(1f, 0f, x < 2 ? x / 2f : (width - x - 1) / 2f);
                }
                
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    static Sprite CreateArrowWarningSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y) - center;
                float alpha = 0f;
                
                // 화살표 모양
                if (Mathf.Abs(pos.x) < 8f && pos.y > -20f && pos.y < 20f)
                {
                    alpha = 1f;
                }
                else if (Mathf.Abs(pos.x) < 16f && pos.y > 10f && pos.y < 30f)
                {
                    alpha = 1f;
                }
                
                // 가장자리 부드럽게
                if (alpha > 0f)
                {
                    float edgeDistance = Mathf.Min(
                        Mathf.Abs(pos.x - 8f),
                        Mathf.Abs(pos.x + 8f),
                        Mathf.Abs(pos.y - 20f),
                        Mathf.Abs(pos.y + 20f)
                    );
                    
                    if (edgeDistance < 2f)
                    {
                        alpha = Mathf.Lerp(1f, 0f, edgeDistance / 2f);
                    }
                }
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    void OnDrawGizmosSelected()
    {
        // 경고 범위 표시
        Gizmos.color = Color.red;
        if (isExplosionWarning)
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else if (isBeamWarning)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(5f, 20f, 1f));
        }
    }
} 