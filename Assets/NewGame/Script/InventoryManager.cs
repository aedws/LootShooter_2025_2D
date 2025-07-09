using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum SortType
{
    None,
    Name,
    Type,
    Damage,
    FireRate,
    Defense,    // 🆕 방어구 방어력 정렬
    Rarity      // 🆕 방어구 레어리티 정렬
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
public enum InventoryTab
{
    Weapons,
    Armors
}

[System.Serializable]
public class InventoryManager : MonoBehaviour
{
    [Header("📋 사용 방법")]
    [TextArea(4, 10)]
    public string instructions = "🆕 동적 세로 인벤토리 시스템 + 스크롤바:\n1. inventoryPanel에 인벤토리 UI 패널 연결\n2. slotParent에 VerticalLayoutGroup이 있는 부모 Transform 연결\n3. slotPrefab에 InventorySlot 컴포넌트가 있는 프리팹 연결\n4. weaponSlotManager에 WeaponSlotManager 연결 (3개 슬롯 지원)\n5. slotSize로 가로/세로 크기 개별 조정 가능 (기본: 200x50)\n6. 무기 추가 시 자동으로 슬롯 생성 (1개씩 세로로)\n7. 무기 제거 시 불필요한 빈 슬롯 자동 정리\n8. I키로 인벤토리 열기/닫기\n9. 🆕 스크롤바: scrollViewHeight로 스크롤 영역 높이 조정\n10. 🆕 스크롤바: scrollbar에 Scrollbar 컴포넌트 연결 (선택사항)\n11. 🆕 마스킹: viewport 영역 밖의 슬롯들은 자동으로 숨김\n\n💡 이제 격자가 아닌 리스트 형태로 동적 확장되며 스크롤 가능합니다!";
    
    [Header("🔧 UI References")]
    [Tooltip("인벤토리 UI 전체 패널 (활성화/비활성화됨)")]
    public GameObject inventoryPanel;
    
    [Tooltip("🆕 무기 슬롯 패널 (인벤토리와 함께 표시됨)")]
    public GameObject weaponSlotsPanel;
    
    [Tooltip("슬롯들이 생성될 부모 Transform (VerticalLayoutGroup 자동 생성됨)")]
    public Transform slotParent;
    
    [Tooltip("InventorySlot 컴포넌트가 있는 슬롯 프리팹")]
    public GameObject slotPrefab;
    
    [Header("📜 Scroll System")]
    [Tooltip("🆕 스크롤 영역을 제한하는 viewport 패널")]
    public GameObject scrollViewport;
    
    [Tooltip("🆕 스크롤바 UI 컴포넌트")]
    public Scrollbar scrollbar;
    
    [Tooltip("🆕 스크롤 영역의 높이 (픽셀)")]
    [Range(200f, 800f)]
    public float scrollViewHeight = 400f;
    
    [Header("🔫 Weapon Slot System")]
    [Tooltip("🆕 무기 슬롯 매니저 (3개 슬롯 지원)")]
    public WeaponSlotManager weaponSlotManager;
    
    [Tooltip("⚠️ 레거시 무기 슬롯 (단일 슬롯, 호환성 유지)")]
    public WeaponSlot weaponSlot;
    
    [Header("🛡️ Armor Slot System")]
    [Tooltip("🆕 방어구 슬롯 매니저 (6개 슬롯 지원)")]
    public ArmorSlotManager armorSlotManager;
    
    [Tooltip("🆕 방어구 슬롯 패널 (인벤토리와 함께 표시됨)")]
    public GameObject armorSlotsPanel;
    
    [Header("⚙️ Inventory Settings")]
    [Tooltip("각 슬롯의 크기 (픽셀) - X: 가로, Y: 세로")]
    public Vector2 slotSize = new Vector2(200f, 50f);
    [Tooltip("슬롯 간 간격 (픽셀)")]
    [Range(5f, 20f)]
    public float slotSpacing = 10f;
    [Tooltip("최소 빈 슬롯 개수 (항상 이만큼 여유분 유지)")]
    [Range(1, 5)]
    public int minEmptySlots = 10;
    [Tooltip("인벤토리 최대 슬롯 수 (확장 가능)")]
    public int maxInventorySlots = 100;
    [Tooltip("인벤토리 정보 텍스트 (현재/최대/장착 수 등)")]
    public Text inventoryInfoText;
    
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
    
    [Header("📑 Tab System")]
    [Tooltip("무기 탭 버튼")]
    public Button weaponTabButton;
    
    [Tooltip("방어구 탭 버튼")]
    public Button armorTabButton;
    
    [Tooltip("무기 탭 활성화 색상")]
    public Color activeTabColor = Color.cyan;
    
    [Tooltip("탭 비활성화 색상")]
    public Color inactiveTabColor = Color.gray;
    
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
    
    // 🆕 방어구 관련 변수들
    private List<ArmorData> armors = new List<ArmorData>();
    private List<ArmorData> filteredArmors = new List<ArmorData>();
    
    private PlayerInventory playerInventory;
    private bool isInitialized = false;
    private bool isOpen = false;
    private SortType currentSort = SortType.None;
    private FilterType currentFilter = FilterType.All;
    private string currentSearchTerm = "";
    
    // 🆕 탭 시스템 변수들
    private InventoryTab currentTab = InventoryTab.Weapons;
    
    // Events
    public System.Action<bool> OnInventoryToggle;
    public System.Action<WeaponData> OnWeaponEquipped;
    public System.Action<WeaponData> OnWeaponUnequipped;
    
    // 🆕 방어구 이벤트들
    public System.Action<ArmorData> OnArmorAdded;
    public System.Action<ArmorData> OnArmorRemoved;
    
    void Awake()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        
        // WeaponSlotManager 자동 연결
        if (weaponSlotManager == null)
            weaponSlotManager = FindFirstObjectByType<WeaponSlotManager>();
        
        // WeaponSlotsPanel 자동 연결
        if (weaponSlotsPanel == null)
            weaponSlotsPanel = GameObject.Find("WeaponSlotsPanel");
        
        // 🆕 ArmorSlotManager 자동 연결
        if (armorSlotManager == null)
            armorSlotManager = FindFirstObjectByType<ArmorSlotManager>();
        
        // 🆕 ArmorSlotsPanel 자동 연결
        if (armorSlotsPanel == null)
            armorSlotsPanel = GameObject.Find("ArmorSlotsPanel");
        
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
        
        // 🆕 방어구 슬롯 패널도 함께 관리
        if (armorSlotsPanel != null)
            armorSlotsPanel.SetActive(false);
        
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
        
        // 🆕 스크롤 시스템 설정
        SetupScrollSystem();
        
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
        // 위쪽 여유 10 적용
        verticalLayout.padding.top = 10;
        // 세로 레이아웃 설정 (위쪽 기준점에서 아래로만 늘어남)
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
        
        // 🆕 slotParent 설정 (위쪽 중앙 앵커, Y축 0에서 시작)
        RectTransform parentRect = slotParent.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            // 앵커를 위쪽 중앙으로 설정
            parentRect.anchorMin = new Vector2(0.5f, 1f);
            parentRect.anchorMax = new Vector2(0.5f, 1f);
            parentRect.pivot = new Vector2(0.5f, 1f);
            // 위치 조정: X축 -245, Y축 -10
            parentRect.anchoredPosition = new Vector2(-245f, -10f);
        }
        
        // 초기 슬롯 생성 (동적으로 필요한 만큼만 생성)
        CreateInitialSlots();
    }
    
    // 🆕 스크롤바 가시성 제어
    void UpdateScrollbarVisibility()
    {
        if (scrollbar == null) return;
        
        // Content의 높이와 Viewport의 높이 비교
        RectTransform contentRect = slotParent.GetComponent<RectTransform>();
        RectTransform viewportRect = scrollViewport != null ? scrollViewport.GetComponent<RectTransform>() : null;
        
        if (contentRect != null && viewportRect != null)
        {
            float contentHeight = contentRect.sizeDelta.y;
            float viewportHeight = viewportRect.sizeDelta.y;
            
            // Content가 Viewport보다 크면 스크롤바 표시
            bool shouldShowScrollbar = contentHeight > viewportHeight;
            scrollbar.gameObject.SetActive(shouldShowScrollbar);
        }
    }
    
    // 🆕 스크롤 시스템 설정
    void SetupScrollSystem()
    {
        // ScrollRect 컴포넌트 추가
        ScrollRect scrollRect = slotParent.parent.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = slotParent.parent.gameObject.AddComponent<ScrollRect>();
        }
        
        // ScrollRect 설정
        scrollRect.content = slotParent.GetComponent<RectTransform>();
        scrollRect.horizontal = false; // 수평 스크롤 비활성화
        scrollRect.vertical = true;    // 수직 스크롤 활성화
        scrollRect.scrollSensitivity = 10f; // 스크롤 감도
        scrollRect.inertia = true;     // 관성 스크롤 활성화
        scrollRect.decelerationRate = 0.135f; // 감속률
        
        // 🆕 Viewport 설정 (스크롤 영역 제한)
        if (scrollViewport == null)
        {
            // Viewport 자동 생성
            scrollViewport = new GameObject("ScrollViewport");
            scrollViewport.transform.SetParent(slotParent.parent);
            scrollViewport.transform.SetSiblingIndex(slotParent.GetSiblingIndex());
        }
        
        RectTransform viewportRect = scrollViewport.GetComponent<RectTransform>();
        if (viewportRect == null)
            viewportRect = scrollViewport.AddComponent<RectTransform>();
        
        // Viewport 크기 및 위치 설정 (Unity에서 설정한 값 그대로 사용)
        // anchorMin, anchorMax, sizeDelta 등은 Unity에서 직접 설정
        
        // Mask 컴포넌트 추가 (영역 밖 숨김)
        Mask mask = scrollViewport.GetComponent<Mask>();
        if (mask == null)
            mask = scrollViewport.AddComponent<Mask>();
        
        // Mask 배경 이미지 추가
        Image maskImage = scrollViewport.GetComponent<Image>();
        if (maskImage == null)
            maskImage = scrollViewport.AddComponent<Image>();
        
        maskImage.color = new Color(1f, 1f, 1f, 0.1f); // 반투명 배경
        
        // slotParent를 viewport의 자식으로 이동
        slotParent.SetParent(viewportRect);
        
        // ScrollRect의 viewport 설정
        scrollRect.viewport = viewportRect;
        
        // 🆕 스크롤바 설정 (필요할 때만 표시)
        if (scrollbar != null)
        {
            scrollRect.verticalScrollbar = scrollbar;
            scrollbar.onValueChanged.AddListener((value) => {
                // 스크롤바 값 변경 시 추가 처리 (필요시)
            });
            
            // 초기에는 스크롤바 숨김
            scrollbar.gameObject.SetActive(false);
        }
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
        
        // 🆕 탭 버튼 설정
        if (weaponTabButton != null)
        {
            weaponTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Weapons));
        }
        
        if (armorTabButton != null)
        {
            armorTabButton.onClick.AddListener(() => SwitchTab(InventoryTab.Armors));
        }
        
        // 🆕 스크롤바 설정
        if (scrollbar != null)
        {
            // 스크롤바 값 변경 시 추가 처리 (필요시)
            scrollbar.onValueChanged.AddListener((value) => {
                // 스크롤바 드래그 시 추가 로직 (필요시)
            });
        }
    }
    
    // 🆕 탭 전환 메서드
    public void SwitchTab(InventoryTab newTab)
    {
        if (currentTab == newTab) return;
        
        currentTab = newTab;
        UpdateTabVisuals();
        RefreshInventory();
        
        // 🆕 UI 입력 포커스 설정으로 게임 입력 충돌 방지
        StartCoroutine(ClearInputFocusAfterTabSwitch());
        
        Debug.Log($"🔄 인벤토리 탭 전환: {currentTab}");
    }
    
    // 🆕 탭 전환 후 입력 포커스 정리
    System.Collections.IEnumerator ClearInputFocusAfterTabSwitch()
    {
        // 1프레임 대기 후 입력 포커스 해제
        yield return null;
        
        // EventSystem에서 선택된 UI 요소 해제
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    
    // 🆕 탭 시각적 업데이트
    void UpdateTabVisuals()
    {
        if (weaponTabButton != null)
        {
            ColorBlock colors = weaponTabButton.colors;
            colors.normalColor = currentTab == InventoryTab.Weapons ? activeTabColor : inactiveTabColor;
            weaponTabButton.colors = colors;
        }
        
        if (armorTabButton != null)
        {
            ColorBlock colors = armorTabButton.colors;
            colors.normalColor = currentTab == InventoryTab.Armors ? activeTabColor : inactiveTabColor;
            armorTabButton.colors = colors;
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
            
            // 🆕 탭 시각적 업데이트
            UpdateTabVisuals();
            
            RefreshInventory();
            
            // 무기 슬롯 패널도 함께 활성화
            if (weaponSlotsPanel != null)
            {
                weaponSlotsPanel.SetActive(true);
            }
            
            // 🆕 방어구 슬롯 패널도 함께 활성화
            if (armorSlotsPanel != null)
            {
                armorSlotsPanel.SetActive(true);
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
            
            // 🆕 방어구 슬롯 패널도 함께 비활성화
            if (armorSlotsPanel != null)
            {
                armorSlotsPanel.SetActive(false);
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
        
        Debug.Log($"🔄 [InventoryManager] RefreshInventory 시작 - 현재 탭: {currentTab}, 방어구 개수: {armors.Count}");
        
        ApplyFiltersAndSort();
        UpdateSlots();
        UpdateUI();
        
        // 🆕 스크롤바 가시성 업데이트
        UpdateScrollbarVisibility();
        
        Debug.Log($"✅ [InventoryManager] RefreshInventory 완료");
    }
    
    void ApplyFiltersAndSort()
    {
        // 🆕 무기 필터링
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
        
        // 🆕 방어구 필터링 (장착된 방어구 제외)
        filteredArmors = armors.Where(armor => 
        {
            // ArmorSlotManager에 장착된 방어구들은 인벤토리에서 제외
            if (armorSlotManager != null && armorSlotManager.IsArmorEquipped(armor))
            {
                return false;
            }
            
            // 검색 필터 (방어구 이름으로도 검색 가능)
            if (!string.IsNullOrEmpty(currentSearchTerm))
            {
                if (!armor.armorName.ToLower().Contains(currentSearchTerm.ToLower()))
                    return false;
            }
            
            return true;
        }).ToList();
        
        // 정렬
        switch (currentSort)
        {
            case SortType.Name:
                filteredWeapons = filteredWeapons.OrderBy(w => w.weaponName).ToList();
                filteredArmors = filteredArmors.OrderBy(a => a.armorName).ToList();
                break;
            case SortType.Type:
                filteredWeapons = filteredWeapons.OrderBy(w => w.weaponType).ToList();
                filteredArmors = filteredArmors.OrderBy(a => a.armorType).ToList();
                break;
            case SortType.Damage:
                filteredWeapons = filteredWeapons.OrderByDescending(w => w.damage).ToList();
                filteredArmors = filteredArmors.OrderByDescending(a => a.defense).ToList();
                break;
            case SortType.FireRate:
                filteredWeapons = filteredWeapons.OrderBy(w => w.fireRate).ToList();
                // 방어구는 발사속도가 없으므로 레어리티로 정렬
                filteredArmors = filteredArmors.OrderByDescending(a => a.rarity).ToList();
                break;
            case SortType.Defense:
                // 🆕 방어구 전용: 방어력 순 정렬
                filteredWeapons = filteredWeapons.OrderByDescending(w => w.damage).ToList(); // 무기는 데미지로 대체
                filteredArmors = filteredArmors.OrderByDescending(a => a.defense).ToList();
                break;
            case SortType.Rarity:
                // 🆕 방어구 전용: 레어리티 순 정렬
                filteredWeapons = filteredWeapons.OrderByDescending(w => w.damage).ToList(); // 무기는 데미지로 대체
                filteredArmors = filteredArmors.OrderByDescending(a => a.rarity).ToList();
                break;
        }
    }
    
    void ForceRebuildLayout()
    {
        if (slotParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(slotParent.GetComponent<RectTransform>());
        }
    }

    System.Collections.IEnumerator DelayedRebuildLayout()
    {
        yield return null; // 한 프레임 대기
        ForceRebuildLayout();
        UpdateScrollbarVisibility();
    }

    void UpdateSlots()
    {
        List<object> itemsToShow = new List<object>();
        if (currentTab == InventoryTab.Weapons)
        {
            itemsToShow.AddRange(filteredWeapons.Cast<object>());
            Debug.Log($"🔫 [InventoryManager] 무기 탭 - 표시할 무기: {filteredWeapons.Count}개");
        }
        else if (currentTab == InventoryTab.Armors)
        {
            itemsToShow.AddRange(filteredArmors.Cast<object>());
            Debug.Log($"🛡️ [InventoryManager] 방어구 탭 - 표시할 방어구: {filteredArmors.Count}개");
        }

        int targetSlotCount = Mathf.Min(itemsToShow.Count + minEmptySlots, maxInventorySlots);
        Debug.Log($"📦 [InventoryManager] 목표 슬롯 수: {targetSlotCount} (아이템: {itemsToShow.Count}, 최소 빈 슬롯: {minEmptySlots})");

        // 슬롯이 부족하면 새로 생성
        while (inventorySlots.Count < targetSlotCount)
        {
            CreateSingleSlot(inventorySlots.Count);
        }
        
        // 남는 슬롯은 ClearSlot만 호출 (파괴하지 않음)
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < itemsToShow.Count)
            {
                if (currentTab == InventoryTab.Weapons)
                {
                    WeaponData weapon = itemsToShow[i] as WeaponData;
                    inventorySlots[i].isArmorSlot = false;
                    inventorySlots[i].SetWeapon(weapon);
                }
                else if (currentTab == InventoryTab.Armors)
                {
                    ArmorData armor = itemsToShow[i] as ArmorData;
                    inventorySlots[i].isArmorSlot = true;
                    inventorySlots[i].SetArmor(armor);
                    Debug.Log($"🛡️ [InventoryManager] 슬롯 {i}에 방어구 설정: {armor.armorName} (등급: {armor.rarity})");
                }
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
        StartCoroutine(DelayedRebuildLayout());
    }
    
    void UpdateUI()
    {
        if (inventoryInfoText != null)
        {
            string info = "";
            if (currentTab == InventoryTab.Weapons)
            {
                int totalWeapons = weapons.Count;
                int equippedWeaponCount = GetEquippedWeaponCount();
                info = $"무기: {totalWeapons} / {maxInventorySlots} | 장착: {equippedWeaponCount}";
            }
            else if (currentTab == InventoryTab.Armors)
            {
                int totalArmors = armors.Count;
                int equippedArmorCount = armorSlotManager != null ? armorSlotManager.GetEquippedArmorCount() : 0;
                info = $"방어구: {totalArmors} / {maxInventorySlots} | 장착: {equippedArmorCount}";
            }
            inventoryInfoText.text = info;
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
        
        // 🆕 탭 상태 저장
        PlayerPrefs.SetInt("InventoryTab", (int)currentTab);
    }
    
    void LoadInventoryState()
    {
        currentSort = (SortType)PlayerPrefs.GetInt("InventorySort", 0);
        currentFilter = (FilterType)PlayerPrefs.GetInt("InventoryFilter", 0);
        currentSearchTerm = PlayerPrefs.GetString("InventorySearch", "");
        
        // 🆕 탭 상태 로드
        currentTab = (InventoryTab)PlayerPrefs.GetInt("InventoryTab", 0);
        
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
    
    // 🆕 방어구 관련 메서드들
    
    public void AddArmor(ArmorData armor)
    {
        if (armor == null) 
        {
            Debug.LogError("❌ [InventoryManager] 방어구 데이터가 null입니다!");
            return;
        }
        
        if (!armors.Contains(armor))
        {
            armors.Add(armor);
            OnArmorAdded?.Invoke(armor);
            Debug.Log($"🛡️ 방어구 추가: {armor.armorName} (총 {armors.Count}개 보유)");
            
            // 🆕 방어구 추가 후 인벤토리 리프레시
            Debug.Log($"🔄 [InventoryManager] 방어구 추가 후 인벤토리 리프레시 시작");
            RefreshInventory();
            Debug.Log($"✅ [InventoryManager] 방어구 추가 후 인벤토리 리프레시 완료");
        }
        else
        {
            Debug.LogWarning($"⚠️ [InventoryManager] 이미 보유한 방어구입니다: {armor.armorName}");
        }
    }
    
    public void RemoveArmor(ArmorData armor, bool shouldRefresh = true)
    {
        if (armor == null) return;
        
        if (armors.Remove(armor))
        {
            OnArmorRemoved?.Invoke(armor);
            Debug.Log($"🛡️ 방어구 제거: {armor.armorName}");
            
            if (shouldRefresh)
            {
                RefreshInventory();
            }
        }
    }
    
    public List<ArmorData> GetArmorsByType(ArmorType armorType)
    {
        return armors.Where(armor => armor.armorType == armorType).ToList();
    }
    
    public List<ArmorData> GetArmorsByRarity(ArmorRarity rarity)
    {
        return armors.Where(armor => armor.rarity == rarity).ToList();
    }
    
    public List<ArmorData> GetAllArmors()
    {
        return new List<ArmorData>(armors);
    }
    
    public bool HasArmor(ArmorData armor)
    {
        return armors.Contains(armor);
    }
    
    public int GetArmorCount()
    {
        return armors.Count;
    }
    
    public int GetArmorCountByType(ArmorType armorType)
    {
        return armors.Count(armor => armor.armorType == armorType);
    }
    
    void OnDestroy()
    {
        SaveInventoryState();
        
        // 🆕 방어구 이벤트 구독 해제
        OnArmorAdded = null;
        OnArmorRemoved = null;
    }
} 