using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ArmorSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("📋 방어구 슬롯 사용법")]
    [TextArea(4, 6)]
    public string instructions = "🛡️ 방어구 슬롯 시스템:\n• 좌클릭: 방어구 장착\n• 우클릭: 방어구 해제\n• 호버: 툴팁 표시\n• 필터: 슬롯 타입에 맞는 방어구만 장착 가능\n• 시각적 피드백: 레어리티별 색상 표시";

    [Header("🔧 슬롯 설정")]
    [Tooltip("이 슬롯이 받을 수 있는 방어구 타입")]
    public ArmorType allowedArmorType = ArmorType.Chest;
    
    [Tooltip("슬롯 이름 (UI 표시용)")]
    public string slotName = "상체";
    
    [Header("🎨 UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private GameObject emptyIcon;
    [SerializeField] private GameObject equippedIcon;
    
    [Header("🎨 Visual Feedback")]
    [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color equippedSlotColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Header("🔗 References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryManager inventoryManager;
    
    [Header("🛡️ 기본 방어구 아이콘 (icon이 null일 때)")]
    public Sprite defaultHelmetIcon;
    public Sprite defaultChestIcon;
    public Sprite defaultLegsIcon;
    public Sprite defaultBootsIcon;
    public Sprite defaultShoulderIcon;
    public Sprite defaultAccessoryIcon;
    
    // Private variables
    private ArmorData armorData;
    private bool isHighlighted = false;
    private Color originalBackgroundColor;
    
    // 🆕 드래그 앤 드롭을 위한 전역 상태 (WeaponSlot과 동일한 패턴)
    public static ArmorData CurrentlyDraggedArmor { get; private set; } = null;
    public static ArmorSlot CurrentlyDraggedSlot { get; private set; } = null;
    
    // Events
    public System.Action<ArmorData> OnArmorEquipped;
    public System.Action<ArmorData> OnArmorUnequipped;
    
    void Awake()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        // UI 컴포넌트 자동 찾기
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        
        if (nameText == null)
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        
        if (typeText == null)
            typeText = transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
        
        if (defenseText == null)
            defenseText = transform.Find("DefenseText")?.GetComponent<TextMeshProUGUI>();
        
        if (emptyIcon == null)
            emptyIcon = transform.Find("EmptyIcon")?.gameObject;
        
        if (equippedIcon == null)
            equippedIcon = transform.Find("EquippedIcon")?.gameObject;
        
        // 초기 색상 저장
        if (backgroundImage != null)
            originalBackgroundColor = backgroundImage.color;
    }
    
    void Start()
    {
        // 게임 시작 시 UI 레이어 충돌 자동 해결
        StartCoroutine(AutoFixUILayerConflictsOnStart());
        
        // 초기 UI 업데이트
        UpdateVisuals();
    }
    
    void Update()
    {
        // 호버 효과 업데이트
        if (isHighlighted && backgroundImage != null)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, highlightColor, Time.deltaTime * 5f);
        }
    }
    
    // 🆕 게임 시작 시 UI 레이어 충돌 자동 해결
    System.Collections.IEnumerator AutoFixUILayerConflictsOnStart()
    {
        yield return new WaitForSeconds(0.5f);
        AutoFixUILayerConflicts();
    }
    
    // 🆕 마우스 위치에 의존하지 않는 자동 UI 충돌 해결
    void AutoFixUILayerConflicts()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector3 worldPosition = rectTransform.position;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
        
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;
        
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = screenPosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, raycastResults);
        
        int fixedCount = 0;
        for (int i = 0; i < raycastResults.Count; i++)
        {
            var blockingUI = raycastResults[i].gameObject;
            
            if (blockingUI.name.Contains("InventoryPanel") || 
                blockingUI.name.Contains("Panel") ||
                blockingUI.name.Contains("Background"))
            {
                Image img = blockingUI.GetComponent<Image>();
                if (img != null && img.raycastTarget)
                {
                    img.raycastTarget = false;
                    fixedCount++;
                }
            }
            else
            {
                Image img = blockingUI.GetComponent<Image>();
                if (img != null && img.color.a < 0.1f && img.raycastTarget)
                {
                    img.raycastTarget = false;
                    fixedCount++;
                }
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"✅ {fixedCount}개의 UI 요소를 자동으로 수정했습니다! ArmorSlot 클릭이 이제 작동해야 합니다!");
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭: 방어구 장착 시도
            TryEquipArmor();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭: 방어구 해제 (인벤토리로 반환)
            UnequipArmor();
        }
    }
    
    // 🆕 드래그 앤 드롭 지원
    public void OnDrop(PointerEventData eventData)
    {
        // InventorySlot에서 드래그된 방어구 확인
        ArmorData draggedArmor = InventorySlot.CurrentlyDraggedArmor;
        InventorySlot sourceSlot = InventorySlot.CurrentlyDraggingSlot;
        
        if (draggedArmor != null && sourceSlot != null)
        {
            // 타입 체크 (All 타입이 아닌 경우에만)
            if (allowedArmorType != ArmorType.All && draggedArmor.armorType != allowedArmorType)
            {
                Debug.LogWarning($"⚠️ [ArmorSlot] {draggedArmor.armorName}은(는) {slotName} 슬롯에 장착할 수 없습니다!");
                return;
            }
            
            // 현재 슬롯에 방어구가 있는지 확인
            ArmorData currentArmor = armorData;
            
            // 새 방어구 장착
            EquipArmor(draggedArmor);
            
            // 기존 방어구가 있었다면 인벤토리로 반환
            if (currentArmor != null)
            {
                ReturnArmorToInventory(currentArmor);
            }
            
            // 🆕 인벤토리 새로고침
            if (inventoryManager != null)
            {
                inventoryManager.RefreshInventory();
            }
            
            Debug.Log($"🛡️ {draggedArmor.armorName}이(가) {slotName} 슬롯에 드래그 앤 드롭으로 장착되었습니다!");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlighted = true;
        
        // 호버 시 색상 변경
        if (backgroundImage != null && armorData != null)
        {
            backgroundImage.color = highlightColor;
        }
        
        // 툴팁 표시
        if (armorData != null)
        {
            ShowTooltip();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
        
        // 방어구가 있으면 등급색, 없으면 emptySlotColor로 복원
        if (backgroundImage != null)
        {
            if (armorData != null)
                backgroundImage.color = armorData.GetRarityColor(); // 등급색으로 복원
            else
                backgroundImage.color = emptySlotColor;
        }

        HideTooltip();
    }
    
    // 방어구 장착 시도
    public void TryEquipArmor()
    {
        // 인벤토리에서 해당 타입의 방어구 찾기
        if (inventoryManager != null)
        {
            var availableArmors = inventoryManager.GetArmorsByType(allowedArmorType);
            if (availableArmors.Count > 0)
            {
                // 첫 번째 방어구 장착
                EquipArmor(availableArmors[0]);
            }
            else
            {
                Debug.Log($"⚠️ {allowedArmorType} 타입의 방어구가 인벤토리에 없습니다!");
            }
        }
    }
    
    // 방어구 장착
    public void EquipArmor(ArmorData newArmorData)
    {
        // 타입 체크 (All 타입이 아닌 경우에만)
        if (allowedArmorType != ArmorType.All && newArmorData.armorType != allowedArmorType)
        {
            Debug.LogWarning($"⚠️ {newArmorData.armorName}은(는) {allowedArmorType} 슬롯에 장착할 수 없습니다!");
            return;
        }
        
        // 기존 방어구가 있다면 인벤토리로 돌려보내기
        if (armorData != null)
        {
            ReturnArmorToInventory(armorData);
        }
        
        // 새 방어구 장착
        armorData = newArmorData;
        
        // 플레이어 인벤토리에 장착 방어구 설정
        if (playerInventory != null)
            playerInventory.SetEquippedArmor(armorData, allowedArmorType);
        
        // 인벤토리에서 장착된 방어구 제거
        if (armorData != null && inventoryManager != null)
        {
            inventoryManager.RemoveArmor(armorData, true); // 🆕 UI 새로고침 활성화
        }
        
        // 플레이어 능력치 업데이트
        UpdatePlayerStats();
        
        UpdateVisuals();
        
        // 🆕 ArmorChipsetPanel의 ChipsetSlotUI에 방어구 자동 설정
        var armorChipsetPanel = GameObject.Find("ArmorChipsetPanel");
        if (armorChipsetPanel != null)
        {
            var chipsetSlotUI = armorChipsetPanel.GetComponent<ChipsetSlotUI>();
            if (chipsetSlotUI != null)
            {
                chipsetSlotUI.SetItem(armorData);
                Debug.Log($"🔧 [ArmorSlot] ArmorChipsetPanel에 방어구 자동 설정: {armorData.armorName}");
            }
        }
        
        // 이벤트 호출
        OnArmorEquipped?.Invoke(armorData);
        
        Debug.Log($"🛡️ {armorData.armorName} 장착 완료!");
    }
    
    // 방어구 해제
    public void UnequipArmor()
    {
        if (armorData == null) return;
        
        ArmorData oldArmor = armorData;
        
        // 방어구 해제
        armorData = null;
        
        // 플레이어 인벤토리에서 장착 방어구 해제
        if (playerInventory != null)
            playerInventory.SetEquippedArmor(null, allowedArmorType);
        
        // 플레이어 능력치 복원
        UpdatePlayerStats();
        
        // 인벤토리에 방어구 다시 추가 및 UI 업데이트
        if (inventoryManager != null)
        {
            inventoryManager.AddArmor(oldArmor);
            inventoryManager.ForceShowArmorsTabAndRefresh();
        }
        
        UpdateVisuals();
        
        // 🆕 ArmorChipsetPanel의 ChipsetSlotUI 초기화
        var armorChipsetPanel = GameObject.Find("ArmorChipsetPanel");
        if (armorChipsetPanel != null)
        {
            var chipsetSlotUI = armorChipsetPanel.GetComponent<ChipsetSlotUI>();
            if (chipsetSlotUI != null)
            {
                chipsetSlotUI.ClearItem();
                Debug.Log($"🔧 [ArmorSlot] ArmorChipsetPanel 초기화");
            }
        }
        
        // 이벤트 호출
        OnArmorUnequipped?.Invoke(oldArmor);
        
        Debug.Log($"🛡️ {oldArmor.armorName} 해제 완료!");
    }
    
    // 인벤토리에 방어구 반환
    void ReturnArmorToInventory(ArmorData armor)
    {
        if (inventoryManager != null)
        {
            inventoryManager.AddArmor(armor);
            inventoryManager.RefreshInventory();
        }
    }
    
    // 플레이어 능력치 업데이트
    void UpdatePlayerStats()
    {
        if (playerInventory == null) return;
        
        // 플레이어의 모든 장착 방어구 능력치를 계산하여 적용
        playerInventory.UpdateArmorStats();
    }
    
    // 툴팁 표시
    void ShowTooltip()
    {
        if (armorData == null) return;
        
        // 간단한 툴팁 (나중에 별도 툴팁 시스템으로 확장 가능)
        string tooltipText = $"{armorData.armorName}\n" +
                           $"타입: {armorData.GetTypeName()}\n" +
                           $"레어리티: {armorData.GetRarityName()}\n" +
                           $"방어력: {armorData.defense}\n" +
                           $"체력: +{armorData.maxHealth}\n" +
                           $"이동속도: +{armorData.moveSpeedBonus:F1}\n" +
                           $"{armorData.description}";
        
        Debug.Log($"🛡️ {tooltipText}");
    }
    
    // 툴팁 숨기기
    void HideTooltip()
    {
        // 툴팁 숨기기 로직 (필요시 구현)
    }
    
    // 시각적 업데이트
    void UpdateVisuals()
    {
        if (armorData != null)
        {
            // 방어구가 장착된 상태
            if (backgroundImage != null)
            {
                backgroundImage.color = armorData.GetRarityColor(); // 등급색으로!
            }
            
            if (iconImage != null)
            {
                // 아이콘이 null이면 타입별 기본 아이콘 사용
                Sprite iconToUse = armorData.icon;
                if (iconToUse == null)
                {
                    switch (armorData.armorType)
                    {
                        case ArmorType.Helmet:
                            iconToUse = defaultHelmetIcon;
                            break;
                        case ArmorType.Chest:
                            iconToUse = defaultChestIcon;
                            break;
                        case ArmorType.Legs:
                            iconToUse = defaultLegsIcon;
                            break;
                        case ArmorType.Boots:
                            iconToUse = defaultBootsIcon;
                            break;
                        case ArmorType.Shoulder:
                            iconToUse = defaultShoulderIcon;
                            break;
                        case ArmorType.Accessory:
                            iconToUse = defaultAccessoryIcon;
                            break;
                        default:
                            iconToUse = null;
                            break;
                    }
                }
                iconImage.sprite = iconToUse;
                iconImage.color = armorData.GetRarityColor();
                iconImage.enabled = (iconToUse != null);
            }
            
            if (nameText != null)
            {
                nameText.text = armorData.armorName;
                nameText.color = armorData.GetRarityColor();
            }
            
            if (typeText != null)
            {
                typeText.text = armorData.GetTypeName();
            }
            
            if (defenseText != null)
            {
                defenseText.text = $"방어력: {armorData.defense}";
            }
            
            if (emptyIcon != null)
                emptyIcon.SetActive(false);
            
            if (equippedIcon != null)
                equippedIcon.SetActive(true);
        }
        else
        {
            // 빈 슬롯 상태
            if (backgroundImage != null)
            {
                backgroundImage.color = emptySlotColor;
            }
            
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }
            
            if (nameText != null)
            {
                nameText.text = slotName;
                nameText.color = Color.gray;
            }
            
            if (typeText != null)
            {
                if (allowedArmorType == ArmorType.All)
                    typeText.text = "모든 타입";
                else
                    typeText.text = allowedArmorType.ToString();
            }
            
            if (defenseText != null)
            {
                defenseText.text = "빈 슬롯";
            }
            
            if (emptyIcon != null)
                emptyIcon.SetActive(true);
            
            if (equippedIcon != null)
                equippedIcon.SetActive(false);
        }
    }
    
    // Public getters
    public ArmorData GetArmorData() => armorData;
    public ArmorType GetAllowedType() => allowedArmorType;
    public bool IsEquipped() => armorData != null;
    
    // 🆕 드래그 앤 드롭을 위한 메서드들
    public void SetArmorData(ArmorData newArmorData)
    {
        armorData = newArmorData;
        UpdateVisuals();
    }
    
    public void ForceUpdateVisuals()
    {
        UpdateVisuals();
    }
    
    // 🆕 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (armorData == null) return;
        
        // 전역 드래그 상태 설정
        CurrentlyDraggedArmor = armorData;
        CurrentlyDraggedSlot = this;
        
        Debug.Log($"🛡️ [ArmorSlot] 드래그 시작: {armorData.armorName}");
    }
    
    // 🆕 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중에는 아무것도 하지 않음 (InventorySlot에서 처리)
    }
    
    // 🆕 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        // 전역 드래그 상태 초기화
        CurrentlyDraggedArmor = null;
        CurrentlyDraggedSlot = null;
        
        Debug.Log($"🛡️ [ArmorSlot] 드래그 종료");
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        OnArmorEquipped = null;
        OnArmorUnequipped = null;
    }
} 