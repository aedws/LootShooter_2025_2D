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
    
    [Header("Tab System")]
    [SerializeField] private Button weaponChipsetTabButton; // 무기 칩셋 탭 버튼
    [SerializeField] private Button armorChipsetTabButton; // 방어구 칩셋 탭 버튼
    [SerializeField] private Button playerChipsetTabButton; // 플레이어 칩셋 탭 버튼
    [SerializeField] private Color activeTabColor = Color.cyan;
    [SerializeField] private Color inactiveTabColor = Color.gray;
    
    [Header("Info Display")]
    [SerializeField] private Text chipsetInfoText; // 칩셋 개수 표시 텍스트
    
    [Header("Settings")]
    [SerializeField] private int weaponMaxCost = 10;
    [SerializeField] private int armorMaxCost = 8;
    [SerializeField] private int playerMaxCost = 12;
    
    [Header("Effect Manager")]
    [SerializeField] private ChipsetEffectManager effectManager;
    
    [Header("Inventory Manager Reference")]
    [SerializeField] private InventoryManager inventoryManager; // 기존 인벤토리 매니저 참조
    
    [Header("ChipsetSlotUI Reference")]
    [SerializeField] private ChipsetSlotUI chipsetSlotUI; // 칩셋 슬롯 UI 참조
    
    [Header("Chipset Spawn Settings")]
    [SerializeField] private GameObject chipsetPickupPrefab; // 칩셋 픽업 프리팹
    [SerializeField] private Transform spawnPoint; // 칩셋 소환 위치 (플레이어 근처)
    
    [Header("🔧 칩셋 해제 옵션")]
    [Tooltip("칩셋 해제 시 인벤토리로 반환할지 여부")]
    public bool returnToInventoryOnUnequip = true;
    
    [Tooltip("칩셋 해제 옵션 토글 버튼 (선택사항)")]
    public Toggle returnToInventoryToggle;
    
    // 현재 선택된 무기/방어구
    private WeaponData currentWeapon;
    private ArmorData currentArmor;
    
    // 플레이어 칩셋 ID 배열
    private string[] playerChipsetIds = new string[0];
    
    // 현재 선택된 탭
    private ChipsetTab currentTab = ChipsetTab.Weapon;
    
    // 칩셋 인벤토리 저장 데이터
    private List<string> playerWeaponChipsetInventory = new List<string>();
    private List<string> playerArmorChipsetInventory = new List<string>();
    private List<string> playerPlayerChipsetInventory = new List<string>();
    
    // 칩셋 탭 enum
    public enum ChipsetTab
    {
        Weapon,
        Armor,
        Player
    }
    
    private void Awake()
    {
        InitializeSlots();
        SetupEventListeners();
        
        if (effectManager == null)
            effectManager = FindAnyObjectByType<ChipsetEffectManager>();
            
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
            
        // 🆕 ChipsetSlotUI는 인벤토리 열 때 찾도록 변경
    }
    
    private void Start()
    {
        // 🆕 ChipsetSlotUI는 인벤토리 열 때 찾도록 변경
        
        // 이벤트 구독
        GameDataRepository.Instance.OnAllDataLoaded += OnDataLoaded;
        
        // 이미 데이터가 로드되어 있다면 바로 로드
        if (GameDataRepository.Instance.IsAllDataLoaded)
        {
            OnDataLoaded();
        }
        
        // 🆕 PlayerPrefs 완전 초기화 (개발 중에만 사용)
        ClearAllChipsetInventories();
        
        // 칩셋 인벤토리 로드
        LoadChipsetInventoryData();
        
        // 카테고리 버튼 설정
        SetupCategoryButtons();
        
        // 🆕 칩셋 해제 옵션 토글 설정
        SetupChipsetReturnToggle();
        
        // 모든 칩셋 패널 상시 활성화
        ShowAllChipsetPanels();
        
        // 탭에 따라 패널 표시/숨김
        UpdatePanelVisibility();
        
        // 탭 UI 업데이트
        UpdateCategoryUI();
        
        Debug.Log("[ChipsetManager] 시작 완료");
    }
    
    /// <summary>
    /// 카테고리 버튼 이벤트 설정
    /// </summary>
    private void SetupCategoryButtons()
    {
        if (weaponChipsetTabButton != null)
            weaponChipsetTabButton.onClick.AddListener(OnWeaponChipsetTabButtonClicked);
        
        if (armorChipsetTabButton != null)
            armorChipsetTabButton.onClick.AddListener(OnArmorChipsetTabButtonClicked);
        
        if (playerChipsetTabButton != null)
            playerChipsetTabButton.onClick.AddListener(OnPlayerChipsetTabButtonClicked);
    }
    
    /// <summary>
    /// 칩셋 해제 옵션 토글 설정
    /// </summary>
    private void SetupChipsetReturnToggle()
    {
        if (returnToInventoryToggle != null)
        {
            returnToInventoryToggle.isOn = returnToInventoryOnUnequip;
            returnToInventoryToggle.onValueChanged.AddListener(OnChipsetReturnToggleChanged);
        }
    }
    
    /// <summary>
    /// 칩셋 해제 옵션 토글 변경 이벤트
    /// </summary>
    private void OnChipsetReturnToggleChanged(bool value)
    {
        returnToInventoryOnUnequip = value;
        Debug.Log($"🔧 [ChipsetManager] 칩셋 해제 옵션 변경: {(value ? "인벤토리 반환" : "소멸")}");
    }
    
    private void Update()
    {
        // F4키로 칩셋 소환
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SpawnRandomChipset();
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameDataRepository.Instance != null)
        {
            GameDataRepository.Instance.OnAllDataLoaded -= OnDataLoaded;
        }
        
        // 🆕 토글 이벤트 구독 해제
        if (returnToInventoryToggle != null)
        {
            returnToInventoryToggle.onValueChanged.RemoveListener(OnChipsetReturnToggleChanged);
        }
    }
    
    private void OnDataLoaded()
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
        // 모든 칩셋을 하나의 인벤토리에 로드 (구분하지 않음)
        LoadAllChipsetItems();
        
        // 탭에 따라 패널 표시/숨김
        UpdatePanelVisibility();
        
        // 탭 UI 업데이트
        UpdateCategoryUI();
    }
    
    /// <summary>
    /// 모든 칩셋 아이템 로드 (구분하지 않음)
    /// </summary>
    private void LoadAllChipsetItems()
    {
        // InventoryManager를 통해 칩셋 아이템들을 슬롯에 추가
        if (inventoryManager != null)
        {
            inventoryManager.ClearChipsets(); // 칩셋 리스트 초기화
            // 무기 칩셋 로드
            foreach (var chipsetId in playerWeaponChipsetInventory)
            {
                if (!string.IsNullOrEmpty(chipsetId))
                {
                    var chipset = GameDataRepository.Instance.GetWeaponChipsetById(chipsetId);
                    if (chipset != null)
                    {
                        // InventoryManager의 슬롯 시스템 사용
                        AddChipsetToInventorySlot(chipset);
                    }
                }
            }
            
            // 방어구 칩셋 로드
            foreach (var chipsetId in playerArmorChipsetInventory)
            {
                if (!string.IsNullOrEmpty(chipsetId))
                {
                    var chipset = GameDataRepository.Instance.GetArmorChipsetById(chipsetId);
                    if (chipset != null)
                    {
                        // InventoryManager의 슬롯 시스템 사용
                        AddChipsetToInventorySlot(chipset);
                    }
                }
            }
            
            // 플레이어 칩셋 로드
            foreach (var chipsetId in playerPlayerChipsetInventory)
            {
                if (!string.IsNullOrEmpty(chipsetId))
                {
                    var chipset = GameDataRepository.Instance.GetPlayerChipsetById(chipsetId);
                    if (chipset != null)
                    {
                        // InventoryManager의 슬롯 시스템 사용
                        AddChipsetToInventorySlot(chipset);
                    }
                }
            }
            
            // InventoryManager UI 새로고침
            inventoryManager.RefreshInventory();
        }
        
        Debug.Log($"[ChipsetManager] 칩셋 인벤토리 로드: (무기: {playerWeaponChipsetInventory.Count}, 방어구: {playerArmorChipsetInventory.Count}, 플레이어: {playerPlayerChipsetInventory.Count})");
    }
    
    /// <summary>
    /// InventoryManager의 슬롯 시스템을 사용하여 칩셋을 인벤토리에 추가
    /// </summary>
    private void AddChipsetToInventorySlot(object chipset)
    {
        if (inventoryManager == null) return;
        
        // InventoryManager의 AddChipset 메서드 사용
        inventoryManager.AddChipset(chipset);
        
        Debug.Log($"[ChipsetManager] 칩셋을 InventoryManager에 추가: {GetChipsetName(chipset)}");
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
    /// 모든 칩셋 패널을 상시 활성화
    /// </summary>
    private void ShowAllChipsetPanels()
    {
        // 무기 칩셋 패널 상시 활성화
        if (weaponChipsetPanel != null)
        {
            weaponChipsetPanel.SetActive(true);
        }
        
        // 방어구 칩셋 패널 상시 활성화
        if (armorChipsetPanel != null)
        {
            armorChipsetPanel.SetActive(true);
        }
        
        // 플레이어 칩셋 패널 상시 활성화
        if (playerChipsetPanel != null)
        {
            playerChipsetPanel.SetActive(true);
        }
        
        Debug.Log("[ChipsetManager] 모든 칩셋 패널이 상시 활성화되었습니다.");
    }
    
    /// <summary>
    /// 탭에 따라 패널 표시/숨김 (현재는 사용하지 않음 - 상시 활성화)
    /// </summary>
    private void UpdatePanelVisibility()
    {
        // 모든 패널을 상시 활성화로 변경
        ShowAllChipsetPanels();
    }
        
    /// <summary>
    /// 카테고리 변경
    /// </summary>
    public void ChangeCategory(ChipsetTab newCategory)
        {
        if (currentTab != newCategory)
        {
            currentTab = newCategory;
            LoadChipsetInventory();
        }
    }
    
    /// <summary>
    /// 카테고리 UI 업데이트
    /// </summary>
    private void UpdateCategoryUI()
    {
        // 탭 버튼 색상 업데이트 (InventoryManager와 동일한 방식)
        if (weaponChipsetTabButton != null)
        {
            ColorBlock colors = weaponChipsetTabButton.colors;
            colors.normalColor = (currentTab == ChipsetTab.Weapon) ? activeTabColor : inactiveTabColor;
            weaponChipsetTabButton.colors = colors;
        }
        if (armorChipsetTabButton != null)
        {
            ColorBlock colors = armorChipsetTabButton.colors;
            colors.normalColor = (currentTab == ChipsetTab.Armor) ? activeTabColor : inactiveTabColor;
            armorChipsetTabButton.colors = colors;
        }
        if (playerChipsetTabButton != null)
        {
            ColorBlock colors = playerChipsetTabButton.colors;
            colors.normalColor = (currentTab == ChipsetTab.Player) ? activeTabColor : inactiveTabColor;
            playerChipsetTabButton.colors = colors;
        }
        
        // 칩셋 개수 정보 업데이트
        UpdateChipsetInfo();
    }
    
    /// <summary>
    /// 카테고리 표시 이름 반환
    /// </summary>
    private string GetCategoryDisplayName(ChipsetTab category)
    {
        switch (category)
        {
            case ChipsetTab.Weapon: return "무기 칩셋";
            case ChipsetTab.Armor: return "방어구 칩셋";
            case ChipsetTab.Player: return "플레이어 칩셋";
            default: return "알 수 없음";
        }
    }
    
    /// <summary>
    /// 칩셋 개수 정보 업데이트 (InventoryManager와 동일한 방식)
    /// </summary>
    public void UpdateChipsetInfo()
    {
        if (chipsetInfoText != null)
        {
            int totalWeaponChipsets = playerWeaponChipsetInventory.Count;
            int totalArmorChipsets = playerArmorChipsetInventory.Count;
            int totalPlayerChipsets = playerPlayerChipsetInventory.Count;
            int totalChipsets = totalWeaponChipsets + totalArmorChipsets + totalPlayerChipsets;
            
            // 장착된 칩셋 개수 계산
            int equippedWeaponChipsets = 0;
            int equippedArmorChipsets = 0;
            int equippedPlayerChipsets = 0;
            
            // 무기 칩셋 장착 개수
            if (currentWeapon != null)
            {
                string[] equippedIds = currentWeapon.GetEquippedChipsetIds();
                equippedWeaponChipsets = equippedIds != null ? equippedIds.Count(id => !string.IsNullOrEmpty(id)) : 0;
            }
            
            // 방어구 칩셋 장착 개수
            if (currentArmor != null)
            {
                string[] equippedIds = currentArmor.GetEquippedChipsetIds();
                equippedArmorChipsets = equippedIds != null ? equippedIds.Count(id => !string.IsNullOrEmpty(id)) : 0;
            }
            
            // 플레이어 칩셋 장착 개수
            equippedPlayerChipsets = playerChipsetIds != null ? playerChipsetIds.Count(id => !string.IsNullOrEmpty(id)) : 0;
            
            int totalEquipped = equippedWeaponChipsets + equippedArmorChipsets + equippedPlayerChipsets;
            
            string info = $"칩셋: {totalChipsets}개 | 장착: {totalEquipped}개";
            chipsetInfoText.text = info;
        }
    }
    
    // 카테고리 변경 버튼 이벤트 메서드들
    public void OnWeaponChipsetTabButtonClicked()
    {
        // 🆕 탭 버튼 클릭 시 ChipsetSlotUI 찾기
        if (chipsetSlotUI == null)
        {
            var playerChipsetPanel = GameObject.Find("PlayerChipsetPanel");
            if (playerChipsetPanel != null)
            {
                chipsetSlotUI = playerChipsetPanel.GetComponent<ChipsetSlotUI>();
                Debug.Log($"🔧 [ChipsetManager] 무기 탭 버튼 클릭 시 PlayerChipsetPanel에서 ChipsetSlotUI 찾기: {(chipsetSlotUI != null ? "성공" : "실패")}");
            }
        }
        
        // 칩셋 탭으로 전환 (PlayerPrefs에서 다시 로드하지 않음)
        if (inventoryManager != null)
        {
            inventoryManager.SwitchTab(InventoryTab.Chipsets);
            inventoryManager.OpenInventory();
            // 강제로 새로고침 실행
            StartCoroutine(DelayedRefreshInventory());
        }
    }
    
    public void OnArmorChipsetTabButtonClicked()
    {
        Debug.Log($"🔧 [ChipsetManager] 방어구 칩셋 탭 버튼 클릭됨");
        
        // 🆕 탭 버튼 클릭 시 ChipsetSlotUI 찾기
        if (chipsetSlotUI == null)
        {
            var armorChipsetPanel = GameObject.Find("ArmorChipsetPanel");
            if (armorChipsetPanel != null)
            {
                chipsetSlotUI = armorChipsetPanel.GetComponent<ChipsetSlotUI>();
                Debug.Log($"🔧 [ChipsetManager] 방어구 탭 버튼 클릭 시 ArmorChipsetPanel에서 ChipsetSlotUI 찾기: {(chipsetSlotUI != null ? "성공" : "실패")}");
            }
        }
        
        // 🆕 현재 장착된 방어구 중 첫 번째 방어구를 자동으로 선택
        if (chipsetSlotUI != null)
        {
            var armorSlotManager = FindFirstObjectByType<ArmorSlotManager>();
            if (armorSlotManager != null)
            {
                var equippedArmors = armorSlotManager.GetAllEquippedArmors();
                if (equippedArmors.Count > 0)
                {
                    // 첫 번째 장착된 방어구를 선택
                    var firstArmor = equippedArmors.Values.First();
                    chipsetSlotUI.SetItem(firstArmor);
                    currentArmor = firstArmor;
                    Debug.Log($"🔧 [ChipsetManager] 자동으로 방어구 선택: {firstArmor.armorName}");
                }
                else
                {
                    Debug.LogWarning($"🔧 [ChipsetManager] 장착된 방어구가 없습니다!");
                }
            }
        }
        
        // 칩셋 탭으로 전환 (PlayerPrefs에서 다시 로드하지 않음)
        if (inventoryManager != null)
        {
            inventoryManager.SwitchTab(InventoryTab.Chipsets);
            inventoryManager.OpenInventory();
            // 강제로 새로고침 실행
            StartCoroutine(DelayedRefreshInventory());
        }
    }
    
    public void OnPlayerChipsetTabButtonClicked()
    {
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 탭 버튼 클릭됨");
        
        // 🆕 탭 버튼 클릭 시 ChipsetSlotUI 찾기
        if (chipsetSlotUI == null)
        {
            var playerChipsetPanel = GameObject.Find("PlayerChipsetPanel");
            if (playerChipsetPanel != null)
            {
                chipsetSlotUI = playerChipsetPanel.GetComponent<ChipsetSlotUI>();
                Debug.Log($"🔧 [ChipsetManager] 탭 버튼 클릭 시 PlayerChipsetPanel에서 ChipsetSlotUI 찾기: {(chipsetSlotUI != null ? "성공" : "실패")}");
            }
            else
            {
                Debug.LogError($"🔧 [ChipsetManager] 탭 버튼 클릭 시 PlayerChipsetPanel을 찾을 수 없습니다!");
            }
        }
        
        Debug.Log($"🔧 [ChipsetManager] chipsetSlotUI 참조: {(chipsetSlotUI != null ? "있음" : "없음")}");
        Debug.Log($"🔧 [ChipsetManager] playerChipsetIds: {(playerChipsetIds != null ? string.Join(",", playerChipsetIds) : "null")}");
        
        // 🆕 ChipsetSlotUI에 플레이어 칩셋 설정
        if (chipsetSlotUI != null)
        {
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI.SetPlayerChipsets 호출 전 - 전달할 데이터: {(playerChipsetIds != null ? string.Join(",", playerChipsetIds) : "null")}");
            chipsetSlotUI.SetPlayerChipsets(playerChipsetIds);
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI에 플레이어 칩셋 설정 완료");
        }
        else
        {
            Debug.LogError($"🔧 [ChipsetManager] ChipsetSlotUI 참조가 없습니다!");
        }
        
        // 칩셋 탭으로 전환 (PlayerPrefs에서 다시 로드하지 않음)
        if (inventoryManager != null)
        {
            inventoryManager.SwitchTab(InventoryTab.Chipsets);
            inventoryManager.OpenInventory();
            // 강제로 새로고침 실행
            StartCoroutine(DelayedRefreshInventory());
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
            
            // 개수 정보 업데이트
            UpdateChipsetInfo();
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
            
            // 개수 정보 업데이트
            UpdateChipsetInfo();
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 장착 이벤트
    /// </summary>
    private void OnArmorChipsetEquipped(ChipsetSlot slot, object chipset)
    {
        Debug.Log($"🔧 [ChipsetManager] 방어구 칩셋 장착 시작 - Slot: {slot.GetSlotIndex()}, Chipset: {chipset}");
        
        UpdateArmorCostDisplay();
        CheckArmorCostOver();
        
        // 🆕 ArmorChipsetPanel의 ChipsetSlotUI에서 현재 방어구 가져오기
        if (chipsetSlotUI != null && chipsetSlotUI.currentType == ChipsetSlotUI.ItemType.Armor)
        {
            currentArmor = chipsetSlotUI.currentArmor;
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI에서 현재 방어구 가져옴: {(currentArmor != null ? currentArmor.armorName : "null")}");
        }
        
        // 방어구 데이터에 칩셋 ID 저장
        if (currentArmor != null && chipset is ArmorChipsetData armorChipset)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentArmor.GetEquippedChipsetIds();
            
            Debug.Log($"🔧 [ChipsetManager] 장착 전 방어구 칩셋: {(currentChipsets != null ? string.Join(",", currentChipsets) : "null")}");
            
            // 배열 크기 확장
            if (currentChipsets.Length <= slotIndex)
            {
                string[] newChipsets = new string[armorSlots.Length];
                currentChipsets.CopyTo(newChipsets, 0);
                currentChipsets = newChipsets;
                Debug.Log($"🔧 [ChipsetManager] 배열 크기 확장: {currentChipsets.Length}");
            }
            
            currentChipsets[slotIndex] = armorChipset.chipsetId;
            currentArmor.SetEquippedChipsetIds(currentChipsets);
            
            Debug.Log($"🔧 [ChipsetManager] 장착 후 방어구 칩셋: {string.Join(",", currentChipsets)}");
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateArmorEffects(currentArmor);
            }
            
            // 개수 정보 업데이트
            UpdateChipsetInfo();
            
            Debug.Log($"🔧 [ChipsetManager] 방어구 칩셋 장착 완료");
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 해제 이벤트
    /// </summary>
    private void OnArmorChipsetUnequipped(ChipsetSlot slot, object chipset)
    {
        Debug.Log($"🔧 [ChipsetManager] 방어구 칩셋 해제 시작 - Slot: {slot.GetSlotIndex()}, Chipset: {chipset}");
        
        UpdateArmorCostDisplay();
        CheckArmorCostOver();
        
        // 🆕 ArmorChipsetPanel의 ChipsetSlotUI에서 현재 방어구 가져오기
        if (chipsetSlotUI != null && chipsetSlotUI.currentType == ChipsetSlotUI.ItemType.Armor)
        {
            currentArmor = chipsetSlotUI.currentArmor;
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI에서 현재 방어구 가져옴: {(currentArmor != null ? currentArmor.armorName : "null")}");
        }
        
        // 방어구 데이터에서 칩셋 ID 제거
        if (currentArmor != null)
        {
            int slotIndex = slot.GetSlotIndex();
            string[] currentChipsets = currentArmor.GetEquippedChipsetIds();
            
            Debug.Log($"🔧 [ChipsetManager] 해제 전 방어구 칩셋: {(currentChipsets != null ? string.Join(",", currentChipsets) : "null")}");
            
            if (slotIndex < currentChipsets.Length)
            {
                currentChipsets[slotIndex] = null;
                currentArmor.SetEquippedChipsetIds(currentChipsets);
                Debug.Log($"🔧 [ChipsetManager] 해제 후 방어구 칩셋: {string.Join(",", currentChipsets)}");
            }
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculateArmorEffects(currentArmor);
            }
            
            // 개수 정보 업데이트
            UpdateChipsetInfo();
            
            Debug.Log($"🔧 [ChipsetManager] 방어구 칩셋 해제 완료");
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 장착 이벤트
    /// </summary>
    private void OnPlayerChipsetEquipped(ChipsetSlot slot, object chipset)
    {
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 장착 시작 - Slot: {slot.GetSlotIndex()}, Chipset: {chipset}");
        
        // 🆕 ChipsetSlotUI를 플레이어 칩셋 모드로 설정
        if (chipsetSlotUI != null)
        {
            chipsetSlotUI.SetPlayerChipsets(playerChipsetIds);
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI를 플레이어 칩셋 모드로 설정");
        }
        
        UpdatePlayerCostDisplay();
        CheckPlayerCostOver();
        
        // 플레이어 칩셋 ID 배열 업데이트
        if (chipset is PlayerChipsetData playerChipset)
        {
            int slotIndex = slot.GetSlotIndex();
            Debug.Log($"🔧 [ChipsetManager] 장착 전 playerChipsetIds: {(playerChipsetIds != null ? string.Join(",", playerChipsetIds) : "null")}");
            
            if (playerChipsetIds == null || playerChipsetIds.Length <= slotIndex)
            {
                System.Array.Resize(ref playerChipsetIds, playerSlots.Length);
                Debug.Log($"🔧 [ChipsetManager] 배열 크기 확장: {playerChipsetIds.Length}");
            }
            
            playerChipsetIds[slotIndex] = playerChipset.chipsetId;
            Debug.Log($"🔧 [ChipsetManager] 장착 후 playerChipsetIds: {string.Join(",", playerChipsetIds)}");
            
            // 🆕 즉시 저장
            SavePlayerChipsetsImmediately();
            
            // 🆕 ChipsetSlotUI에 업데이트된 플레이어 칩셋 설정
            if (chipsetSlotUI != null)
            {
                chipsetSlotUI.SetPlayerChipsets(playerChipsetIds);
                Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI에 업데이트된 플레이어 칩셋 설정");
            }
            
            // 효과 적용
            if (effectManager != null)
            {
                effectManager.CalculatePlayerEffects(playerChipsetIds);
            }
            
            // 개수 정보 업데이트
            UpdateChipsetInfo();
            
            Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 장착 완료");
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 해제 이벤트
    /// </summary>
    private void OnPlayerChipsetUnequipped(ChipsetSlot slot, object chipset)
    {
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 해제 시작 - Slot: {slot.GetSlotIndex()}, Chipset: {chipset}");
        
        // 🆕 ChipsetSlotUI를 플레이어 칩셋 모드로 설정
        if (chipsetSlotUI != null)
        {
            chipsetSlotUI.SetPlayerChipsets(playerChipsetIds);
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI를 플레이어 칩셋 모드로 설정");
        }
        
        UpdatePlayerCostDisplay();
        CheckPlayerCostOver();
        
        // 플레이어 칩셋 ID 배열에서 제거
        int slotIndex = slot.GetSlotIndex();
        Debug.Log($"🔧 [ChipsetManager] 해제 전 playerChipsetIds: {(playerChipsetIds != null ? string.Join(",", playerChipsetIds) : "null")}");
        
        if (playerChipsetIds != null && slotIndex < playerChipsetIds.Length)
        {
            playerChipsetIds[slotIndex] = null;
            Debug.Log($"🔧 [ChipsetManager] 해제 후 playerChipsetIds: {string.Join(",", playerChipsetIds)}");
        }
        
        // 🆕 즉시 저장
        SavePlayerChipsetsImmediately();
        
        // 🆕 ChipsetSlotUI에 업데이트된 플레이어 칩셋 설정
        if (chipsetSlotUI != null)
        {
            chipsetSlotUI.SetPlayerChipsets(playerChipsetIds);
            Debug.Log($"🔧 [ChipsetManager] ChipsetSlotUI에 업데이트된 플레이어 칩셋 설정");
        }
        
        // 효과 적용
        if (effectManager != null)
        {
            effectManager.CalculatePlayerEffects(playerChipsetIds);
        }
        
        // 개수 정보 업데이트
        UpdateChipsetInfo();
        
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 해제 완료");
    }
    
    /// <summary>
    /// 플레이어 칩셋 즉시 저장
    /// </summary>
    private void SavePlayerChipsetsImmediately()
    {
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 즉시 저장 시작");
        Debug.Log($"🔧 [ChipsetManager] 저장할 playerChipsetIds: {(playerChipsetIds != null ? string.Join(",", playerChipsetIds) : "null")}");
        
        // 플레이어 장착 칩셋 저장
        string equippedPlayerChipsets = string.Join(",", playerChipsetIds ?? new string[0]);
        PlayerPrefs.SetString("PlayerEquippedChipsets", equippedPlayerChipsets);
        PlayerPrefs.Save();
        
        Debug.Log($"🔧 [ChipsetManager] PlayerPrefs에 저장된 데이터: '{equippedPlayerChipsets}'");
        Debug.Log($"🔧 [ChipsetManager] 플레이어 칩셋 즉시 저장 완료");
    }
    
    /// <summary>
    /// 모든 코스트 표시 업데이트
    /// </summary>
    public void UpdateAllCostDisplays()
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
        
        // 데이터 로드 상태 확인
        if (!GameDataRepository.Instance.IsAllDataLoaded)
        {
            Debug.LogWarning("[ChipsetManager] 데이터가 아직 로드되지 않았습니다. 무기 칩셋 로드를 건너뜁니다.");
            return;
        }
        
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
                else
                {
                    Debug.LogWarning($"[ChipsetManager] 무기 칩셋을 찾을 수 없습니다: {chipsetId}");
            }
        }
        }
        
        // 코스트 표시 업데이트
        UpdateWeaponCostDisplay();
        CheckWeaponCostOver();
    }
    
    /// <summary>
    /// 방어구 칩셋 로드
    /// </summary>
    private void LoadArmorChipsets()
    {
        // 🆕 ArmorChipsetPanel의 ChipsetSlotUI에서 현재 방어구 가져오기
        if (chipsetSlotUI != null && chipsetSlotUI.currentType == ChipsetSlotUI.ItemType.Armor)
        {
            currentArmor = chipsetSlotUI.currentArmor;
            Debug.Log($"🔧 [ChipsetManager] LoadArmorChipsets에서 ChipsetSlotUI에서 현재 방어구 가져옴: {(currentArmor != null ? currentArmor.armorName : "null")}");
        }
        
        if (currentArmor == null) return;
        
        // 데이터 로드 상태 확인
        if (!GameDataRepository.Instance.IsAllDataLoaded)
        {
            Debug.LogWarning("[ChipsetManager] 데이터가 아직 로드되지 않았습니다. 방어구 칩셋 로드를 건너뜁니다.");
            return;
        }
        
        // 기존 칩셋 해제
        foreach (var slot in armorSlots)
        {
            slot.UnequipChipset();
        }
        
        // 방어구에 장착된 칩셋 로드
        string[] equippedChipsets = currentArmor.GetEquippedChipsetIds();
        Debug.Log($"[ChipsetManager] 방어구에 장착된 칩셋: {(equippedChipsets != null ? string.Join(",", equippedChipsets) : "null")}");
        
        for (int i = 0; i < equippedChipsets.Length && i < armorSlots.Length; i++)
        {
            var chipsetId = equippedChipsets[i];
            if (!string.IsNullOrEmpty(chipsetId))
            {
                var chipset = GameDataRepository.Instance.GetArmorChipsetById(chipsetId);
                if (chipset != null)
                {
                    armorSlots[i].EquipArmorChipset(chipset);
                    Debug.Log($"[ChipsetManager] 방어구 칩셋 로드: 슬롯 {i} = {chipset.chipsetName}");
                }
                else
                {
                    Debug.LogWarning($"[ChipsetManager] 방어구 칩셋을 찾을 수 없습니다: {chipsetId}");
            }
        }
        }
        
        // 코스트 표시 업데이트
        UpdateArmorCostDisplay();
        CheckArmorCostOver();
        
        Debug.Log($"[ChipsetManager] 방어구 칩셋 로드 완료");
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
    
    /// <summary>
    /// 칩셋 인벤토리 패널 표시 (ChipsetTabButton에서 호출)
    /// </summary>
    public void ShowInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            
            // 🆕 인벤토리 열 때 ChipsetSlotUI 찾기
            if (chipsetSlotUI == null)
            {
                var playerChipsetPanel = GameObject.Find("PlayerChipsetPanel");
                if (playerChipsetPanel != null)
                {
                    chipsetSlotUI = playerChipsetPanel.GetComponent<ChipsetSlotUI>();
                    Debug.Log($"🔧 [ChipsetManager] 인벤토리 열 때 PlayerChipsetPanel에서 ChipsetSlotUI 찾기: {(chipsetSlotUI != null ? "성공" : "실패")}");
                }
                else
                {
                    Debug.LogError($"🔧 [ChipsetManager] 인벤토리 열 때 PlayerChipsetPanel을 찾을 수 없습니다!");
                }
            }
            
            // 인벤토리 새로고침
            LoadChipsetInventory();
            
            Debug.Log("[ChipsetManager] 칩셋 인벤토리 패널 표시");
        }
        else
        {
            Debug.LogWarning("[ChipsetManager] 인벤토리 패널이 설정되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 패널 숨기기
    /// </summary>
    public void HideInventoryPanel()
    {
        if (inventoryPanel != null)
    {
        inventoryPanel.SetActive(false);
            Debug.Log("[ChipsetManager] 칩셋 인벤토리 패널 숨김");
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 패널 토글 (열기/닫기)
    /// </summary>
    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            bool isActive = inventoryPanel.activeSelf;
            if (isActive)
            {
                HideInventoryPanel();
            }
            else
            {
                ShowInventoryPanel();
            }
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 데이터 저장
    /// </summary>
    public void SaveChipsetInventoryData()
    {
        // 무기 칩셋 인벤토리 저장
        string weaponChipsetData = string.Join(",", playerWeaponChipsetInventory);
        PlayerPrefs.SetString("PlayerWeaponChipsetInventory", weaponChipsetData);
        
        // 방어구 칩셋 인벤토리 저장
        string armorChipsetData = string.Join(",", playerArmorChipsetInventory);
        PlayerPrefs.SetString("PlayerArmorChipsetInventory", armorChipsetData);
        
        // 플레이어 칩셋 인벤토리 저장
        string playerChipsetData = string.Join(",", playerPlayerChipsetInventory);
        PlayerPrefs.SetString("PlayerPlayerChipsetInventory", playerChipsetData);
        
        // 플레이어 장착 칩셋 저장
        string equippedPlayerChipsets = string.Join(",", playerChipsetIds);
        PlayerPrefs.SetString("PlayerEquippedChipsets", equippedPlayerChipsets);
        
        // 🆕 칩셋 해제 옵션 저장
        PlayerPrefs.SetInt("ChipsetReturnToInventory", returnToInventoryOnUnequip ? 1 : 0);
        
        PlayerPrefs.Save();
        Debug.Log("[ChipsetManager] 칩셋 인벤토리 저장 완료");
    }
    
    /// <summary>
    /// 칩셋 인벤토리 데이터 로드
    /// </summary>
    public void LoadChipsetInventoryData()
    {
        // 무기 칩셋 인벤토리 로드
        string weaponChipsetData = PlayerPrefs.GetString("PlayerWeaponChipsetInventory", "");
        Debug.Log($"[ChipsetManager] PlayerPrefs에서 로드된 무기 칩셋 데이터: '{weaponChipsetData}'");
        if (!string.IsNullOrEmpty(weaponChipsetData))
        {
            var weaponIds = weaponChipsetData.Split(',');
            playerWeaponChipsetInventory = new List<string>();
            foreach (var id in weaponIds)
            {
                if (!string.IsNullOrEmpty(id.Trim()))
                {
                    playerWeaponChipsetInventory.Add(id.Trim());
                }
            }
        }
        
        // 방어구 칩셋 인벤토리 로드
        string armorChipsetData = PlayerPrefs.GetString("PlayerArmorChipsetInventory", "");
        Debug.Log($"[ChipsetManager] PlayerPrefs에서 로드된 방어구 칩셋 데이터: '{armorChipsetData}'");
        if (!string.IsNullOrEmpty(armorChipsetData))
        {
            var armorIds = armorChipsetData.Split(',');
            playerArmorChipsetInventory = new List<string>();
            foreach (var id in armorIds)
            {
                if (!string.IsNullOrEmpty(id.Trim()))
                {
                    playerArmorChipsetInventory.Add(id.Trim());
                }
            }
        }
        
        // 플레이어 칩셋 인벤토리 로드
        string playerChipsetData = PlayerPrefs.GetString("PlayerPlayerChipsetInventory", "");
        Debug.Log($"[ChipsetManager] PlayerPrefs에서 로드된 플레이어 칩셋 데이터: '{playerChipsetData}'");
        if (!string.IsNullOrEmpty(playerChipsetData))
        {
            var playerIds = playerChipsetData.Split(',');
            playerPlayerChipsetInventory = new List<string>();
            foreach (var id in playerIds)
            {
                if (!string.IsNullOrEmpty(id.Trim()))
                {
                    playerPlayerChipsetInventory.Add(id.Trim());
                }
            }
        }
        
        // 플레이어 장착 칩셋 로드
        string equippedPlayerChipsets = PlayerPrefs.GetString("PlayerEquippedChipsets", "");
        Debug.Log($"🔧 [ChipsetManager] PlayerPrefs에서 로드된 장착 플레이어 칩셋 데이터: '{equippedPlayerChipsets}'");
        if (!string.IsNullOrEmpty(equippedPlayerChipsets))
        {
            var equippedIds = equippedPlayerChipsets.Split(',');
            var validIds = new List<string>();
            foreach (var id in equippedIds)
            {
                if (!string.IsNullOrEmpty(id.Trim()))
                {
                    validIds.Add(id.Trim());
                }
            }
            playerChipsetIds = validIds.ToArray();
            Debug.Log($"🔧 [ChipsetManager] 로드된 playerChipsetIds: {string.Join(",", playerChipsetIds)}");
        }
        else
        {
            Debug.Log($"🔧 [ChipsetManager] PlayerPrefs에 플레이어 칩셋 데이터가 없어서 빈 배열로 초기화");
            playerChipsetIds = new string[0];
        }
        
        // 🆕 칩셋 해제 옵션 로드
        returnToInventoryOnUnequip = PlayerPrefs.GetInt("ChipsetReturnToInventory", 0) == 1;
        
        Debug.Log($"[ChipsetManager] 칩셋 인벤토리 로드 완료 - 무기: {playerWeaponChipsetInventory.Count}개, 방어구: {playerArmorChipsetInventory.Count}개, 플레이어: {playerPlayerChipsetInventory.Count}개");
        Debug.Log($"[ChipsetManager] 칩셋 해제 옵션: {(returnToInventoryOnUnequip ? "인벤토리 반환" : "소멸")}");
    }
    
    /// <summary>
    /// 모든 칩셋 인벤토리 초기화 (디버그용)
    /// </summary>
    [ContextMenu("칩셋 인벤토리 초기화")]
    public void ClearAllChipsetInventories()
    {
        // 메모리에서 초기화
        playerWeaponChipsetInventory.Clear();
        playerArmorChipsetInventory.Clear();
        playerPlayerChipsetInventory.Clear();
        playerChipsetIds = new string[0];
        
        // PlayerPrefs에서도 삭제
        PlayerPrefs.DeleteKey("PlayerWeaponChipsetInventory");
        PlayerPrefs.DeleteKey("PlayerArmorChipsetInventory");
        PlayerPrefs.DeleteKey("PlayerPlayerChipsetInventory");
        PlayerPrefs.DeleteKey("PlayerEquippedChipsets");
        PlayerPrefs.DeleteKey("ChipsetReturnToInventory");
        PlayerPrefs.Save();
        
        // InventoryManager에서도 초기화
        if (inventoryManager != null)
        {
            inventoryManager.ClearChipsets();
            inventoryManager.RefreshInventory();
        }
        
        Debug.Log("[ChipsetManager] 모든 칩셋 인벤토리가 초기화되었습니다.");
    }
    
    /// <summary>
    /// 칩셋을 플레이어 인벤토리에 추가
    /// </summary>
    public void AddChipsetToInventory(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipset)
        {
            // 중복 체크 제거 - 모든 칩셋을 인벤토리에 추가
            playerWeaponChipsetInventory.Add(weaponChipset.chipsetId);
            SaveChipsetInventoryData();
            LoadChipsetInventory(); // 항상 인벤토리 새로고침
            UpdateChipsetInfo(); // 개수 정보 업데이트
            Debug.Log($"[ChipsetManager] 무기 칩셋 추가: {weaponChipset.chipsetName}");
        }
        else if (chipset is ArmorChipsetData armorChipset)
        {
            // 중복 체크 제거 - 모든 칩셋을 인벤토리에 추가
            playerArmorChipsetInventory.Add(armorChipset.chipsetId);
            SaveChipsetInventoryData();
            LoadChipsetInventory(); // 항상 인벤토리 새로고침
            UpdateChipsetInfo(); // 개수 정보 업데이트
            Debug.Log($"[ChipsetManager] 방어구 칩셋 추가: {armorChipset.chipsetName}");
        }
        else if (chipset is PlayerChipsetData playerChipset)
        {
            // 중복 체크 제거 - 모든 칩셋을 인벤토리에 추가
            playerPlayerChipsetInventory.Add(playerChipset.chipsetId);
            SaveChipsetInventoryData();
            LoadChipsetInventory(); // 항상 인벤토리 새로고침
            UpdateChipsetInfo(); // 개수 정보 업데이트
            Debug.Log($"[ChipsetManager] 플레이어 칩셋 추가: {playerChipset.chipsetName}");
        }
    }
    
    /// <summary>
    /// 랜덤 칩셋 소환 (F4키)
    /// </summary>
    private void SpawnRandomChipset()
    {
        if (!GameDataRepository.Instance.IsAllDataLoaded)
        {
            Debug.LogWarning("[ChipsetManager] 데이터가 로드되지 않아 칩셋을 소환할 수 없습니다.");
            return;
        }
        
        // 소환 위치 설정
        Vector3 spawnPosition = Vector3.zero;
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            Debug.Log($"[ChipsetManager] spawnPoint 사용: {spawnPosition}");
        }
        else
        {
            // 플레이어 근처에 소환
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                spawnPosition = player.transform.position + Vector3.up * 2f;
                Debug.Log($"[ChipsetManager] 플레이어 근처 소환: {spawnPosition}");
            }
            else
            {
                // 플레이어가 없으면 월드 원점에 소환
                spawnPosition = Vector3.up * 2f;
                Debug.Log($"[ChipsetManager] 월드 원점 소환: {spawnPosition}");
            }
        }
        
        // 랜덤 칩셋 타입 선택
        int chipsetType = Random.Range(0, 3);
        object randomChipset = null;
        string chipsetName = "";
        
        switch (chipsetType)
        {
            case 0: // 무기 칩셋
                var weaponChipsets = GameDataRepository.Instance.GetAllWeaponChipsets();
                if (weaponChipsets.Count > 0)
                {
                    randomChipset = weaponChipsets[Random.Range(0, weaponChipsets.Count)];
                    chipsetName = ((WeaponChipsetData)randomChipset).chipsetName;
                }
                break;
            case 1: // 방어구 칩셋
                var armorChipsets = GameDataRepository.Instance.GetAllArmorChipsets();
                if (armorChipsets.Count > 0)
                {
                    randomChipset = armorChipsets[Random.Range(0, armorChipsets.Count)];
                    chipsetName = ((ArmorChipsetData)randomChipset).chipsetName;
                }
                break;
            case 2: // 플레이어 칩셋
                var playerChipsets = GameDataRepository.Instance.GetAllPlayerChipsets();
                if (playerChipsets.Count > 0)
                {
                    randomChipset = playerChipsets[Random.Range(0, playerChipsets.Count)];
                    chipsetName = ((PlayerChipsetData)randomChipset).chipsetName;
                }
                break;
        }
        
        if (randomChipset != null)
        {
            Debug.Log($"[ChipsetManager] 칩셋 데이터 준비 완료: {chipsetName}");
            
            // 칩셋 픽업 오브젝트 생성
            if (chipsetPickupPrefab != null)
            {
                Debug.Log($"[ChipsetManager] 프리팹 생성 시도: {spawnPosition}");
                var pickupGO = Instantiate(chipsetPickupPrefab, spawnPosition, Quaternion.identity);
                
                if (pickupGO != null)
                {
                    Debug.Log($"[ChipsetManager] 프리팹 인스턴스 생성 성공: {pickupGO.name}");
                    
                    var chipsetPickup = pickupGO.GetComponent<ChipsetPickup>();
                    if (chipsetPickup != null)
                    {
                        chipsetPickup.Initialize(randomChipset);
                        Debug.Log($"[ChipsetManager] ChipsetPickup 초기화 완료");
                    }
                    else
                    {
                        Debug.LogError($"[ChipsetManager] ChipsetPickup 컴포넌트를 찾을 수 없습니다!");
                    }
                }
                else
                {
                    Debug.LogError($"[ChipsetManager] 프리팹 인스턴스 생성 실패!");
                }
            }
            else
            {
                Debug.LogError($"[ChipsetManager] chipsetPickupPrefab이 할당되지 않았습니다!");
            }
            
            Debug.Log($"[ChipsetManager] 칩셋 소환 완료: {chipsetName} at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("[ChipsetManager] 소환할 칩셋이 없습니다.");
        }
    }
    
    /// <summary>
    /// 지연된 인벤토리 새로고침 (탭 전환 버그 해결용)
    /// </summary>
    private System.Collections.IEnumerator DelayedRefreshInventory()
    {
        // 1프레임 대기
        yield return null;
        
        // 인벤토리 매니저가 초기화되었는지 확인하고 강제 새로고침
        if (inventoryManager != null)
        {
            // 강제로 새로고침 실행 (탭 상태는 이미 SwitchTab에서 설정됨)
            inventoryManager.RefreshInventory();
        }
    }
    
    // 🆕 스탯 UI용 칩셋 개수 반환 메서드들
    public int GetWeaponChipsetCount()
    {
        int count = 0;
        foreach (var slot in weaponSlots)
        {
            if (slot != null && slot.IsEquipped())
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetArmorChipsetCount()
    {
        int count = 0;
        foreach (var slot in armorSlots)
        {
            if (slot != null && slot.IsEquipped())
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetPlayerChipsetCount()
    {
        int count = 0;
        foreach (var slot in playerSlots)
        {
            if (slot != null && slot.IsEquipped())
            {
                count++;
            }
        }
        return count;
    }
} 