using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 칩셋 슬롯 컴포넌트
/// 드래그 앤 드롭으로 칩셋을 장착/해제할 수 있는 슬롯
/// </summary>
public enum ChipsetOwnerType { Weapon, Armor, Player }

public class ChipsetSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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
    }
    public void UnequipChipset()
    {
        var removedChipset = GetCurrentChipset();
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
    
    // 드래그 앤 드롭 이벤트
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그된 칩셋 아이템 처리
        var draggedChipset = eventData.pointerDrag?.GetComponent<ChipsetItem>();
        if (draggedChipset != null)
        {
            // 칩셋 타입 검증
            bool isValidDrop = false;
            
            if (draggedChipset.weaponChipset != null && ownerType == ChipsetOwnerType.Weapon)
            {
                EquipWeaponChipset(draggedChipset.weaponChipset);
                isValidDrop = true;
            }
            else if (draggedChipset.armorChipset != null && ownerType == ChipsetOwnerType.Armor)
            {
                EquipArmorChipset(draggedChipset.armorChipset);
                isValidDrop = true;
            }
            else if (draggedChipset.playerChipset != null && ownerType == ChipsetOwnerType.Player)
            {
                EquipPlayerChipset(draggedChipset.playerChipset);
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