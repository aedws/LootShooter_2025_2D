using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    private SpriteRenderer innerRenderer; // 안쪽 폭발
    private SpriteRenderer outerRenderer; // 외곽 화염
    private Color explosionColor;
    private float explosionSize;
    
    // 폭발 이펙트 지속시간 (공통 적용)
    private const float EXPLOSION_DURATION = 0.4f;
    
    void Awake()
    {
        // 안쪽 폭발 렌더러
        innerRenderer = GetComponent<SpriteRenderer>();
        
        // 외곽 화염 렌더러 생성
        GameObject outerFlame = new GameObject("OuterFlame");
        outerFlame.transform.SetParent(transform);
        outerFlame.transform.localPosition = Vector3.zero;
        outerFlame.transform.localScale = Vector3.one;
        
        outerRenderer = outerFlame.AddComponent<SpriteRenderer>();
        outerRenderer.sprite = innerRenderer.sprite; // 같은 스프라이트 사용
        outerRenderer.sortingOrder = innerRenderer.sortingOrder - 1; // 안쪽보다 뒤에
        outerRenderer.sortingLayerName = innerRenderer.sortingLayerName;
    }
    
    public void StartAnimation(Color color, float duration, float size = 3.5f)
    {
        explosionColor = color;
        explosionSize = size;
        
        Debug.Log($"💥 [EXPLOSION_EFFECT] 이중 레이어 애니메이션 시작: 색상={color}, 지속시간={EXPLOSION_DURATION}, 크기={size}");
        
        // 즉시 코루틴 시작
        StartCoroutine(DualLayerExplosionAnimation());
    }
    
    private IEnumerator DualLayerExplosionAnimation()
    {
        Debug.Log($"💥 [EXPLOSION_EFFECT] 이중 레이어 애니메이션 코루틴 시작");
        
        // 초기 설정
        transform.localScale = Vector3.one * 0.1f;
        
        float timer = 0f;
        float expandTime = EXPLOSION_DURATION * 0.6f; // 60% 시간 동안 확장
        float fadeTime = EXPLOSION_DURATION * 0.4f;   // 40% 시간 동안 페이드아웃
        
        Debug.Log($"💥 [EXPLOSION_EFFECT] 확장 시간: {expandTime}, 페이드 시간: {fadeTime}");
        
        // 확장 단계
        while (timer < expandTime)
        {
            float progress = timer / expandTime;
            
            // 크기 확장 (WeaponData에서 가져온 크기 사용)
            float scale = Mathf.Lerp(0.1f, explosionSize, progress);
            transform.localScale = Vector3.one * scale;
            
            // 안쪽 폭발 효과 (밝은 중심부)
            float innerBrightness = Mathf.Lerp(0.5f, 2.5f, progress);
            float innerAlpha = Mathf.Lerp(0.3f, 1f, progress);
            
            Color innerColor = new Color(
                explosionColor.r * innerBrightness,
                explosionColor.g * innerBrightness,
                explosionColor.b * innerBrightness,
                innerAlpha
            );
            innerRenderer.color = innerColor;
            
            // 외곽 화염 효과 (어두운 외곽선)
            float outerBrightness = Mathf.Lerp(0.2f, 1.5f, progress);
            float outerAlpha = Mathf.Lerp(0.1f, 0.8f, progress);
            
            // 외곽은 더 어둡고 붉은 색상
            Color outerColor = new Color(
                explosionColor.r * outerBrightness * 1.2f,
                explosionColor.g * outerBrightness * 0.6f,
                explosionColor.b * outerBrightness * 0.3f,
                outerAlpha
            );
            outerRenderer.color = outerColor;
            
            // 외곽 화염은 약간 더 크게
            outerRenderer.transform.localScale = Vector3.one * (1f + progress * 0.3f);
            
            // 회전 효과
            float rotation = progress * 60f;
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            outerRenderer.transform.rotation = Quaternion.Euler(0, 0, -rotation * 0.5f); // 반대 방향
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log($"💥 [EXPLOSION_EFFECT] 확장 완료, 페이드아웃 시작");
        
        // 페이드아웃 단계
        timer = 0f;
        while (timer < fadeTime)
        {
            float progress = timer / fadeTime;
            float alpha = 1f - progress;
            
            // 안쪽 폭발 페이드아웃
            float innerBrightness = Mathf.Lerp(2.5f, 0.3f, progress);
            innerRenderer.color = new Color(
                explosionColor.r * innerBrightness,
                explosionColor.g * innerBrightness,
                explosionColor.b * innerBrightness,
                alpha
            );
            
            // 외곽 화염 페이드아웃 (더 빠르게)
            float outerBrightness = Mathf.Lerp(1.5f, 0.1f, progress);
            float outerAlpha = alpha * 0.7f; // 더 빠르게 사라짐
            outerRenderer.color = new Color(
                explosionColor.r * outerBrightness * 1.2f,
                explosionColor.g * outerBrightness * 0.6f,
                explosionColor.b * outerBrightness * 0.3f,
                outerAlpha
            );
            
            // 계속 회전
            float rotation = 60f + progress * 120f;
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            outerRenderer.transform.rotation = Quaternion.Euler(0, 0, -rotation * 0.5f);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log($"💥 [EXPLOSION_EFFECT] 이중 레이어 애니메이션 완료, 오브젝트 제거");
        
        // 확실히 제거
        Destroy(gameObject);
    }
} 