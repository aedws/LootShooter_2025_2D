using UnityEngine;

[System.Serializable]
public class InventorySetupHelper : MonoBehaviour
{
    [Header("📚 인벤토리 시스템 완전 가이드")]
    [TextArea(10, 15)]
    public string fullGuide = 
        "🎯 인벤토리 시스템 설정 완전 가이드\n\n" +
        "1️⃣ 기본 설정:\n" +
        "• GameObject에 InventoryManager 추가\n" +
        "• Player에 PlayerInventory 추가\n" +
        "• Canvas에 인벤토리 UI 패널 생성\n\n" +
        
        "2️⃣ UI 구조 만들기:\n" +
        "• InventoryPanel (전체 패널)\n" +
        "  └── SlotParent (Grid Layout Group)\n" +
        "  └── WeaponSlot (장착 슬롯)\n" +
        "  └── Controls (정렬, 검색 등)\n\n" +
        
        "3️⃣ 프리팹 설정:\n" +
        "• 슬롯 프리팹에 InventorySlot 스크립트\n" +
        "• 무기 슬롯에 WeaponSlot 스크립트\n" +
        "• Image, Text 컴포넌트들 연결\n\n" +
        
        "4️⃣ 테스트:\n" +
        "• InventoryTester 추가\n" +
        "• WeaponData 에셋 생성\n" +
        "• 게임 실행 후 F1-F5 키 테스트\n\n" +
        
        "5️⃣ 조작법:\n" +
        "• I키: 인벤토리 열기/닫기\n" +
        "• 좌클릭: 슬롯 선택\n" +
        "• 우클릭: 무기 장착\n" +
        "• 드래그: 무기 이동";

    [Header("🚀 빠른 설정")]
    [Tooltip("이 체크박스를 체크하면 기본 설정값들을 자동으로 적용합니다")]
    public bool applyQuickSetup = false;
    
    [Header("📋 체크리스트")]
    [Tooltip("새로운 InventoryManager가 씬에 있는지 확인")]
    public bool hasInventoryManager = false;
    
    [Tooltip("PlayerInventory 컴포넌트가 플레이어에 있는지 확인")]
    public bool hasPlayerInventory = false;
    
    [Tooltip("인벤토리 UI 설정이 완료되었는지 확인 (InventoryManager 권장, InventoryUI는 레거시)")]
    public bool hasInventoryUI = false;
    
    [Tooltip("슬롯 프리팹이 준비되었는지 확인")]
    public bool hasSlotPrefab = false;
    
    [Tooltip("WeaponData 에셋이 생성되었는지 확인")]
    public bool hasWeaponData = false;
    
    [Tooltip("InventoryTester가 테스트용으로 추가되었는지 확인")]
    public bool hasInventoryTester = false;
    
    [Header("🔍 현재 상태")]
    [TextArea(3, 5)]
    public string currentStatus = "상태를 확인하려면 'Check Current Status' 버튼을 클릭하세요.";
    
    void Start()
    {
        CheckCurrentStatus();
        
        if (applyQuickSetup)
        {
            ApplyQuickSetup();
            applyQuickSetup = false;
        }
    }
    
    void Update()
    {
        // R키를 누르면 상태 새로고침
        if (Input.GetKeyDown(KeyCode.R))
        {
            CheckCurrentStatus();
        }
    }
    
    [ContextMenu("Check Current Status")]
    public void CheckCurrentStatus()
    {
        hasInventoryManager = FindAnyObjectByType<InventoryManager>() != null;
        hasPlayerInventory = FindAnyObjectByType<PlayerInventory>() != null;
        
        // 레거시 InventoryUI 체크 (obsolete 경고 무시)
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        hasInventoryUI = FindAnyObjectByType<InventoryUI>() != null;
#pragma warning restore CS0618
        
        hasInventoryTester = FindAnyObjectByType<InventoryTester>() != null;
        
        // WeaponData 에셋 확인
        WeaponData[] weaponAssets = Resources.FindObjectsOfTypeAll<WeaponData>();
        hasWeaponData = weaponAssets.Length > 0;
        
        // 상태 텍스트 업데이트
        UpdateStatusText();
        
        Debug.Log("[InventorySetupHelper] 현재 상태를 확인했습니다. Inspector를 확인하세요.");
    }
    
