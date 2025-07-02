using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🔍 SimpleTest 시작 ===");
        
        // 기본 정보 출력
        Debug.Log($"Unity 버전: {Application.unityVersion}");
        Debug.Log($"현재 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // EventSystem 확인
        EventSystem eventSystem = EventSystem.current;
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        Debug.Log($"EventSystem.current: {(eventSystem != null ? eventSystem.name : "null")}");
        Debug.Log($"씬의 EventSystem 개수: {allEventSystems.Length}");
        
        if (allEventSystems.Length > 0)
        {
            for (int i = 0; i < allEventSystems.Length; i++)
            {
                var es = allEventSystems[i];
                Debug.Log($"EventSystem {i + 1}: {es.name} (활성: {es.gameObject.activeInHierarchy}, 컴포넌트: {es.enabled})");
                
                if (es.currentInputModule != null)
                {
                    Debug.Log($"  Input Module: {es.currentInputModule.GetType().Name}");
                }
                else
                {
                    Debug.Log("  Input Module: null");
                }
            }
        }
        
        // Canvas 확인
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Canvas 개수: {canvases.Length}");
        
        // Input System 컴파일 플래그 확인
        Debug.Log("=== Input System 컴파일 플래그 ===");
        
        #if ENABLE_INPUT_SYSTEM
        Debug.Log("✅ ENABLE_INPUT_SYSTEM 활성화");
        #else
        Debug.Log("❌ ENABLE_INPUT_SYSTEM 비활성화");
        #endif
        
        #if ENABLE_LEGACY_INPUT_MANAGER
        Debug.Log("⚠️ ENABLE_LEGACY_INPUT_MANAGER 활성화");
        #else
        Debug.Log("✅ ENABLE_LEGACY_INPUT_MANAGER 비활성화");
        #endif
        
        #if UNITY_INPUT_SYSTEM_ENABLE_UI
        Debug.Log("✅ UNITY_INPUT_SYSTEM_ENABLE_UI 활성화");
        #else
        Debug.Log("❌ UNITY_INPUT_SYSTEM_ENABLE_UI 비활성화");
        #endif
        
        Debug.Log("=== 🔍 SimpleTest 완료 ===");
    }
    
    void Update()
    {
        // 매 프레임 마우스 입력 확인 (5초간만)
        if (Time.time < 5f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"🖱️ 좌클릭 감지! 위치: {Input.mousePosition}");
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"🖱️ 우클릭 감지! 위치: {Input.mousePosition}");
            }
        }
    }
    
    // UI 버튼으로 테스트할 수 있는 메서드
    public void TestButton()
    {
        Debug.Log("🔘 Test Button 클릭됨!");
        
        // EventSystem 재확인
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            Debug.Log($"EventSystem: {eventSystem.name}");
            if (eventSystem.currentInputModule != null)
            {
                Debug.Log($"Input Module: {eventSystem.currentInputModule.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("EventSystem.current가 null입니다!");
        }
    }
} 