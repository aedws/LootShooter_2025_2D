using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 칩셋 슬롯 컴포넌트
/// 드래그 앤 드롭으로 칩셋을 장착/해제할 수 있는 슬롯
/// </summary>
public enum ChipsetOwnerType { Weapon, Armor, Player }

public class ChipsetSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image chipsetIcon;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private GameObject costOverlay;
    [SerializeField] private GameObject equippedOverlay;
    [SerializeField] private GameObject warningIcon;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private Color costOverColor = Color.red;
    [SerializeField] private Color equippedColor = Color.green;
    
    [Header("Settings")]
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private bool isEquipped = false;
    [SerializeField] private bool isCostOver = false;
    
    [Header("Chipset Owner Info")]
    public ChipsetOwnerType ownerType; // 이 슬롯이 속한 장비 타입
    public int ownerIndex; // 무기/방어구/플레이어 인덱스(예: 0번째 무기)
    public ChipsetSlotUI parentSlotUI; // 이 슬롯이 속한 ChipsetSlotUI 참조
    
    // 현재 장착된 칩셋 데이터
    private WeaponChipsetData weaponChipset;
    private ArmorChipsetData armorChipset;
    private PlayerChipsetData playerChipset;
    
    // 이벤트
    public System.Action<ChipsetSlot, object> OnChipsetEquipped;
    public System.Action<ChipsetSlot, object> OnChipsetUnequipped;
    
    private void Awake()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // 슬롯 번호 설정
        if (slotNumberText != null)
        {
            slotNumberText.text = (slotIndex + 1).ToString();
        }
        
        // 초기 상태 설정
        UpdateVisualState();
    }
    
    /// <summary>
    /// 슬롯 인덱스 설정
    /// </summary>
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
        if (slotNumberText != null)
        {
            slotNumberText.text = (slotIndex + 1).ToString();
        }
    }
    
    /// <summary>
    /// 무기 칩셋 장착
    /// </summary>
    public void EquipWeaponChipset(WeaponChipsetData chipset)
    {
        weaponChipset = chipset;
        armorChipset = null;
        playerChipset = null;
        isEquipped = true;
        
        // 🆕 인벤토리에서 칩셋 제거
        RemoveChipsetFromInventory(chipset);
        
        // 칩셋 배열 갱신
        if (parentSlotUI != null && parentSlotUI.currentType == ChipsetSlotUI.ItemType.Weapon && parentSlotUI.currentWeapon != null)
        {
            var arr = parentSlotUI.currentWeapon.GetEquippedChipsetIds();
            if (arr.Length <= slotIndex)
            {
                var newArr = new string[slotIndex + 1];
                arr.CopyTo(newArr, 0);
                arr = newArr;
            }
            arr[slotIndex] = chipset.chipsetId;
            parentSlotUI.currentWeapon.SetEquippedChipsetIds(arr);
        }
        UpdateVisualState();
        OnChipsetEquipped?.Invoke(this, chipset);
    }
    
    public void EquipArmorChipset(ArmorChipsetData chipset)
    {
        armorChipset = chipset;
        weaponChipset = null;
        playerChipset = null;
        isEquipped = true;
        
        // 🆕 인벤토리에서 칩셋 제거
        RemoveChipsetFromInventory(chipset);
        
        // 칩셋 배열 갱신
        if (parentSlotUI != null && parentSlotUI.currentType == ChipsetSlotUI.ItemType.Armor && parentSlotUI.currentArmor != null)
        {
            var arr = parentSlotUI.currentArmor.GetEquippedChipsetIds();
            if (arr.Length <= slotIndex)
            {
                var newArr = new string[slotIndex + 1];
                arr.CopyTo(newArr, 0);
                arr = newArr;
            }
            arr[slotIndex] = chipset.chipsetId;
            parentSlotUI.currentArmor.SetEquippedChipsetIds(arr);
        }
        UpdateVisualState();
        OnChipsetEquipped?.Invoke(this, chipset);
    }
    
    public void EquipPlayerChipset(PlayerChipsetData chipset)
    {
        playerChipset = chipset;
        weaponChipset = null;
        armorChipset = null;
        isEquipped = true;
        
        // 🆕 플레이어 칩셋도 인벤토리에서 제거
        RemoveChipsetFromInventory(chipset);
        
        // 칩셋 배열 갱신
        if (parentSlotUI != null && parentSlotUI.currentType == ChipsetSlotUI.ItemType.Player && parentSlotUI.playerEquippedChipsets != null)
        {
            var arr = parentSlotUI.playerEquippedChipsets;
            if (arr.Length <= slotIndex)
            {
                var newArr = new string[slotIndex + 1];
                arr.CopyTo(newArr, 0);
                arr = newArr;
                parentSlotUI.playerEquippedChipsets = arr;
            }
            arr[slotIndex] = chipset.chipsetId;
        }
        UpdateVisualState();
        OnChipsetEquipped?.Invoke(this, chipset);
        
        // 🆕 플레이어 칩셋 상시 저장 (장착 시)
        if (parentSlotUI != null)
        {
            parentSlotUI.SavePlayerChipsetsImmediately(true); // true = 장착 시
        }
    }
    
    // UI에만 칩셋을 표시 (데이터/이벤트/인벤토리 건드리지 않음)
    public void DisplayPlayerChipset(PlayerChipsetData chipset)
    {
        playerChipset = chipset;
        weaponChipset = null;
        armorChipset = null;
        isEquipped = chipset != null;
        UpdateVisualState();
    }
    
    public void UnequipChipset()
    {
        var removedChipset = GetCurrentChipset();
        
        Debug.Log($"🔧 [ChipsetSlot] UnequipChipset 호출됨 - 칩셋: {GetChipsetName(removedChipset)}");
        Debug.Log($"🔧 [ChipsetSlot] parentSlotUI: {(parentSlotUI != null ? "있음" : "없음")}");
        Debug.Log($"🔧 [ChipsetSlot] currentType: {(parentSlotUI != null ? parentSlotUI.currentType.ToString() : "parentSlotUI 없음")}");
        
        // 🆕 플레이어 칩셋인 경우 parentSlotUI를 Player 모드로 강제 설정
        if (removedChipset is PlayerChipsetData && parentSlotUI != null)
        {
            Debug.Log($"🔧 [ChipsetSlot] 플레이어 칩셋 해제 시 parentSlotUI를 Player 모드로 강제 설정");
            parentSlotUI.currentType = ChipsetSlotUI.ItemType.Player;
        }
        
        // 🆕 칩셋 해제 처리
        if (removedChipset != null)
        {
            // 플레이어 칩셋의 경우 항상 인벤토리로 반환
            if (parentSlotUI != null && parentSlotUI.currentType == ChipsetSlotUI.ItemType.Player)
            {
                ReturnChipsetToInventory(removedChipset);
                Debug.Log($"🔧 [ChipsetSlot] 플레이어 칩셋 해제 - 인벤토리로 반환: {GetChipsetName(removedChipset)}");
            }
            else
            {
                // 무기/방어구 칩셋은 옵션에 따라 처리
                bool returnToInventory = GetChipsetReturnOption();
                if (returnToInventory)
                {
                    // 인벤토리로 반환
                    ReturnChipsetToInventory(removedChipset);
                    Debug.Log($"🔧 [ChipsetSlot] 칩셋 해제 - 인벤토리로 반환: {GetChipsetName(removedChipset)}");
                }
                else
                {
                    // 소멸 (기존 동작)
                    Debug.Log($"🔧 [ChipsetSlot] 칩셋 해제 - 소멸: {GetChipsetName(removedChipset)}");
                }
            }
        }
        
        // 칩셋 배열 갱신
        if (parentSlotUI != null)
        {
            if (parentSlotUI.currentType == ChipsetSlotUI.ItemType.Weapon && parentSlotUI.currentWeapon != null)
            {
                var arr = parentSlotUI.currentWeapon.GetEquippedChipsetIds();
                if (slotIndex < arr.Length)
                {
                    arr[slotIndex] = null;
                    parentSlotUI.currentWeapon.SetEquippedChipsetIds(arr);
                }
            }
            else if (parentSlotUI.currentType == ChipsetSlotUI.ItemType.Armor && parentSlotUI.currentArmor != null)
            {
                var arr = parentSlotUI.currentArmor.GetEquippedChipsetIds();
                if (slotIndex < arr.Length)
                {
                    arr[slotIndex] = null;
                    parentSlotUI.currentArmor.SetEquippedChipsetIds(arr);
                }
            }
            else if (parentSlotUI.currentType == ChipsetSlotUI.ItemType.Player && parentSlotUI.playerEquippedChipsets != null)
            {
                var arr = parentSlotUI.playerEquippedChipsets;
                if (slotIndex < arr.Length)
                {
                    arr[slotIndex] = null;
                }
            }
        }
        weaponChipset = null;
        armorChipset = null;
        playerChipset = null;
        isEquipped = false;
        isCostOver = false;
        UpdateVisualState();
        if (removedChipset != null)
        {
            OnChipsetUnequipped?.Invoke(this, removedChipset);
        }
        
        // 🆕 플레이어 칩셋 해제 시 상시 저장
        if (parentSlotUI != null && parentSlotUI.currentType == ChipsetSlotUI.ItemType.Player)
        {
            parentSlotUI.SavePlayerChipsetsImmediately(false); // false = 해제 시
        }
    }
    
    /// <summary>
    /// 칩셋 해제 시 인벤토리 반환 옵션을 가져옴
    /// </summary>
    private bool GetChipsetReturnOption()
    {
        // ChipsetManager에서 옵션을 가져옴
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            return chipsetManager.returnToInventoryOnUnequip;
        }
        return false; // 기본값: 소멸
    }
    
    /// <summary>
    /// 칩셋을 인벤토리로 반환
    /// </summary>
    private void ReturnChipsetToInventory(object chipset)
    {
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.AddChipset(chipset);
            
            // 🆕 칩셋 탭으로 강제 전환 후 UI 새로고침
            inventoryManager.SwitchTab(InventoryTab.Chipsets);
            inventoryManager.RefreshInventory();
            
            Debug.Log($"🔧 [ChipsetSlot] 칩셋이 인벤토리로 반환되었습니다: {GetChipsetName(chipset)}");
        }
        else
        {
            Debug.LogError($"🔧 [ChipsetSlot] InventoryManager를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 인벤토리에서 칩셋 제거
    /// </summary>
    private void RemoveChipsetFromInventory(object chipset)
    {
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RemoveChipset(chipset, false); // UI 새로고침 없이 제거
            inventoryManager.RefreshInventory(); // 수동으로 UI 새로고침
        }
        
        // 🆕 ChipsetManager의 개수 정보 업데이트
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            chipsetManager.UpdateChipsetInfo();
        }
    }
    
    /// <summary>
    /// 칩셋 이름을 반환하는 헬퍼 메서드
    /// </summary>
    private string GetChipsetName(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipset)
            return weaponChipset.chipsetName;
        else if (chipset is ArmorChipsetData armorChipset)
            return armorChipset.chipsetName;
        else if (chipset is PlayerChipsetData playerChipset)
            return playerChipset.chipsetName;
        else
            return "알 수 없는 칩셋";
    }
    
    /// <summary>
    /// 코스트 초과 상태 설정
    /// </summary>
    public void SetCostOver(bool over)
    {
        isCostOver = over;
        UpdateVisualState();
    }
    
    /// <summary>
    /// 현재 장착된 칩셋 반환
    /// </summary>
    public object GetCurrentChipset()
    {
        if (weaponChipset != null) return weaponChipset;
        if (armorChipset != null) return armorChipset;
        if (playerChipset != null) return playerChipset;
        return null;
    }
    
    /// <summary>
    /// 현재 칩셋의 코스트 반환
    /// </summary>
    public int GetCurrentCost()
    {
        if (weaponChipset != null) return weaponChipset.cost;
        if (armorChipset != null) return armorChipset.cost;
        if (playerChipset != null) return playerChipset.cost;
        return 0;
    }
    
    /// <summary>
    /// 칩셋이 장착되어 있는지 확인
    /// </summary>
    public bool IsEquipped()
    {
        return isEquipped;
    }

    /// <summary>
    /// 장착된 칩셋 반환 (GameSaveManager용)
    /// </summary>
    public object GetEquippedChipset()
    {
        if (weaponChipset != null) return weaponChipset;
        if (armorChipset != null) return armorChipset;
        if (playerChipset != null) return playerChipset;
        return null;
    }

    /// <summary>
    /// 칩셋 장착 (GameSaveManager용)
    /// </summary>
    public void EquipChipset(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipsetData)
        {
            EquipWeaponChipset(weaponChipsetData);
        }
        else if (chipset is ArmorChipsetData armorChipsetData)
        {
            EquipArmorChipset(armorChipsetData);
        }
        else if (chipset is PlayerChipsetData playerChipsetData)
        {
            EquipPlayerChipset(playerChipsetData);
        }
    }
    
    /// <summary>
    /// 코스트가 초과되었는지 확인
    /// </summary>
    public bool IsCostOver()
    {
        return isCostOver;
    }
    
    /// <summary>
    /// 슬롯 인덱스 반환
    /// </summary>
    public int GetSlotIndex()
    {
        return slotIndex;
    }
    
    /// <summary>
    /// 시각적 상태 업데이트
    /// </summary>
    private void UpdateVisualState()
    {
        if (slotBackground != null)
        {
            if (isCostOver)
            {
                slotBackground.color = costOverColor;
            }
            else if (isEquipped)
            {
                slotBackground.color = equippedColor;
            }
            else
            {
                slotBackground.color = normalColor;
            }
        }
        
        // 칩셋 아이콘 업데이트
        if (chipsetIcon != null)
        {
            chipsetIcon.gameObject.SetActive(isEquipped);
            if (isEquipped)
            {
                // 칩셋 타입에 따른 아이콘 설정 (나중에 구현)
                chipsetIcon.color = GetChipsetRarityColor();
            }
        }
        
        // 코스트 텍스트 업데이트
        if (costText != null)
        {
            costText.gameObject.SetActive(isEquipped);
            if (isEquipped)
            {
                costText.text = GetCurrentCost().ToString();
                costText.color = isCostOver ? Color.red : Color.white;
            }
        }
        
        // 오버레이 업데이트
        if (costOverlay != null)
        {
            costOverlay.SetActive(isCostOver);
        }
        
        if (equippedOverlay != null)
        {
            equippedOverlay.SetActive(isEquipped && !isCostOver);
        }
        
        if (warningIcon != null)
        {
            warningIcon.SetActive(isCostOver);
        }
    }
    
    /// <summary>
    /// 칩셋의 희귀도에 따른 색상 반환
    /// </summary>
    private Color GetChipsetRarityColor()
    {
        if (weaponChipset != null) return weaponChipset.GetRarityColor();
        if (armorChipset != null) return armorChipset.GetRarityColor();
        if (playerChipset != null) return playerChipset.GetRarityColor();
        return Color.white;
    }
    
    // 🆕 우클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && isEquipped)
        {
            // 우클릭으로 칩셋 해제
            UnequipChipset();
        }
    }
    
    // 드래그 앤 드롭 이벤트
    public void OnDrop(PointerEventData eventData)
    {
        // 1. 인벤토리 슬롯에서 드래그된 칩셋 처리
        if (InventorySlot.CurrentlyDraggedChipset != null)
        {
            object draggedChipset = InventorySlot.CurrentlyDraggedChipset;
            bool isValidDrop = false;
            if (draggedChipset is WeaponChipsetData weaponChipset && ownerType == ChipsetOwnerType.Weapon)
            {
                EquipWeaponChipset(weaponChipset);
                isValidDrop = true;
            }
            else if (draggedChipset is ArmorChipsetData armorChipset && ownerType == ChipsetOwnerType.Armor)
            {
                EquipArmorChipset(armorChipset);
                isValidDrop = true;
            }
            else if (draggedChipset is PlayerChipsetData playerChipset && ownerType == ChipsetOwnerType.Player)
            {
                EquipPlayerChipset(playerChipset);
                isValidDrop = true;
            }
            if (!isValidDrop)
            {
                Debug.LogWarning($"[ChipsetSlot] 잘못된 칩셋 타입입니다. 슬롯 타입: {ownerType}");
            }
            InventorySlot.CurrentlyDraggedChipset = null;
            return;
        }
        // 2. 기존 ChipsetItem 방식(예비)
        var draggedChipsetItem = eventData.pointerDrag?.GetComponent<ChipsetItem>();
        if (draggedChipsetItem != null)
        {
            bool isValidDrop = false;
            if (draggedChipsetItem.weaponChipset != null && ownerType == ChipsetOwnerType.Weapon)
            {
                EquipWeaponChipset(draggedChipsetItem.weaponChipset);
                isValidDrop = true;
            }
            else if (draggedChipsetItem.armorChipset != null && ownerType == ChipsetOwnerType.Armor)
            {
                EquipArmorChipset(draggedChipsetItem.armorChipset);
                isValidDrop = true;
            }
            else if (draggedChipsetItem.playerChipset != null && ownerType == ChipsetOwnerType.Player)
            {
                EquipPlayerChipset(draggedChipsetItem.playerChipset);
                isValidDrop = true;
            }
            if (!isValidDrop)
            {
                Debug.LogWarning($"[ChipsetSlot] 잘못된 칩셋 타입입니다. 슬롯 타입: {ownerType}");
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotBackground != null && !isEquipped && !isCostOver)
        {
            slotBackground.color = hoverColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisualState();
    }
} 