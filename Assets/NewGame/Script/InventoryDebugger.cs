using UnityEngine;

public class InventoryDebugger : MonoBehaviour
{
    [Header("🔧 개발자 모드")]
    [Tooltip("체크 해제하면 디버거가 비활성화됩니다")]
    public bool enableDebugger = false;
    
    [Header("🔍 인벤토리 문제 진단")]
    [TextArea(6, 8)]
    public string debugInfo = "I키를 눌러보세요!\n진단 결과가 여기에 표시됩니다.\n\n- F6: 슬롯 활성화 (비활성화된 슬롯 수정)\n- F7: 슬롯 시스템 진단 및 수정\n- F8: 패널 연결 수정\n- F9: 강제 인벤토리 열기\n- F10: 시스템 상태 확인\n- F11: 자동 수정 시도\n- F12: 레거시 충돌 해결\n\n🆕 WeaponSlot 전용:\n- Ctrl+F6: WeaponSlot UI 자동 생성\n- Ctrl+F7: WeaponSlot 진단 및 수정";
    
    [Header("🎯 진단 결과")]
    public bool hasInventoryManager = false;
    public bool hasPlayerInventory = false;
    public bool hasInventoryPanel = false;
    public bool isInputWorking = false;
    public bool isUIConnected = false;
    public bool hasSlotsGenerated = false;
    public int currentSlotCount = 0;
    
    [Header("🔗 발견된 컴포넌트들")]
    public InventoryManager foundInventoryManager;
    public PlayerInventory foundPlayerInventory;
    
    [System.Obsolete("레거시 지원용 - 새 프로젝트에서는 InventoryManager 사용")]
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
    public InventoryUI foundInventoryUI;
#pragma warning restore CS0618
    
    private bool isDebugging = false;
    
    void Start()
    {
        // 디버거가 활성화되어 있을 때만 자동 진단
        if (enableDebugger)
        {
            DiagnoseInventorySystem();
            
            // 자동 수정 옵션 (원한다면 주석 해제)
            // if (!HasAllComponents() || !isUIConnected)
            // {
            //     Debug.Log("🔧 [InventoryDebugger] 문제 발견! 자동 수정을 시작합니다...");
            //     AutoFixInventory();
            // }
        }
    }
    
    void Update()
    {
        // 디버거가 비활성화되어 있으면 아무것도 하지 않음
        if (!enableDebugger) return;
        
        // I키 입력 감지 및 진단
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("🔑 [InventoryDebugger] I키가 눌렸습니다!");
            isInputWorking = true;
            
            if (!isDebugging)
            {
                StartDebugging();
            }
        }
        
        // 디버그 키들
        if (Input.GetKeyDown(KeyCode.F8))
        {
            FixWrongPanelConnection();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            DiagnoseAndFixSlots();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ActivateAllSlots();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ForceOpenInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            DiagnoseInventorySystem();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            AutoFixInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            FixLegacyConflicts();
        }
        
