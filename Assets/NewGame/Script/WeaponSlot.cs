using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("📋 무기 슬롯 사용법")]
    [TextArea(3, 5)]
    public string weaponSlotInstructions = "• 인벤토리에서 무기를 드래그하여 장착\n• 우클릭으로 무기 해제\n• 마우스 호버로 무기 정보 툴팁 표시\n• 무기 장착 시 녹색으로 표시\n• PlayerInventory와 자동 연동";

    [Header("🖼️ Slot Components")]
    [Tooltip("장착된 무기 아이콘을 표시할 Image")]
    public Image icon;
    
    [Tooltip("슬롯 배경 이미지 (상태별 색상 변경)")]
    public Image backgroundImage;
    
    [Tooltip("장착된 무기 이름을 표시할 Text")]
    public Text weaponNameText;
    
    [Tooltip("무기의 탄약 정보를 표시할 Text")]
    public Text ammoText;
    
    [Header("🎨 Visual States")]
    [Tooltip("기본 빈 슬롯 색상")]
    public Color normalColor = Color.white;
    
    [Tooltip("마우스 호버 시 색상")]
    public Color hoverColor = Color.yellow;
    
    [Tooltip("무기 장착 시 색상")]
    public Color equippedColor = Color.green;
    
    [Header("🔗 References")]
    [Tooltip("플레이어 인벤토리 컴포넌트 (자동 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 컴포넌트 (자동 연결됨)")]
    public InventoryManager inventoryManager;
    
    // Properties
    public WeaponData weaponData { get; private set; }
    private bool isHovered = false;

    void Start()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        UpdateVisuals();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (draggedSlot != null && draggedSlot.weaponData != null)
        {
            // 무기 장착
            EquipWeapon(draggedSlot.weaponData);
        }
    }
    
    // 마우스 이벤트 핸들러들
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisuals();
        
        // 툴팁 표시
        if (weaponData != null && inventoryManager != null)
        {
            Vector3 tooltipPosition = transform.position + new Vector3(100, 0, 0);
            inventoryManager.ShowTooltip(weaponData, tooltipPosition);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
        
        // 툴팁 숨기기
        if (inventoryManager != null)
        {
            inventoryManager.HideTooltip();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && weaponData != null)
        {
            // 우클릭으로 무기 해제
            ClearSlot();
        }
    }

    public void EquipWeapon(WeaponData newWeaponData)
    {
        // 기존 무기가 있다면 인벤토리로 돌려보내기
        if (weaponData != null && playerInventory != null)
        {
            playerInventory.AddWeapon(weaponData);
        }
        
        // 새 무기 장착
        weaponData = newWeaponData;
        
        // 플레이어 인벤토리에 장착 무기 설정
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(weaponData);
        
        // 인벤토리에서 장착된 무기 제거
        if (weaponData != null && playerInventory != null)
        {
            playerInventory.RemoveWeapon(weaponData);
        }
        
        UpdateVisuals();
        
        Debug.Log($"[WeaponSlot] 무기 장착: {(weaponData != null ? weaponData.weaponName : "None")}");
    }

    public void ClearSlot()
    {
        if (weaponData != null && playerInventory != null)
        {
            // 무기 해제 시 인벤토리에 다시 추가
            playerInventory.AddWeapon(weaponData);
        }
        
        WeaponData oldWeapon = weaponData;
        weaponData = null;
        
        // 플레이어 인벤토리에서 장착 무기 해제
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(null);
        
        UpdateVisuals();
        
        Debug.Log($"[WeaponSlot] 무기 해제: {(oldWeapon != null ? oldWeapon.weaponName : "None")}");
    }
    
    void UpdateVisuals()
    {
        // 아이콘 업데이트
        if (icon != null)
        {
            if (weaponData != null)
            {
                icon.sprite = weaponData.icon;
                icon.enabled = true;
            }
            else
            {
                icon.sprite = null;
                icon.enabled = false;
            }
        }
        
        // 무기 이름 표시
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponData != null ? weaponData.weaponName : "Empty";
        }
        
        // 탄약 정보 표시
        if (ammoText != null)
        {
            if (weaponData != null)
            {
                if (weaponData.infiniteAmmo)
                    ammoText.text = "∞";
                else
                    ammoText.text = $"{weaponData.currentAmmo}/{weaponData.maxAmmo}";
            }
            else
            {
                ammoText.text = "";
            }
        }
        
        // 배경 색상 업데이트
        if (backgroundImage != null)
        {
            Color targetColor = normalColor;
            
            if (weaponData != null)
            {
                targetColor = equippedColor;
            }
            else if (isHovered)
            {
                targetColor = hoverColor;
            }
            
            backgroundImage.color = targetColor;
        }
    }
}