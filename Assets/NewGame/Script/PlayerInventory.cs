using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("📋 플레이어 인벤토리 사용법")]
    [TextArea(3, 6)]
    public string playerInventoryInstructions = "🎯 주요 기능:\n• 무기 추가/제거 관리\n• 장착된 무기 생성/제거\n• InventoryManager와 자동 연동\n• PlayerController와 연결하여 무기 시스템 통합\n\n⚙️ 설정: weaponHolder에 무기가 생성될 Transform 연결";

    [Header("🔫 Weapon Management")]
    [Tooltip("현재 장착된 무기 데이터")]
    public WeaponData equippedWeapon;
    
    [Tooltip("무기 프리팹이 생성될 위치 (플레이어 자식 오브젝트 권장)")]
    public Transform weaponHolder;
    
    private GameObject currentWeaponObj;
    
    [Header("🔗 UI References")]
    [Tooltip("인벤토리 매니저 (자동으로 찾아서 연결됨)")]
    public InventoryManager inventoryManager;
    
    [Header("⚠️ Legacy Support (사용 중단 예정)")]
    [System.Obsolete("Use InventoryManager instead")]
    [Tooltip("레거시 인벤토리 UI (새 프로젝트에서는 사용하지 마세요)")]
    public InventoryUI inventoryUI;
    
    [Header("🎮 Player References")]
    [Tooltip("플레이어 컨트롤러 (자동으로 찾아서 연결됨)")]
    public PlayerController playerController;

    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        // 인벤토리 매니저 자동 연결
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
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
                Debug.Log($"[PlayerInventory] 무기 추가: {weapon.weaponName}");
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
        
        // 새로운 인벤토리 매니저 사용
        if (inventoryManager != null)
        {
            inventoryManager.RemoveWeapon(weapon);
            Debug.Log($"[PlayerInventory] 무기 제거: {weapon.weaponName}");
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
        }
        
        // 🏃‍♂️ 플레이어 이동속도 업데이트
        UpdatePlayerMovementSpeed(weaponData);
    }

    public WeaponData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        // 기존 무기 해제
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);

        // 새 무기 생성 (월드 오브젝트에만)
        currentWeaponObj = Instantiate(weaponData.weaponPrefab, weaponHolder);
        // 필요시 위치/회전만 초기화 (scale은 건드리지 않음)
        currentWeaponObj.transform.localPosition = Vector3.zero;
        currentWeaponObj.transform.localRotation = Quaternion.identity;
    }

    public void UnequipWeapon()
    {
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);
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
            Debug.LogWarning("⚠️ [PlayerInventory] PlayerController를 찾을 수 없어 이동속도를 업데이트할 수 없습니다!");
        }
    }
} 