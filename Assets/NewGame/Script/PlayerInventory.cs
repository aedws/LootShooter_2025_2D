using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [Header("📋 플레이어 인벤토리 사용법")]
    [TextArea(3, 6)]
    public string playerInventoryInstructions = "🎯 주요 기능:\n• 무기 추가/제거 관리\n• 3개 슬롯 무기 시스템 관리\n• InventoryManager와 자동 연동\n• PlayerController와 연결하여 무기 시스템 통합\n\n⚙️ 설정: weaponHolder에 무기가 생성될 Transform 연결";

    [Header("🔫 Weapon Management")]
    [Tooltip("현재 활성화된 무기 데이터 (WeaponSlotManager에서 관리)")]
    public WeaponData equippedWeapon;
    
    [Tooltip("무기 프리팹이 생성될 위치 (플레이어 자식 오브젝트 권장)")]
    public Transform weaponHolder;
    
    private GameObject currentWeaponObj;
    
    [Header("🛡️ Armor Management")]
    [Tooltip("장착된 방어구들 (타입별로 관리)")]
    public Dictionary<ArmorType, ArmorData> equippedArmors = new Dictionary<ArmorType, ArmorData>();
    
    [Tooltip("방어구 슬롯 매니저 (자동으로 찾아서 연결됨)")]
    public ArmorSlotManager armorSlotManager;
    
    [Header("🔗 UI References")]
    [Tooltip("인벤토리 매니저 (자동으로 찾아서 연결됨)")]
    public InventoryManager inventoryManager;
    
    [Tooltip("무기 슬롯 매니저 (자동으로 찾아서 연결됨)")]
    public WeaponSlotManager weaponSlotManager;
    
    [Header("⚠️ Legacy Support (사용 중단 예정)")]
    [System.Obsolete("Use InventoryManager instead")]
    [Tooltip("레거시 인벤토리 UI (새 프로젝트에서는 사용하지 마세요)")]
    public InventoryUI inventoryUI;
    
    [Header("🎮 Player References")]
    [Tooltip("플레이어 컨트롤러 (자동으로 찾아서 연결됨)")]
    public PlayerController playerController;

    [Header("🔔 Events")]
    // 무기 변경 시 발생하는 이벤트 (새 무기, 이전 무기)
    public System.Action<WeaponData, WeaponData> OnWeaponChanged;

    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        // 자동 연결
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        if (weaponSlotManager == null)
            weaponSlotManager = FindAnyObjectByType<WeaponSlotManager>();
        
        // 🆕 ArmorSlotManager 자동 연결
        if (armorSlotManager == null)
            armorSlotManager = FindAnyObjectByType<ArmorSlotManager>();
        
        // WeaponSlotManager 이벤트 구독
        if (weaponSlotManager != null)
        {
            weaponSlotManager.OnWeaponSwitched += OnWeaponSwitched;
        }
        
        // 🆕 ArmorSlotManager 이벤트 구독
        if (armorSlotManager != null)
        {
            armorSlotManager.OnArmorEquipped += OnArmorEquipped;
            armorSlotManager.OnArmorUnequipped += OnArmorUnequipped;
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (weaponSlotManager != null)
        {
            weaponSlotManager.OnWeaponSwitched -= OnWeaponSwitched;
        }
        
        // 🆕 ArmorSlotManager 이벤트 구독 해제
        if (armorSlotManager != null)
        {
            armorSlotManager.OnArmorEquipped -= OnArmorEquipped;
            armorSlotManager.OnArmorUnequipped -= OnArmorUnequipped;
        }
    }

    // WeaponSlotManager에서 무기가 교체될 때 호출되는 이벤트 핸들러
    void OnWeaponSwitched(WeaponData newWeapon)
    {
        WeaponData oldWeapon = equippedWeapon;
        SetEquippedWeapon(newWeapon);
        
        // 무기 변경 이벤트 발생
        OnWeaponChanged?.Invoke(newWeapon, oldWeapon);
        
        // Debug.Log($"🔄 [PlayerInventory] 무기 교체됨: {(newWeapon != null ? newWeapon.weaponName : "없음")}");
    }

    public void AddWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        
        // 새로운 인벤토리 매니저 사용
        if (inventoryManager != null)
        {
            if (!inventoryManager.HasWeapon(weapon))
            {
                inventoryManager.AddWeapon(weapon);
                // Debug.Log($"[PlayerInventory] 무기 추가: {weapon.weaponName}");
            }
        }
        
        // 레거시 호환성
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        if (inventoryUI != null)
        {
            inventoryUI.RefreshInventory(inventoryManager != null ? inventoryManager.GetWeapons() : new List<WeaponData>());
        }
