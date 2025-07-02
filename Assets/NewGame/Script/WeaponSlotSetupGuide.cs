using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class WeaponSlotSetupGuide : MonoBehaviour
{
    [Header("🎯 다중 무기 슬롯 시스템 설정 가이드")]
    [TextArea(10, 15)]
    public string setupGuide = 
        "📚 3개 무기 슬롯 시스템 설정 가이드\n\n" +
        
        "1️⃣ 기본 설정:\n" +
        "• GameObject에 WeaponSlotManager 추가\n" +
        "• PlayerInventory에 weaponSlotManager 연결\n" +
        "• InventoryManager에 weaponSlotManager 연결\n\n" +
        
        "2️⃣ UI 구조 만들기:\n" +
        "Canvas → WeaponSlotsPanel\n" +
        "├── Slot1 (WeaponSlot 컴포넌트)\n" +
        "├── Slot2 (WeaponSlot 컴포넌트)\n" +
        "└── Slot3 (WeaponSlot 컴포넌트)\n\n" +
        
        "3️⃣ 조작법:\n" +
        "• Tab: 무기 교체\n" +
        "• 1/2/3: 직접 슬롯 선택\n" +
        "• Ctrl+1/2/3: 슬롯별 무기 장착\n" +
        "• Shift+1/2/3: 슬롯별 무기 해제\n\n" +
        
        "4️⃣ 테스트:\n" +
        "• InventoryTester로 무기 추가\n" +
        "• F6-F8: 다중 슬롯 전용 테스트\n\n" +
        
        "5️⃣ 문제 해결:\n" +
        "• R키: 시스템 상태 확인\n" +
        "• F11키: 종합 진단 (문제점 상세 분석)\n" +
        "• F12키: 자동 설정 (누락된 컴포넌트 생성)\n" +
        "• Ctrl+F11키: WeaponSlot 크기 수정 (프리팹 크기 120x60 적용)\n\n" +
        
        "6️⃣ 크기 문제 해결:\n" +
        "• WeaponSlot이 원하는 크기로 나오지 않으면?\n" +
        "• Ctrl+F11 사용하여 프리팹 크기로 자동 수정\n" +
        "• LayoutElement 컴포넌트가 자동으로 추가됩니다";

    [Header("🔧 개발자 전용 도구")]
    [TextArea(5, 8)]
    [Tooltip("개발자 전용 다중 무기 슬롯 시스템 설정 도구입니다.")]
    public string developerInfo = 
        "=== 🔧 개발자 전용 도구 ===\n" +
        "• Shift+Ctrl+F12: 개발자 모드 활성화/비활성화\n" +
        "• 개발자 모드에서만 F11, F12 키 작동\n" +
        "• F11: 종합 진단\n" +
        "• F12: 자동 설정\n" +
        "• Ctrl+F11: WeaponSlot 크기 수정\n\n" +
        "일반 사용자에게는 보이지 않습니다.";

    [Header("🔒 개발자 모드 설정")]
    [SerializeField] private bool isDeveloperModeEnabled = false;
    [SerializeField] private bool showDeveloperUI = false;
    
    [Header("🔧 자동 설정 도구")]
    [SerializeField] private bool enableDebugger = false;
    
    [Tooltip("체크하면 자동으로 WeaponSlot UI를 생성합니다")]
    public bool autoCreateWeaponSlots = false;
    
    [Tooltip("무기 슬롯들이 생성될 부모 Transform")]
    public Transform weaponSlotsParent;
    
    [Tooltip("무기 슬롯 프리팹 (WeaponSlot 컴포넌트 포함)")]
    public GameObject weaponSlotPrefab;
    
    [Tooltip("슬롯 간 간격 (X: 가로, Y: 세로)")]
    public Vector2 slotSpacing = new Vector2(130f, 70f);
    
    [Tooltip("각 슬롯의 크기")]
    public Vector2 slotSize = new Vector2(120, 60);  // 🔧 프리팹 크기에 맞춤
    
    [Header("📊 시스템 상태")]
    public bool hasWeaponSlotManager = false;
    public bool hasPlayerInventory = false;
    public bool hasInventoryManager = false;
    public bool hasInventoryTester = false;
    public bool hasWeaponSlots = false;
    public int weaponSlotCount = 0;
    
    [Header("🔗 발견된 컴포넌트들")]
    public WeaponSlotManager foundWeaponSlotManager;
    public PlayerInventory foundPlayerInventory;
    public InventoryManager foundInventoryManager;
    public InventoryTester foundInventoryTester;
    public WeaponSlot[] foundWeaponSlots = new WeaponSlot[0];
    
    [Header("🎨 UI 설정")]
    [Tooltip("현재 슬롯 정보를 표시할 텍스트")]
    public Text currentSlotDisplayText;
    
    [Tooltip("무기 교체 힌트를 표시할 텍스트")]
    public Text weaponSwitchHintText;

    void Start()
    {
        // 개발자 모드가 아니면 비활성화
        if (!isDeveloperModeEnabled)
        {
            showDeveloperUI = false;
            enableDebugger = false;
        }
        
        CheckSystemStatus();
        
        if (autoCreateWeaponSlots && weaponSlotsParent != null && weaponSlotPrefab != null && isDeveloperModeEnabled)
        {
            CreateWeaponSlots();
        }
        
        SetupHintTexts();
    }

    void Update()
    {
        // 🔒 개발자 모드 토글 (Shift+Ctrl+F12)
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F12))
        {
            isDeveloperModeEnabled = !isDeveloperModeEnabled;
            showDeveloperUI = isDeveloperModeEnabled;
            enableDebugger = isDeveloperModeEnabled;
            
            if (isDeveloperModeEnabled)
            {
                Debug.Log("🔧 [개발자 모드] 활성화됨 - F11: 진단, F12: 자동설정, Ctrl+F11: 크기수정");
            }
            else
            {
                Debug.Log("🔒 [개발자 모드] 비활성화됨");
            }
        }
        
        // 개발자 모드에서만 동작
        if (!isDeveloperModeEnabled) return;
        
        if (enableDebugger)
        {
            CheckSystemStatus();
            
            // 종합 진단 (F11)
            if (Input.GetKeyDown(KeyCode.F11))
            {
                PerformComprehensiveDiagnosis();
            }
            
            // 자동 설정 (F12)
            if (Input.GetKeyDown(KeyCode.F12))
            {
                AutoSetupSystem();
            }
            
            // 🆕 WeaponSlot 크기 수정 (Ctrl+F11)
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F11))
            {
                FixWeaponSlotSizes();
            }
        }
        
        // R키로 시스템 상태 새로고침 (개발자 모드에서만)
        if (Input.GetKeyDown(KeyCode.R))
        {
            CheckSystemStatus();
        }
        
        // 현재 슬롯 정보 실시간 업데이트
        UpdateSlotDisplay();
    }

    void CheckSystemStatus()
    {
        // WeaponSlotManager 확인
        foundWeaponSlotManager = FindAnyObjectByType<WeaponSlotManager>();
        hasWeaponSlotManager = foundWeaponSlotManager != null;
        
        // PlayerInventory 확인
        foundPlayerInventory = FindAnyObjectByType<PlayerInventory>();
        hasPlayerInventory = foundPlayerInventory != null;
        
        // InventoryManager 확인
        foundInventoryManager = FindAnyObjectByType<InventoryManager>();
        hasInventoryManager = foundInventoryManager != null;
        
        // InventoryTester 확인
        foundInventoryTester = FindAnyObjectByType<InventoryTester>();
        hasInventoryTester = foundInventoryTester != null;
        
        // WeaponSlot들 확인
        foundWeaponSlots = FindObjectsByType<WeaponSlot>(FindObjectsSortMode.None);
        weaponSlotCount = foundWeaponSlots.Length;
        hasWeaponSlots = weaponSlotCount >= 3;
        
        if (isDeveloperModeEnabled)
        {
            LogSystemStatus();
        }
    }

    void LogSystemStatus()
    {
        // Debug.Log("=== 🎯 다중 슬롯 시스템 상태 ===");
        // Debug.Log($"✅ WeaponSlotManager: {GetStatusIcon(hasWeaponSlotManager)}");
        // Debug.Log($"✅ PlayerInventory: {GetStatusIcon(hasPlayerInventory)}");
        // Debug.Log($"✅ InventoryManager: {GetStatusIcon(hasInventoryManager)}");
        // Debug.Log($"✅ InventoryTester: {GetStatusIcon(hasInventoryTester)}");
        // Debug.Log($"✅ WeaponSlots: {GetStatusIcon(hasWeaponSlots)} ({weaponSlotCount}/3개)");
        
        if (HasAllComponents())
        {
            Debug.Log("🎉 모든 컴포넌트가 준비되었습니다!");
            
            // 연결 상태도 확인
            CheckConnections();
        }
        else
        {
            Debug.Log("⚠️ 일부 컴포넌트가 누락되었습니다. F12키로 자동 설정을 시도해보세요.");
        }
    }

    void CheckConnections()
    {
        // Debug.Log("=== 🔗 컴포넌트 연결 상태 ===");
        
        if (foundWeaponSlotManager != null)
        {
            // 슬롯 연결 상태 체크 (경고 해결을 위해 간소화)
            for (int i = 0; i < 3; i++)
            {
                if (i >= foundWeaponSlotManager.weaponSlots.Length || foundWeaponSlotManager.weaponSlots[i] == null)
                {
                    // 연결되지 않은 슬롯이 있음
                    break;
                }
            }
            // Debug.Log($"WeaponSlotManager → WeaponSlots: {GetStatusIcon(slotsConnected)}");
        }
        
        if (foundPlayerInventory != null)
        {
            // 연결 상태만 체크 (변수 저장 불필요)
            // Debug.Log($"PlayerInventory → WeaponSlotManager: {GetStatusIcon(managerConnected)}");
        }
        
        if (foundInventoryManager != null)
        {
            // 연결 상태만 체크 (변수 저장 불필요)
            // Debug.Log($"InventoryManager → WeaponSlotManager: {GetStatusIcon(managerConnected)}");
        }
    }

    void AutoSetupSystem()
    {
        Debug.Log("🔧 [WeaponSlotSetupGuide] 자동 설정 시작...");
        
        // 0. 인벤토리 상태 확인 및 임시 활성화
        bool wasInventoryOpen = false;
        bool needToCloseInventory = false;
        
        if (foundInventoryManager != null && foundInventoryManager.inventoryPanel != null)
        {
            wasInventoryOpen = foundInventoryManager.inventoryPanel.activeSelf;
            if (!wasInventoryOpen)
            {
                // Debug.Log("🔧 자동 설정을 위해 인벤토리를 잠시 활성화합니다...");
                foundInventoryManager.inventoryPanel.SetActive(true);
                needToCloseInventory = true;
            }
        }
        
        // 1. WeaponSlotManager 생성/연결
        if (!hasWeaponSlotManager)
        {
            GameObject managerObj = new GameObject("WeaponSlotManager");
            foundWeaponSlotManager = managerObj.AddComponent<WeaponSlotManager>();
            hasWeaponSlotManager = true;
            Debug.Log("✅ WeaponSlotManager 생성 완료");
        }
        
        // 2. WeaponSlot 생성 또는 기존 GameObject에 컴포넌트 추가
        bool slotsFixed = FixExistingWeaponSlots();
        
        if (!slotsFixed && weaponSlotCount < 3)
        {
            CreateWeaponSlots();
        }
        
        // 3. 컴포넌트 간 연결
        ConnectComponents();
        
        // 4. 인벤토리 상태 복원
        if (needToCloseInventory && foundInventoryManager != null && foundInventoryManager.inventoryPanel != null)
        {
            // Debug.Log("🔧 자동 설정 완료 후 인벤토리를 원래 상태로 복원합니다.");
            foundInventoryManager.inventoryPanel.SetActive(false);
        }
        
        // 5. 상태 재확인
        CheckSystemStatus();
        
        Debug.Log("🎯 [WeaponSlotSetupGuide] 자동 설정 완료!");
    }

    // 🔧 기존 WeaponSlot GameObject들에 컴포넌트 추가
    bool FixExistingWeaponSlots()
    {
        // Debug.Log("🔧 기존 WeaponSlot GameObject들에 컴포넌트 추가 시도...");
        
        GameObject weaponSlotsPanel = FindWeaponSlotsPanel();
        if (weaponSlotsPanel == null)
        {
            // Debug.Log("WeaponSlotsPanel을 찾을 수 없어서 기존 슬롯 수정을 건너뜁니다.");
            return false;
        }
        
        Transform panelTransform = weaponSlotsPanel.transform;
        List<WeaponSlot> fixedSlots = new List<WeaponSlot>();
        int fixedCount = 0;
        
        for (int i = 0; i < panelTransform.childCount; i++)
        {
            Transform child = panelTransform.GetChild(i);
            
            if (child.name.Contains("WeaponSlot"))
            {
                WeaponSlot existingSlot = child.GetComponent<WeaponSlot>();
                
                if (existingSlot == null)
                {
                    // WeaponSlot 컴포넌트 추가
                    WeaponSlot newSlot = child.gameObject.AddComponent<WeaponSlot>();
                    
                    // UI 컴포넌트 자동 설정
                    SetupWeaponSlotComponents(newSlot);
                    
                    fixedSlots.Add(newSlot);
                    fixedCount++;
                    
                    Debug.Log($"✅ {child.name}에 WeaponSlot 컴포넌트 추가 완료");
                }
                else
                {
                    // 기존 컴포넌트가 있으면 그대로 사용
                    fixedSlots.Add(existingSlot);
                    // Debug.Log($"✅ {child.name}에 이미 WeaponSlot 컴포넌트 있음");
                }
            }
        }
        
        // WeaponSlotManager에 연결
        if (foundWeaponSlotManager != null && fixedSlots.Count >= 3)
        {
            // 배열 크기 조정
            foundWeaponSlotManager.weaponSlots = new WeaponSlot[3];
            
            for (int i = 0; i < 3 && i < fixedSlots.Count; i++)
            {
                foundWeaponSlotManager.weaponSlots[i] = fixedSlots[i];
            }
            
            Debug.Log($"✅ WeaponSlotManager에 {fixedSlots.Count}개 슬롯 연결 완료");
            
            // 상태 업데이트
            weaponSlotCount = fixedSlots.Count;
            hasWeaponSlots = weaponSlotCount >= 3;
            
            return true;
        }
        
        // Debug.Log($"기존 슬롯 수정 결과: {fixedCount}개 컴포넌트 추가, 총 {fixedSlots.Count}개 슬롯");
        return fixedSlots.Count >= 3;
    }

    // 🔧 WeaponSlot 컴포넌트의 UI 요소들 자동 설정
    void SetupWeaponSlotComponents(WeaponSlot weaponSlot)
    {
        // Background Image 설정
        Image backgroundImage = weaponSlot.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = weaponSlot.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
        weaponSlot.backgroundImage = backgroundImage;
        
        // Icon Image 찾기 또는 생성
        Transform iconTransform = weaponSlot.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                weaponSlot.icon = iconImage;
                // Debug.Log($"    - 기존 Icon 이미지 연결됨");
            }
        }
        else
        {
            // Icon 자식 오브젝트 생성
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(weaponSlot.transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5, 5);
            iconRect.offsetMax = new Vector2(-5, -5);
            
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;
            
            weaponSlot.icon = iconImg;
            // Debug.Log($"    - 새 Icon 이미지 생성됨");
        }
        
        // Debug.Log($"    - WeaponSlot UI 컴포넌트 설정 완료");
    }

    void CreateWeaponSlots()
    {
        if (weaponSlotsParent == null)
        {
            // 부모 오브젝트 생성
            GameObject parentObj = new GameObject("WeaponSlotsPanel");
            parentObj.transform.SetParent(FindAnyObjectByType<Canvas>()?.transform, false);
            
            // RectTransform 설정
            RectTransform parentRect = parentObj.AddComponent<RectTransform>();
            parentRect.anchorMin = new Vector2(0, 1);
            parentRect.anchorMax = new Vector2(0, 1);
            parentRect.anchoredPosition = new Vector2(100, -100);
            parentRect.sizeDelta = new Vector2(300, 100);
            
            weaponSlotsParent = parentObj.transform;
            Debug.Log("✅ WeaponSlotsPanel 생성 완료");
        }
        
        // 기존 슬롯 제거 (3개 미만인 경우만)
        if (weaponSlotCount < 3)
        {
            foreach (Transform child in weaponSlotsParent)
            {
                if (child.GetComponent<WeaponSlot>() != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        // 3개 슬롯 생성
        WeaponSlot[] newSlots = new WeaponSlot[3];
        
        for (int i = 0; i < 3; i++)
        {
            GameObject slotObj;
            
            if (weaponSlotPrefab != null)
            {
                slotObj = Instantiate(weaponSlotPrefab, weaponSlotsParent);
            }
            else
            {
                slotObj = CreateBasicWeaponSlot();
                slotObj.transform.SetParent(weaponSlotsParent, false);
            }
            
            slotObj.name = $"WeaponSlot_{i + 1}";
            
            // 위치 설정
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 1);
            slotRect.anchorMax = new Vector2(0, 1);
            slotRect.anchoredPosition = new Vector2(slotSpacing.x * i, -slotSpacing.y * i);
            
            // 🆕 프리팹 크기 유지를 위한 LayoutElement 추가
            EnsureProperSizing(slotObj);
            
            // WeaponSlot 컴포넌트 설정
            WeaponSlot weaponSlot = slotObj.GetComponent<WeaponSlot>();
            if (weaponSlot == null)
            {
                weaponSlot = slotObj.AddComponent<WeaponSlot>();
            }
            
            SetupWeaponSlotComponents(weaponSlot);
            newSlots[i] = weaponSlot;
            
            // Debug.Log($"✅ WeaponSlot_{i + 1} 생성 완료 (크기: {slotSize.x}x{slotSize.y})");
        }
        
        // WeaponSlotManager 연결
        if (foundWeaponSlotManager != null)
        {
            foundWeaponSlotManager.weaponSlots = newSlots;
            Debug.Log("✅ WeaponSlotManager에 슬롯 배열 연결 완료");
        }
        
        weaponSlotCount = 3;
        hasWeaponSlots = true;
        foundWeaponSlots = newSlots;
        
        Debug.Log("🎉 무기 슬롯 생성 완료!");
    }

    // 🆕 프리팹 크기를 유지하기 위한 메서드
    void EnsureProperSizing(GameObject weaponSlotObj)
    {
        RectTransform rectTransform = weaponSlotObj.GetComponent<RectTransform>();
        
        // 원하는 크기 설정 (프리팹 크기: 120x60)
        Vector2 desiredSize = new Vector2(120f, 60f);
        rectTransform.sizeDelta = desiredSize;
        
        // LayoutElement 컴포넌트 추가/확인
        UnityEngine.UI.LayoutElement layoutElement = weaponSlotObj.GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = weaponSlotObj.AddComponent<UnityEngine.UI.LayoutElement>();
        }
        
        // 프리팹 크기를 강제로 유지
        layoutElement.preferredWidth = desiredSize.x;
        layoutElement.preferredHeight = desiredSize.y;
        layoutElement.minWidth = desiredSize.x;
        layoutElement.minHeight = desiredSize.y;
        
        // Layout 시스템이 크기를 제어하지 못하도록 설정
        layoutElement.ignoreLayout = false; // Layout은 인식하되, 크기는 고정
        
        Debug.Log($"📏 {weaponSlotObj.name} 크기 설정: {desiredSize.x}x{desiredSize.y}");
    }

    GameObject CreateBasicWeaponSlot()
    {
        GameObject slotObj = new GameObject("WeaponSlot");
        
        // RectTransform
        RectTransform rect = slotObj.AddComponent<RectTransform>();
        rect.sizeDelta = slotSize;
        
        // Background Image
        Image bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // WeaponSlot 컴포넌트
        WeaponSlot weaponSlot = slotObj.AddComponent<WeaponSlot>();
        weaponSlot.backgroundImage = bg;
        
        // Icon Image (자식 오브젝트)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(5, 5);
        iconRect.offsetMax = new Vector2(-5, -5);
        
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.enabled = false;
        
        weaponSlot.icon = iconImg;
        
        return slotObj;
    }

    void ConnectComponents()
    {
        // Debug.Log("🔗 [WeaponSlotSetupGuide] 컴포넌트 연결 중...");
        
        // PlayerInventory 연결
        if (foundPlayerInventory != null && foundWeaponSlotManager != null)
        {
            if (foundPlayerInventory.weaponSlotManager != foundWeaponSlotManager)
            {
                foundPlayerInventory.weaponSlotManager = foundWeaponSlotManager;
                Debug.Log("✅ PlayerInventory → WeaponSlotManager 연결");
            }
        }
        
        // InventoryManager 연결
        if (foundInventoryManager != null && foundWeaponSlotManager != null)
        {
            if (foundInventoryManager.weaponSlotManager != foundWeaponSlotManager)
            {
                foundInventoryManager.weaponSlotManager = foundWeaponSlotManager;
                Debug.Log("✅ InventoryManager → WeaponSlotManager 연결");
            }
        }
        
        // InventoryTester 연결
        if (foundInventoryTester != null && foundWeaponSlotManager != null)
        {
            if (foundInventoryTester.weaponSlotManager != foundWeaponSlotManager)
            {
                foundInventoryTester.weaponSlotManager = foundWeaponSlotManager;
                Debug.Log("✅ InventoryTester → WeaponSlotManager 연결");
            }
        }
    }

    void SetupHintTexts()
    {
        if (weaponSwitchHintText != null)
        {
            weaponSwitchHintText.text = "Tab키: 무기 교체 | 1/2/3키: 직접 선택";
        }
    }

    void UpdateSlotDisplay()
    {
        if (currentSlotDisplayText != null && foundWeaponSlotManager != null)
        {
            WeaponData currentWeapon = foundWeaponSlotManager.GetCurrentWeapon();
            string weaponName = currentWeapon != null ? currentWeapon.weaponName : "비어있음";
            currentSlotDisplayText.text = $"슬롯 {foundWeaponSlotManager.currentSlotIndex + 1}: {weaponName}";
        }
    }

    bool HasAllComponents()
    {
        return hasWeaponSlotManager && hasPlayerInventory && hasInventoryManager && hasWeaponSlots;
    }

    string GetStatusIcon(bool status)
    {
        return status ? "✅" : "❌";
    }

    // 컨텍스트 메뉴들
    [ContextMenu("Check System Status")]
    void ContextCheckStatus()
    {
        CheckSystemStatus();
    }

    [ContextMenu("Comprehensive Diagnosis")]
    void ContextDiagnosis()
    {
        PerformComprehensiveDiagnosis();
    }

    [ContextMenu("Auto Setup System")]
    void ContextAutoSetup()
    {
        AutoSetupSystem();
    }

    [ContextMenu("Create Weapon Slots")]
    void ContextCreateSlots()
    {
        CreateWeaponSlots();
    }

    [ContextMenu("Test Multi-Slot Speed")]
    void ContextTestSpeed()
    {
        if (foundWeaponSlotManager != null)
        {
            WeaponMovementHelper.LogMultiSlotSpeedInfo(foundWeaponSlotManager);
        }
        else
        {
            Debug.LogWarning("⚠️ WeaponSlotManager가 없어서 속도 테스트를 할 수 없습니다!");
        }
    }

    void OnGUI()
    {
        // 개발자 모드에서만 UI 표시
        if (!showDeveloperUI) return;
        
        // 화면 우하단에 상태 표시
        GUILayout.BeginArea(new Rect(Screen.width - 300, Screen.height - 200, 290, 190));
        GUILayout.Label("=== 🔧 개발자 도구 ===");
        GUILayout.Label($"WeaponSlotManager: {GetStatusIcon(hasWeaponSlotManager)}");
        GUILayout.Label($"PlayerInventory: {GetStatusIcon(hasPlayerInventory)}");
        GUILayout.Label($"InventoryManager: {GetStatusIcon(hasInventoryManager)}");
        GUILayout.Label($"WeaponSlots: {GetStatusIcon(hasWeaponSlots)} ({weaponSlotCount}/3)");
        GUILayout.Label($"InventoryTester: {GetStatusIcon(hasInventoryTester)}");
        
        if (foundWeaponSlotManager != null)
        {
            GUILayout.Label($"현재 슬롯: {foundWeaponSlotManager.currentSlotIndex + 1}");
            WeaponData currentWeapon = foundWeaponSlotManager.GetCurrentWeapon();
            GUILayout.Label($"현재 무기: {(currentWeapon != null ? currentWeapon.weaponName : "없음")}");
        }
        
        GUILayout.Label("Shift+Ctrl+F12: 개발자모드");
        GUILayout.Label("F11: 진단 | F12: 자동설정");
        GUILayout.Label("Ctrl+F11: 크기수정 | R: 새로고침");
        GUILayout.EndArea();
    }

    // 🔍 새로운 종합 진단 시스템
    void PerformComprehensiveDiagnosis()
    {
        Debug.Log("🔍🔍🔍 === 종합 진단 시작 === 🔍🔍🔍");
        
        // 1단계: 기본 컴포넌트 존재 여부
        DiagnoseBasicComponents();
        
        // 2단계: 컴포넌트 간 연결 상태
        DiagnoseConnections();
        
        // 3단계: UI 구조 분석
        DiagnoseUIStructure();
        
        // 4단계: 설정 상태 분석
        DiagnoseSettings();
        
        // 5단계: 문제점 및 해결방안 제시
        ProvideRecommendations();
        
        Debug.Log("🔍🔍🔍 === 종합 진단 완료 === 🔍🔍🔍");
    }

    void DiagnoseBasicComponents()
    {
        // Debug.Log("🔧 [1단계] 기본 컴포넌트 진단:");
        
        // WeaponSlotManager 진단
        if (foundWeaponSlotManager == null)
        {
            Debug.LogError("❌ WeaponSlotManager가 없습니다!");
            Debug.Log("💡 해결방법: GameObject에 WeaponSlotManager 컴포넌트를 추가하세요.");
        }
        else
        {
            // Debug.Log("✅ WeaponSlotManager 발견");
            
            // WeaponSlotManager 내부 설정 체크
            if (foundWeaponSlotManager.weaponSlots == null || foundWeaponSlotManager.weaponSlots.Length < 3)
            {
                Debug.LogWarning($"⚠️ WeaponSlotManager.weaponSlots 배열 문제 (현재: {foundWeaponSlotManager.weaponSlots?.Length ?? 0}/3)");
            }
            else
            {
                int nullSlots = 0;
                for (int i = 0; i < foundWeaponSlotManager.weaponSlots.Length; i++)
                {
                    if (foundWeaponSlotManager.weaponSlots[i] == null)
                        nullSlots++;
                }
                if (nullSlots > 0)
                {
                    Debug.LogWarning($"⚠️ WeaponSlotManager에 {nullSlots}개 슬롯이 null로 연결되어 있습니다.");
                }
            }
        }
        
        // PlayerInventory 진단
        if (foundPlayerInventory == null)
        {
            Debug.LogError("❌ PlayerInventory가 없습니다!");
            Debug.Log("💡 해결방법: Player GameObject에 PlayerInventory 컴포넌트를 추가하세요.");
        }
        else
        {
            // Debug.Log("✅ PlayerInventory 발견");
            
            // weaponSlotManager 연결 체크
            if (foundPlayerInventory.weaponSlotManager == null)
            {
                Debug.LogWarning("⚠️ PlayerInventory.weaponSlotManager가 연결되지 않았습니다.");
            }
            
            // weaponHolder 체크
            if (foundPlayerInventory.weaponHolder == null)
            {
                Debug.LogWarning("⚠️ PlayerInventory.weaponHolder가 설정되지 않았습니다.");
                Debug.Log("💡 해결방법: Player 자식 오브젝트에 무기가 생성될 Transform을 weaponHolder에 연결하세요.");
            }
        }
        
        // InventoryManager 진단
        if (foundInventoryManager == null)
        {
            Debug.LogError("❌ InventoryManager가 없습니다!");
            Debug.Log("💡 해결방법: GameObject에 InventoryManager 컴포넌트를 추가하세요.");
        }
        else
        {
            // Debug.Log("✅ InventoryManager 발견");
            
            // weaponSlotManager 연결 체크
            if (foundInventoryManager.weaponSlotManager == null)
            {
                Debug.LogWarning("⚠️ InventoryManager.weaponSlotManager가 연결되지 않았습니다.");
            }
            
            // 기본 UI 설정 체크
            if (foundInventoryManager.inventoryPanel == null)
            {
                Debug.LogWarning("⚠️ InventoryManager.inventoryPanel이 설정되지 않았습니다.");
            }
            if (foundInventoryManager.slotParent == null)
            {
                Debug.LogWarning("⚠️ InventoryManager.slotParent가 설정되지 않았습니다.");
            }
            if (foundInventoryManager.slotPrefab == null)
            {
                Debug.LogWarning("⚠️ InventoryManager.slotPrefab이 설정되지 않았습니다.");
            }
        }
        
        // 🔍 WeaponSlot 상세 진단 (개선된 버전)
        DiagnoseWeaponSlotsDetailed();
        
        // Canvas 체크
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas가 없습니다!");
            Debug.Log("💡 해결방법: UI를 위한 Canvas를 생성하세요.");
        }
    }

    // 🔍 새로운 WeaponSlot 상세 진단 메서드
    void DiagnoseWeaponSlotsDetailed()
    {
        // Debug.Log("🔍 [WeaponSlot 상세 진단]:");
        
        // 1. FindObjectsByType으로 찾은 WeaponSlot 컴포넌트들
        // Debug.Log($"FindObjectsByType<WeaponSlot>() 결과: {weaponSlotCount}개");
        
        // 2. 인벤토리 상태 확인 및 임시 활성화
        bool wasInventoryOpen = false;
        bool needToCloseInventory = false;
        
        if (foundInventoryManager != null)
        {
            // 현재 인벤토리가 열려있는지 확인
            if (foundInventoryManager.inventoryPanel != null)
            {
                wasInventoryOpen = foundInventoryManager.inventoryPanel.activeSelf;
                // Debug.Log($"인벤토리 현재 상태: {(wasInventoryOpen ? "열림" : "닫힘")}");
                
                // 인벤토리가 닫혀있으면 진단을 위해 잠시 열기
                if (!wasInventoryOpen)
                {
                    // Debug.Log("🔧 진단을 위해 인벤토리를 잠시 활성화합니다...");
                    foundInventoryManager.inventoryPanel.SetActive(true);
                    needToCloseInventory = true;
                }
            }
        }
        
        // 3. WeaponSlotsPanel 탐색 (개선된 버전)
        GameObject weaponSlotsPanel = FindWeaponSlotsPanel();
        int actualWeaponSlotCount = 0;
        
        if (weaponSlotsPanel != null)
        {
            // Debug.Log($"✅ WeaponSlotsPanel 발견: {weaponSlotsPanel.name} (활성화: {weaponSlotsPanel.activeSelf})");
            actualWeaponSlotCount = AnalyzeWeaponSlotsPanel(weaponSlotsPanel);
        }
        else
        {
            Debug.LogWarning("⚠️ WeaponSlotsPanel을 찾을 수 없습니다.");
            
            // 대체 탐색 방법
            actualWeaponSlotCount = SearchAllCanvasesForWeaponSlots();
        }
        
        // 4. 전역 변수 업데이트 (중요!)
        if (actualWeaponSlotCount > 0)
        {
            // Debug.Log($"🔧 실제 발견된 WeaponSlot 개수로 업데이트: {weaponSlotCount} → {actualWeaponSlotCount}");
            weaponSlotCount = actualWeaponSlotCount;
            hasWeaponSlots = weaponSlotCount >= 3;
            
            // foundWeaponSlots 배열도 업데이트
            foundWeaponSlots = FindObjectsByType<WeaponSlot>(FindObjectsSortMode.None);
        }
        
        // 5. 인벤토리 상태 복원
        if (needToCloseInventory && foundInventoryManager != null && foundInventoryManager.inventoryPanel != null)
        {
            // Debug.Log("🔧 진단 완료 후 인벤토리를 원래 상태로 복원합니다.");
            foundInventoryManager.inventoryPanel.SetActive(false);
        }
        
        // 6. 전체 결론
        // Debug.Log("📊 [WeaponSlot 진단 결과]:");
        // Debug.Log($"WeaponSlot 컴포넌트 개수: {weaponSlotCount} (필요: 3개)");
        
        if (weaponSlotCount == 0)
        {
            Debug.LogError("❌ WeaponSlot 컴포넌트가 하나도 없습니다!");
            Debug.Log("💡 해결방법: F12키로 자동 생성하거나 수동으로 WeaponSlot UI를 만드세요.");
        }
        else if (weaponSlotCount < 3)
        {
            Debug.LogWarning($"⚠️ WeaponSlot이 {weaponSlotCount}개만 있습니다. 3개가 필요합니다.");
        }
        else
        {
            Debug.Log("✅ WeaponSlot이 충분히 있습니다!");
        }
    }

    // 🔍 WeaponSlotsPanel을 다양한 방법으로 찾기
    GameObject FindWeaponSlotsPanel()
    {
        // 방법 1: 직접 찾기 (활성화된 경우)
        GameObject panel = GameObject.Find("WeaponSlotsPanel");
        if (panel != null)
        {
            // Debug.Log("✅ GameObject.Find()로 WeaponSlotsPanel 발견");
            return panel;
        }
        
        // 방법 2: InventoryManager를 통해 찾기
        if (foundInventoryManager != null && foundInventoryManager.inventoryPanel != null)
        {
            // Debug.Log("🔍 InventoryManager.inventoryPanel 하위에서 WeaponSlotsPanel 탐색 중...");
            Transform inventoryTransform = foundInventoryManager.inventoryPanel.transform;
            
            // 재귀적으로 WeaponSlotsPanel 찾기
            Transform weaponSlotsTransform = FindChildRecursive(inventoryTransform, "WeaponSlotsPanel");
            if (weaponSlotsTransform != null)
            {
                // Debug.Log("✅ InventoryPanel 하위에서 WeaponSlotsPanel 발견");
                return weaponSlotsTransform.gameObject;
            }
        }
        
        // 방법 3: Resources.FindObjectsOfTypeAll로 비활성화된 것도 찾기
        // Debug.Log("🔍 비활성화된 오브젝트 포함하여 WeaponSlotsPanel 탐색 중...");
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.name == "WeaponSlotsPanel" && obj.scene.isLoaded)
            {
                // Debug.Log($"✅ Resources.FindObjectsOfTypeAll()로 WeaponSlotsPanel 발견 (활성화: {obj.activeSelf})");
                return obj;
            }
        }
        
        return null;
    }

    // 🔍 재귀적으로 자식 오브젝트 찾기
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    // 🔍 WeaponSlotsPanel 분석 (개수 반환하도록 수정)
    int AnalyzeWeaponSlotsPanel(GameObject weaponSlotsPanel)
    {
        Transform panelTransform = weaponSlotsPanel.transform;
        // Debug.Log($"WeaponSlotsPanel 자식 오브젝트 수: {panelTransform.childCount}");
        
        int actualWeaponSlots = 0;
        int gameObjectsWithSlotName = 0;
        
        for (int i = 0; i < panelTransform.childCount; i++)
        {
            Transform child = panelTransform.GetChild(i);
            // Debug.Log($"  자식 {i + 1}: '{child.name}' - 활성화: {child.gameObject.activeSelf}");
            
            // WeaponSlot으로 보이는 이름 체크
            if (child.name.Contains("WeaponSlot"))
            {
                gameObjectsWithSlotName++;
                
                // WeaponSlot 컴포넌트 체크
                WeaponSlot slotComponent = child.GetComponent<WeaponSlot>();
                if (slotComponent != null)
                {
                    actualWeaponSlots++;
                    // Debug.Log($"    ✅ WeaponSlot 컴포넌트 있음 - 활성화: {slotComponent.enabled}");
                    
                    // 컴포넌트 내부 설정 체크
                    // Debug.Log($"    - backgroundImage: {(slotComponent.backgroundImage != null ? "✅" : "❌")}");
                    // Debug.Log($"    - icon: {(slotComponent.icon != null ? "✅" : "❌")}");
                }
                else
                {
                    Debug.LogWarning($"    ❌ '{child.name}'에 WeaponSlot 컴포넌트가 없습니다!");
                    Debug.Log($"    💡 해결방법: {child.name}에 WeaponSlot 컴포넌트를 추가하세요.");
                }
            }
        }
        
        // Debug.Log($"WeaponSlot 이름을 가진 GameObject: {gameObjectsWithSlotName}개");
        // Debug.Log($"실제 WeaponSlot 컴포넌트: {actualWeaponSlots}개");
        
        // 문제 진단
        if (gameObjectsWithSlotName >= 3 && actualWeaponSlots == 0)
        {
            Debug.LogError("🚨 문제 발견: GameObject는 있지만 WeaponSlot 컴포넌트가 없습니다!");
            Debug.Log("💡 해결방법: 각 WeaponSlot_ GameObject에 WeaponSlot 컴포넌트를 추가하세요.");
            Debug.Log("   1. WeaponSlot_1 선택 → Add Component → WeaponSlot");
            Debug.Log("   2. WeaponSlot_2 선택 → Add Component → WeaponSlot");
            Debug.Log("   3. WeaponSlot_3 선택 → Add Component → WeaponSlot");
        }
        else if (gameObjectsWithSlotName >= 3 && actualWeaponSlots < gameObjectsWithSlotName)
        {
            Debug.LogWarning($"⚠️ 일부 GameObject에만 WeaponSlot 컴포넌트가 있습니다. ({actualWeaponSlots}/{gameObjectsWithSlotName})");
        }
        else if (actualWeaponSlots >= 3)
        {
            Debug.Log("✅ WeaponSlot 컴포넌트가 충분히 있습니다!");
        }
        
        return actualWeaponSlots; // 실제 개수 반환
    }

    // 🔍 모든 Canvas에서 WeaponSlot 탐색 (개수 반환하도록 수정)
    int SearchAllCanvasesForWeaponSlots()
    {
        // Debug.Log("🔍 모든 Canvas에서 WeaponSlot 관련 오브젝트 탐색 중...");
        
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        // Debug.Log($"발견된 Canvas 수: {canvases.Length}");
        
        int totalWeaponSlots = 0;
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.scene.isLoaded) // 씬에 로드된 Canvas만
            {
                // Debug.Log($"Canvas '{canvas.name}' 탐색 중... (활성화: {canvas.gameObject.activeSelf})");
                totalWeaponSlots += SearchForWeaponSlotObjects(canvas.transform, 0);
            }
        }
        
        return totalWeaponSlots;
    }

    // 🔍 재귀적으로 WeaponSlot 관련 오브젝트 찾기 (개선된 버전)
    int SearchForWeaponSlotObjects(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        int weaponSlots = 0;
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            
            if (child.name.Contains("WeaponSlot") || child.name.Contains("Slot"))
            {
                WeaponSlot slot = child.GetComponent<WeaponSlot>();
                // Debug.Log($"{indent}📍 발견: '{child.name}' - WeaponSlot 컴포넌트: {(slot != null ? "✅" : "❌")} (활성화: {child.gameObject.activeSelf})");
                
                if (child.name.Contains("WeaponSlot") && slot == null)
                {
                    Debug.Log($"{indent}   💡 해결방법: {child.name}에 WeaponSlot 컴포넌트를 추가하세요.");
                }
                
                // 실제 WeaponSlot 컴포넌트가 있을 때만 카운트
                if (slot != null)
                {
                    weaponSlots++;
                }
            }
            
            // WeaponSlotsPanel도 찾기
            if (child.name.Contains("WeaponSlotsPanel"))
            {
                // Debug.Log($"{indent}📋 WeaponSlotsPanel 발견: '{child.name}' (활성화: {child.gameObject.activeSelf})");
            }
            
            // 깊이 제한 (무한 루프 방지)
            if (depth < 4)
            {
                weaponSlots += SearchForWeaponSlotObjects(child, depth + 1);
            }
        }
        
        return weaponSlots;
    }

    void DiagnoseConnections()
    {
        // Debug.Log("🔗 [2단계] 컴포넌트 연결 진단:");
        
        if (foundWeaponSlotManager != null && foundPlayerInventory != null)
        {
            bool connected = foundPlayerInventory.weaponSlotManager == foundWeaponSlotManager;
            // Debug.Log($"PlayerInventory ↔ WeaponSlotManager: {GetStatusIcon(connected)}");
            
            if (!connected)
            {
                Debug.LogWarning("⚠️ PlayerInventory와 WeaponSlotManager가 연결되지 않았습니다.");
                Debug.Log("💡 해결방법: PlayerInventory Inspector에서 weaponSlotManager 필드에 WeaponSlotManager를 드래그하세요.");
            }
        }
        
        if (foundWeaponSlotManager != null && foundInventoryManager != null)
        {
            bool connected = foundInventoryManager.weaponSlotManager == foundWeaponSlotManager;
            // Debug.Log($"InventoryManager ↔ WeaponSlotManager: {GetStatusIcon(connected)}");
            
            if (!connected)
            {
                Debug.LogWarning("⚠️ InventoryManager와 WeaponSlotManager가 연결되지 않았습니다.");
                Debug.Log("💡 해결방법: InventoryManager Inspector에서 weaponSlotManager 필드에 WeaponSlotManager를 드래그하세요.");
            }
        }
        
        if (foundWeaponSlotManager != null && foundInventoryTester != null)
        {
            bool connected = foundInventoryTester.weaponSlotManager == foundWeaponSlotManager;
            // Debug.Log($"InventoryTester ↔ WeaponSlotManager: {GetStatusIcon(connected)}");
        }
        
        // WeaponSlot 배열 연결 체크
        if (foundWeaponSlotManager != null)
        {
            // Debug.Log("WeaponSlotManager → WeaponSlots 배열:");
            
            if (foundWeaponSlotManager.weaponSlots == null)
            {
                Debug.LogError("❌ weaponSlots 배열이 null입니다!");
                Debug.Log("💡 해결방법: WeaponSlotManager Inspector에서 weaponSlots 배열 크기를 3으로 설정하고 WeaponSlot들을 드래그하세요.");
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < foundWeaponSlotManager.weaponSlots.Length)
                    {
                        bool slotConnected = foundWeaponSlotManager.weaponSlots[i] != null;
                        // Debug.Log($"  슬롯 {i + 1}: {GetStatusIcon(slotConnected)}");
                        
                        if (!slotConnected)
                        {
                            Debug.LogWarning($"⚠️ weaponSlots[{i}]이 null입니다.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ weaponSlots 배열에 슬롯 {i + 1}이 없습니다!");
                    }
                }
            }
        }
    }

    void DiagnoseUIStructure()
    {
        // Debug.Log("🎨 [3단계] UI 구조 진단:");
        
        // Canvas 체크
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        // Debug.Log($"Canvas 개수: {canvases.Length}");
        
        if (canvases.Length == 0)
        {
            Debug.LogError("❌ Canvas가 없습니다!");
            return;
        }
        
        // WeaponSlot UI 위치 분석
        foreach (WeaponSlot slot in foundWeaponSlots)
        {
            if (slot != null)
            {
                Transform parent = slot.transform.parent;
                string hierarchy = GetUIHierarchy(slot.transform);
                // Debug.Log($"WeaponSlot '{slot.name}' 위치: {hierarchy}");
                
                // 필수 컴포넌트 체크
                Image bg = slot.backgroundImage;
                Image icon = slot.icon;
                
                // Debug.Log($"  - backgroundImage: {(bg != null ? "✅" : "❌")}");
                // Debug.Log($"  - icon: {(icon != null ? "✅" : "❌")}");
                
                if (bg == null)
                {
                    Debug.LogWarning($"⚠️ {slot.name}의 backgroundImage가 설정되지 않았습니다.");
                }
                if (icon == null)
                {
                    Debug.LogWarning($"⚠️ {slot.name}의 icon이 설정되지 않았습니다.");
                }
            }
        }
        
        // 인벤토리 UI 구조 체크
        if (foundInventoryManager != null)
        {
            // Debug.Log("InventoryManager UI 구조:");
            // Debug.Log($"  - inventoryPanel: {(foundInventoryManager.inventoryPanel != null ? "✅" : "❌")}");
            // Debug.Log($"  - slotParent: {(foundInventoryManager.slotParent != null ? "✅" : "❌")}");
            // Debug.Log($"  - slotPrefab: {(foundInventoryManager.slotPrefab != null ? "✅" : "❌")}");
        }
    }

    void DiagnoseSettings()
    {
        // Debug.Log("⚙️ [4단계] 설정 상태 진단:");
        
        // WeaponSlotManager 설정
        if (foundWeaponSlotManager != null)
        {
            // Debug.Log("WeaponSlotManager 설정:");
            // Debug.Log($"  - currentSlotIndex: {foundWeaponSlotManager.currentSlotIndex}");
            // Debug.Log($"  - activeSlotColor: {foundWeaponSlotManager.activeSlotColor}");
            // Debug.Log($"  - inactiveSlotColor: {foundWeaponSlotManager.inactiveSlotColor}");
            
            // UI 텍스트 연결 체크
            bool hasCurrentSlotText = foundWeaponSlotManager.currentSlotText != null;
            bool hasHintText = foundWeaponSlotManager.weaponSwitchHintText != null;
            
            // Debug.Log($"  - currentSlotText: {GetStatusIcon(hasCurrentSlotText)}");
            // Debug.Log($"  - weaponSwitchHintText: {GetStatusIcon(hasHintText)}");
        }
        
        // 이동속도 계산 모드
        // Debug.Log($"이동속도 계산 모드: {WeaponMovementHelper.CurrentCalculationMode}");
    }

    void ProvideRecommendations()
    {
        // Debug.Log("💡 [5단계] 문제점 및 해결방안:");
        
        List<string> issues = new List<string>();
        List<string> solutions = new List<string>();
        
        // 문제점 수집
        if (!hasWeaponSlotManager)
        {
            issues.Add("WeaponSlotManager 없음");
            solutions.Add("F12키로 자동 생성 또는 수동으로 GameObject에 추가");
        }
        
        if (!hasPlayerInventory)
        {
            issues.Add("PlayerInventory 없음");
            solutions.Add("Player GameObject에 PlayerInventory 컴포넌트 추가");
        }
        
        if (!hasInventoryManager)
        {
            issues.Add("InventoryManager 없음");
            solutions.Add("GameObject에 InventoryManager 컴포넌트 추가");
        }
        
        if (weaponSlotCount < 3)
        {
            issues.Add($"WeaponSlot 부족 ({weaponSlotCount}/3)");
            solutions.Add("F12키로 자동 생성 또는 Canvas에 WeaponSlot UI 수동 생성");
        }
        
        // 연결 문제 체크
        if (hasWeaponSlotManager && hasPlayerInventory)
        {
            if (foundPlayerInventory.weaponSlotManager != foundWeaponSlotManager)
            {
                issues.Add("PlayerInventory ↔ WeaponSlotManager 연결 안됨");
                solutions.Add("PlayerInventory Inspector에서 weaponSlotManager 필드 연결");
            }
        }
        
        if (hasWeaponSlotManager && hasInventoryManager)
        {
            if (foundInventoryManager.weaponSlotManager != foundWeaponSlotManager)
            {
                issues.Add("InventoryManager ↔ WeaponSlotManager 연결 안됨");
                solutions.Add("InventoryManager Inspector에서 weaponSlotManager 필드 연결");
            }
        }
        
        // 결과 출력
        if (issues.Count == 0)
        {
            Debug.Log("🎉 문제점이 발견되지 않았습니다! 시스템이 정상적으로 설정되어 있습니다.");
        }
        else
        {
            Debug.Log($"❌ {issues.Count}개의 문제점이 발견되었습니다:");
            for (int i = 0; i < issues.Count; i++)
            {
                Debug.LogWarning($"  {i + 1}. {issues[i]}");
                Debug.Log($"     💡 해결방법: {solutions[i]}");
            }
            Debug.Log("\n🔧 F12키를 눌러 자동 수정을 시도해보세요!");
        }
        
        // 다음 단계 제안
        if (HasAllComponents())
        {
            // Debug.Log("📋 다음 단계 제안:");
            // Debug.Log("  1. InventoryTester 컴포넌트 추가 (F1-F5 키 테스트)");
            // Debug.Log("  2. WeaponData 에셋 생성 및 sampleWeapons에 추가");
            // Debug.Log("  3. F1키로 무기 추가 후 Tab키로 무기 교체 테스트");
            // Debug.Log("  4. Ctrl+1/2/3으로 특정 슬롯 장착 테스트");
        }
    }

    string GetUIHierarchy(Transform transform)
    {
        string hierarchy = transform.name;
        Transform current = transform.parent;
        
        while (current != null)
        {
            hierarchy = current.name + "/" + hierarchy;
            current = current.parent;
        }
        
        return hierarchy;
    }

    [ContextMenu("Fix WeaponSlot Sizes to Prefab Size")]
    public void FixWeaponSlotSizes()
    {
        Debug.Log("🔧 [WeaponSlotSetupGuide] WeaponSlot 크기를 프리팹 크기로 수정합니다...");
        
        // 모든 WeaponSlot 찾기
        WeaponSlot[] allWeaponSlots = FindObjectsByType<WeaponSlot>(FindObjectsSortMode.None);
        
        if (allWeaponSlots.Length == 0)
        {
            Debug.LogWarning("⚠️ WeaponSlot을 찾을 수 없습니다!");
            return;
        }
        
        int fixedCount = 0;
        
        foreach (WeaponSlot weaponSlot in allWeaponSlots)
        {
            if (weaponSlot != null)
            {
                EnsureProperSizing(weaponSlot.gameObject);
                fixedCount++;
            }
        }
        
        Debug.Log($"✅ {fixedCount}개의 WeaponSlot 크기가 프리팹 크기(120x60)로 수정되었습니다!");
        Debug.Log("💡 이제 WeaponSlot들이 올바른 크기로 표시됩니다.");
    }
} 