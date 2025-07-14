using UnityEngine;
using TMPro;

public class ChipsetSlotUI : MonoBehaviour
{
    public enum ItemType { None, Weapon, Armor, Player }

    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public ChipsetSlot[] chipsetSlots;
    public GameObject saveButton;
    public GameObject clearButton;

    [Header("현재 올려진 아이템 정보")]
    public ItemType currentType = ItemType.None;
    public WeaponData currentWeapon;
    public ArmorData currentArmor;
    public string[] playerEquippedChipsets = new string[6]; // 플레이어 칩셋 슬롯 (6칸 예시)

    public WeaponSlot linkedWeaponSlot; // 인스펙터에서 연결

    // 저장(완성) 버튼 클릭 시 호출될 이벤트
    public System.Action<WeaponData> OnWeaponSave;
    public System.Action<ArmorData> OnArmorSave;
    public System.Action<string[]> OnPlayerSave;

    void Awake()
    {
        if (linkedWeaponSlot != null)
        {
            linkedWeaponSlot.OnWeaponChanged += OnLinkedWeaponChanged;
        }
        
        // GameDataRepository 데이터 로드 완료 이벤트 구독
        GameDataRepository.Instance.OnAllDataLoaded += OnDataLoaded;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (linkedWeaponSlot != null)
        {
            linkedWeaponSlot.OnWeaponChanged -= OnLinkedWeaponChanged;
        }
        
        if (GameDataRepository.Instance != null)
        {
            GameDataRepository.Instance.OnAllDataLoaded -= OnDataLoaded;
        }
    }
    
    private void OnDataLoaded()
    {
        // 데이터 로드 완료 시 UI 업데이트
        if (currentType != ItemType.None)
        {
            UpdateUI();
        }
    }
    
    private void OnLinkedWeaponChanged(WeaponData weapon)
    {
        SetItem(weapon);
    }

