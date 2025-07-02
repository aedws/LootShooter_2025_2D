using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("📋 슬롯 사용법")]
    [TextArea(3, 5)]
    public string slotInstructions = "• 좌클릭: 슬롯 선택\n• 우클릭: 무기 즉시 장착\n• 드래그: WeaponSlot으로 무기 이동\n• 드롭: WeaponSlot에서 무기 반환 받기\n• 마우스 호버: 0.5초 후 툴팁 표시\n• 무기 타입별로 테두리 색상 변경";

    [Header("🖼️ Slot Components")]
    [Tooltip("무기 아이콘을 표시할 Image 컴포넌트")]
    public Image iconImage;
    
    [Tooltip("슬롯 배경 이미지 (상태에 따라 색상 변경)")]
    public Image backgroundImage;
    
    [Tooltip("슬롯 테두리 이미지 (무기 타입별 색상)")]
    public Image borderImage;
    
    [Tooltip("탄약 정보를 표시할 Text 컴포넌트")]
    public Text ammoText;
    
    [Tooltip("고급 무기용 빛 효과 오브젝트")]
    public GameObject rarityGlow;
    
    [Header("🎨 Visual States")]
    [Tooltip("기본 상태 색상")]
    public Color normalColor = Color.white;
    
    [Tooltip("마우스 호버 시 색상")]
    public Color hoverColor = Color.yellow;
    
    [Tooltip("선택된 상태 색상")]
    public Color selectedColor = Color.green;
    
    [Tooltip("빈 슬롯 색상")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Tooltip("드래그 중 빈 슬롯 색상 (더 어둡게)")]
    public Color draggingEmptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Header("🔫 Weapon Type Colors")]
    [Tooltip("돌격소총(AR) 테두리 색상")]
    public Color arColor = Color.red;
    
    [Tooltip("권총(HG) 테두리 색상")]
    public Color hgColor = Color.blue;
    
    [Tooltip("기관총(MG) 테두리 색상")]
    public Color mgColor = Color.magenta;
    
    [Tooltip("산탄총(SG) 테두리 색상")]
    public Color sgColor = new Color(1f, 0.5f, 0f, 1f); // 오렌지 색상
    
    [Tooltip("기관단총(SMG) 테두리 색상")]
    public Color smgColor = Color.cyan;
    
    [Tooltip("저격총(SR) 테두리 색상")]
    public Color srColor = Color.green;
    
    // 🌍 전역 드래그 상태 (WeaponSlot에서 접근 가능)
    public static WeaponData CurrentlyDraggedWeapon { get; private set; } = null;
    public static InventorySlot CurrentlyDraggingSlot { get; private set; } = null;
    
    // Public properties
    public WeaponData weaponData { get; private set; }
    public int slotIndex { get; set; }
    public InventoryManager inventoryManager { get; set; }
    
    // Private variables
    private Canvas canvas;
    private bool isDragging = false;
    private bool isSelected = false;
    private bool isHovered = false;
    
    // 🎮 게임식 드래그앤드롭을 위한 변수들
    private GameObject draggedItemImage; // 드래그되는 아이템 이미지
    private WeaponData draggedWeaponData; // 드래그 중인 무기 데이터
    private bool isTemporarilyEmpty = false; // 드래그 중 일시적으로 빈 상태
    
    // Tooltip variables
    private float tooltipTimer = 0f;
    private const float TOOLTIP_DELAY = 0.5f;
    private bool showingTooltip = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // 기본 색상 설정
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }
    
    void Update()
    {
        // 툴팁 타이머 처리
        if (isHovered && !isDragging && weaponData != null && !isTemporarilyEmpty)
        {
            tooltipTimer += Time.deltaTime;
            if (tooltipTimer >= TOOLTIP_DELAY && !showingTooltip)
            {
                ShowTooltip();
            }
        }
    }

        public void SetWeapon(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;
        UpdateVisuals();
    }
    
    public void ClearSlot()
    {
        weaponData = null;
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        // 드래그 중 일시적으로 빈 상태라면 빈 슬롯으로 표시
        if (isTemporarilyEmpty)
        {
            ShowEmptySlot(true);
            return;
        }
        
        if (weaponData != null)
        {
            // 아이콘 설정
            if (iconImage != null)
            {
                iconImage.sprite = weaponData.icon;
                iconImage.color = Color.white;
                iconImage.enabled = true;
            }
            
            // 탄약 정보 표시
            if (ammoText != null)
            {
                if (weaponData.infiniteAmmo)
                    ammoText.text = "∞";
                else
                    ammoText.text = $"{weaponData.currentAmmo}/{weaponData.maxAmmo}";
                ammoText.enabled = true;
            }
            
            // 무기 타입별 색상
            if (borderImage != null)
            {
                borderImage.color = GetWeaponTypeColor(weaponData.weaponType);
                borderImage.enabled = true;
            }
            
            // 희귀도 효과 (나중에 확장 가능)
            if (rarityGlow != null)
            {
                rarityGlow.SetActive(weaponData.damage > 50); // 임시로 데미지 50 이상이면 효과
            }
        }
        else
        {
            ShowEmptySlot(false);
        }
        
        UpdateSlotColor();
    }
    
    void ShowEmptySlot(bool isDraggingEmpty)
    {
        // 빈 슬롯 상태
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
    }

        if (ammoText != null)
            ammoText.enabled = false;
        
        if (borderImage != null)
            borderImage.enabled = false;
        
        if (rarityGlow != null)
            rarityGlow.SetActive(false);
        
        // 배경 색상 설정
        if (backgroundImage != null)
        {
            backgroundImage.color = isDraggingEmpty ? draggingEmptyColor : emptyColor;
        }
    }
    
    void UpdateSlotColor()
    {
        if (backgroundImage == null) return;
        if (isTemporarilyEmpty) return; // 드래그 중에는 색상 변경 안함
        
        Color targetColor = normalColor;
        
        if (weaponData == null)
        {
            targetColor = emptyColor;
        }
        else if (isSelected)
        {
            targetColor = selectedColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }
        
        backgroundImage.color = targetColor;
    }
    
    Color GetWeaponTypeColor(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.AR: return arColor;
            case WeaponType.HG: return hgColor;
            case WeaponType.MG: return mgColor;
            case WeaponType.SG: return sgColor;
            case WeaponType.SMG: return smgColor;
            case WeaponType.SR: return srColor;
            default: return Color.white;
        }
    }

    // 🎮 진짜 게임식 드래그 앤 드롭 시스템
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        isDragging = true;
        draggedWeaponData = weaponData;
        
        // 🌍 전역 드래그 상태 설정
        CurrentlyDraggedWeapon = draggedWeaponData;
        CurrentlyDraggingSlot = this;
        
        // 드래그할 아이템 이미지 생성
        CreateDraggedItemImage();
        
        // 원래 슬롯을 일시적으로 빈 상태로 표시
        isTemporarilyEmpty = true;
        UpdateVisuals();
        
        HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || draggedItemImage == null) return;
        
        // 드래그된 아이템 이미지만 마우스를 따라다님
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);
        
        draggedItemImage.transform.position = canvas.transform.TransformPoint(localPoint);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        bool itemMoved = false;
        
        // 드롭 대상 찾기
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        
        // WeaponSlot에 드롭했는지 확인
        WeaponSlot weaponSlot = dropTarget?.GetComponent<WeaponSlot>();
        if (weaponSlot != null)
        {
            // WeaponSlot이 OnDrop에서 처리하도록 놔둠
            itemMoved = true; // WeaponSlot 드롭은 성공으로 간주
        }
        else
        {
            // 다른 InventorySlot에 드롭했는지 확인
            InventorySlot targetSlot = dropTarget?.GetComponent<InventorySlot>();
            if (targetSlot != null && targetSlot != this)
            {
                // 슬롯 간 아이템 교환
                SwapItems(targetSlot);
                itemMoved = true;
            }
        }
        
        // 🆕 WeaponSlot 처리를 위해 약간 지연 후 전역 상태 초기화
        StartCoroutine(ClearDragStateDelayed(itemMoved));
        
        // 드래그된 아이템 이미지 제거
        if (draggedItemImage != null)
        {
            Destroy(draggedItemImage);
            draggedItemImage = null;
        }
        
        draggedWeaponData = null;
    }
    
    // 🆕 지연된 드래그 상태 초기화
    System.Collections.IEnumerator ClearDragStateDelayed(bool itemMoved)
    {
        // WeaponSlot의 OnDrop이 처리될 시간을 줌 (1프레임 대기)
        yield return null;
        
        // 🌍 전역 드래그 상태 초기화
        CurrentlyDraggedWeapon = null;
        CurrentlyDraggingSlot = null;
        
        // 아이템이 이동했다면 원래 슬롯에서 무기 제거
        if (itemMoved)
        {
            weaponData = null; // 🔥 원래 슬롯에서 무기 제거
            isTemporarilyEmpty = false;
            UpdateVisuals();
        }
        else
        {
            // 아이템이 이동하지 않았다면 원래 슬롯으로 복원
            isTemporarilyEmpty = false;
            UpdateVisuals();
        }
    }
    
    void CreateDraggedItemImage()
    {
        if (draggedWeaponData == null || canvas == null) return;
        
        // 드래그될 아이템 이미지 오브젝트 생성
        draggedItemImage = new GameObject("DraggedItem");
        draggedItemImage.transform.SetParent(canvas.transform, false);
        
        // RectTransform 설정
        RectTransform rect = draggedItemImage.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(70, 70); // 슬롯과 같은 크기
        
        // Image 컴포넌트 추가
        Image dragImage = draggedItemImage.AddComponent<Image>();
        dragImage.sprite = draggedWeaponData.icon;
        dragImage.color = new Color(1f, 1f, 1f, 0.8f); // 약간 투명하게
        dragImage.raycastTarget = false; // 레이캐스트 차단 안함
        
        // Canvas Group 추가 (드래그 중 우선순위)
        CanvasGroup dragCanvasGroup = draggedItemImage.AddComponent<CanvasGroup>();
        dragCanvasGroup.blocksRaycasts = false;
        
        // 가장 위에 표시되도록 설정
        draggedItemImage.transform.SetAsLastSibling();
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // WeaponSlot에서 드래그된 무기 확인
        WeaponData weaponSlotDraggedWeapon = WeaponSlot.CurrentlyDraggedWeapon;
        WeaponSlot weaponSlotSource = WeaponSlot.CurrentlyDraggedSlot;
        
        if (weaponSlotDraggedWeapon != null && weaponSlotSource != null)
        {
            // 현재 슬롯에 무기가 있다면 WeaponSlot으로 이동
            if (weaponData != null)
            {
                weaponSlotSource.SetWeaponData(weaponData);
            }
            else
            {
                // WeaponSlot을 비움
                weaponSlotSource.SetWeaponData(null);
            }
            
            // 현재 슬롯에 WeaponSlot의 무기 설정
            weaponData = weaponSlotDraggedWeapon;
            
            // 두 슬롯 모두 시각적 업데이트
            weaponSlotSource.ForceUpdateVisuals();
            UpdateVisuals();
            
            // 인벤토리 새로고침
            if (inventoryManager != null)
            {
                inventoryManager.RefreshInventory();
            }
            
            return;
        }
        
        // 다른 InventorySlot에서 드래그된 무기 확인 (기존 로직)
        WeaponData inventoryDraggedWeapon = CurrentlyDraggedWeapon;
        InventorySlot inventorySlotSource = CurrentlyDraggingSlot;
        
        if (inventoryDraggedWeapon != null && inventorySlotSource != null && inventorySlotSource != this)
        {
            SwapItems(inventorySlotSource);
            return;
        }
    }
    
    void SwapItems(InventorySlot targetSlot)
    {
        if (targetSlot == null || inventoryManager == null) return;
        
        WeaponData myWeapon = draggedWeaponData;
        WeaponData targetWeapon = targetSlot.weaponData;
        
        // 아이템 교환
        weaponData = targetWeapon;
        targetSlot.weaponData = myWeapon;
        
        // 두 슬롯 모두 업데이트
        isTemporarilyEmpty = false;
        UpdateVisuals();
        targetSlot.UpdateVisuals();
        
        // 인벤토리 매니저에 변경사항 알림
        inventoryManager.RefreshInventory();
    }
    
    // 마우스 호버 이벤트들
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        tooltipTimer = 0f;
        UpdateSlotColor();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        tooltipTimer = 0f;
        HideTooltip();
        UpdateSlotColor();
    }
    
    // 클릭 이벤트
    public void OnPointerClick(PointerEventData eventData)
    {
        if (weaponData == null || isTemporarilyEmpty) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭: 선택/선택 해제
            ToggleSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭: 무기 장착
            if (inventoryManager != null)
            {
                inventoryManager.EquipWeapon(weaponData);
            }
        }
    }
    
    void ToggleSelection()
    {
        isSelected = !isSelected;
        UpdateSlotColor();
        
        // 다른 슬롯들의 선택 해제 (단일 선택)
        if (isSelected && inventoryManager != null)
        {
            var allSlots = inventoryManager.transform.GetComponentsInChildren<InventorySlot>();
            foreach (var slot in allSlots)
            {
                if (slot != this && slot.isSelected)
                {
                    slot.isSelected = false;
                    slot.UpdateSlotColor();
                }
            }
        }
    }
    
    void ShowTooltip()
    {
        if (inventoryManager != null && weaponData != null && !isTemporarilyEmpty)
        {
            Vector3 tooltipPosition = transform.position + new Vector3(100, 0, 0); // 슬롯 오른쪽에 표시
            inventoryManager.ShowTooltip(weaponData, tooltipPosition);
            showingTooltip = true;
        }
    }
    
    void HideTooltip()
    {
        if (inventoryManager != null)
        {
            inventoryManager.HideTooltip();
        }
        showingTooltip = false;
        tooltipTimer = 0f;
    }
    
    // 외부에서 호출할 수 있는 선택 상태 관리
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSlotColor();
    }
    
    public bool IsSelected()
    {
        return isSelected;
    }
    
    public bool IsEmpty()
    {
        return weaponData == null;
    }
    
    public bool HasWeapon()
    {
        return weaponData != null;
    }
} 