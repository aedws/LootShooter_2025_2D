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
    public string instructions = "🆕 동적 세로 인벤토리 시스템:\n1. inventoryPanel에 인벤토리 UI 패널 연결\n2. slotParent에 VerticalLayoutGroup이 있는 부모 Transform 연결\n3. slotPrefab에 InventorySlot 컴포넌트가 있는 프리팹 연결\n4. weaponSlotManager에 WeaponSlotManager 연결 (3개 슬롯 지원)\n5. slotSize로 가로/세로 크기 개별 조정 가능 (기본: 200x50)\n6. 무기 추가 시 자동으로 슬롯 생성 (1개씩 세로로)\n7. 무기 제거 시 불필요한 빈 슬롯 자동 정리\n8. I키로 인벤토리 열기/닫기\n\n💡 이제 격자가 아닌 리스트 형태로 동적 확장됩니다!";
    
    [Header("🔧 UI References")]
    [Tooltip("인벤토리 UI 전체 패널 (활성화/비활성화됨)")]
    public GameObject inventoryPanel;
    
    [Tooltip("🆕 무기 슬롯 패널 (인벤토리와 함께 표시됨)")]
    public GameObject weaponSlotsPanel;
    
    [Tooltip("슬롯들이 생성될 부모 Transform (VerticalLayoutGroup 자동 생성됨)")]
    public Transform slotParent;
    
    [Tooltip("InventorySlot 컴포넌트가 있는 슬롯 프리팹")]
    public GameObject slotPrefab;
    
    [Header("🔫 Weapon Slot System")]
    [Tooltip("🆕 무기 슬롯 매니저 (3개 슬롯 지원)")]
    public WeaponSlotManager weaponSlotManager;
    
    [Tooltip("⚠️ 레거시 무기 슬롯 (단일 슬롯, 호환성 유지)")]
    public WeaponSlot weaponSlot;
    
    [Header("⚙️ Inventory Settings")]
    [Tooltip("각 슬롯의 크기 (픽셀) - X: 가로, Y: 세로")]
    public Vector2 slotSize = new Vector2(200f, 50f);
    
    [Tooltip("슬롯 간 간격 (픽셀)")]
    [Range(5f, 20f)]
    public float slotSpacing = 10f;
    
    [Tooltip("최소 빈 슬롯 개수 (항상 이만큼 여유분 유지)")]
    [Range(1, 5)]
    public int minEmptySlots = 2;
    
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
        
        // 기존 GridLayoutGroup 제거 (있다면)
        GridLayoutGroup gridLayout = slotParent.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            DestroyImmediate(gridLayout);
        }
        
        // VerticalLayoutGroup 설정 (1개씩 세로로 배열)
        VerticalLayoutGroup verticalLayout = slotParent.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
            verticalLayout = slotParent.gameObject.AddComponent<VerticalLayoutGroup>();
        
        // 세로 레이아웃 설정
        verticalLayout.spacing = slotSpacing;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;
        
        // ContentSizeFitter 추가 (동적 크기 조정)
        ContentSizeFitter contentSizeFitter = slotParent.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
            contentSizeFitter = slotParent.gameObject.AddComponent<ContentSizeFitter>();
        
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // 초기 슬롯 생성 (동적으로 필요한 만큼만 생성)
        CreateInitialSlots();
    }
    
    void CreateInitialSlots()
    {
        // 초기에는 빈 슬롯 몇 개만 생성
        int initialSlotCount = Mathf.Max(1, weapons.Count + minEmptySlots); // 현재 무기 + 최소 빈 슬롯
        
        for (int i = 0; i < initialSlotCount; i++)
        {
            CreateSingleSlot(i);
        }
    }
    
    void CreateSingleSlot(int slotIndex)
    {
        GameObject slotObj = Instantiate(slotPrefab, slotParent);
        
        // 슬롯 크기 설정 (가로/세로 개별 설정 가능)
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            slotRect.sizeDelta = slotSize; // Vector2 직접 사용
        }
        
        // 슬롯 활성화 보장
        slotObj.SetActive(true);
        
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        
        if (slot != null)
        {
            slot.slotIndex = slotIndex;
            slot.inventoryManager = this;
            inventorySlots.Add(slot);
            
            // 슬롯 컴포넌트도 활성화 보장
            slot.enabled = true;
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
            
            // 동적으로 슬롯 생성 (필요한 경우)
            EnsureEnoughSlots();
            
            // 초기화가 완료된 경우에만 UI 새로고침
            if (isInitialized)
            {
                RefreshInventory();
            }
            

        }
    }
    
    void EnsureEnoughSlots()
    {
        // 필요한 슬롯 수 계산 (현재 무기 + 최소 빈 슬롯)
        int requiredSlots = weapons.Count + minEmptySlots;
        
        // 현재 슬롯 수보다 더 많이 필요하면 슬롯 추가
        while (inventorySlots.Count < requiredSlots)
        {
            CreateSingleSlot(inventorySlots.Count);
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
            
            // 빈 슬롯이 너무 많으면 제거
            CleanupExcessSlots();
            
            // 초기화가 완료되고 새로고침이 요청된 경우에만 UI 새로고침
            if (shouldRefresh && isInitialized)
            {
                RefreshInventory();
            }
            

        }
    }
    
    void CleanupExcessSlots()
    {
        // 필요한 슬롯 수 계산
        int requiredSlots = weapons.Count + minEmptySlots;
        int maxAllowedSlots = weapons.Count + (minEmptySlots * 2); // 최대 허용 슬롯 (여유분 2배)
        
        // 슬롯이 너무 많으면 뒤에서부터 제거
        while (inventorySlots.Count > maxAllowedSlots && inventorySlots.Count > requiredSlots)
        {
            int lastIndex = inventorySlots.Count - 1;
            InventorySlot lastSlot = inventorySlots[lastIndex];
            
            // 빈 슬롯만 제거
            if (lastSlot != null && lastSlot.weaponData == null)
            {
                inventorySlots.RemoveAt(lastIndex);
                if (lastSlot.gameObject != null)
                {
                    DestroyImmediate(lastSlot.gameObject);
                }
            }
            else
            {
                break; // 무기가 있는 슬롯을 만나면 중단
            }
        }
    }
    
    public void RefreshInventory()
    {
        // 초기화가 완료되지 않았으면 새로고침 건너뛰기
        if (!isInitialized)
        {
            // Debug.LogWarning("⚠️ [InventoryManager] 아직 초기화가 완료되지 않아 새로고침을 건너뜁니다.");
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
                // Debug.LogWarning("⚠️ [InventoryManager] 모든 무기 슬롯이 가득참!");
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
            // Debug.LogError("❌ [InventoryManager] WeaponSlotManager와 weaponSlot이 모두 없습니다!");
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
            // Debug.LogError("❌ [InventoryManager] WeaponSlotManager가 없습니다!");
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
            // Debug.LogError("❌ [InventoryManager] WeaponSlotManager가 없습니다!");
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
        // 동적 인벤토리에서는 항상 확장 가능하므로 가득 차지 않음
        // 대신 과도한 무기 수집을 방지하기 위한 합리적인 제한 설정
        int maxReasonableWeapons = 50; // 합리적인 최대 무기 수
        return weapons.Count >= maxReasonableWeapons;
    }
    
    void OnDestroy()
    {
        SaveInventoryState();
    }
} 