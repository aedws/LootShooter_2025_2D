using UnityEngine;

public class DamageTextTest : MonoBehaviour
{
    [Header("테스트 설정")]
    public KeyCode testKey = KeyCode.T;
    public KeyCode criticalKey = KeyCode.Y;
    public KeyCode healKey = KeyCode.U;
    public KeyCode playerDamageKey = KeyCode.I;
    
    [Header("테스트 데미지")]
    public int testDamage = 25;
    public int criticalDamage = 50;
    public int healAmount = 30;
    public int playerDamage = 15;
    
    [Header("테스트 위치")]
    public float testHeight = 1.5f; // 테스트 위치 높이
    
    void Start()
    {
        // DamageTextManager 초기화
        DamageTextManager.GetInstance();
    }
    
    void Update()
    {
        // 일반 데미지 테스트
        if (Input.GetKeyDown(testKey))
        {
            Vector3 testPosition = transform.position + Vector3.up * testHeight;
            DamageTextManager.ShowDamage(testPosition, testDamage, false, false, false);
            Debug.Log($"일반 데미지 테스트: {testDamage}");
        }
        
        // 크리티컬 데미지 테스트
        if (Input.GetKeyDown(criticalKey))
        {
            Vector3 testPosition = transform.position + Vector3.up * testHeight;
            DamageTextManager.ShowDamage(testPosition, criticalDamage, true, false, false);
            Debug.Log($"크리티컬 데미지 테스트: {criticalDamage}");
        }
        
        // 힐 테스트
        if (Input.GetKeyDown(healKey))
        {
            Vector3 testPosition = transform.position + Vector3.up * testHeight;
            DamageTextManager.ShowDamage(testPosition, healAmount, false, true, false);
            Debug.Log($"힐 테스트: {healAmount}");
        }
        
        // 플레이어 데미지 테스트
        if (Input.GetKeyDown(playerDamageKey))
        {
            Vector3 testPosition = transform.position + Vector3.up * testHeight;
            DamageTextManager.ShowDamage(testPosition, playerDamage, false, false, true);
            Debug.Log($"플레이어 데미지 테스트: {playerDamage}");
        }
    }
    
    void OnGUI()
    {
        // 화면에 테스트 안내 표시
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("데미지 텍스트 테스트");
        GUILayout.Label($"T: 일반 데미지 ({testDamage})");
        GUILayout.Label($"Y: 크리티컬 데미지 ({criticalDamage})");
        GUILayout.Label($"U: 힐 ({healAmount})");
        GUILayout.Label($"I: 플레이어 데미지 ({playerDamage})");
        GUILayout.EndArea();
    }
} 