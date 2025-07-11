using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 칩셋 시스템 메인 매니저
/// 무기, 방어구, 플레이어 칩셋을 관리하는 UI 시스템
/// </summary>
public class ChipsetManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject weaponChipsetPanel;
    [SerializeField] private GameObject armorChipsetPanel;
    [SerializeField] private GameObject playerChipsetPanel;
    [SerializeField] private GameObject inventoryPanel;
    
    [Header("Weapon Chipset UI")]
    [SerializeField] private Transform weaponSlotsParent;
    [SerializeField] private ChipsetSlot[] weaponSlots;
    [SerializeField] private TextMeshProUGUI weaponTotalCostText;
    [SerializeField] private TextMeshProUGUI weaponMaxCostText;
    [SerializeField] private Slider weaponCostSlider;
    
    [Header("Armor Chipset UI")]
    [SerializeField] private Transform armorSlotsParent;
    [SerializeField] private ChipsetSlot[] armorSlots;
    [SerializeField] private TextMeshProUGUI armorTotalCostText;
    [SerializeField] private TextMeshProUGUI armorMaxCostText;
    [SerializeField] private Slider armorCostSlider;
    
    [Header("Player Chipset UI")]
    [SerializeField] private Transform playerSlotsParent;
    [SerializeField] private ChipsetSlot[] playerSlots;
    [SerializeField] private TextMeshProUGUI playerTotalCostText;
    [SerializeField] private TextMeshProUGUI playerMaxCostText;
    [SerializeField] private Slider playerCostSlider;
    
    [Header("Inventory UI")]
    [SerializeField] private Transform weaponChipsetInventoryParent;
    [SerializeField] private Transform armorChipsetInventoryParent;
    [SerializeField] private Transform playerChipsetInventoryParent;
    [SerializeField] private GameObject chipsetItemPrefab;
    
    [Header("Settings")]
    [SerializeField] private int weaponMaxCost = 10;
    [SerializeField] private int armorMaxCost = 8;
    [SerializeField] private int playerMaxCost = 12;
    
    [Header("Effect Manager")]
    [SerializeField] private ChipsetEffectManager effectManager;
    
    // 현재 선택된 무기/방어구
    private WeaponData currentWeapon;
    private ArmorData currentArmor;
    
    // 플레이어 칩셋 ID 배열
    private string[] playerChipsetIds = new string[0];
    
    // 칩셋 인벤토리 아이템들
    private List<ChipsetItem> weaponChipsetItems = new List<ChipsetItem>();
    private List<ChipsetItem> armorChipsetItems = new List<ChipsetItem>();
    private List<ChipsetItem> playerChipsetItems = new List<ChipsetItem>();
    
    private void Awake()
    {
        InitializeSlots();
        SetupEventListeners();
        
        if (effectManager == null)
            effectManager = FindObjectOfType<ChipsetEffectManager>();
    }
    
    private void Start()
    {
        LoadChipsetInventory();
        UpdateAllCostDisplays();
    }
    
    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    private void InitializeSlots()
    {
        // 무기 슬롯 초기화
        if (weaponSlots == null || weaponSlots.Length == 0)
        {
            weaponSlots = weaponSlotsParent?.GetComponentsInChildren<ChipsetSlot>();
        }
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].SetSlotIndex(i);
        }
        
        // 방어구 슬롯 초기화
        if (armorSlots == null || armorSlots.Length == 0)
        {
            armorSlots = armorSlotsParent?.GetComponentsInChildren<ChipsetSlot>();
        }
        
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].SetSlotIndex(i);
        }
        
        // 플레이어 슬롯 초기화
        if (playerSlots == null || playerSlots.Length == 0)
        {
            playerSlots = playerSlotsParent?.GetComponentsInChildren<ChipsetSlot>();
        }
        
        for (int i = 0; i < playerSlots.Length; i++)
        {
            playerSlots[i].SetSlotIndex(i);
        }
    }
    
    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        // 무기 슬롯 이벤트
        foreach (var slot in weaponSlots)
        {
            slot.OnChipsetEquipped += OnWeaponChipsetEquipped;
            slot.OnChipsetUnequipped += OnWeaponChipsetUnequipped;
        }
        
        // 방어구 슬롯 이벤트
        foreach (var slot in armorSlots)
        {
            slot.OnChipsetEquipped += OnArmorChipsetEquipped;
            slot.OnChipsetUnequipped += OnArmorChipsetUnequipped;
        }
        
        // 플레이어 슬롯 이벤트
        foreach (var slot in playerSlots)
        {
            slot.OnChipsetEquipped += OnPlayerChipsetEquipped;
            slot.OnChipsetUnequipped += OnPlayerChipsetUnequipped;
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 로드
    /// </summary>
    private void LoadChipsetInventory()
    {
        LoadWeaponChipsetInventory();
        LoadArmorChipsetInventory();
        LoadPlayerChipsetInventory();
    }
    
    /// <summary>
    /// 무기 칩셋 인벤토리 로드
    /// </summary>
    private void LoadWeaponChipsetInventory()
    {
        var weaponChipsets = GameDataRepository.Instance.GetAllWeaponChipsets();
        
        foreach (var chipset in weaponChipsets)
        {
            CreateChipsetItem(chipset, weaponChipsetInventoryParent, weaponChipsetItems);
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 인벤토리 로드
    /// </summary>
    private void LoadArmorChipsetInventory()
    {
        var armorChipsets = GameDataRepository.Instance.GetAllArmorChipsets();
        
        foreach (var chipset in armorChipsets)
        {
            CreateChipsetItem(chipset, armorChipsetInventoryParent, armorChipsetItems);
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 인벤토리 로드
    /// </summary>
    private void LoadPlayerChipsetInventory()
    {
        var playerChipsets = GameDataRepository.Instance.GetAllPlayerChipsets();
        
        foreach (var chipset in playerChipsets)
        {
            CreateChipsetItem(chipset, playerChipsetInventoryParent, playerChipsetItems);
        }
    }
    
    /// <summary>
    /// 칩셋 아이템 생성
    /// </summary>
    private void CreateChipsetItem(object chipset, Transform parent, List<ChipsetItem> itemList)
    {
        if (chipsetItemPrefab == null || parent == null) return;
        
        var itemGO = Instantiate(chipsetItemPrefab, parent);
        var chipsetItem = itemGO.GetComponent<ChipsetItem>();
        
        if (chipsetItem != null)
        {
            if (chipset is WeaponChipsetData weaponChipset)
            {
                chipsetItem.Initialize(weaponChipset);
            }
            else if (chipset is ArmorChipsetData armorChipset)
            {
                chipsetItem.Initialize(armorChipset);
            }
            else if (chipset is PlayerChipsetData playerChipset)
            {
                chipsetItem.Initialize(playerChipset);
            }
            
            itemList.Add(chipsetItem);
        }
    }
    
    /// <summary>
    /// 무기 칩셋 장착 이벤트
    /// </summary>
    private void OnWeaponChipsetEquipped(ChipsetSlot slot, object chipset)
    {
        UpdateWeaponCostDisplay();
        CheckWeaponCostOver();
        
        // 무기 데이터에 칩셋 ID 저장
        if (currentWeapon != null && chipset is WeaponChipsetData weaponChipset)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentWeapon.GetEquippedChipsetIds();
            
            // 배열 크기 확장
            if (currentChipsets.Length <= slotIndex)
            {
                string[] newChipsets = new string[weaponSlots.Length];
                currentChipsets.CopyTo(newChipsets, 0);
                currentChipsets = newChipsets;
            }
            
            currentChipsets[slotIndex] = weaponChipset.chipsetId;
            currentWeapon.SetEquippedChipsetIds(currentChipsets);
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateWeaponEffects(currentWeapon);
            }
        }
    }
    
    /// <summary>
    /// 무기 칩셋 해제 이벤트
    /// </summary>
    private void OnWeaponChipsetUnequipped(ChipsetSlot slot, object chipset)
    {
        UpdateWeaponCostDisplay();
        CheckWeaponCostOver();
        
        // 무기 데이터에서 칩셋 ID 제거
        if (currentWeapon != null)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentWeapon.GetEquippedChipsetIds();
            
            if (slotIndex < currentChipsets.Length)
            {
                currentChipsets[slotIndex] = null;
                currentWeapon.SetEquippedChipsetIds(currentChipsets);
            }
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateWeaponEffects(currentWeapon);
            }
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 장착 이벤트
    /// </summary>
    private void OnArmorChipsetEquipped(ChipsetSlot slot, object chipset)
    {
        UpdateArmorCostDisplay();
        CheckArmorCostOver();
        
        // 방어구 데이터에 칩셋 ID 저장
        if (currentArmor != null && chipset is ArmorChipsetData armorChipset)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentArmor.GetEquippedChipsetIds();
            
            // 배열 크기 확장
            if (currentChipsets.Length <= slotIndex)
            {
                string[] newChipsets = new string[armorSlots.Length];
                currentChipsets.CopyTo(newChipsets, 0);
                currentChipsets = newChipsets;
            }
            
            currentChipsets[slotIndex] = armorChipset.chipsetId;
            currentArmor.SetEquippedChipsetIds(currentChipsets);
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateArmorEffects(currentArmor);
            }
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 해제 이벤트
    /// </summary>
    private void OnArmorChipsetUnequipped(ChipsetSlot slot, object chipset)
    {
        UpdateArmorCostDisplay();
        CheckArmorCostOver();
        
        // 방어구 데이터에서 칩셋 ID 제거
        if (currentArmor != null)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentArmor.GetEquippedChipsetIds();
            
            if (slotIndex < currentChipsets.Length)
            {
                currentChipsets[slotIndex] = null;
                currentArmor.SetEquippedChipsetIds(currentChipsets);
            }
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateArmorEffects(currentArmor);
            }
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 장착 이벤트
    /// </summary>
    private void OnPlayerChipsetEquipped(ChipsetSlot slot, object chipset)
    {
        UpdatePlayerCostDisplay();
        CheckPlayerCostOver();
        
        // 플레이어 칩셋 ID 배열 업데이트
        if (chipset is PlayerChipsetData playerChipset)
        {
            int slotIndex = slot.GetSlotIndex();
            if (playerChipsetIds == null || playerChipsetIds.Length <= slotIndex)
            {
                System.Array.Resize(ref playerChipsetIds, playerSlots.Length);
            }
            
            playerChipsetIds[slotIndex] = playerChipset.chipsetId;
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculatePlayerEffects(playerChipsetIds);
            }
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 해제 이벤트
    /// </summary>
    private void OnPlayerChipsetUnequipped(ChipsetSlot slot, object chipset)
    {
        UpdatePlayerCostDisplay();
        CheckPlayerCostOver();
        
        // 플레이어 칩셋 ID 배열에서 제거
        int slotIndex = slot.GetSlotIndex();
        if (playerChipsetIds != null && slotIndex < playerChipsetIds.Length)
        {
            playerChipsetIds[slotIndex] = null;
        }
        
        // 효과 적용
        if (effectManager != null)
        {
            effectManager.CalculatePlayerEffects(playerChipsetIds);
        }
    }
    
    /// <summary>
    /// 모든 코스트 표시 업데이트
    /// </summary>
    private void UpdateAllCostDisplays()
    {
        UpdateWeaponCostDisplay();
        UpdateArmorCostDisplay();
        UpdatePlayerCostDisplay();
    }
    
    /// <summary>
    /// 무기 코스트 표시 업데이트
    /// </summary>
    private void UpdateWeaponCostDisplay()
    {
        int totalCost = GetWeaponTotalCost();
        
        if (weaponTotalCostText != null)
        {
            weaponTotalCostText.text = $"{totalCost}/{weaponMaxCost}";
        }
        
        if (weaponCostSlider != null)
        {
            weaponCostSlider.value = (float)totalCost / weaponMaxCost;
        }
    }
    
    /// <summary>
    /// 방어구 코스트 표시 업데이트
    /// </summary>
    private void UpdateArmorCostDisplay()
    {
        int totalCost = GetArmorTotalCost();
        
        if (armorTotalCostText != null)
        {
            armorTotalCostText.text = $"{totalCost}/{armorMaxCost}";
        }
        
        if (armorCostSlider != null)
        {
            armorCostSlider.value = (float)totalCost / armorMaxCost;
        }
    }
    
    /// <summary>
    /// 플레이어 코스트 표시 업데이트
    /// </summary>
    private void UpdatePlayerCostDisplay()
    {
        int totalCost = GetPlayerTotalCost();
        
        if (playerTotalCostText != null)
        {
            playerTotalCostText.text = $"{totalCost}/{playerMaxCost}";
        }
        
        if (playerCostSlider != null)
        {
            playerCostSlider.value = (float)totalCost / playerMaxCost;
        }
    }
    
    /// <summary>
    /// 무기 총 코스트 계산
    /// </summary>
    private int GetWeaponTotalCost()
    {
        return weaponSlots.Sum(slot => slot.GetCurrentCost());
    }
    
    /// <summary>
    /// 방어구 총 코스트 계산
    /// </summary>
    private int GetArmorTotalCost()
    {
        return armorSlots.Sum(slot => slot.GetCurrentCost());
    }
    
    /// <summary>
    /// 플레이어 총 코스트 계산
    /// </summary>
    private int GetPlayerTotalCost()
    {
        return playerSlots.Sum(slot => slot.GetCurrentCost());
    }
    
    /// <summary>
    /// 무기 코스트 초과 체크
    /// </summary>
    private void CheckWeaponCostOver()
    {
        int totalCost = GetWeaponTotalCost();
        bool isOver = totalCost > weaponMaxCost;
        
        foreach (var slot in weaponSlots)
        {
            slot.SetCostOver(isOver);
        }
    }
    
    /// <summary>
    /// 방어구 코스트 초과 체크
    /// </summary>
    private void CheckArmorCostOver()
    {
        int totalCost = GetArmorTotalCost();
        bool isOver = totalCost > armorMaxCost;
        
        foreach (var slot in armorSlots)
        {
            slot.SetCostOver(isOver);
        }
    }
    
    /// <summary>
    /// 플레이어 코스트 초과 체크
    /// </summary>
    private void CheckPlayerCostOver()
    {
        int totalCost = GetPlayerTotalCost();
        bool isOver = totalCost > playerMaxCost;
        
        foreach (var slot in playerSlots)
        {
            slot.SetCostOver(isOver);
        }
    }
    
    /// <summary>
    /// 현재 무기 설정
    /// </summary>
    public void SetCurrentWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        LoadWeaponChipsets();
    }
    
    /// <summary>
    /// 현재 방어구 설정
    /// </summary>
    public void SetCurrentArmor(ArmorData armor)
    {
        currentArmor = armor;
        LoadArmorChipsets();
    }
    
    /// <summary>
    /// 무기 칩셋 로드
    /// </summary>
    private void LoadWeaponChipsets()
    {
        if (currentWeapon == null) return;
        
        // 기존 칩셋 해제
        foreach (var slot in weaponSlots)
        {
            slot.UnequipChipset();
        }
        
        // 무기에 장착된 칩셋 로드
        string[] equippedChipsets = currentWeapon.GetEquippedChipsetIds();
        for (int i = 0; i < equippedChipsets.Length && i < weaponSlots.Length; i++)
        {
            var chipsetId = equippedChipsets[i];
            if (!string.IsNullOrEmpty(chipsetId))
            {
                var chipset = GameDataRepository.Instance.GetWeaponChipsetById(chipsetId);
                if (chipset != null)
                {
                    weaponSlots[i].EquipWeaponChipset(chipset);
                }
            }
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 로드
    /// </summary>
    private void LoadArmorChipsets()
    {
        if (currentArmor == null) return;
        
        // 기존 칩셋 해제
        foreach (var slot in armorSlots)
        {
            slot.UnequipChipset();
        }
        
        // 방어구에 장착된 칩셋 로드
        string[] equippedChipsets = currentArmor.GetEquippedChipsetIds();
        for (int i = 0; i < equippedChipsets.Length && i < armorSlots.Length; i++)
        {
            var chipsetId = equippedChipsets[i];
            if (!string.IsNullOrEmpty(chipsetId))
            {
                var chipset = GameDataRepository.Instance.GetArmorChipsetById(chipsetId);
                if (chipset != null)
                {
                    armorSlots[i].EquipArmorChipset(chipset);
                }
            }
        }
    }
    
    /// <summary>
    /// 패널 전환
    /// </summary>
    public void ShowWeaponPanel()
    {
        weaponChipsetPanel.SetActive(true);
        armorChipsetPanel.SetActive(false);
        playerChipsetPanel.SetActive(false);
    }
    
    public void ShowArmorPanel()
    {
        weaponChipsetPanel.SetActive(false);
        armorChipsetPanel.SetActive(true);
        playerChipsetPanel.SetActive(false);
    }
    
    public void ShowPlayerPanel()
    {
        weaponChipsetPanel.SetActive(false);
        armorChipsetPanel.SetActive(false);
        playerChipsetPanel.SetActive(true);
    }
    
    public void ShowInventoryPanel()
    {
        inventoryPanel.SetActive(true);
    }
    
    public void HideInventoryPanel()
    {
        inventoryPanel.SetActive(false);
    }
    
    /// <summary>
    /// 칩셋 효과 요약 정보 반환
    /// </summary>
    public string GetEffectsSummary()
    {
        if (effectManager != null)
        {
            return effectManager.GetEffectsSummary();
        }
        return "효과 매니저가 없습니다.";
    }
} 