using System.Collections;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private DamageTextManager manager;
    private Vector3 startPosition;
    private float lifetime;
    private float currentTime;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }
    }
    
    public void Initialize(DamageTextManager damageTextManager)
    {
        manager = damageTextManager;
        
        // 폰트 설정
        if (manager.damageFont != null)
        {
            textMesh.font = manager.damageFont;
        }
        
        textMesh.fontSize = manager.fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.ForceMeshUpdate();
    }
    
    public void ShowDamage(int damage, Vector3 position, bool isCritical)
    {
        // 위치 설정 (스프라이트 위쪽에 랜덤 오프셋)
        float randomX = Random.Range(-0.5f, 0.5f);
        float randomY = Random.Range(0.5f, 1.0f);
        startPosition = position + new Vector3(randomX, randomY, 0);
        transform.position = startPosition;
        
        // 텍스트 설정
        textMesh.text = damage.ToString();
        
        // 색상 설정
        textMesh.color = isCritical ? manager.criticalDamageColor : manager.normalDamageColor;
        
        // 크리티컬이면 크기 증가
        if (isCritical)
        {
            textMesh.fontSize = manager.fontSize * 1.2f;
        }
        else
        {
            textMesh.fontSize = manager.fontSize;
        }
        
        // 활성화
        gameObject.SetActive(true);
        
        // 애니메이션 시작
        lifetime = manager.textLifetime;
        currentTime = 0f;
        
        StartCoroutine(AnimateText());
    }
    
    private IEnumerator AnimateText()
    {
        Vector3 originalPosition = transform.position;
        Color originalColor = textMesh.color;
        
        while (currentTime < lifetime)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / lifetime;
            
            // 위로 이동
            transform.position = originalPosition + Vector3.up * manager.moveSpeed * progress;
            
            // 페이드 아웃
            Color newColor = originalColor;
            newColor.a = 1f - progress;
            textMesh.color = newColor;
            
            yield return null;
        }
        
        // 풀로 반환
        manager.ReturnToPool(gameObject);
    }
} 