using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IDropHandler
{
    public Image icon;
    public WeaponData weaponData;
    public PlayerInventory playerInventory; // Inspector에서 연결

    void Start()
    {
        // 안전하게 playerInventory 자동 연결 (선택사항)
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
    }

    void Update()
    {
        // 우클릭으로 무기 해제
        if (weaponData != null && Input.GetMouseButtonDown(1)) // 1: 우클릭
        {
            ClearSlot();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (draggedSlot != null && draggedSlot.weaponData != null)
        {
            // 1. 무기 장착
            EquipWeapon(draggedSlot.weaponData);

            // 2. 인벤토리에서 무기 제거
            if (playerInventory != null)
                playerInventory.RemoveWeapon(draggedSlot.weaponData);
        }
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        this.weaponData = weaponData;
        
        // 아이콘 업데이트
        if (icon != null && weaponData != null)
        {
            icon.sprite = weaponData.icon;
            icon.enabled = true;
        }
        else if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        
        // 플레이어 인벤토리에 장착 무기 설정
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(weaponData);
    }

    public void ClearSlot()
    {
        if (weaponData != null && playerInventory != null)
        {
            // 무기 해제 시 인벤토리에 다시 추가
            playerInventory.AddWeapon(weaponData);
        }
        weaponData = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        
        // 플레이어 인벤토리에서 장착 무기 해제
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(null);
    }
}