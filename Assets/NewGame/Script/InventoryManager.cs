using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum SortType
{
    None,
    Name,
    Type,
    Damage,
    FireRate
}

[System.Serializable]
public enum FilterType
{
    All,
    AR,  // Assault Rifle
    HG,  // Handgun
    MG,  // Machine Gun
    SG,  // Shotgun
    SMG, // Submachine Gun
    SR   // Sniper Rifle
}

[System.Serializable]
public class InventoryManager : MonoBehaviour
{
    [Header("📋 사용 방법")]
    [TextArea(3, 8)]
    public string instructions = "1. inventoryPanel에 인벤토리 UI 패널 연결\n2. weaponSlotsPanel에 무기 슬롯 패널 연결 (자동 연결됨)\n3. slotParent에 슬롯들이 들어갈 부모 Transform 연결\n4. slotPrefab에 InventorySlot 컴포넌트가 있는 프리팹 연결\n5. weaponSlotManager에 WeaponSlotManager 연결 (3개 슬롯 지원)\n6. 게임 실행 후 I키로 인벤토리 열기\n7. F1-F5로 테스트 (InventoryTester 필요)\n\n💡 WeaponSlotsPanel은 인벤토리와 함께 열리고 닫힙니다!";
    
    [Header("🔧 UI References")]
    [Tooltip("인벤토리 UI 전체 패널 (활성화/비활성화됨)")]
    public GameObject inventoryPanel;
    
    [Tooltip("🆕 무기 슬롯 패널 (인벤토리와 함께 표시됨)")]
    public GameObject weaponSlotsPanel;
    
    [Tooltip("슬롯들이 생성될 부모 Transform (GridLayoutGroup 권장)")]
    public Transform slotParent;
    
    [Tooltip("InventorySlot 컴포넌트가 있는 슬롯 프리팹")]
    public GameObject slotPrefab;
    
    [Header("🔫 Weapon Slot System")]
    [Tooltip("🆕 무기 슬롯 매니저 (3개 슬롯 지원)")]
    public WeaponSlotManager weaponSlotManager;
    
    [Tooltip("⚠️ 레거시 무기 슬롯 (단일 슬롯, 호환성 유지)")]
    public WeaponSlot weaponSlot;
    
    [Header("⚙️ Inventory Settings")]
    [Tooltip("최대 슬롯 개수 (권장: 20-40개)")]
    [Range(5, 50)]
    public int maxSlots = 20;
    
    [Tooltip("한 줄에 표시할 슬롯 개수")]
    [Range(3, 10)]
    public int slotsPerRow = 5;
    
    [Tooltip("각 슬롯의 크기 (픽셀)")]
    [Range(50f, 100f)]
    public float slotSize = 70f;
    
    [Tooltip("슬롯 간 간격 (픽셀)")]
    [Range(5f, 20f)]
    public float slotSpacing = 10f;
    
    [Header("🎛️ UI Components (선택사항)")]
    [Tooltip("정렬 방식 선택 드롭다운")]
    public Dropdown sortDropdown;
    
    [Tooltip("무기 타입 필터 드롭다운")]
    public Dropdown filterDropdown;
    
    [Tooltip("정렬 새로고침 버튼")]
    public Button sortButton;
    
    [Tooltip("무기 이름 검색 필드")]
    public InputField searchField;
    
    [Tooltip("인벤토리 제목 텍스트 (개수 표시용)")]
    public Text inventoryTitle;
    
    [Header("💬 Tooltip System (선택사항)")]
    [Tooltip("툴팁 표시 패널")]
    public GameObject tooltipPanel;
    
    [Tooltip("무기 이름 표시 텍스트")]
    public Text tooltipName;
    
    [Tooltip("무기 타입 표시 텍스트")]
    public Text tooltipType;
    
    [Tooltip("데미지 정보 표시 텍스트")]
    public Text tooltipDamage;
    
    [Tooltip("발사속도 정보 표시 텍스트")]
    public Text tooltipFireRate;
    
