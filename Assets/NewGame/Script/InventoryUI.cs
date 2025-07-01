using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 패널 및 슬롯")]
    public GameObject inventoryPanel; // 인벤토리 전체 패널
    public List<InventorySlot> slots; // Inspector에서 슬롯 오브젝트 할당
    public WeaponSlot weaponSlot; // Inspector에서 WeaponSlot 할당

    private bool isOpen = false;

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // I키로 인벤토리 열기/닫기
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