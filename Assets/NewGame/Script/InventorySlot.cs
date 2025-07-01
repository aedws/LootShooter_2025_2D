using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("📋 슬롯 사용법")]
    [TextArea(3, 5)]
    public string slotInstructions = "• 좌클릭: 슬롯 선택\n• 우클릭: 무기 즉시 장착\n• 드래그: WeaponSlot으로 드래그하여 장착\n• 마우스 호버: 0.5초 후 툴팁 표시\n• 무기 타입별로 테두리 색상 변경";

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
    
    // Public properties
    public WeaponData weaponData { get; private set; }
    public int slotIndex { get; set; }
    public InventoryManager inventoryManager { get; set; }
    
    // Private variables
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isDragging = false;
    private bool isSelected = false;
    private bool isHovered = false;
    
    // Tooltip variables
    private float tooltipTimer = 0f;
    private const float TOOLTIP_DELAY = 0.5f;
    private bool showingTooltip = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // 기본 색상 설정
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }
    
    void Update()
    {
        // 툴팁 타이머 처리
        if (isHovered && !isDragging && weaponData != null)
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
        }
        
        UpdateSlotColor();
    }
    
    void UpdateSlotColor()
    {
        if (backgroundImage == null) return;
        
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

    // 드래그 앤 드롭 핸들러들
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (weaponData == null || !isDragging) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // WeaponSlot에 드롭했는지 확인
        WeaponSlot weaponSlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<WeaponSlot>();
        if (weaponSlot != null)
        {
            // 무기 장착
            if (inventoryManager != null)
            {
                inventoryManager.EquipWeapon(weaponData);
            }
        }
        else
        {
            // 원래 위치로 복원
            rectTransform.anchoredPosition = originalPosition;
        }
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
        if (weaponData == null) return;
        
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
        if (inventoryManager != null && weaponData != null)
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