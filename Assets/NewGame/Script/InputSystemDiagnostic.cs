using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InputSystemDiagnostic : MonoBehaviour
{
    [Header("🔍 Input System 진단 도구")]
    [TextArea(5, 10)]
    public string instructions = 
        "이 도구는 Unity의 Input System 설정 문제를 진단합니다.\n\n" +
        "🔧 Inspector 버튼:\n" +
        "• Check Status 버튼 클릭\n" +
        "• Enable Actions 버튼 클릭\n" +
        "• Diagnose EventSystem 버튼 클릭\n\n" +
        "⌨️ 키보드 단축키 (Play 모드에서):\n" +
        "• F9: Input System 상태 확인\n" +
        "• F10: Input Actions 활성화\n" +
        "• F11: EventSystem 진단\n" +
        "• F12: 마우스 이벤트 테스트\n\n" +
        "⚠️ 주요 문제:\n" +
        "1. Input System 패키지가 설치되었지만 활성화되지 않음\n" +
        "2. StandaloneInputModule과 InputSystemUIInputModule 충돌\n" +
        "3. EventSystem 설정 문제";

    [Header("🎮 테스트 버튼들")]
    [Space(10)]
    public bool _______________BUTTONS_______________ = false;
    
    void Start()
    {
        // Debug.Log("🔧 [InputSystemDiagnostic] 진단 도구 준비됨 - F9, F10, F11, F12 키 사용 가능");
    }
    
    void Update()
    {
        // 키보드 단축키로 실행
        if (Input.GetKeyDown(KeyCode.F9))
        {
            CheckInputSystemStatus();
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            CheckAndEnableInputActions();
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            DiagnoseEventSystem();
        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            TestMouseEvents();
        }
    }

    [ContextMenu("Check Input System Status")]
    public void CheckInputSystemStatus()
    {
        Debug.Log("🔍 [InputSystemDiagnostic] Input System 상태 확인 시작...");
        
        // 1. Input System 패키지 확인
        #if UNITY_INPUT_SYSTEM_ENABLE_UI
        Debug.Log("✅ Input System UI 지원 활성화됨 (UNITY_INPUT_SYSTEM_ENABLE_UI)");
        #else
        Debug.LogWarning("⚠️ Input System UI 지원이 비활성화됨");
        #endif
        
        #if ENABLE_INPUT_SYSTEM
        Debug.Log("✅ Input System 백엔드 활성화됨 (ENABLE_INPUT_SYSTEM)");
        #else
        Debug.LogWarning("⚠️ Input System 백엔드가 비활성화됨");
        #endif
        
        #if ENABLE_LEGACY_INPUT_MANAGER
        Debug.LogWarning("⚠️ 레거시 Input Manager가 여전히 활성화됨 (ENABLE_LEGACY_INPUT_MANAGER)");
        #else
        Debug.Log("✅ 레거시 Input Manager 비활성화됨");
        #endif
        
        // 2. EventSystem 확인 (개선된 방법)
        EventSystem eventSystem = EventSystem.current;
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        Debug.Log($"📋 씬의 EventSystem 개수: {allEventSystems.Length}");
        Debug.Log($"📋 EventSystem.current: {(eventSystem != null ? eventSystem.name : "null")}");
        
        if (allEventSystems.Length == 0)
        {
            Debug.LogError("❌ 씬에 EventSystem이 전혀 없습니다!");
            Debug.Log("💡 해결책: GameObject > UI > Event System 생성");
            return;
        }
        
        // 모든 EventSystem 상태 확인
        for (int i = 0; i < allEventSystems.Length; i++)
        {
            var es = allEventSystems[i];
            Debug.Log($"  - EventSystem {i + 1}: {es.name}");
            Debug.Log($"    GameObject 활성화: {es.gameObject.activeInHierarchy}");
            Debug.Log($"    Component 활성화: {es.enabled}");
            Debug.Log($"    Current 여부: {es == EventSystem.current}");
        }
        
        // EventSystem.current가 null인 경우 첫 번째 활성화된 EventSystem 사용
        if (eventSystem == null && allEventSystems.Length > 0)
        {
            foreach (var es in allEventSystems)
            {
                if (es.gameObject.activeInHierarchy && es.enabled)
                {
                    eventSystem = es;
                    Debug.LogWarning($"⚠️ EventSystem.current가 null이므로 '{es.name}'을 사용합니다.");
                    break;
                }
            }
        }
        
        if (eventSystem == null)
        {
            Debug.LogError("❌ 활성화된 EventSystem을 찾을 수 없습니다!");
            
            // EventSystem 강제 활성화 시도
            if (allEventSystems.Length > 0)
            {
                var es = allEventSystems[0];
                Debug.Log($"🔧 EventSystem '{es.name}' 강제 활성화 시도...");
                
                es.gameObject.SetActive(true);
                es.enabled = true;
                
                Debug.Log("✅ EventSystem 강제 활성화 완료!");
                eventSystem = es;
            }
            else
            {
                return;
            }
        }
        
        Debug.Log($"✅ 사용할 EventSystem: {eventSystem.name}");
        
        // Input Module 확인
        var inputModule = eventSystem.currentInputModule;
        if (inputModule == null)
        {
            Debug.LogError("❌ Input Module이 없습니다!");
        }
        else
        {
            Debug.Log($"📋 현재 Input Module: {inputModule.GetType().Name}");
            
            // Input Module 타입별 분석
            if (inputModule is StandaloneInputModule)
            {
                Debug.LogWarning("⚠️ StandaloneInputModule 사용 중 - Input System 패키지와 충돌 가능");
                Debug.LogWarning("💡 해결책: InputSystemUIInputModule로 교체 필요");
            }
            #if UNITY_INPUT_SYSTEM_ENABLE_UI
            else if (inputModule.GetType().Name == "InputSystemUIInputModule")
            {
                Debug.Log("✅ InputSystemUIInputModule 사용 중 - 올바른 설정");
            }
            #endif
            else
            {
                Debug.LogWarning($"⚠️ 알 수 없는 Input Module: {inputModule.GetType().Name}");
            }
        }
        
        // 3. Canvas와 GraphicRaycaster 확인
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"📋 씬의 Canvas 개수: {canvases.Length}");
        
        foreach (var canvas in canvases)
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning($"⚠️ Canvas '{canvas.name}'에 GraphicRaycaster가 없습니다!");
            }
            else
            {
                Debug.Log($"✅ Canvas '{canvas.name}': GraphicRaycaster 확인");
            }
        }
        
        Debug.Log("🔍 [InputSystemDiagnostic] Input System 상태 확인 완료!");
    }
    
    [ContextMenu("Fix Input System Settings")]
    public void FixInputSystemSettings()
    {
        Debug.Log("🔧 [InputSystemDiagnostic] Input System 설정 자동 수정 시작...");
        
        // EventSystem 찾기
        EventSystem eventSystem = EventSystem.current;
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (allEventSystems.Length == 0)
        {
            Debug.LogError("❌ EventSystem이 없습니다! EventSystem을 먼저 생성하세요.");
            return;
        }
        
        // EventSystem.current가 null인 경우 첫 번째 활성화된 EventSystem 사용
        if (eventSystem == null)
        {
            eventSystem = allEventSystems[0];
            eventSystem.gameObject.SetActive(true);
            eventSystem.enabled = true;
            Debug.Log($"🔧 EventSystem '{eventSystem.name}' 강제 활성화!");
        }
        
        Debug.Log($"✅ 사용할 EventSystem: {eventSystem.name}");
        
        // Input Module 확인 및 추가
        var currentModule = eventSystem.currentInputModule;
        
        if (currentModule == null)
        {
            Debug.LogWarning("⚠️ Input Module이 없습니다! 자동으로 추가합니다...");
            
            // 먼저 InputSystemUIInputModule 시도
            #if UNITY_INPUT_SYSTEM_ENABLE_UI
            try
            {
                Debug.Log("🔧 InputSystemUIInputModule 추가 시도...");
                var inputSystemModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                
                // AssignDefaultActions 호출
                var assignDefaultActionsMethod = inputSystemModule.GetType().GetMethod("AssignDefaultActions");
                if (assignDefaultActionsMethod != null)
                {
                    assignDefaultActionsMethod.Invoke(inputSystemModule, null);
                    Debug.Log("✅ InputSystemUIInputModule 추가 및 기본 액션 할당 완료!");
                }
                else
                {
                    Debug.Log("✅ InputSystemUIInputModule 추가 완료!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ InputSystemUIInputModule 추가 실패: {e.Message}");
                
                // 대안: StandaloneInputModule 추가
                Debug.Log("🔧 대안으로 StandaloneInputModule 추가...");
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("✅ StandaloneInputModule 추가 완료!");
            }
            #else
            // Input System UI가 비활성화된 경우 StandaloneInputModule 사용
            Debug.Log("🔧 Input System UI가 비활성화됨. StandaloneInputModule 추가...");
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ StandaloneInputModule 추가 완료!");
            #endif
        }
        else if (currentModule is StandaloneInputModule standaloneModule)
        {
            Debug.Log("🔄 StandaloneInputModule에서 InputSystemUIInputModule로 교체 시도...");
            
            #if UNITY_INPUT_SYSTEM_ENABLE_UI && UNITY_EDITOR
            try
            {
                // StandaloneInputModule 제거
                DestroyImmediate(standaloneModule);
                
                // InputSystemUIInputModule 추가
                var newModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                
                // AssignDefaultActions 호출
                var assignDefaultActionsMethod = newModule.GetType().GetMethod("AssignDefaultActions");
                if (assignDefaultActionsMethod != null)
                {
                    assignDefaultActionsMethod.Invoke(newModule, null);
                }
                
                Debug.Log("✅ InputSystemUIInputModule로 교체 완료!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 교체 실패: {e.Message}");
                Debug.LogWarning("💡 Player Settings에서 Input System을 활성화한 후 다시 시도하세요.");
            }
            #else
            Debug.LogWarning("⚠️ Input System UI 지원이 활성화되지 않아 교체할 수 없습니다.");
            #endif
        }
        else
        {
            Debug.Log($"✅ 기존 Input Module 유지: {currentModule.GetType().Name}");
        }
        
        // Canvas에 GraphicRaycaster 추가
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log($"✅ Canvas '{canvas.name}'에 GraphicRaycaster 추가");
            }
        }
        
        Debug.Log("🔧 [InputSystemDiagnostic] Input System 설정 자동 수정 완료!");
        Debug.LogWarning("⚠️ 변경사항 적용을 위해 Unity 재시작이 필요할 수 있습니다.");
        
        // 수정 후 상태 재확인
        Debug.Log("🔍 수정 후 상태 재확인...");
        CheckInputSystemStatus();
    }
    
    [ContextMenu("Diagnose EventSystem")]
    public void DiagnoseEventSystem()
    {
        Debug.Log("🔍 [InputSystemDiagnostic] EventSystem 진단 시작...");
        
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        EventSystem currentEventSystem = EventSystem.current;
        
        Debug.Log($"📋 씬의 EventSystem 개수: {eventSystems.Length}");
        Debug.Log($"📋 EventSystem.current: {(currentEventSystem != null ? currentEventSystem.name : "null")}");
        
        if (eventSystems.Length == 0)
        {
            Debug.LogError("❌ EventSystem이 씬에 없습니다!");
            Debug.Log("💡 해결책: GameObject > UI > Event System 생성");
        }
        else if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"⚠️ EventSystem이 {eventSystems.Length}개 있습니다! 하나만 있어야 합니다.");
            
            for (int i = 0; i < eventSystems.Length; i++)
            {
                var es = eventSystems[i];
                Debug.Log($"  - EventSystem {i + 1}: {es.name}");
                Debug.Log($"    GameObject 활성화: {es.gameObject.activeInHierarchy}");
                Debug.Log($"    Component 활성화: {es.enabled}");
                Debug.Log($"    Current 여부: {es == EventSystem.current}");
            }
        }
        else
        {
            EventSystem eventSystem = eventSystems[0];
            Debug.Log($"✅ EventSystem 발견: {eventSystem.name}");
            
            // 상세 진단
            Debug.Log($"📋 GameObject 활성화: {eventSystem.gameObject.activeInHierarchy}");
            Debug.Log($"📋 Component 활성화: {eventSystem.enabled}");
            Debug.Log($"📋 EventSystem.current와 일치: {eventSystem == EventSystem.current}");
            Debug.Log($"📋 First Selected: {(eventSystem.firstSelectedGameObject != null ? eventSystem.firstSelectedGameObject.name : "null")}");
            Debug.Log($"📋 Current Selected: {(eventSystem.currentSelectedGameObject != null ? eventSystem.currentSelectedGameObject.name : "null")}");
            
            // EventSystem.current가 null인데 EventSystem이 있는 경우
            if (EventSystem.current == null)
            {
                Debug.LogWarning("⚠️ EventSystem이 있지만 EventSystem.current가 null입니다!");
                
                if (!eventSystem.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("⚠️ EventSystem GameObject가 비활성화되어 있습니다!");
                    Debug.Log("🔧 EventSystem GameObject 활성화 시도...");
                    eventSystem.gameObject.SetActive(true);
                }
                
                if (!eventSystem.enabled)
                {
                    Debug.LogWarning("⚠️ EventSystem Component가 비활성화되어 있습니다!");
                    Debug.Log("🔧 EventSystem Component 활성화 시도...");
                    eventSystem.enabled = true;
                }
                
                // 강제 새로고침
                Debug.Log("🔄 EventSystem 강제 새로고침...");
                eventSystem.enabled = false;
                eventSystem.enabled = true;
            }
            
            // Input Module 상세 정보
            var inputModule = eventSystem.currentInputModule;
            if (inputModule != null)
            {
                Debug.Log($"📋 Input Module 타입: {inputModule.GetType().FullName}");
                Debug.Log($"📋 Input Module 활성화: {inputModule.enabled}");
                Debug.Log($"📋 Input Module GameObject: {inputModule.gameObject.name}");
                
                // InputSystemUIInputModule 특별 진단
                if (inputModule.GetType().Name == "InputSystemUIInputModule")
                {
                    Debug.Log("🔍 InputSystemUIInputModule 상세 진단...");
                    
                    try
                    {
                        var actionsAssetProperty = inputModule.GetType().GetProperty("actionsAsset");
                        if (actionsAssetProperty != null)
                        {
                            var actionsAsset = actionsAssetProperty.GetValue(inputModule);
                            Debug.Log($"📋 ActionsAsset: {(actionsAsset != null ? actionsAsset.ToString() : "null")}");
                            
                            if (actionsAsset != null)
                            {
                                var enabledProperty = actionsAsset.GetType().GetProperty("enabled");
                                if (enabledProperty != null)
                                {
                                    bool isEnabled = (bool)enabledProperty.GetValue(actionsAsset);
                                    Debug.Log($"📋 ActionsAsset 활성화: {isEnabled}");
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"⚠️ InputSystemUIInputModule 진단 중 오류: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ Input Module이 없습니다!");
            }
        }
        
        Debug.Log("🔍 [InputSystemDiagnostic] EventSystem 진단 완료!");
    }
    
    [ContextMenu("Test Mouse Events")]
    public void TestMouseEvents()
    {
        Debug.Log("🖱️ [InputSystemDiagnostic] 마우스 이벤트 테스트 중...");
        Debug.Log($"📋 마우스 위치: {Input.mousePosition}");
        Debug.Log($"📋 좌클릭: {Input.GetMouseButton(0)}");
        Debug.Log($"📋 우클릭: {Input.GetMouseButton(1)}");
        Debug.Log($"📋 중간클릭: {Input.GetMouseButton(2)}");
        
        // EventSystem을 통한 UI 레이캐스팅 테스트
        if (EventSystem.current != null)
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            Debug.Log($"📋 UI 레이캐스트 결과: {results.Count}개");
            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"  - {i + 1}. {results[i].gameObject.name} (모듈: {results[i].module.GetType().Name})");
            }
        }
    }

    [ContextMenu("Check And Enable Input Actions")]
    public void CheckAndEnableInputActions()
    {
        Debug.Log("🔍 [InputSystemDiagnostic] Input Actions 상태 확인 및 활성화 시작...");
        
        // EventSystem 찾기 (개선된 방법)
        EventSystem eventSystem = EventSystem.current;
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (allEventSystems.Length == 0)
        {
            Debug.LogError("❌ 씬에 EventSystem이 없습니다!");
            return;
        }
        
        // EventSystem.current가 null인 경우 첫 번째 활성화된 EventSystem 사용
        if (eventSystem == null)
        {
            foreach (var es in allEventSystems)
            {
                if (es.gameObject.activeInHierarchy && es.enabled)
                {
                    eventSystem = es;
                    Debug.LogWarning($"⚠️ EventSystem.current가 null이므로 '{es.name}'을 사용합니다.");
                    break;
                }
            }
            
            // 여전히 null이면 강제 활성화
            if (eventSystem == null && allEventSystems.Length > 0)
            {
                eventSystem = allEventSystems[0];
                eventSystem.gameObject.SetActive(true);
                eventSystem.enabled = true;
                Debug.Log($"🔧 EventSystem '{eventSystem.name}' 강제 활성화!");
            }
        }
        
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem을 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"✅ 사용할 EventSystem: {eventSystem.name}");
        
        var inputModule = eventSystem.currentInputModule;
        if (inputModule != null && inputModule.GetType().Name == "InputSystemUIInputModule")
        {
            try
            {
                // actionsAsset 프로퍼티 확인
                var actionsAssetProperty = inputModule.GetType().GetProperty("actionsAsset");
                if (actionsAssetProperty != null)
                {
                    var actionsAsset = actionsAssetProperty.GetValue(inputModule);
                    Debug.Log($"📋 ActionsAsset: {(actionsAsset != null ? actionsAsset.ToString() : "null")}");
                    
                    if (actionsAsset != null)
                    {
                        // InputActionAsset의 활성화 상태 확인
                        var enabledProperty = actionsAsset.GetType().GetProperty("enabled");
                        if (enabledProperty != null)
                        {
                            bool isEnabled = (bool)enabledProperty.GetValue(actionsAsset);
                            Debug.Log($"📋 ActionsAsset 활성화 상태: {isEnabled}");
                            
                            if (!isEnabled)
                            {
                                Debug.LogWarning("⚠️ ActionsAsset이 비활성화되어 있습니다! 활성화합니다...");
                                
                                // Enable 메서드 호출
                                var enableMethod = actionsAsset.GetType().GetMethod("Enable");
                                if (enableMethod != null)
                                {
                                    enableMethod.Invoke(actionsAsset, null);
                                    Debug.Log("✅ ActionsAsset 활성화 완료!");
                                }
                            }
                            else
                            {
                                Debug.Log("✅ ActionsAsset이 이미 활성화되어 있습니다.");
                            }
                        }
                        
                        // 개별 Action들의 상태 확인
                        var actionsProperty = actionsAsset.GetType().GetProperty("actions");
                        if (actionsProperty != null)
                        {
                            var actions = actionsProperty.GetValue(actionsAsset) as System.Collections.IEnumerable;
                            if (actions != null)
                            {
                                int actionCount = 0;
                                int enabledCount = 0;
                                
                                foreach (var action in actions)
                                {
                                    actionCount++;
                                    var actionEnabledProperty = action.GetType().GetProperty("enabled");
                                    if (actionEnabledProperty != null)
                                    {
                                        bool actionEnabled = (bool)actionEnabledProperty.GetValue(action);
                                        if (actionEnabled) enabledCount++;
                                        
                                        var nameProperty = action.GetType().GetProperty("name");
                                        string actionName = nameProperty?.GetValue(action)?.ToString() ?? "Unknown";
                                        
                                        Debug.Log($"  - Action '{actionName}': {(actionEnabled ? "활성화" : "비활성화")}");
                                    }
                                }
                                
                                Debug.Log($"📋 총 Actions: {actionCount}개, 활성화된 Actions: {enabledCount}개");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ ActionsAsset이 null입니다!");
                        
                        // DefaultInputActions 자동 할당 시도
                        Debug.Log("🔧 DefaultInputActions 자동 할당 시도...");
                        
                        var assignDefaultActionsMethod = inputModule.GetType().GetMethod("AssignDefaultActions");
                        if (assignDefaultActionsMethod != null)
                        {
                            assignDefaultActionsMethod.Invoke(inputModule, null);
                            Debug.Log("✅ DefaultInputActions 할당 완료!");
                            
                            // 재귀 호출로 다시 확인
                            CheckAndEnableInputActions();
                            return;
                        }
                        else
                        {
                            Debug.LogError("❌ AssignDefaultActions 메서드를 찾을 수 없습니다!");
                        }
                    }
                }
                
                // Mouse Position과 Click 확인
                var pointActionProperty = inputModule.GetType().GetProperty("point");
                var leftClickActionProperty = inputModule.GetType().GetProperty("leftClick");
                var rightClickActionProperty = inputModule.GetType().GetProperty("rightClick");
                
                if (pointActionProperty != null)
                {
                    var pointAction = pointActionProperty.GetValue(inputModule);
                    Debug.Log($"📋 Point Action: {(pointAction != null ? pointAction.ToString() : "null")}");
                }
                
                if (leftClickActionProperty != null)
                {
                    var leftClickAction = leftClickActionProperty.GetValue(inputModule);
                    Debug.Log($"📋 Left Click Action: {(leftClickAction != null ? leftClickAction.ToString() : "null")}");
                }
                
                if (rightClickActionProperty != null)
                {
                    var rightClickAction = rightClickActionProperty.GetValue(inputModule);
                    Debug.Log($"📋 Right Click Action: {(rightClickAction != null ? rightClickAction.ToString() : "null")}");
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Input Actions 확인 실패: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("❌ InputSystemUIInputModule을 찾을 수 없습니다!");
        }
        
        Debug.Log("🔍 [InputSystemDiagnostic] Input Actions 상태 확인 완료!");
    }
} 