using UnityEngine;
using System.Collections.Generic;

public class InventoryTester : MonoBehaviour
{
    [Header("📋 테스터 사용법")]
    [TextArea(5, 10)]
    public string testerInstructions = "🎮 기본 테스트 키:\n• F1: 랜덤 무기 추가\n• F2: 랜덤 무기 제거\n• F3: 모든 무기 제거\n• F4: 인벤토리 상태 콘솔 출력\n• F5: 무기 생성 도움말\n\n🔫 다중 슬롯 테스트 키:\n• Ctrl+1: 슬롯 1에 무기 장착\n• Ctrl+2: 슬롯 2에 무기 장착\n• Ctrl+3: 슬롯 3에 무기 장착\n• Shift+1/2/3: 각 슬롯 무기 해제\n• Tab: 무기 교체 테스트\n• 1/2/3: 직접 슬롯 선택\n\n🎯 조작법:\n• I: 인벤토리 열기/닫기";

    [Header("⚙️ Test Settings")]
    [Tooltip("게임 시작 시 샘플 무기들을 자동으로 추가할지 여부")]
    public bool addSampleWeaponsOnStart = true;
    
    [Tooltip("시작 시 추가할 무기 개수")]
    [Range(1, 20)]
    public int numberOfWeaponsToAdd = 10;
    
    [Header("🔫 Sample Weapon Assets")]
    [Tooltip("테스트용 WeaponData 에셋들을 여기에 추가하세요 (Create -> LootShooter -> WeaponData)")]
    public List<WeaponData> sampleWeapons = new List<WeaponData>();
    
    [Header("🔗 References (자동 연결)")]
    [Tooltip("플레이어 인벤토리 (자동으로 찾아서 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 (자동으로 찾아서 연결됨)")]
    public InventoryManager inventoryManager;
    
    [Tooltip("무기 슬롯 매니저 (자동으로 찾아서 연결됨)")]
    public WeaponSlotManager weaponSlotManager;
    
    void Start()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        if (weaponSlotManager == null)
            weaponSlotManager = FindAnyObjectByType<WeaponSlotManager>();
        
        if (addSampleWeaponsOnStart)
        {
            AddSampleWeapons();
        }
        