        // 🆕 WeaponSlot 전용 단축키들
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F6))
        {
            FixWeaponSlotUI();
        }
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F7))
        {
            DiagnoseWeaponSlot();
        }
    }
    
    void StartDebugging()
    {
        isDebugging = true;
        Debug.Log("🚨 [InventoryDebugger] 인벤토리가 열리지 않음을 감지! 진단을 시작합니다...");
        
        DiagnoseInventorySystem();
        
        // 2초 후 디버깅 모드 해제
        Invoke(nameof(StopDebugging), 2f);
    }
    
    void StopDebugging()
    {
        isDebugging = false;
    }
    
    [ContextMenu("Diagnose Inventory System")]
    public void DiagnoseInventorySystem()
    {
        Debug.Log("🔍 [InventoryDebugger] 인벤토리 시스템 진단 시작...");
        
        // 컴포넌트들 찾기
        foundInventoryManager = FindAnyObjectByType<InventoryManager>();
        foundPlayerInventory = FindAnyObjectByType<PlayerInventory>();
        
        // 레거시 InventoryUI 찾기 (obsolete 경고 무시)
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        foundInventoryUI = FindAnyObjectByType<InventoryUI>();
#pragma warning restore CS0618
        
        // 상태 체크
        hasInventoryManager = foundInventoryManager != null;
        hasPlayerInventory = foundPlayerInventory != null;
        
        // UI 패널 체크
        if (foundInventoryManager != null)
        {
            hasInventoryPanel = foundInventoryManager.inventoryPanel != null;
            isUIConnected = hasInventoryPanel;
            
            // 슬롯 상태 체크
            if (foundInventoryManager.slotParent != null)
            {
                currentSlotCount = foundInventoryManager.slotParent.childCount;
                hasSlotsGenerated = currentSlotCount > 0;
            }
            else
            {
                currentSlotCount = 0;
                hasSlotsGenerated = false;
            }
        }
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        else if (foundInventoryUI != null)
        {
            hasInventoryPanel = foundInventoryUI.inventoryPanel != null;
            isUIConnected = hasInventoryPanel;
            currentSlotCount = 0;
            hasSlotsGenerated = false;
        }
#pragma warning restore CS0618
        
        // 진단 결과 업데이트
        UpdateDebugInfo();
        
        // 콘솔에 상세 정보 출력
        LogDiagnosisResults();
    }
    
    void UpdateDebugInfo()
    {
        debugInfo = "=== 인벤토리 진단 결과 ===\n";
        debugInfo += GetStatusIcon(hasInventoryManager) + " InventoryManager\n";
        debugInfo += GetStatusIcon(hasPlayerInventory) + " PlayerInventory\n";
        debugInfo += GetStatusIcon(hasInventoryPanel) + " UI Panel 연결\n";
        debugInfo += GetStatusIcon(hasSlotsGenerated) + $" 슬롯 시스템 ({currentSlotCount}개)\n";
        
        // 🔧 슬롯 프리팹 및 활성화 상태 체크 추가
        if (foundInventoryManager != null)
        {
            bool hasPrefab = foundInventoryManager.slotPrefab != null;
            debugInfo += GetStatusIcon(hasPrefab) + " 슬롯 프리팹 연결\n";
            
            if (hasPrefab)
            {
                bool prefabActive = foundInventoryManager.slotPrefab.activeSelf;
                debugInfo += GetStatusIcon(prefabActive) + $" 프리팹 활성화 상태 {(prefabActive ? "(정상)" : "(수정됨)")}\n";
            }
            
            // 비활성화된 슬롯 개수 체크
            if (foundInventoryManager.slotParent != null && currentSlotCount > 0)
            {
                int inactiveSlots = 0;
                for (int i = 0; i < currentSlotCount; i++)
                {
                    Transform slotTransform = foundInventoryManager.slotParent.GetChild(i);
                    if (!slotTransform.gameObject.activeSelf)
                    {
                        inactiveSlots++;
                    }
                }
                
                if (inactiveSlots > 0)
                {
                    debugInfo += $"⚠️ 비활성화된 슬롯: {inactiveSlots}개 (F6으로 수정)\n";
                }
                else
                {
                    debugInfo += "✅ 모든 슬롯 활성화됨\n";
                }
            }
        }
        
        debugInfo += GetStatusIcon(isInputWorking) + " I키 입력 감지\n";
        debugInfo += GetStatusIcon(isUIConnected) + " UI 시스템 연결\n\n";
        
        if (HasAllComponents())
        {
            debugInfo += "🎉 모든 설정이 완료되었습니다!\n";
            if (!isUIConnected)
            {
                debugInfo += "❗ 하지만 UI가 연결되지 않았습니다.";
            }
        }
        else
        {
            debugInfo += "❌ 문제가 발견되었습니다:\n";
            debugInfo += GetFixSuggestions();
        }
        
        debugInfo += "\n🔧 디버그 키:\n- F6: 슬롯 활성화\n- F7: 슬롯 진단\n- F8: 패널 수정\n- F9: 강제 열기\n- F10: 재진단\n- F11: 자동 수정\n- F12: 충돌 해결";
    }
    
    string GetStatusIcon(bool status)
    {
        return status ? "✅" : "❌";
    }
    
    bool HasAllComponents()
    {
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        return (hasInventoryManager || foundInventoryUI != null) && hasPlayerInventory && hasSlotsGenerated;
#pragma warning restore CS0618
    }
    
    string GetFixSuggestions()
    {
        string suggestions = "";
        
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        if (!hasInventoryManager && foundInventoryUI == null)
        {
            suggestions += "• InventoryManager 또는 InventoryUI 추가 필요\n";
        }
#pragma warning restore CS0618
        
        if (!hasPlayerInventory)
        {
            suggestions += "• PlayerInventory 컴포넌트 추가 필요\n";
        }
        
        if (!hasInventoryPanel)
        {
            suggestions += "• inventoryPanel UI 연결 필요\n";
        }
        
        if (!hasSlotsGenerated)
        {
            suggestions += "• 슬롯 시스템 설정 필요 (F7 키 누르세요)\n";
        }
        
        return suggestions;
    }
    
    void LogDiagnosisResults()
    {
        Debug.Log("📊 [InventoryDebugger] === 상세 진단 결과 ===");
        Debug.Log($"InventoryManager: {(foundInventoryManager != null ? "✅ 발견됨" : "❌ 없음")}");
        Debug.Log($"PlayerInventory: {(foundPlayerInventory != null ? "✅ 발견됨" : "❌ 없음")}");
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        Debug.Log($"InventoryUI (레거시): {(foundInventoryUI != null ? "⚠️ 발견됨" : "❌ 없음")}");
        
        if (foundInventoryManager != null)
        {
            Debug.Log($"InventoryPanel 연결: {(foundInventoryManager.inventoryPanel != null ? "✅" : "❌")}");
            
            // 🔧 슬롯 프리팹 활성화 상태 진단 추가
            if (foundInventoryManager.slotPrefab != null)
            {
                bool prefabActive = foundInventoryManager.slotPrefab.activeSelf;
                Debug.Log($"슬롯 프리팹 활성화 상태: {(prefabActive ? "✅ 활성화됨" : "⚠️ 비활성화됨 (이제 자동 수정됨)")}");
                
                if (!prefabActive)
                {
                    Debug.Log("💡 슬롯 프리팹이 비활성화 상태였지만, 새로운 InventoryManager가 생성 시 자동으로 활성화합니다!");
                }
            }
            else
            {
                Debug.Log("❌ 슬롯 프리팹이 연결되지 않았습니다!");
            }
            
            // 슬롯 상태 진단
            if (foundInventoryManager.slotParent != null)
            {
                int slotCount = foundInventoryManager.slotParent.childCount;
                Debug.Log($"생성된 슬롯 개수: {slotCount}");
                
                // 비활성화된 슬롯 개수 체크
                int inactiveSlots = 0;
                for (int i = 0; i < slotCount; i++)
                {
                    Transform slotTransform = foundInventoryManager.slotParent.GetChild(i);
                    if (!slotTransform.gameObject.activeSelf)
                    {
                        inactiveSlots++;
                    }
                }
                
                if (inactiveSlots > 0)
                {
                    Debug.LogWarning($"⚠️ {inactiveSlots}개의 슬롯이 비활성화 상태입니다! F6 키로 해결하세요.");
                }
                else if (slotCount > 0)
                {
                    Debug.Log("✅ 모든 슬롯이 활성화 상태입니다!");
                }
            }
        }
        
        if (foundInventoryUI != null)
        {
            Debug.Log($"InventoryUI Panel 연결: {(foundInventoryUI.inventoryPanel != null ? "✅" : "❌")}");
        }
#pragma warning restore CS0618
    }
    
    [ContextMenu("Force Open Inventory")]
    public void ForceOpenInventory()
    {
        Debug.Log("🔧 [InventoryDebugger] 강제로 인벤토리를 열어봅니다...");
        
        if (foundInventoryManager != null)
        {
            Debug.Log("InventoryManager로 인벤토리 열기 시도...");
            Debug.Log($"InventoryManager.inventoryPanel: {(foundInventoryManager.inventoryPanel != null ? "✅ 존재" : "❌ null")}");
            
            if (foundInventoryManager.inventoryPanel != null)
            {
                Debug.Log($"Panel 활성화 상태: {foundInventoryManager.inventoryPanel.activeSelf}");
                Debug.Log($"Panel 이름: {foundInventoryManager.inventoryPanel.name}");
            }
            
            try
            {
                foundInventoryManager.OpenInventory();
                Debug.Log("✅ InventoryManager.OpenInventory() 호출 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ InventoryManager.OpenInventory() 에러: {ex.Message}");
                Debug.LogError($"스택 트레이스: {ex.StackTrace}");
            }
        }
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        else if (foundInventoryUI != null)
        {
            Debug.Log("InventoryUI로 인벤토리 열기 시도...");
            if (foundInventoryUI.inventoryPanel != null)
            {
                foundInventoryUI.inventoryPanel.SetActive(true);
                Debug.Log("✅ InventoryUI 패널이 활성화되었습니다!");
            }
            else
            {
                Debug.LogError("❌ InventoryUI의 inventoryPanel이 연결되지 않았습니다!");
            }
        }
#pragma warning restore CS0618
        else if (foundPlayerInventory != null)
        {
            Debug.Log("PlayerInventory로 인벤토리 열기 시도...");
            foundPlayerInventory.OpenInventory();
        }
        else
        {
            Debug.LogError("❌ 어떤 인벤토리 시스템도 찾을 수 없습니다!");
        }
    }
    
    [ContextMenu("Auto Fix Inventory")]
    public void AutoFixInventory()
    {
        Debug.Log("🔧 [InventoryDebugger] 자동 수정을 시작합니다...");
        
        // InventoryManager가 없으면 추가
        if (!hasInventoryManager)
        {
            GameObject managerObj = new GameObject("InventoryManager");
            foundInventoryManager = managerObj.AddComponent<InventoryManager>();
            Debug.Log("✅ InventoryManager가 추가되었습니다!");
        }
        
        // PlayerInventory가 없으면 추가
        if (!hasPlayerInventory)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                foundPlayerInventory = playerObj.AddComponent<PlayerInventory>();
                Debug.Log("✅ PlayerInventory가 플레이어에 추가되었습니다!");
            }
            else
            {
                Debug.LogWarning("⚠️ Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            }
        }
        
        // UI 패널이 없으면 기본 패널 생성
        if (!hasInventoryPanel && foundInventoryManager != null)
        {
            CreateBasicInventoryPanel();
        }
        
        // 재진단
        DiagnoseInventorySystem();
        
        Debug.Log("🎉 [InventoryDebugger] 자동 수정 완료! F9로 테스트해보세요.");
    }
    
    [ContextMenu("Resolve Legacy Conflicts")]
    public void FixLegacyConflicts()
    {
        Debug.Log("🔧 [InventoryDebugger] 레거시 시스템 충돌 해결을 시작합니다...");
        
        int conflictsResolved = 0;
        
        // 1. 레거시 InventoryUI 비활성화
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        if (foundInventoryUI != null)
        {
            Debug.Log("⚠️ 레거시 InventoryUI 발견! 비활성화합니다...");
            foundInventoryUI.enabled = false;
            conflictsResolved++;
            Debug.Log("✅ InventoryUI 컴포넌트가 비활성화되었습니다.");
        }
#pragma warning restore CS0618
        
        // 2. PlayerInventory에서 레거시 연결 해제
        if (foundPlayerInventory != null)
        {
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
            if (foundPlayerInventory.inventoryUI != null)
            {
                Debug.Log("🔗 PlayerInventory에서 레거시 InventoryUI 연결 해제...");
                foundPlayerInventory.inventoryUI = null;
                conflictsResolved++;
                Debug.Log("✅ PlayerInventory.inventoryUI 연결이 해제되었습니다.");
            }
#pragma warning restore CS0618
            
            // InventoryManager 연결 확인 및 설정
            if (foundPlayerInventory.inventoryManager == null && foundInventoryManager != null)
            {
                Debug.Log("🔗 PlayerInventory에 InventoryManager 연결...");
                foundPlayerInventory.inventoryManager = foundInventoryManager;
                conflictsResolved++;
                Debug.Log("✅ PlayerInventory.inventoryManager가 연결되었습니다.");
            }
        }
        
        // 3. 중복 Input 처리 비활성화
        CheckForDuplicateInputHandlers();
        
        // 4. InventoryManager UI 연결 확인
        if (foundInventoryManager != null && foundInventoryManager.inventoryPanel == null)
        {
            Debug.Log("🔗 InventoryManager에 UI 패널이 연결되지 않음. 생성합니다...");
            CreateBasicInventoryPanel();
            conflictsResolved++;
        }
        
        // 5. 재진단
        DiagnoseInventorySystem();
        
        if (conflictsResolved > 0)
        {
            Debug.Log($"🎉 [InventoryDebugger] {conflictsResolved}개의 충돌이 해결되었습니다!");
            Debug.Log("💡 이제 F9를 눌러서 인벤토리 열기를 테스트해보세요.");
        }
        else
        {
            Debug.Log("ℹ️ [InventoryDebugger] 발견된 충돌이 없습니다.");
        }
    }
    
    void CheckForDuplicateInputHandlers()
    {
        // 씬에서 I키를 처리하는 모든 컴포넌트 찾기
        MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int duplicateHandlers = 0;
        
        foreach (var component in allComponents)
        {
            // InventoryUI가 활성화되어 있으면 문제
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
            if (component is InventoryUI inventoryUI && inventoryUI.enabled)
            {
                Debug.Log($"⚠️ 활성화된 InventoryUI 발견: {inventoryUI.gameObject.name}");
                duplicateHandlers++;
            }
#pragma warning restore CS0618
        }
        
        if (duplicateHandlers > 0)
        {
                         Debug.LogWarning($"⚠️ {duplicateHandlers}개의 중복 입력 핸들러가 발견되었습니다!");
         }
     }
     
     [ContextMenu("Fix Wrong Panel Connection")]
     public void FixWrongPanelConnection()
     {
         Debug.Log("🔧 [InventoryDebugger] 잘못된 패널 연결을 수정합니다...");
         
         if (foundInventoryManager == null)
         {
             Debug.LogError("❌ InventoryManager를 찾을 수 없습니다!");
             return;
         }
         
         // 현재 연결된 패널 정보 출력
         if (foundInventoryManager.inventoryPanel != null)
         {
             Debug.Log($"🔍 현재 연결된 오브젝트: {foundInventoryManager.inventoryPanel.name}");
             Debug.Log($"🔍 오브젝트 타입: {foundInventoryManager.inventoryPanel.GetType().Name}");
             
             // "Slot_1" 같은 잘못된 연결인지 확인
             if (foundInventoryManager.inventoryPanel.name.Contains("Slot") || 
                 foundInventoryManager.inventoryPanel.name.Contains("slot"))
             {
                 Debug.LogWarning("⚠️ 슬롯 오브젝트가 인벤토리 패널로 연결되어 있습니다! 이를 수정합니다.");
                 foundInventoryManager.inventoryPanel = null;
             }
         }
         
         // 올바른 인벤토리 패널 찾기
         GameObject correctPanel = FindCorrectInventoryPanel();
         
         if (correctPanel != null)
         {
             Debug.Log($"✅ 올바른 인벤토리 패널 발견: {correctPanel.name}");
             foundInventoryManager.inventoryPanel = correctPanel;
         }
         else
         {
             Debug.Log("❌ 올바른 인벤토리 패널을 찾을 수 없습니다. 새로 생성합니다...");
             CreateProperInventoryPanel();
         }
         
         // 테스트
         Debug.Log("🧪 수정된 연결로 인벤토리 열기 테스트...");
         if (foundInventoryManager.inventoryPanel != null)
         {
             foundInventoryManager.inventoryPanel.SetActive(true);
             Debug.Log("✅ 인벤토리 패널이 성공적으로 열렸습니다!");
             
             // 3초 후 자동으로 닫기
             StartCoroutine(CloseInventoryAfterDelay(3f));
         }
     }
     
     GameObject FindCorrectInventoryPanel()
     {
         // Canvas에서 인벤토리 패널로 적합한 오브젝트들 찾기
         Canvas canvas = FindAnyObjectByType<Canvas>();
         if (canvas == null) return null;
         
         // 이름으로 찾기
         string[] possibleNames = {"InventoryPanel", "Inventory", "InventoryUI", "InvPanel"};
         
         foreach (string name in possibleNames)
         {
             Transform found = canvas.transform.Find(name);
             if (found != null)
             {
                 Debug.Log($"🔍 이름으로 인벤토리 패널 발견: {found.name}");
                 return found.gameObject;
             }
         }
         
         // 자식 오브젝트들을 검색해서 적합한 패널 찾기
         foreach (Transform child in canvas.transform)
         {
             // 이름에 "inventory"가 포함되고 "slot"이 포함되지 않은 오브젝트
             string childName = child.name.ToLower();
             if (childName.Contains("inventory") && !childName.Contains("slot"))
             {
                 Debug.Log($"🔍 검색으로 인벤토리 패널 발견: {child.name}");
                 return child.gameObject;
             }
         }
         
         return null;
     }
     
     void CreateProperInventoryPanel()
     {
         GameObject canvas = FindAnyObjectByType<Canvas>()?.gameObject;
         if (canvas == null)
         {
             Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
             return;
         }
         
         // 적절한 인벤토리 패널 생성
         GameObject properPanel = new GameObject("InventoryPanel");
         properPanel.transform.SetParent(canvas.transform, false);
         
         // Image 컴포넌트 추가 (배경)
         UnityEngine.UI.Image panelImage = properPanel.AddComponent<UnityEngine.UI.Image>();
         panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 진한 반투명
         
         // RectTransform 설정 (화면 중앙)
         RectTransform rect = properPanel.GetComponent<RectTransform>();
         rect.anchorMin = new Vector2(0.5f, 0.5f);
         rect.anchorMax = new Vector2(0.5f, 0.5f);
         rect.anchoredPosition = Vector2.zero;
         rect.sizeDelta = new Vector2(600, 400); // 적당한 크기
         
         // 제목 추가
         GameObject title = new GameObject("Title");
         title.transform.SetParent(properPanel.transform, false);
         UnityEngine.UI.Text titleText = title.AddComponent<UnityEngine.UI.Text>();
         titleText.text = "Inventory";
         titleText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
         titleText.fontSize = 24;
         titleText.color = Color.white;
         titleText.alignment = TextAnchor.MiddleCenter;
         
         RectTransform titleRect = title.GetComponent<RectTransform>();
         titleRect.anchorMin = new Vector2(0, 0.8f);
         titleRect.anchorMax = new Vector2(1, 1);
         titleRect.offsetMin = Vector2.zero;
         titleRect.offsetMax = Vector2.zero;
         
         // SlotParent 생성 (나중에 슬롯들이 들어갈 곳)
         GameObject slotParent = new GameObject("SlotParent");
         slotParent.transform.SetParent(properPanel.transform, false);
         
         RectTransform slotRect = slotParent.AddComponent<RectTransform>();
         slotRect.anchorMin = new Vector2(0.1f, 0.1f);
         slotRect.anchorMax = new Vector2(0.9f, 0.7f);
         slotRect.offsetMin = Vector2.zero;
         slotRect.offsetMax = Vector2.zero;
         
         // Grid Layout Group 추가
         UnityEngine.UI.GridLayoutGroup grid = slotParent.AddComponent<UnityEngine.UI.GridLayoutGroup>();
         grid.cellSize = new Vector2(60, 60);
         grid.spacing = new Vector2(10, 10);
         grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
         grid.constraintCount = 5;
         
         // InventoryManager에 연결
         foundInventoryManager.inventoryPanel = properPanel;
         foundInventoryManager.slotParent = slotParent.transform;
         
         // 처음엔 비활성화
         properPanel.SetActive(false);
         
         Debug.Log("✅ 완전한 인벤토리 패널이 생성되었습니다!");
     }
     
     System.Collections.IEnumerator CloseInventoryAfterDelay(float delay)
     {
         yield return new WaitForSeconds(delay);
         if (foundInventoryManager != null && foundInventoryManager.inventoryPanel != null)
         {
             foundInventoryManager.inventoryPanel.SetActive(false);
             Debug.Log("ℹ️ 인벤토리 패널이 자동으로 닫혔습니다.");
                   }
      }
      
      [ContextMenu("Diagnose And Fix Slots")]
      public void DiagnoseAndFixSlots()
      {
          Debug.Log("🔍 [InventoryDebugger] 슬롯 시스템 진단을 시작합니다...");
          
          if (foundInventoryManager == null)
          {
              Debug.LogError("❌ InventoryManager를 찾을 수 없습니다!");
              return;
          }
          
          bool hasIssues = false;
          
          // 1. slotParent 확인
          Debug.Log($"🔍 SlotParent: {(foundInventoryManager.slotParent != null ? "✅ 존재" : "❌ null")}");
          if (foundInventoryManager.slotParent != null)
          {
              Debug.Log($"🔍 SlotParent 이름: {foundInventoryManager.slotParent.name}");
              Debug.Log($"🔍 SlotParent 자식 개수: {foundInventoryManager.slotParent.childCount}");
          }
          
          // 2. slotPrefab 확인
          Debug.Log($"🔍 SlotPrefab: {(foundInventoryManager.slotPrefab != null ? "✅ 존재" : "❌ null")}");
          if (foundInventoryManager.slotPrefab != null)
          {
              Debug.Log($"🔍 SlotPrefab 이름: {foundInventoryManager.slotPrefab.name}");
              InventorySlot slotComponent = foundInventoryManager.slotPrefab.GetComponent<InventorySlot>();
              Debug.Log($"🔍 SlotPrefab에 InventorySlot 컴포넌트: {(slotComponent != null ? "✅ 있음" : "❌ 없음")}");
          }
          
          // 3. Grid Layout Group 확인
          if (foundInventoryManager.slotParent != null)
          {
              UnityEngine.UI.GridLayoutGroup grid = foundInventoryManager.slotParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
              Debug.Log($"🔍 Grid Layout Group: {(grid != null ? "✅ 있음" : "❌ 없음")}");
              if (grid != null)
              {
                  Debug.Log($"🔍 Grid 설정 - Cell Size: {grid.cellSize}, Spacing: {grid.spacing}, Constraint Count: {grid.constraintCount}");
              }
          }
          
          // 4. 문제들 수정
          if (foundInventoryManager.slotParent == null)
          {
              Debug.LogWarning("⚠️ SlotParent가 없습니다. 생성합니다...");
              CreateSlotParent();
              hasIssues = true;
          }
          
          if (foundInventoryManager.slotPrefab == null)
          {
              Debug.LogWarning("⚠️ SlotPrefab이 없습니다. 기본 슬롯 프리팹을 생성합니다...");
              CreateDefaultSlotPrefab();
              hasIssues = true;
          }
          
          // 5. 슬롯들이 실제로 생성되었는지 확인
          if (foundInventoryManager.slotParent != null && foundInventoryManager.slotParent.childCount == 0)
          {
              Debug.LogWarning("⚠️ 슬롯들이 생성되지 않았습니다. 수동으로 생성합니다...");
              CreateInventorySlots();
              hasIssues = true;
          }
          
          // 6. InventoryManager의 CreateInventoryGrid 강제 호출
          if (foundInventoryManager.slotParent != null && foundInventoryManager.slotPrefab != null)
          {
              Debug.Log("🔧 InventoryManager.CreateInventoryGrid() 강제 호출...");
              try
              {
                  // 리플렉션을 사용해서 private 메서드 호출
                  var method = foundInventoryManager.GetType().GetMethod("CreateInventoryGrid", 
                      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                  if (method != null)
                  {
                      method.Invoke(foundInventoryManager, null);
                      Debug.Log("✅ CreateInventoryGrid() 호출 성공!");
                  }
                  else
                  {
                      Debug.LogWarning("⚠️ CreateInventoryGrid() 메서드를 찾을 수 없습니다. 수동으로 슬롯을 생성합니다.");
                      CreateInventorySlots();
                  }
              }
              catch (System.Exception ex)
              {
                  Debug.LogError($"❌ CreateInventoryGrid() 호출 실패: {ex.Message}");
                  CreateInventorySlots();
              }
          }
          
          // 7. 최종 확인
          if (foundInventoryManager.slotParent != null)
          {
              int finalSlotCount = foundInventoryManager.slotParent.childCount;
              Debug.Log($"🎯 최종 슬롯 개수: {finalSlotCount}");
              
              if (finalSlotCount > 0)
              {
                  Debug.Log("✅ 슬롯 시스템이 정상적으로 작동합니다!");
                  
                  // 테스트: 인벤토리 열어서 슬롯들 보여주기
                  if (foundInventoryManager.inventoryPanel != null)
                  {
                      foundInventoryManager.inventoryPanel.SetActive(true);
                      Debug.Log("🧪 인벤토리를 열어서 슬롯들을 확인해보세요! (5초 후 자동 닫힘)");
                      StartCoroutine(CloseInventoryAfterDelay(5f));
                  }
              }
              else
              {
                  Debug.LogError("❌ 슬롯 생성에 실패했습니다!");
              }
          }
          
          if (!hasIssues)
          {
              Debug.Log("ℹ️ 슬롯 시스템에 문제가 없습니다.");
          }
      }
      
      void CreateSlotParent()
      {
          if (foundInventoryManager.inventoryPanel == null)
          {
              Debug.LogError("❌ InventoryPanel이 없어서 SlotParent를 생성할 수 없습니다!");
              return;
          }
          
          GameObject slotParent = new GameObject("SlotParent");
          slotParent.transform.SetParent(foundInventoryManager.inventoryPanel.transform, false);
          
          RectTransform rect = slotParent.AddComponent<RectTransform>();
          rect.anchorMin = new Vector2(0.1f, 0.1f);
          rect.anchorMax = new Vector2(0.9f, 0.8f);
          rect.offsetMin = Vector2.zero;
          rect.offsetMax = Vector2.zero;
          
          // Grid Layout Group 추가
          UnityEngine.UI.GridLayoutGroup grid = slotParent.AddComponent<UnityEngine.UI.GridLayoutGroup>();
          grid.cellSize = new Vector2(70, 70);
          grid.spacing = new Vector2(10, 10);
          grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
          grid.constraintCount = 5;
          
          foundInventoryManager.slotParent = slotParent.transform;
          Debug.Log("✅ SlotParent가 생성되었습니다!");
      }
      
      void CreateDefaultSlotPrefab()
      {
          // 기본 슬롯 프리팹 생성
          GameObject slotPrefab = new GameObject("DefaultSlotPrefab");
          
          // RectTransform 추가
          RectTransform rect = slotPrefab.AddComponent<RectTransform>();
          rect.sizeDelta = new Vector2(70, 70);
          
          // 배경 이미지 추가
          UnityEngine.UI.Image bgImage = slotPrefab.AddComponent<UnityEngine.UI.Image>();
          bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
          
          // 아이콘 이미지 자식 오브젝트 생성
          GameObject iconObj = new GameObject("Icon");
          iconObj.transform.SetParent(slotPrefab.transform, false);
          
          RectTransform iconRect = iconObj.AddComponent<RectTransform>();
          iconRect.anchorMin = Vector2.zero;
          iconRect.anchorMax = Vector2.one;
          iconRect.offsetMin = new Vector2(5, 5);
          iconRect.offsetMax = new Vector2(-5, -5);
          
          UnityEngine.UI.Image iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();
          iconImage.color = Color.white;
          
          // InventorySlot 컴포넌트 추가
          InventorySlot slotComponent = slotPrefab.AddComponent<InventorySlot>();
          slotComponent.iconImage = iconImage;
          slotComponent.backgroundImage = bgImage;
          
          // CanvasGroup 추가 (드래그용)
          slotPrefab.AddComponent<CanvasGroup>();
          
          foundInventoryManager.slotPrefab = slotPrefab;
          Debug.Log("✅ 기본 슬롯 프리팹이 생성되었습니다!");
      }
      
      void CreateInventorySlots()
      {
          if (foundInventoryManager.slotParent == null || foundInventoryManager.slotPrefab == null)
          {
              Debug.LogError("❌ SlotParent 또는 SlotPrefab이 없어서 슬롯을 생성할 수 없습니다!");
              return;
          }
          
          Debug.Log($"🔧 {foundInventoryManager.maxSlots}개의 슬롯을 생성합니다...");
          
          for (int i = 0; i < foundInventoryManager.maxSlots; i++)
          {
              GameObject slotObj = Instantiate(foundInventoryManager.slotPrefab, foundInventoryManager.slotParent);
              slotObj.name = $"Slot_{i + 1}";
              
              InventorySlot slot = slotObj.GetComponent<InventorySlot>();
              if (slot != null)
              {
                  slot.slotIndex = i;
                  slot.inventoryManager = foundInventoryManager;
              }
          }
          
                     Debug.Log($"✅ {foundInventoryManager.maxSlots}개의 슬롯이 생성되었습니다!");
       }
       
       [ContextMenu("Activate All Slots")]
       public void ActivateAllSlots()
       {
           Debug.Log("🔧 [InventoryDebugger] 모든 슬롯을 활성화합니다...");
           
           if (foundInventoryManager == null)
           {
               Debug.LogError("❌ InventoryManager를 찾을 수 없습니다!");
               return;
           }
           
           if (foundInventoryManager.slotParent == null)
           {
               Debug.LogError("❌ SlotParent가 없습니다!");
               return;
           }
           
           int activatedCount = 0;
           int totalSlots = foundInventoryManager.slotParent.childCount;
           
           Debug.Log($"🔍 총 {totalSlots}개의 슬롯을 검사합니다...");
           
           // 1. 부모 오브젝트들 활성화 확인
           if (!foundInventoryManager.slotParent.gameObject.activeSelf)
           {
               Debug.LogWarning("⚠️ SlotParent가 비활성화되어 있습니다. 활성화합니다...");
               foundInventoryManager.slotParent.gameObject.SetActive(true);
           }
           
           if (foundInventoryManager.inventoryPanel != null && !foundInventoryManager.inventoryPanel.activeSelf)
           {
               Debug.LogWarning("⚠️ InventoryPanel이 비활성화되어 있습니다. 활성화합니다...");
               foundInventoryManager.inventoryPanel.SetActive(true);
           }
           
           // 2. 각 슬롯 활성화 및 수정
           for (int i = 0; i < totalSlots; i++)
           {
               Transform slotTransform = foundInventoryManager.slotParent.GetChild(i);
               GameObject slotObj = slotTransform.gameObject;
               
               Debug.Log($"🔍 슬롯 {i + 1}: {slotObj.name} - 활성화 상태: {slotObj.activeSelf}");
               
               // 슬롯 오브젝트 활성화
               if (!slotObj.activeSelf)
               {
                   slotObj.SetActive(true);
                   activatedCount++;
                   Debug.Log($"✅ 슬롯 {i + 1} 활성화됨");
               }
               
               // 슬롯 컴포넌트들 확인 및 수정
               FixSlotComponents(slotObj, i + 1);
           }
           
           Debug.Log($"🎯 활성화된 슬롯 개수: {activatedCount}개");
           
           // 3. 최종 테스트
           Debug.Log("🧪 슬롯 활성화 테스트를 위해 인벤토리를 엽니다...");
           if (foundInventoryManager.inventoryPanel != null)
           {
               foundInventoryManager.inventoryPanel.SetActive(true);
               Debug.Log("✅ 인벤토리가 열렸습니다! 슬롯들이 보이는지 확인해보세요! (7초 후 자동 닫힘)");
               StartCoroutine(CloseInventoryAfterDelay(7f));
           }
           
           // 4. 재진단
           DiagnoseInventorySystem();
       }
       
       void FixSlotComponents(GameObject slotObj, int slotNumber)
       {
           // Image 컴포넌트들 확인
           UnityEngine.UI.Image[] images = slotObj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
           
           foreach (var image in images)
           {
               // 이미지 컴포넌트 활성화
               if (!image.enabled)
               {
                   image.enabled = true;
                   Debug.Log($"  ✅ 슬롯 {slotNumber}의 Image 컴포넌트 활성화됨");
               }
               
               // 투명도 확인 (너무 투명하면 보이지 않음)
               if (image.color.a < 0.1f)
               {
                   Color newColor = image.color;
                   newColor.a = 0.8f; // 적당한 투명도로 설정
                   image.color = newColor;
                   Debug.Log($"  ✅ 슬롯 {slotNumber}의 투명도 수정됨");
               }
           }
           
           // InventorySlot 컴포넌트 확인
           InventorySlot inventorySlot = slotObj.GetComponent<InventorySlot>();
           if (inventorySlot != null)
           {
               if (!inventorySlot.enabled)
               {
                   inventorySlot.enabled = true;
                   Debug.Log($"  ✅ 슬롯 {slotNumber}의 InventorySlot 컴포넌트 활성화됨");
               }
               
               // InventoryManager 연결 확인
               if (inventorySlot.inventoryManager == null)
               {
                   inventorySlot.inventoryManager = foundInventoryManager;
                   Debug.Log($"  ✅ 슬롯 {slotNumber}에 InventoryManager 연결됨");
               }
               
               // 슬롯 인덱스 설정
               inventorySlot.slotIndex = slotNumber - 1;
           }
           
           // CanvasGroup 확인 (드래그 앤 드롭용)
           CanvasGroup canvasGroup = slotObj.GetComponent<CanvasGroup>();
           if (canvasGroup != null)
           {
               canvasGroup.alpha = 1f; // 완전 불투명
               canvasGroup.interactable = true;
               canvasGroup.blocksRaycasts = true;
           }
           
           // RectTransform 확인
           RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
           if (rectTransform != null)
           {
               // 크기가 너무 작으면 보이지 않을 수 있음
               if (rectTransform.sizeDelta.magnitude < 10f)
               {
                   rectTransform.sizeDelta = new Vector2(70, 70);
                   Debug.Log($"  ✅ 슬롯 {slotNumber}의 크기 수정됨");
               }
           }
       }
      
       void CreateBasicInventoryPanel()
    {
        GameObject canvas = FindAnyObjectByType<Canvas>()?.gameObject;
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
            return;
        }
        
        // 기본 인벤토리 패널 생성
        GameObject panel = new GameObject("InventoryPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        // Image 컴포넌트 추가
        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        // RectTransform 설정
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // InventoryManager에 연결
        if (foundInventoryManager != null)
        {
            foundInventoryManager.inventoryPanel = panel;
            Debug.Log("✅ 기본 인벤토리 패널이 생성되고 연결되었습니다!");
        }
        
        // 처음에는 비활성화
        panel.SetActive(false);
    }
    
    void OnGUI()
    {
        // 디버거가 비활성화되어 있으면 UI도 표시하지 않음
        if (!enableDebugger) return;
        
        // 화면 중앙 상단에 실시간 상태 표시
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, 10, 500, 150));
        GUILayout.Label("=== 인벤토리 디버거 ===");
        GUILayout.Label($"I키 입력: {(isInputWorking ? "✅" : "❌")}");
        GUILayout.Label($"시스템: {(HasAllComponents() ? "✅" : "❌")}");
        GUILayout.Label($"UI 연결: {(isUIConnected ? "✅" : "❌")}");
        GUILayout.Label($"슬롯: {(hasSlotsGenerated ? "✅" : "❌")} ({currentSlotCount}개)");
        
        // 🔧 비활성화된 슬롯 정보 추가
        if (foundInventoryManager != null && foundInventoryManager.slotParent != null && currentSlotCount > 0)
        {
            int inactiveSlots = 0;
            for (int i = 0; i < currentSlotCount; i++)
            {
                Transform slotTransform = foundInventoryManager.slotParent.GetChild(i);
                if (!slotTransform.gameObject.activeSelf)
                {
                    inactiveSlots++;
                }
            }
            
            if (inactiveSlots > 0)
            {
                // 경고 색상으로 표시
                GUI.color = Color.yellow;
                GUILayout.Label($"⚠️ 비활성화 슬롯: {inactiveSlots}개 (F6으로 수정)");
                GUI.color = Color.white; // 색상 원래대로
            }
            else if (currentSlotCount > 0)
            {
                // 성공 색상으로 표시
                GUI.color = Color.green;
                GUILayout.Label("✅ 모든 슬롯 활성화됨");
                GUI.color = Color.white; // 색상 원래대로
            }
        }
        
        GUILayout.Label("F6:슬롯활성화 F7:슬롯진단 F8:패널수정 F9:강제열기 F10:진단 F11:수정 F12:충돌해결");
        GUILayout.EndArea();
    }

    // 🆕 WeaponSlot 전용 메서드들
    [ContextMenu("Fix WeaponSlot UI")]
    void FixWeaponSlotUI()
    {
        Debug.Log("🔧 [InventoryDebugger] WeaponSlot UI 자동 수정 시작...");
        
        // WeaponSlot 찾기
        WeaponSlot[] weaponSlots = FindObjectsByType<WeaponSlot>(FindObjectsSortMode.None);
        
        if (weaponSlots.Length == 0)
        {
            Debug.LogError("❌ WeaponSlot을 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"🎯 {weaponSlots.Length}개의 WeaponSlot 발견됨");
        
        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            Debug.Log($"🔧 WeaponSlot '{weaponSlot.name}' 수정 중...");
            
            // 리플렉션을 사용해서 SetupUIComponents 호출
            var method = weaponSlot.GetType().GetMethod("SetupUIComponents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(weaponSlot, null);
                Debug.Log($"✅ WeaponSlot '{weaponSlot.name}' UI 컴포넌트 설정 완료");
            }
            else
            {
                Debug.LogWarning($"⚠️ WeaponSlot '{weaponSlot.name}'에서 SetupUIComponents 메서드를 찾을 수 없습니다");
            }
        }
        
        Debug.Log("🎉 WeaponSlot UI 자동 수정 완료!");
    }
    
    [ContextMenu("Diagnose WeaponSlot")]
    void DiagnoseWeaponSlot()
    {
        Debug.Log("🧪 [InventoryDebugger] WeaponSlot 진단 시작...");
        
        WeaponSlot[] weaponSlots = FindObjectsByType<WeaponSlot>(FindObjectsSortMode.None);
        
        if (weaponSlots.Length == 0)
        {
            Debug.LogError("❌ WeaponSlot이 없습니다! 씬에 WeaponSlot을 추가해야 합니다.");
            return;
        }
        
        Debug.Log($"📊 총 {weaponSlots.Length}개의 WeaponSlot 발견됨");
        
        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            Debug.Log($"\n🔍 WeaponSlot '{weaponSlot.name}' 진단:");
            Debug.Log($"   - GameObject 활성화: {weaponSlot.gameObject.activeSelf}");
            Debug.Log($"   - Component 활성화: {weaponSlot.enabled}");
            
            // 리플렉션으로 private 필드들 확인
            var iconField = weaponSlot.GetType().GetField("icon", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var backgroundField = weaponSlot.GetType().GetField("backgroundImage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var nameField = weaponSlot.GetType().GetField("weaponNameText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var ammoField = weaponSlot.GetType().GetField("ammoText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            var icon = iconField?.GetValue(weaponSlot) as UnityEngine.UI.Image;
            var background = backgroundField?.GetValue(weaponSlot) as UnityEngine.UI.Image;
            var nameText = nameField?.GetValue(weaponSlot) as UnityEngine.UI.Text;
            var ammoText = ammoField?.GetValue(weaponSlot) as UnityEngine.UI.Text;
            
            Debug.Log($"   - Icon Image: {(icon != null ? "✅ 연결됨" : "❌ 없음")}");
            Debug.Log($"   - Background Image: {(background != null ? "✅ 연결됨" : "❌ 없음")}");
            Debug.Log($"   - Name Text: {(nameText != null ? "✅ 연결됨" : "❌ 없음")}");
            Debug.Log($"   - Ammo Text: {(ammoText != null ? "✅ 연결됨" : "❌ 없음")}");
            
            // 자식 오브젝트 확인
            Debug.Log($"   - 자식 오브젝트 개수: {weaponSlot.transform.childCount}");
            for (int i = 0; i < weaponSlot.transform.childCount; i++)
            {
                Transform child = weaponSlot.transform.GetChild(i);
                Debug.Log($"     └── {child.name} (활성화: {child.gameObject.activeSelf})");
            }
            
            // RectTransform 정보
            RectTransform rect = weaponSlot.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"   - 위치: {rect.anchoredPosition}");
                Debug.Log($"   - 크기: {rect.sizeDelta}");
            }
        }
        
        Debug.Log("🧪 WeaponSlot 진단 완료");
    }
} 