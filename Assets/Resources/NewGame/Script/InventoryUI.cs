using UnityEngine;
using System.Collections.Generic;

[System.Obsolete("Use InventoryManager instead. This class is kept for legacy compatibility.")]
public class InventoryUI : MonoBehaviour
{
    [Header("⚠️ 중요: 이 클래스는 더 이상 사용하지 마세요!")]
    [TextArea(4, 6)]
    public string deprecationWarning = "❌ 이 InventoryUI는 구버전입니다!\n\n✅ 대신 InventoryManager를 사용하세요:\n1. InventoryManager 컴포넌트 추가\n2. UI 연결 및 설정\n3. 이 컴포넌트 제거\n\n새로운 시스템이 훨씬 더 많은 기능을 제공합니다!";

    [Header("📦 UI 패널 및 슬롯 (레거시)")]
    [Tooltip("인벤토리 전체 패널")]
    public GameObject inventoryPanel;
    
    [Tooltip("슬롯 오브젝트들")]
    public List<InventorySlot> slots;
    
    [Tooltip("무기 장착 슬롯")]
    public WeaponSlot weaponSlot;
    
    [Header("🔄 새로운 시스템 연동")]
    [Tooltip("새로운 InventoryManager (있으면 이 UI 비활성화됨)")]
    public InventoryManager inventoryManager;

    private bool isOpen = false;

    void Start()
    {
        // 새로운 인벤토리 매니저 자동 연결
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        
        // 새로운 시스템이 있으면 이 UI는 비활성화
        if (inventoryManager != null)
        {
            // Debug.Log("[InventoryUI] 새로운 InventoryManager가 발견되었습니다. 레거시 UI는 비활성화됩니다.");
            this.enabled = false;
        }
    }

    void Update()
    {
        // 새로운 시스템이 있으면 이 Update는 실행되지 않음
        if (inventoryManager != null) return;
        
        // I키로 인벤토리 열기/닫기 (레거시)
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(isOpen);
        }
    }

    // 무기 리스트를 받아 슬롯에 반영
    public void RefreshInventory(List<WeaponData> weapons)
    {
        // Debug.Log($"[RefreshInventory] weapons.Count={weapons.Count}, slots.Count={slots.Count}, weaponSlot null? {weaponSlot == null}");
        // 장착 슬롯에 있는 무기는 인벤토리에서 제외
        List<WeaponData> inventoryWeapons = new List<WeaponData>(weapons);
        if (weaponSlot != null && weaponSlot.weaponData != null)
        {
            inventoryWeapons.Remove(weaponSlot.weaponData);
        }
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < inventoryWeapons.Count)
            {
                // Debug.Log($"[RefreshInventory] 슬롯 {i}에 무기 할당: {inventoryWeapons[i].weaponName}");
                slots[i].SetWeapon(inventoryWeapons[i]);
            }
            else
            {
                // Debug.Log($"[RefreshInventory] 슬롯 {i} 비움");
                slots[i].ClearSlot();
            }
        }
        

    }
} 