        // 다중 슬롯 시스템 상태 출력
        LogMultiSlotSystemStatus();
    }
    
    void Update()
    {
        HandleTestInput();
    }

    void HandleTestInput()
    {
        // 기본 테스트 키들
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddRandomWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            RemoveRandomWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            ClearAllWeapons();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            LogInventoryStatus();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowWeaponCreationHelp();
        }
        
        // 🔫 새로운 다중 슬롯 테스트 키들
        // Ctrl + 숫자: 특정 슬롯에 무기 장착
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EquipRandomWeaponToSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EquipRandomWeaponToSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                EquipRandomWeaponToSlot(2);
            }
        }
        // Shift + 숫자: 특정 슬롯 무기 해제
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UnequipWeaponFromSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UnequipWeaponFromSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UnequipWeaponFromSlot(2);
            }
        }
        
        // F6: 모든 슬롯 상태 출력
        if (Input.GetKeyDown(KeyCode.F6))
        {
            LogAllSlotStatus();
        }
        
        // F7: 슬롯 순환 테스트
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestSlotCycling();
        }
        
        // F8: 무기 교체 속도 테스트
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TestWeaponSwitchSpeed();
        }
    }

    void AddSampleWeapons()
    {
        if (sampleWeapons.Count == 0)
        {
            Debug.LogWarning("⚠️ [InventoryTester] sampleWeapons 리스트가 비어있습니다! WeaponData 에셋을 추가해주세요.");
            return;
        }

        int addedCount = 0;
        for (int i = 0; i < numberOfWeaponsToAdd && addedCount < numberOfWeaponsToAdd; i++)
        {
            WeaponData randomWeapon = sampleWeapons[Random.Range(0, sampleWeapons.Count)];
            if (playerInventory != null)
            {
                playerInventory.AddWeapon(randomWeapon);
                addedCount++;
            }
        }
    }

    void AddRandomWeapon()
    {
        if (sampleWeapons.Count == 0)
        {
            Debug.LogWarning("⚠️ [InventoryTester] 추가할 무기가 없습니다!");
            return;
        }

        WeaponData randomWeapon = sampleWeapons[Random.Range(0, sampleWeapons.Count)];
        if (playerInventory != null)
        {
            playerInventory.AddWeapon(randomWeapon);
        }
    }

    void RemoveRandomWeapon()
    {
        if (inventoryManager == null) return;

        List<WeaponData> weapons = inventoryManager.GetWeapons();
        if (weapons.Count == 0)
        {
            Debug.LogWarning("⚠️ [InventoryTester] 제거할 무기가 없습니다!");
            return;
        }

        WeaponData randomWeapon = weapons[Random.Range(0, weapons.Count)];
        if (playerInventory != null)
        {
            playerInventory.RemoveWeapon(randomWeapon);
        }
    }

    void ClearAllWeapons()
    {
        if (inventoryManager == null) return;

        // 장착된 무기들도 모두 해제
        if (weaponSlotManager != null)
        {
            for (int i = 0; i < weaponSlotManager.GetSlotCount(); i++)
            {
                if (!weaponSlotManager.IsSlotEmpty(i))
                {
                    weaponSlotManager.UnequipWeaponFromSlot(i);
                }
            }
        }

        List<WeaponData> weapons = new List<WeaponData>(inventoryManager.GetWeapons());
        foreach (WeaponData weapon in weapons)
        {
            if (playerInventory != null)
            {
                playerInventory.RemoveWeapon(weapon);
            }
        }
    }

    void LogInventoryStatus()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("❌ [InventoryTester] InventoryManager가 없습니다!");
            return;
        }

        Debug.Log("=== 📋 인벤토리 상태 ===");
        Debug.Log($"총 무기 개수: {inventoryManager.GetWeaponCount()}");
        Debug.Log($"장착된 무기 개수: {inventoryManager.GetEquippedWeaponCount()}");
        Debug.Log($"인벤토리 가득참: {inventoryManager.IsFull()}");

        List<WeaponData> weapons = inventoryManager.GetWeapons();
        Debug.Log($"인벤토리 무기 목록 ({weapons.Count}개):");
        for (int i = 0; i < weapons.Count; i++)
        {
            Debug.Log($"  {i + 1}. {weapons[i].weaponName} ({weapons[i].weaponType})");
        }
        
        // 다중 슬롯 상태도 출력
        LogAllSlotStatus();
    }

    // 🔫 새로운 다중 슬롯 테스트 메서드들
    void EquipRandomWeaponToSlot(int slotIndex)
    {
        if (inventoryManager == null || weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryTester] 필요한 매니저가 없습니다!");
            return;
        }

        List<WeaponData> availableWeapons = inventoryManager.GetWeapons();
        if (availableWeapons.Count == 0)
        {
            Debug.LogWarning($"⚠️ [InventoryTester] 슬롯 {slotIndex + 1}에 장착할 무기가 없습니다!");
            return;
        }

        WeaponData randomWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
        inventoryManager.EquipWeaponToSpecificSlot(randomWeapon, slotIndex);
    }

    void UnequipWeaponFromSlot(int slotIndex)
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryTester] WeaponSlotManager가 없습니다!");
            return;
        }

        WeaponData weaponToUnequip = weaponSlotManager.GetWeaponInSlot(slotIndex);
        if (weaponToUnequip != null)
        {
            inventoryManager.UnequipWeaponFromSpecificSlot(slotIndex);
        }
        else
        {
            Debug.LogWarning($"⚠️ [InventoryTester] 슬롯 {slotIndex + 1}이 이미 비어있습니다!");
        }
    }

    void LogAllSlotStatus()
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryTester] WeaponSlotManager가 없습니다!");
            return;
        }

        Debug.Log("=== 🔫 무기 슬롯 상태 ===");
        Debug.Log($"현재 활성 슬롯: {weaponSlotManager.currentSlotIndex + 1}");

        for (int i = 0; i < weaponSlotManager.GetSlotCount(); i++)
        {
            WeaponData weapon = weaponSlotManager.GetWeaponInSlot(i);
            string status = i == weaponSlotManager.currentSlotIndex ? "[활성]" : "[비활성]";
            string weaponName = weapon != null ? weapon.weaponName : "비어있음";
            Debug.Log($"슬롯 {i + 1} {status}: {weaponName}");
        }

        WeaponData currentWeapon = weaponSlotManager.GetCurrentWeapon();
        Debug.Log($"현재 사용 중인 무기: {(currentWeapon != null ? currentWeapon.weaponName : "없음")}");
    }

    void TestSlotCycling()
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryTester] WeaponSlotManager가 없습니다!");
            return;
        }
        
        int originalSlot = weaponSlotManager.currentSlotIndex;
        for (int i = 0; i < weaponSlotManager.GetSlotCount(); i++)
        {
            weaponSlotManager.SwitchToSlot(i);
        }
        
        // 원래 슬롯으로 복귀
        weaponSlotManager.SwitchToSlot(originalSlot);
    }

    void TestWeaponSwitchSpeed()
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryTester] WeaponSlotManager가 없습니다!");
            return;
        }
        
        int switchCount = 10;
        
        for (int i = 0; i < switchCount; i++)
        {
            weaponSlotManager.SwitchToNextWeapon();
        }
    }

    void LogMultiSlotSystemStatus()
    {
        // 상태 로그는 중요하므로 유지
        Debug.Log("=== 🎯 다중 슬롯 시스템 상태 ===");
        Debug.Log($"PlayerInventory: {(playerInventory != null ? "✅" : "❌")}");
        Debug.Log($"InventoryManager: {(inventoryManager != null ? "✅" : "❌")}");
        Debug.Log($"WeaponSlotManager: {(weaponSlotManager != null ? "✅" : "❌")}");
        
        if (weaponSlotManager != null)
        {
            Debug.Log($"무기 슬롯 개수: {weaponSlotManager.GetSlotCount()}");
            Debug.Log($"현재 활성 슬롯: {weaponSlotManager.currentSlotIndex + 1}");
        }
    }

    void ShowWeaponCreationHelp()
    {
        if (sampleWeapons.Count == 0)
        {
            Debug.LogWarning("⚠️ 현재 sampleWeapons 리스트가 비어있습니다!");
            Debug.Log("WeaponData 에셋을 생성하여 sampleWeapons 리스트에 추가하세요.");
        }
        else
        {
            Debug.Log($"✅ 현재 {sampleWeapons.Count}개의 샘플 무기가 등록되어 있습니다.");
        }
    }
} 