    // 무기 올리기
    public void SetItem(WeaponData weapon)
    {
        currentType = ItemType.Weapon;
        currentWeapon = weapon;
        currentArmor = null;
        // playerEquippedChipsets는 null로 설정하지 않고 빈 배열로 유지
        UpdateUI();
    }
    // 방어구 올리기
    public void SetItem(ArmorData armor)
    {
        currentType = ItemType.Armor;
        currentWeapon = null;
        currentArmor = armor;
        // playerEquippedChipsets는 null로 설정하지 않고 빈 배열로 유지
        UpdateUI();
    }
    // 플레이어 칩셋 배열 올리기
    public void SetPlayerChipsets(string[] chipsets)
    {
        Debug.Log($"🔧 [ChipsetSlotUI] SetPlayerChipsets 호출됨 - chipsets: {(chipsets != null ? string.Join(",", chipsets) : "null")}");
        Debug.Log($"🔧 [ChipsetSlotUI] SetPlayerChipsets 호출 전 currentType: {currentType}");
        
        currentType = ItemType.Player;
        currentWeapon = null;
        currentArmor = null;
        playerEquippedChipsets = chipsets != null ? chipsets : new string[6];
        
        Debug.Log($"🔧 [ChipsetSlotUI] currentType 설정됨: {currentType}");
        Debug.Log($"🔧 [ChipsetSlotUI] playerEquippedChipsets 설정됨: {string.Join(",", playerEquippedChipsets)}");
        
        UpdateUI();
        
        Debug.Log($"🔧 [ChipsetSlotUI] UpdateUI 호출 후 currentType: {currentType}");
    }
    // 초기화
    public void ClearItem()
    {
        currentType = ItemType.None;
        currentWeapon = null;
        currentArmor = null;
        playerEquippedChipsets = new string[6]; // 빈 배열로 초기화
        UpdateUI();
    }
    // UI 갱신
    private void UpdateUI()
    {
        Debug.Log($"🔧 [ChipsetSlotUI] UpdateUI 시작 - currentType: {currentType}");
        
        if (itemNameText != null)
        {
            if (currentType == ItemType.Weapon && currentWeapon != null)
                itemNameText.text = currentWeapon.weaponName;
            else if (currentType == ItemType.Armor && currentArmor != null)
                itemNameText.text = currentArmor.armorName;
            else if (currentType == ItemType.Player)
                itemNameText.text = "Player";
            else
                itemNameText.text = "-";
        }
        
        // 🆕 플레이어 칩셋일 때 Save 버튼 비활성화
        if (saveButton != null)
        {
            saveButton.SetActive(currentType != ItemType.Player);
        }
        
        Debug.Log($"🔧 [ChipsetSlotUI] UpdateUI SyncChipsetSlots 호출 전 - currentType: {currentType}");
        SyncChipsetSlots();
        Debug.Log($"🔧 [ChipsetSlotUI] UpdateUI SyncChipsetSlots 호출 후 - currentType: {currentType}");
    }
    // 칩셋 슬롯 UI 동기화
    private void SyncChipsetSlots()
    {
        Debug.Log($"🔧 [ChipsetSlotUI] SyncChipsetSlots 시작 - currentType: {currentType}");
        
        if (chipsetSlots == null) return;
        
        // GameDataRepository가 로드되지 않았으면 대기
        if (!GameDataRepository.Instance.IsAllDataLoaded)
        {
            Debug.LogWarning("[ChipsetSlotUI] 데이터가 아직 로드되지 않았습니다. 나중에 다시 시도합니다.");
            return;
        }
        
        string[] equippedChipsets = null;
        if (currentType == ItemType.Weapon && currentWeapon != null)
            equippedChipsets = currentWeapon.GetEquippedChipsetIds();
        else if (currentType == ItemType.Armor && currentArmor != null)
            equippedChipsets = currentArmor.GetEquippedChipsetIds();
        else if (currentType == ItemType.Player)
            equippedChipsets = playerEquippedChipsets;
        else
            equippedChipsets = new string[chipsetSlots.Length];
            
        Debug.Log($"🔧 [ChipsetSlotUI] SyncChipsetSlots - currentType: {currentType}, equippedChipsets: {(equippedChipsets != null ? string.Join(",", equippedChipsets) : "null")}");
            
        for (int i = 0; i < chipsetSlots.Length; i++)
        {
            chipsetSlots[i].parentSlotUI = this;
            chipsetSlots[i].SetSlotIndex(i);
            
            if (equippedChipsets != null && i < equippedChipsets.Length && !string.IsNullOrEmpty(equippedChipsets[i]))
            {
                object chipset = null;
                if (currentType == ItemType.Weapon)
                    chipset = GameDataRepository.Instance.GetWeaponChipsetById(equippedChipsets[i]);
                else if (currentType == ItemType.Armor)
                    chipset = GameDataRepository.Instance.GetArmorChipsetById(equippedChipsets[i]);
                else if (currentType == ItemType.Player)
                    chipset = GameDataRepository.Instance.GetPlayerChipsetById(equippedChipsets[i]);
                    
                if (chipset != null)
                {
                    if (currentType == ItemType.Weapon)
                        chipsetSlots[i].EquipWeaponChipset((WeaponChipsetData)chipset);
                    else if (currentType == ItemType.Armor)
                        chipsetSlots[i].EquipArmorChipset((ArmorChipsetData)chipset);
                    else if (currentType == ItemType.Player)
                        chipsetSlots[i].EquipPlayerChipset((PlayerChipsetData)chipset);
                }
                else
                {
                    Debug.LogWarning($"[ChipsetSlotUI] 칩셋을 찾을 수 없습니다: {equippedChipsets[i]}");
                    chipsetSlots[i].UnequipChipset();
                }
            }
            else
            {
                Debug.Log($"🔧 [ChipsetSlotUI] 슬롯 {i} UnequipChipset 호출 전 - currentType: {currentType}");
                chipsetSlots[i].UnequipChipset();
            }
        }
    }
    // 저장(완성) 버튼 클릭 시 호출
    public void OnSaveButtonClicked()
    {
        if (currentType == ItemType.Weapon && currentWeapon != null)
        {
            // 🆕 Save 시 인벤토리에서 칩셋 제거
            RemoveChipsetsFromInventoryOnSave();
            
            OnWeaponSave?.Invoke(currentWeapon);
            Debug.Log($"🔧 [ChipsetSlotUI] 무기 칩셋 저장 완료: {currentWeapon.weaponName}");
        }
        else if (currentType == ItemType.Armor && currentArmor != null)
        {
            // 🆕 Save 시 인벤토리에서 칩셋 제거
            RemoveChipsetsFromInventoryOnSave();
            
            OnArmorSave?.Invoke(currentArmor);
            Debug.Log($"🔧 [ChipsetSlotUI] 방어구 칩셋 저장 완료: {currentArmor.armorName}");
        }
        else if (currentType == ItemType.Player)
        {
            // 플레이어 칩셋은 상시 저장되므로 Save 버튼이 필요 없음
            Debug.Log($"🔧 [ChipsetSlotUI] 플레이어 칩셋은 상시 저장됩니다. Save 버튼이 필요하지 않습니다.");
            return;
        }
        
        // 🆕 Save 후 UI 업데이트
        UpdateUIAfterSave();
    }
    
    /// <summary>
    /// Save 시 인벤토리에서 칩셋 제거
    /// </summary>
    private void RemoveChipsetsFromInventoryOnSave()
    {
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null) 
        {
            Debug.LogError($"🔧 [ChipsetSlotUI] InventoryManager를 찾을 수 없습니다!");
            return;
        }
        