#pragma warning restore CS0618
    }

    public void RemoveWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        
        // WeaponSlotManager에서 해당 무기 제거
        if (weaponSlotManager != null && weaponSlotManager.HasWeapon(weapon))
        {
            for (int i = 0; i < weaponSlotManager.GetSlotCount(); i++)
            {
                if (weaponSlotManager.GetWeaponInSlot(i) == weapon)
                {
                    weaponSlotManager.UnequipWeaponFromSlot(i);
                    break;
                }
            }
        }
        
        // 새로운 인벤토리 매니저 사용
        if (inventoryManager != null)
        {
            inventoryManager.RemoveWeapon(weapon);
            // Debug.Log($"[PlayerInventory] 무기 제거: {weapon.weaponName}");
        }
        
        // 레거시 호환성
#pragma warning disable CS0618 // 타입 또는 멤버는 사용되지 않습니다.
        if (inventoryUI != null)
        {
            inventoryUI.RefreshInventory(inventoryManager != null ? inventoryManager.GetWeapons() : new List<WeaponData>());
        }
#pragma warning restore CS0618
    }

    public void SetEquippedWeapon(WeaponData weaponData)
    {
        equippedWeapon = weaponData;

        // 기존 무기 오브젝트 파괴
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);

        // 새 무기 생성 및 장착
        if (weaponData != null && weaponData.weaponPrefab != null)
        {
            Vector3 prefabScale = weaponData.weaponPrefab.transform.localScale;
            currentWeaponObj = Instantiate(weaponData.weaponPrefab, weaponHolder);
            currentWeaponObj.transform.localPosition = Vector3.zero;
            currentWeaponObj.transform.localRotation = Quaternion.identity;
            currentWeaponObj.transform.localScale = prefabScale; // 프리팹 크기 유지
            
            // Debug.Log($"✅ [PlayerInventory] 무기 오브젝트 생성: {weaponData.weaponName}");
        }
        else
        {
            // Debug.Log($"🔄 [PlayerInventory] 무기 해제됨");
        }
        
        // 🏃‍♂️ 플레이어 이동속도 업데이트
        UpdatePlayerMovementSpeed(weaponData);
    }

    public WeaponData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    // 🔫 새로운 다중 슬롯 지원 메서드들
    public bool EquipWeaponToSlot(WeaponData weapon, int slotIndex = -1)
    {
        if (weapon == null)
        {
            // Debug.LogWarning("⚠️ [PlayerInventory] 장착할 무기가 null입니다!");
            return false;
        }
        
        if (weaponSlotManager == null)
        {
            // Debug.LogError("❌ [PlayerInventory] WeaponSlotManager가 없습니다!");
            return false;
        }
        
        // 슬롯 인덱스가 지정되지 않았으면 빈 슬롯 찾기
        if (slotIndex == -1)
        {
            slotIndex = weaponSlotManager.GetEmptySlotIndex();
            if (slotIndex == -1)
            {
                // Debug.LogWarning("⚠️ [PlayerInventory] 빈 슬롯이 없습니다!");
                return false;
            }
        }
        
        // 무기 장착
        bool success = weaponSlotManager.EquipWeaponToSlot(weapon, slotIndex);
        
        if (success)
        {
            // 인벤토리에서 무기 제거 (WeaponSlotManager가 처리하므로 여기서는 UI만 새로고침)
            if (inventoryManager != null)
            {
                inventoryManager.RemoveWeapon(weapon, false); // 새로고침 없이 제거
                inventoryManager.RefreshInventory(); // 수동으로 UI 새로고침
            }
            
            // Debug.Log($"✅ [PlayerInventory] 슬롯 {slotIndex + 1}에 무기 장착: {weapon.weaponName}");
        }
        
        return success;
    }

    public void UnequipWeaponFromSlot(int slotIndex)
    {
        if (weaponSlotManager == null)
        {
            // Debug.LogError("❌ [PlayerInventory] WeaponSlotManager가 없습니다!");
            return;
        }
        
        WeaponData weaponToUnequip = weaponSlotManager.GetWeaponInSlot(slotIndex);
        if (weaponToUnequip != null)
        {
            // 무기 해제 (자동으로 인벤토리에 반환됨)
            weaponSlotManager.UnequipWeaponFromSlot(slotIndex);
            
            // Debug.Log($"🔓 [PlayerInventory] 슬롯 {slotIndex + 1}에서 무기 해제: {weaponToUnequip.weaponName}");
        }
    }

    public void SwitchWeapon(int slotIndex)
    {
        if (weaponSlotManager != null)
        {
            weaponSlotManager.SwitchToSlot(slotIndex);
        }
    }

    public void SwitchToNextWeapon()
    {
        if (weaponSlotManager != null)
        {
            weaponSlotManager.SwitchToNextWeapon();
        }
    }

    // 🔧 레거시 호환성 메서드들 (단일 슬롯 에뮬레이션)
    public void EquipWeapon(WeaponData weaponData)
    {
        // 첫 번째 슬롯에 장착
        EquipWeaponToSlot(weaponData, 0);
    }

    public void UnequipWeapon()
    {
        // 현재 활성 슬롯의 무기 해제
        if (weaponSlotManager != null)
        {
            UnequipWeaponFromSlot(weaponSlotManager.currentSlotIndex);
        }
        else
        {
            // 레거시 방식
            if (currentWeaponObj != null)
                Destroy(currentWeaponObj);
        }
    }

    // 현재 장착 무기 오브젝트 반환
    public GameObject GetCurrentWeaponObject()
    {
        return currentWeaponObj;
    }

    // 현재 장착 무기 Weapon 컴포넌트 반환
    public Weapon GetCurrentWeapon()
    {
        return currentWeaponObj != null ? currentWeaponObj.GetComponent<Weapon>() : null;
    }
    
    // 인벤토리 매니저와의 연동 메소드들
    public List<WeaponData> GetWeapons()
    {
        return inventoryManager != null ? inventoryManager.GetWeapons() : new List<WeaponData>();
    }
    
    public bool HasWeapon(WeaponData weapon)
    {
        return inventoryManager != null ? inventoryManager.HasWeapon(weapon) : false;
    }
    
    public int GetWeaponCount()
    {
        return inventoryManager != null ? inventoryManager.GetWeaponCount() : 0;
    }
    
    public bool IsInventoryFull()
    {
        return inventoryManager != null ? inventoryManager.IsFull() : false;
    }
    
    // WeaponSlotManager와의 연동 메서드들
    public WeaponData GetWeaponInSlot(int slotIndex)
    {
        return weaponSlotManager != null ? weaponSlotManager.GetWeaponInSlot(slotIndex) : null;
    }
    
    public List<WeaponData> GetAllEquippedWeapons()
    {
        return weaponSlotManager != null ? weaponSlotManager.GetAllEquippedWeapons() : new List<WeaponData>();
    }
    
    public int GetCurrentSlotIndex()
    {
        return weaponSlotManager != null ? weaponSlotManager.currentSlotIndex : 0;
    }
    
    public bool IsSlotEmpty(int slotIndex)
    {
        return weaponSlotManager != null ? weaponSlotManager.IsSlotEmpty(slotIndex) : true;
    }
    
    // 인벤토리 UI 열기/닫기
    public void ToggleInventory()
    {
        if (inventoryManager != null)
            inventoryManager.ToggleInventory();
    }
    
    public void OpenInventory()
    {
        if (inventoryManager != null)
            inventoryManager.OpenInventory();
    }
    
    public void CloseInventory()
    {
        if (inventoryManager != null)
            inventoryManager.CloseInventory();
    }
    
    // 🏃‍♂️ 플레이어 이동속도 업데이트 메서드
    void UpdatePlayerMovementSpeed(WeaponData weaponData)
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.UpdateMovementSpeed(weaponData);
        }
        else
        {
            // Debug.LogWarning("⚠️ [PlayerInventory] PlayerController를 찾을 수 없어 이동속도를 업데이트할 수 없습니다!");
        }
    }
    
    // 🆕 방어구 관련 메서드들
    
    // 방어구 장착 이벤트 핸들러
    void OnArmorEquipped(ArmorData armor)
    {
        equippedArmors[armor.armorType] = armor;
        UpdateArmorStats();
        Debug.Log($"🛡️ [PlayerInventory] 방어구 장착: {armor.armorName}");
    }
    
    // 방어구 해제 이벤트 핸들러
    void OnArmorUnequipped(ArmorData armor)
    {
        equippedArmors.Remove(armor.armorType);
        UpdateArmorStats();
        Debug.Log($"🛡️ [PlayerInventory] 방어구 해제: {armor.armorName}");
    }
    
    // 방어구 장착 설정
    public void SetEquippedArmor(ArmorData armor, ArmorType armorType)
    {
        if (armor != null)
        {
            equippedArmors[armorType] = armor;
        }
        else
        {
            equippedArmors.Remove(armorType);
        }
        UpdateArmorStats();
    }
    
    // 방어구 능력치 업데이트
    public void UpdateArmorStats()
    {
        if (playerController == null) return;
        
        // 총 방어력 계산
        int totalDefense = 0;
        int totalHealthBonus = 0;
        float totalSpeedBonus = 0f;
        float totalDamageReduction = 0f;
        
        foreach (var armor in equippedArmors.Values)
        {
            totalDefense += armor.defense;
            totalHealthBonus += armor.maxHealth;
            totalSpeedBonus += armor.moveSpeedBonus;
            totalDamageReduction += armor.damageReduction;
        }
        
        // 플레이어 능력치 적용
        ApplyArmorStats(totalDefense, totalHealthBonus, totalSpeedBonus, totalDamageReduction);
    }
    
    // 방어구 능력치를 플레이어에 적용
    void ApplyArmorStats(int defense, int healthBonus, float speedBonus, float damageReduction)
    {
        // Health 컴포넌트에 체력 보너스 적용
        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            // 최대 체력 업데이트 (기본 체력 + 보너스)
            int baseMaxHealth = 100; // 기본 최대 체력 (설정에서 가져올 수 있음)
            playerHealth.SetMaxHealth(baseMaxHealth + healthBonus);
        }
        
        // PlayerController에 이동속도 보너스 적용
        if (playerController != null)
        {
            // 이동속도 보너스는 무기와 별도로 적용
            // (무기 이동속도 배수 * 방어구 보너스)
            float baseSpeed = playerController.GetBaseMoveSpeed();
            float weaponSpeedMultiplier = 1f;
            
            // 현재 무기의 이동속도 배수 가져오기
            if (equippedWeapon != null)
            {
                weaponSpeedMultiplier = equippedWeapon.movementSpeedMultiplier;
            }
            
            // 최종 이동속도 = 기본속도 * 무기배수 * (1 + 방어구보너스)
            float finalSpeed = baseSpeed * weaponSpeedMultiplier * (1f + speedBonus);
            playerController.currentMoveSpeed = finalSpeed;
        }
        
        // 데미지 감소율은 Health 컴포넌트에서 처리 (필요시 구현)
        // damageReduction 값을 Health 컴포넌트에 전달
    }
    
    // 방어구 관련 getter 메서드들
    public ArmorData GetEquippedArmor(ArmorType armorType)
    {
        return equippedArmors.ContainsKey(armorType) ? equippedArmors[armorType] : null;
    }
    
    public Dictionary<ArmorType, ArmorData> GetAllEquippedArmors()
    {
        return new Dictionary<ArmorType, ArmorData>(equippedArmors);
    }
    
    public int GetEquippedArmorCount()
    {
        return equippedArmors.Count;
    }
    
    public bool IsArmorEquipped(ArmorType armorType)
    {
        return equippedArmors.ContainsKey(armorType);
    }
    
    public int GetTotalDefense()
    {
        return equippedArmors.Values.Sum(armor => armor.defense);
    }
    
    public int GetTotalHealthBonus()
    {
        return equippedArmors.Values.Sum(armor => armor.maxHealth);
    }
    
    public float GetTotalSpeedBonus()
    {
        return equippedArmors.Values.Sum(armor => armor.moveSpeedBonus);
    }
    
    public float GetTotalDamageReduction()
    {
        float totalReduction = equippedArmors.Values.Sum(armor => armor.damageReduction);
        return Mathf.Clamp01(totalReduction); // 최대 100% 제한
    }
} 