    [Tooltip("탄약 정보 표시 텍스트")]
    public Text tooltipAmmo;
    
    [Tooltip("무기 설명 표시 텍스트")]
    public Text tooltipDescription;
    
    [Header("🔊 Sound Effects (선택사항)")]
    [Tooltip("사운드 재생용 AudioSource")]
    public AudioSource audioSource;
    
    [Tooltip("인벤토리 열기 사운드")]
    public AudioClip openSound;
    
    [Tooltip("인벤토리 닫기 사운드")]
    public AudioClip closeSound;
    
    [Tooltip("무기 장착 사운드")]
    public AudioClip equipSound;
    
    [Tooltip("아이템 드롭 사운드")]
    public AudioClip dropSound;
    
    // Private variables
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private List<WeaponData> weapons = new List<WeaponData>();
    private List<WeaponData> filteredWeapons = new List<WeaponData>();
    private PlayerInventory playerInventory;
    private bool isInitialized = false;
    private bool isOpen = false;
    private SortType currentSort = SortType.None;
    private FilterType currentFilter = FilterType.All;
    private string currentSearchTerm = "";
    
    // Events
    public System.Action<bool> OnInventoryToggle;
    public System.Action<WeaponData> OnWeaponEquipped;
    public System.Action<WeaponData> OnWeaponUnequipped;
    