        // 현재 장착된 칩셋들을 인벤토리에서 제거
        foreach (var slot in chipsetSlots)
        {
            if (slot.IsEquipped())
            {
                var chipset = slot.GetCurrentChipset();
                if (chipset != null)
                {
                    inventoryManager.RemoveChipset(chipset, false); // UI 새로고침 없이 제거
                }
            }
        }
        
        // 수동으로 인벤토리 UI 새로고침
        inventoryManager.RefreshInventory();
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
    /// Save 후 UI 업데이트
    /// </summary>
    private void UpdateUIAfterSave()
    {
        // 🆕 플레이어 칩셋의 경우 playerEquippedChipsets 배열을 현재 슬롯 상태로 업데이트
        if (currentType == ItemType.Player)
        {
            UpdatePlayerEquippedChipsetsFromSlots();
            // 플레이어 칩셋의 경우 SyncChipsetSlots()를 호출하지 않음 (이미 슬롯에 칩셋이 있으므로)
        }
        else
        {
            // 🆕 무기/방어구 칩셋의 경우에만 슬롯 UI 동기화
            SyncChipsetSlots();
        }
        
        // ChipsetManager의 개수 정보 업데이트
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 개수 정보 업데이트를 위한 메서드 호출
            chipsetManager.UpdateChipsetInfo();
            
            // 코스트 표시 업데이트
            chipsetManager.UpdateAllCostDisplays();
        }
        
        // InventoryManager UI 새로고침
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RefreshInventory();
        }
        
        Debug.Log($"🔧 [ChipsetSlotUI] Save 후 UI 업데이트 완료 (타입: {currentType})");
    }
    
    /// <summary>
    /// 현재 슬롯 상태를 기반으로 playerEquippedChipsets 배열 업데이트
    /// </summary>
    private void UpdatePlayerEquippedChipsetsFromSlots()
    {
        if (playerEquippedChipsets == null || playerEquippedChipsets.Length != chipsetSlots.Length)
        {
            playerEquippedChipsets = new string[chipsetSlots.Length];
        }
        
        for (int i = 0; i < chipsetSlots.Length; i++)
        {
            if (chipsetSlots[i].IsEquipped())
            {
                var chipset = chipsetSlots[i].GetCurrentChipset();
                if (chipset is PlayerChipsetData playerChipset)
                {
                    playerEquippedChipsets[i] = playerChipset.chipsetId;
                    Debug.Log($"🔧 [ChipsetSlotUI] 플레이어 칩셋 배열 업데이트: 슬롯 {i} = {playerChipset.chipsetName}");
                }
            }
            else
            {
                playerEquippedChipsets[i] = null;
            }
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 상시 저장 (장착/해제 시 즉시 호출)
    /// </summary>
    /// <param name="isEquipping">true = 장착 시, false = 해제 시</param>
    public void SavePlayerChipsetsImmediately(bool isEquipping = true)
    {
        if (currentType != ItemType.Player) 
        {
            Debug.LogWarning($"🔧 [ChipsetSlotUI] 플레이어 칩셋이 아닙니다: {currentType}");
            return;
        }
        
        // 현재 슬롯 상태를 배열에 반영
        UpdatePlayerEquippedChipsetsFromSlots();
        
        // 즉시 저장 이벤트 호출
        OnPlayerSave?.Invoke(playerEquippedChipsets);
        
        if (isEquipping)
        {
            // 🆕 장착 시에만 인벤토리에서 칩셋 제거
            RemoveChipsetsFromInventoryOnSave();
        }
        
        // UI 업데이트
        UpdateUIAfterSaveForPlayer();
        
        Debug.Log($"🔧 [ChipsetSlotUI] 플레이어 칩셋 상시 저장 완료 (장착: {isEquipping})");
    }
    
    /// <summary>
    /// 플레이어 칩셋 전용 UI 업데이트 (인벤토리 제거 로직 제외)
    /// </summary>
    private void UpdateUIAfterSaveForPlayer()
    {
        // 🆕 플레이어 칩셋의 경우 playerEquippedChipsets 배열을 현재 슬롯 상태로 업데이트
        UpdatePlayerEquippedChipsetsFromSlots();
        
        // ChipsetManager의 개수 정보 업데이트
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 개수 정보 업데이트를 위한 메서드 호출
            chipsetManager.UpdateChipsetInfo();
            
            // 코스트 표시 업데이트
            chipsetManager.UpdateAllCostDisplays();
        }
        
        // InventoryManager UI 새로고침
        var inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RefreshInventory();
        }
        
        Debug.Log($"🔧 [ChipsetSlotUI] 플레이어 칩셋 UI 업데이트 완료");
    }
} 