    void UpdateStatusText()
    {
        currentStatus = "=== 인벤토리 시스템 현재 상태 ===\n";
        currentStatus += GetStatusIcon(hasInventoryManager) + " InventoryManager (새 시스템)\n";
        currentStatus += GetStatusIcon(hasPlayerInventory) + " PlayerInventory\n";
        
        // UI 설정 상태 (새 시스템 우선)
        if (hasInventoryManager)
        {
            currentStatus += "✅ UI 설정 (InventoryManager 사용)\n";
        }
        else if (hasInventoryUI)
        {
            currentStatus += "⚠️ UI 설정 (레거시 InventoryUI)\n";
        }
        else
        {
            currentStatus += "❌ UI 설정 필요\n";
        }
        
        currentStatus += GetStatusIcon(hasWeaponData) + " WeaponData 에셋\n";
        currentStatus += GetStatusIcon(hasInventoryTester) + " InventoryTester\n\n";
        
        if (HasAllComponents())
        {
            currentStatus += "🎉 모든 설정이 완료되었습니다!\nF1-F5 키로 테스트해보세요.";
        }
        else
        {
            currentStatus += "⚠️ 아직 설정이 필요한 항목들이 있습니다.\n";
            
            // 권장사항 추가
            if (!hasInventoryManager && hasInventoryUI)
            {
                currentStatus += "\n💡 권장: 레거시 InventoryUI 대신\nInventoryManager를 사용하세요!";
            }
        }
    }
    
    string GetStatusIcon(bool hasComponent)
    {
        return hasComponent ? "✅" : "❌";
    }
    
    bool HasAllComponents()
    {
        return hasInventoryManager && hasPlayerInventory && hasInventoryUI && hasWeaponData;
    }
    
    [ContextMenu("Apply Quick Setup")]
    void ApplyQuickSetup()
    {
        Debug.Log("[InventorySetupHelper] 빠른 설정을 적용합니다...");
        
        // InventoryManager 자동 추가
        if (!hasInventoryManager)
        {
            GameObject managerObj = new GameObject("InventoryManager");
            managerObj.AddComponent<InventoryManager>();
            Debug.Log("✅ InventoryManager 추가됨");
        }
        
        // PlayerInventory 찾아서 설정
        PlayerInventory playerInv = FindAnyObjectByType<PlayerInventory>();
        if (playerInv == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerInv = playerObj.AddComponent<PlayerInventory>();
                Debug.Log("✅ PlayerInventory 추가됨");
            }
        }
        
        // InventoryTester 자동 추가
        if (!hasInventoryTester)
        {
            GameObject testerObj = new GameObject("InventoryTester");
            testerObj.AddComponent<InventoryTester>();
            Debug.Log("✅ InventoryTester 추가됨");
        }
        
        CheckCurrentStatus();
        Debug.Log("[InventorySetupHelper] 빠른 설정 완료! Inspector를 확인하세요.");
    }
    
    void OnGUI()
    {
        // 화면 우하단에 간단한 상태 표시
        GUILayout.BeginArea(new Rect(Screen.width - 280, Screen.height - 160, 270, 150));
        GUILayout.Label("=== 인벤토리 설정 도우미 ===");
        GUILayout.Label(GetStatusIcon(hasInventoryManager) + " InventoryManager");
        GUILayout.Label(GetStatusIcon(hasPlayerInventory) + " PlayerInventory");
        
        // UI 상태 표시 (새 시스템 우선)
        if (hasInventoryManager)
        {
            GUILayout.Label("✅ UI (새 시스템)");
        }
        else if (hasInventoryUI)
        {
            GUILayout.Label("⚠️ UI (레거시)");
        }
        else
        {
            GUILayout.Label("❌ UI 필요");
        }
        
        GUILayout.Label(GetStatusIcon(hasWeaponData) + " WeaponData");
        GUILayout.Label(GetStatusIcon(hasInventoryTester) + " Tester");
        GUILayout.Label("R키: 상태 새로고침");
        GUILayout.EndArea();
    }
} 