    void Awake()
    {
        playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        // WeaponSlotManager 자동 연결
        if (weaponSlotManager == null)
            weaponSlotManager = FindAnyObjectByType<WeaponSlotManager>();
        
        // WeaponSlotsPanel 자동 연결
        if (weaponSlotsPanel == null)
            weaponSlotsPanel = GameObject.Find("WeaponSlotsPanel");
        
        CreateInventoryGrid();
        SetupUI();
        LoadInventoryState();
        
        isInitialized = true;
    }
    
    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        
        if (weaponSlotsPanel != null)
            weaponSlotsPanel.SetActive(false);
        
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // I키로 인벤토리 열기/닫기
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        
        // ESC키로 인벤토리 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseInventory();
        }
    }
    
    void CreateInventoryGrid()
    {
        if (slotParent == null || slotPrefab == null) return;
        
        // 기존 슬롯들 제거
        foreach (Transform child in slotParent)
        {
            DestroyImmediate(child.gameObject);
        }
        inventorySlots.Clear();
        
        // GridLayoutGroup 설정
        GridLayoutGroup gridLayout = slotParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = slotParent.gameObject.AddComponent<GridLayoutGroup>();
        
        gridLayout.cellSize = new Vector2(slotSize, slotSize);
        gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = slotsPerRow;
        
        // 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            
            // 🔧 슬롯 활성화 보장 (프리팹이 비활성화 상태여도 문제없음)
            slotObj.SetActive(true);
            
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            
            if (slot != null)
            {
                slot.slotIndex = i;
                slot.inventoryManager = this;
                inventorySlots.Add(slot);
                
                // 슬롯 컴포넌트도 활성화 보장
                slot.enabled = true;
            }
            
            // 로그 제거
        }
    }
    
    void SetupUI()
    {
        // 정렬 드롭다운 설정
        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
            sortDropdown.options.Clear();
            
            foreach (SortType sortType in System.Enum.GetValues(typeof(SortType)))
            {
                sortDropdown.options.Add(new Dropdown.OptionData(sortType.ToString()));
            }
            sortDropdown.value = 0;
        }
        
        // 필터 드롭다운 설정
        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            filterDropdown.options.Clear();
            
            foreach (FilterType filterType in System.Enum.GetValues(typeof(FilterType)))
            {
                filterDropdown.options.Add(new Dropdown.OptionData(filterType.ToString()));
            }
            filterDropdown.value = 0;
        }
        
        // 검색 필드 설정
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchChanged);
        }
        
        // 정렬 버튼 설정
        if (sortButton != null)
        {
            sortButton.onClick.AddListener(RefreshInventory);
        }
    }
    
    public void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }
    
    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            isOpen = true;
            RefreshInventory();
            
            // 무기 슬롯 패널도 함께 활성화
            if (weaponSlotsPanel != null)
            {
                weaponSlotsPanel.SetActive(true);
            }
            
            if (audioSource != null && openSound != null)
                audioSource.PlayOneShot(openSound);
            
            OnInventoryToggle?.Invoke(true);
        }
    }
    
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isOpen = false;
            HideTooltip();
            
            // 무기 슬롯 패널도 함께 비활성화
            if (weaponSlotsPanel != null)
            {
                weaponSlotsPanel.SetActive(false);
            }
            
            if (audioSource != null && closeSound != null)
                audioSource.PlayOneShot(closeSound);
            
            OnInventoryToggle?.Invoke(false);
            
            SaveInventoryState();
        }
    }
    
    public void AddWeapon(WeaponData weapon)
    {
        if (weapon != null && !weapons.Contains(weapon))
        {
            weapons.Add(weapon);
            
            // 초기화가 완료된 경우에만 UI 새로고침
            if (isInitialized)
            {
                RefreshInventory();
            }
            

        }
    }
    
    public void RemoveWeapon(WeaponData weapon)
    {
        RemoveWeapon(weapon, true);
    }
    
    public void RemoveWeapon(WeaponData weapon, bool shouldRefresh)
    {
        if (weapon != null && weapons.Contains(weapon))
        {
            weapons.Remove(weapon);
            
            // 초기화가 완료되고 새로고침이 요청된 경우에만 UI 새로고침
            if (shouldRefresh && isInitialized)
            {
                RefreshInventory();
            }
            

        }
    }
    
    public void RefreshInventory()
    {
        // 초기화가 완료되지 않았으면 새로고침 건너뛰기
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ [InventoryManager] 아직 초기화가 완료되지 않아 새로고침을 건너뜁니다.");
            return;
        }
        
        ApplyFiltersAndSort();
        UpdateSlots();
        UpdateUI();
    }
    
    void ApplyFiltersAndSort()
    {
        // 필터링
        filteredWeapons = weapons.Where(weapon => 
        {
            // 🔫 WeaponSlotManager에 장착된 무기들은 인벤토리에서 제외
            if (weaponSlotManager != null && weaponSlotManager.HasWeapon(weapon))
            {
                return false;
            }
            
            // 🔧 레거시 호환성: 기존 단일 weaponSlot 체크
            if (weaponSlot != null && weaponSlot.weaponData == weapon)
            {
                return false;
            }
            
            // 타입 필터
            if (currentFilter != FilterType.All)
            {
                WeaponType filterWeaponType = (WeaponType)System.Enum.Parse(typeof(WeaponType), currentFilter.ToString());
                if (weapon.weaponType != filterWeaponType)
                    return false;
            }
            
            // 검색 필터
            if (!string.IsNullOrEmpty(currentSearchTerm))
            {
                if (!weapon.weaponName.ToLower().Contains(currentSearchTerm.ToLower()))
                    return false;
            }
            
            return true;
        }).ToList();
        
        // 정렬
        switch (currentSort)
        {
            case SortType.Name:
                filteredWeapons = filteredWeapons.OrderBy(w => w.weaponName).ToList();
                break;
            case SortType.Type:
                filteredWeapons = filteredWeapons.OrderBy(w => w.weaponType).ToList();
                break;
            case SortType.Damage:
                filteredWeapons = filteredWeapons.OrderByDescending(w => w.damage).ToList();
                break;
            case SortType.FireRate:
                filteredWeapons = filteredWeapons.OrderBy(w => w.fireRate).ToList();
                break;
        }
    }
    
    void UpdateSlots()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < filteredWeapons.Count)
            {
                inventorySlots[i].SetWeapon(filteredWeapons[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }
    
    void UpdateUI()
    {
        if (inventoryTitle != null)
        {
            int totalWeapons = weapons.Count;
            int filteredCount = filteredWeapons.Count;
            int equippedCount = GetEquippedWeaponCount();
            
            inventoryTitle.text = $"Inventory ({filteredCount}/{totalWeapons}) | Equipped: {equippedCount}";
        }
    }
    
    // UI 이벤트 핸들러들
    void OnSortChanged(int value)
    {
        currentSort = (SortType)value;
        RefreshInventory();
    }
    
    void OnFilterChanged(int value)
    {
        currentFilter = (FilterType)value;
        RefreshInventory();
    }
    
    void OnSearchChanged(string searchTerm)
    {
        currentSearchTerm = searchTerm;
        RefreshInventory();
    }
    
    // 툴팁 시스템
    public void ShowTooltip(WeaponData weapon, Vector3 position)
    {
        if (tooltipPanel == null || weapon == null) return;
        
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.position = position;
        
        if (tooltipName != null) tooltipName.text = weapon.weaponName;
        if (tooltipType != null) tooltipType.text = $"Type: {weapon.weaponType}";
        if (tooltipDamage != null) tooltipDamage.text = $"Damage: {weapon.damage}";
        if (tooltipFireRate != null) tooltipFireRate.text = $"Fire Rate: {weapon.fireRate:F2}s";
        if (tooltipAmmo != null) tooltipAmmo.text = $"Ammo: {weapon.currentAmmo}/{weapon.maxAmmo}";
    }
    
    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    // 무기 장착/해제 (새로운 다중 슬롯 지원)
    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        
        // 🔫 WeaponSlotManager 우선 사용
        if (weaponSlotManager != null)
        {
            // 빈 슬롯 찾아서 장착
            int emptySlot = weaponSlotManager.GetEmptySlotIndex();
            if (emptySlot != -1)
            {
                bool success = weaponSlotManager.EquipWeaponToSlot(weapon, emptySlot);
                if (success)
                {
                    // 인벤토리에서 무기 제거 (UI 새로고침 없이)
                    RemoveWeapon(weapon, false);
                    RefreshInventory(); // 수동으로 UI 새로고침
                    
                    if (audioSource != null && equipSound != null)
                        audioSource.PlayOneShot(equipSound);
                    
                    OnWeaponEquipped?.Invoke(weapon);
                    

                    return;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ [InventoryManager] 모든 무기 슬롯이 가득참!");
                return;
            }
        }
        
        // 🔧 레거시 호환성: 기존 단일 weaponSlot 사용
        if (weaponSlot != null)
        {
            weaponSlot.EquipWeapon(weapon);
            RefreshInventory();
            
            if (audioSource != null && equipSound != null)
                audioSource.PlayOneShot(equipSound);
            
            OnWeaponEquipped?.Invoke(weapon);
            

        }
        else
        {
            Debug.LogError("❌ [InventoryManager] WeaponSlotManager와 weaponSlot이 모두 없습니다!");
        }
    }
    
    public void UnequipWeapon()
    {
        // 🔫 WeaponSlotManager 우선 사용 (현재 활성 슬롯 해제)
        if (weaponSlotManager != null)
        {
            WeaponData currentWeapon = weaponSlotManager.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                weaponSlotManager.UnequipWeaponFromSlot(weaponSlotManager.currentSlotIndex);
                RefreshInventory();
                
                OnWeaponUnequipped?.Invoke(currentWeapon);
                

                return;
            }
        }
        
        // 🔧 레거시 호환성: 기존 단일 weaponSlot 사용
        if (weaponSlot != null && weaponSlot.weaponData != null)
        {
            WeaponData unequippedWeapon = weaponSlot.weaponData;
            weaponSlot.ClearSlot();
            RefreshInventory();
            
            OnWeaponUnequipped?.Invoke(unequippedWeapon);
            

        }
    }
    
    // 🔫 새로운 다중 슬롯 지원 메서드들
    public bool EquipWeaponToSpecificSlot(WeaponData weapon, int slotIndex)
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryManager] WeaponSlotManager가 없습니다!");
            return false;
        }
        
        bool success = weaponSlotManager.EquipWeaponToSlot(weapon, slotIndex);
        if (success)
        {
            // 인벤토리에서 무기 제거
            RemoveWeapon(weapon, false);
            RefreshInventory();
            
            if (audioSource != null && equipSound != null)
                audioSource.PlayOneShot(equipSound);
            
            OnWeaponEquipped?.Invoke(weapon);
            

        }
        
        return success;
    }
    
    public void UnequipWeaponFromSpecificSlot(int slotIndex)
    {
        if (weaponSlotManager == null)
        {
            Debug.LogError("❌ [InventoryManager] WeaponSlotManager가 없습니다!");
            return;
        }
        
        WeaponData weaponToUnequip = weaponSlotManager.GetWeaponInSlot(slotIndex);
        if (weaponToUnequip != null)
        {
            weaponSlotManager.UnequipWeaponFromSlot(slotIndex);
            RefreshInventory();
            
            OnWeaponUnequipped?.Invoke(weaponToUnequip);
            

        }
    }
    
    public List<WeaponData> GetAllEquippedWeapons()
    {
        List<WeaponData> equippedWeapons = new List<WeaponData>();
        
        // WeaponSlotManager에서 장착된 무기들 가져오기
        if (weaponSlotManager != null)
        {
            equippedWeapons.AddRange(weaponSlotManager.GetAllEquippedWeapons());
        }
        
        // 레거시 weaponSlot에서도 가져오기
        if (weaponSlot != null && weaponSlot.weaponData != null)
        {
            if (!equippedWeapons.Contains(weaponSlot.weaponData))
            {
                equippedWeapons.Add(weaponSlot.weaponData);
            }
        }
        
        return equippedWeapons;
    }
    
    public int GetEquippedWeaponCount()
    {
        return GetAllEquippedWeapons().Count;
    }
    
    public bool IsWeaponEquipped(WeaponData weapon)
    {
        if (weapon == null) return false;
        
        // WeaponSlotManager 체크
        if (weaponSlotManager != null && weaponSlotManager.HasWeapon(weapon))
        {
            return true;
        }
        
        // 레거시 weaponSlot 체크
        if (weaponSlot != null && weaponSlot.weaponData == weapon)
        {
            return true;
        }
        
        return false;
    }
    
    // 저장/로드 시스템
    void SaveInventoryState()
    {
        // 간단한 PlayerPrefs 저장 (나중에 JSON으로 확장 가능)
        PlayerPrefs.SetInt("InventorySort", (int)currentSort);
        PlayerPrefs.SetInt("InventoryFilter", (int)currentFilter);
        PlayerPrefs.SetString("InventorySearch", currentSearchTerm);
    }
    
    void LoadInventoryState()
    {
        currentSort = (SortType)PlayerPrefs.GetInt("InventorySort", 0);
        currentFilter = (FilterType)PlayerPrefs.GetInt("InventoryFilter", 0);
        currentSearchTerm = PlayerPrefs.GetString("InventorySearch", "");
        
        // UI 업데이트
        if (sortDropdown != null) sortDropdown.value = (int)currentSort;
        if (filterDropdown != null) filterDropdown.value = (int)currentFilter;
        if (searchField != null) searchField.text = currentSearchTerm;
    }
    
    // PlayerInventory와의 연동
    public List<WeaponData> GetWeapons()
    {
        return new List<WeaponData>(weapons);
    }
    
    public bool HasWeapon(WeaponData weapon)
    {
        return weapons.Contains(weapon);
    }
    
    public int GetWeaponCount()
    {
        return weapons.Count;
    }
    
    public bool IsFull()
    {
        return weapons.Count >= maxSlots;
    }
    
    void OnDestroy()
    {
        SaveInventoryState();
    }
} 