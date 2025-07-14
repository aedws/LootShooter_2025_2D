using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("📋 슬롯 사용법")]
    [TextArea(3, 5)]
    public string slotInstructions = "• 좌클릭: 슬롯 선택\n• 우클릭: 무기 즉시 장착\n• 드래그: WeaponSlot으로 무기 이동\n• 드롭: WeaponSlot에서 무기 반환 받기\n• 마우스 호버: 0.5초 후 툴팁 표시\n• 무기 타입별로 테두리 색상 변경\n• 플레이버 텍스트: 프리팹에서 설정한 대로 표시";

    [Header("🖼️ Slot Components")]
    [Tooltip("무기 아이콘을 표시할 Image 컴포넌트")]
    public Image iconImage;
    
    [Tooltip("슬롯 배경 이미지 (상태에 따라 색상 변경)")]
    public Image backgroundImage;
    
    [Tooltip("슬롯 테두리 이미지 (무기 타입별 색상)")]
    public Image borderImage;
    
    [Tooltip("탄약 정보를 표시할 Text 컴포넌트")]
    public Text ammoText;
    
    [Tooltip("무기 플레이버 텍스트를 표시할 Text 컴포넌트")]
    public Text flavorText;
    
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
    
    [Header("🎯 기본 무기 아이콘 (icon이 null일 때)")]
    [Tooltip("AR 타입 기본 아이콘")]
    public Sprite defaultARIcon;
    
    [Tooltip("HG 타입 기본 아이콘")]
    public Sprite defaultHGIcon;
    
    [Tooltip("MG 타입 기본 아이콘")]
    public Sprite defaultMGIcon;
    
    [Tooltip("SG 타입 기본 아이콘")]
    public Sprite defaultSGIcon;
    
    [Tooltip("SMG 타입 기본 아이콘")]
    public Sprite defaultSMGIcon;
    
    [Tooltip("SR 타입 기본 아이콘")]
    public Sprite defaultSRIcon;
    
    [Header("🛡️ 기본 방어구 아이콘 (icon이 null일 때)")]
    [Tooltip("방어구 기본 아이콘")]
    public Sprite defaultArmorIcon;
    
    [Tooltip("머리 기본 아이콘")]
    public Sprite defaultHelmetIcon;
    
    [Tooltip("상체 기본 아이콘")]
    public Sprite defaultChestIcon;
    
    [Tooltip("하체 기본 아이콘")]
    public Sprite defaultLegsIcon;
    
    [Tooltip("신발 기본 아이콘")]
    public Sprite defaultBootsIcon;
    
    [Tooltip("어깨 기본 아이콘")]
    public Sprite defaultShoulderIcon;
    
    [Tooltip("악세사리 기본 아이콘")]
    public Sprite defaultAccessoryIcon;
    
    // 🌍 전역 드래그 상태 (WeaponSlot에서 접근 가능)
    public static WeaponData CurrentlyDraggedWeapon { get; private set; } = null;
    public static ArmorData CurrentlyDraggedArmor { get; private set; } = null; // 🆕 방어구 드래그 상태
    public static InventorySlot CurrentlyDraggingSlot { get; private set; } = null;
    public static object CurrentlyDraggedChipset { get; set; } = null;
    
    // Public properties
    public WeaponData weaponData { get; private set; }
    public ArmorData armorData { get; private set; } // 🆕 방어구 데이터 추가
    public object chipsetData { get; private set; } // 칩셋 데이터 저장
    public int slotIndex { get; set; }
    public InventoryManager inventoryManager { get; set; }
    public bool isArmorSlot { get; set; } = false; // 🆕 방어구 슬롯 여부
    
    // Private variables
    private Canvas canvas;
    private bool isDragging = false;
    private bool isSelected = false;
    private bool isHovered = false;
    
    // 🎮 게임식 드래그앤드롭을 위한 변수들
    private GameObject draggedItemImage; // 드래그되는 아이템 이미지
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
        armorData = null; // 무기 설정 시 방어구 클리어
        UpdateVisuals();
    }
    
    // 🆕 방어구 설정 메서드
    public void SetArmor(ArmorData newArmorData)
    {
        armorData = newArmorData;
        weaponData = null; // 방어구 설정 시 무기 클리어
        UpdateVisuals();
    }
    
    public void ClearSlot()
    {
        weaponData = null;
        armorData = null;
        chipsetData = null; // 칩셋 데이터도 초기화
        UpdateVisuals();
    }
    
    public void SetChipset(object newChipset)
    {
        chipsetData = newChipset;
        weaponData = null;
        armorData = null;
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
            // 무기 표시
            ShowWeaponVisuals();
        }
        else if (armorData != null)
        {
            // 🆕 방어구 표시
            ShowArmorVisuals();
        }
        else if (chipsetData != null)
        {
            ShowChipsetVisuals();
        }
        else
        {
            ShowEmptySlot(false);
        }
        
        UpdateSlotColor();
    }
    
    // 🆕 무기 시각적 요소 표시
    void ShowWeaponVisuals()
    {
        if (weaponData != null)
        {
            // Debug.Log($"[무기등급] {weaponData.weaponName} rarity: {weaponData.rarity}, color: {weaponData.GetRarityColor()}");
        }
        if (iconImage != null)
        {
            // 🎯 아이콘이 null이면 무기 타입별 기본 아이콘 사용
            if (weaponData.icon != null)
            {
                iconImage.sprite = weaponData.icon;
            }
            else
            {
                // 무기 타입별 기본 아이콘 설정
                iconImage.sprite = GetDefaultWeaponIcon(weaponData.weaponType);
            }
            // 등급별 색상 적용
            iconImage.color = weaponData.GetRarityColor();
            iconImage.enabled = true;
            AdjustIconSize();
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
        
        // 플레이버 텍스트 표시
        if (flavorText != null)
        {
            flavorText.text = weaponData.flavorText;
            flavorText.enabled = true;
        }
        
        // 무기 타입별 색상
        if (borderImage != null)
        {
            borderImage.color = GetWeaponTypeColor(weaponData.weaponType);
            borderImage.enabled = true;
        }
        
        // 희귀도 효과
        if (rarityGlow != null)
        {
            rarityGlow.SetActive(weaponData.damage > 50);
        }
    }
    
    // 🆕 방어구 시각적 요소 표시
    void ShowArmorVisuals()
    {
        if (iconImage != null)
        {
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
            // 🎨 방어구 등급별 색상 적용
            iconImage.color = armorData.GetRarityColor();
            iconImage.enabled = (iconToUse != null);
            AdjustIconSize();
            
            // 방어구 색상 적용 완료
        }
        
        // 방어력 정보 표시 (탄약 텍스트 재사용)
        if (ammoText != null)
        {
            ammoText.text = $"방어력: {armorData.defense}";
            ammoText.enabled = true;
        }
        
        // 방어구 이름 표시 (플레이버 텍스트 재사용)
        if (flavorText != null)
        {
            flavorText.text = armorData.armorName;
            flavorText.enabled = true;
        }
        
        // 방어구 레어리티별 색상
        if (borderImage != null)
        {
            borderImage.color = armorData.GetRarityColor();
            borderImage.enabled = true;
        }
        
        // 레어리티 효과
        if (rarityGlow != null)
        {
            rarityGlow.SetActive(armorData.rarity >= ArmorRarity.Rare);
        }
    }
    
    void ShowChipsetVisuals()
    {
        // 칩셋 이름, 아이콘 등 표시
        string name = "";
        Sprite icon = null;
        Color color = Color.white;
        string desc = "";
        
        if (chipsetData is WeaponChipsetData weaponChipset)
        {
            name = weaponChipset.chipsetName;
            color = weaponChipset.GetRarityColor();
            desc = weaponChipset.description;
            // 무기 칩셋 기본 아이콘 사용
            icon = defaultHGIcon; // 권총 아이콘을 기본으로 사용
        }
        else if (chipsetData is ArmorChipsetData armorChipset)
        {
            name = armorChipset.chipsetName;
            color = armorChipset.GetRarityColor();
            desc = armorChipset.description;
            // 방어구 칩셋 기본 아이콘 사용
            icon = defaultChestIcon; // 상체 아이콘을 기본으로 사용
        }
        else if (chipsetData is PlayerChipsetData playerChipset)
        {
            name = playerChipset.chipsetName;
            color = playerChipset.GetRarityColor();
            desc = playerChipset.description;
            // 플레이어 칩셋 기본 아이콘 사용
            icon = defaultHelmetIcon; // 헬멧 아이콘을 기본으로 사용
        }
        
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = color;
            iconImage.enabled = (icon != null); // 아이콘이 있으면 활성화
            if (icon != null)
            {
                AdjustIconSize();
            }
        }
        
        if (flavorText != null)
        {
            flavorText.text = name;
            flavorText.enabled = true;
        }
        
        if (ammoText != null)
        {
            ammoText.text = desc;
            ammoText.enabled = true;
        }
        
        if (borderImage != null)
        {
            borderImage.color = color;
            borderImage.enabled = true;
        }
        
        if (rarityGlow != null)
        {
            rarityGlow.SetActive(false);
        }
    }
    
    void ShowEmptySlot(bool isDraggingEmpty)
    {
        // 드래그 중 임시 비움 상태면 투명하게
        if (isDraggingEmpty || isTemporarilyEmpty)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (flavorText != null) flavorText.enabled = false;
            if (ammoText != null) ammoText.enabled = false;
            if (borderImage != null) borderImage.enabled = false;
            if (rarityGlow != null) rarityGlow.SetActive(false);
            if (backgroundImage != null) backgroundImage.color = draggingEmptyColor;
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (flavorText != null) flavorText.enabled = false;
            if (ammoText != null) ammoText.enabled = false;
            if (borderImage != null) borderImage.enabled = false;
            if (rarityGlow != null) rarityGlow.SetActive(false);
            if (backgroundImage != null) backgroundImage.color = emptyColor;
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
    
    /// <summary>
    /// 무기 타입별 기본 아이콘을 반환합니다.
    /// </summary>
    Sprite GetDefaultWeaponIcon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.AR:
                return defaultARIcon ?? defaultSRIcon; // AR이 없으면 SR 사용
            case WeaponType.HG:
                return defaultHGIcon ?? defaultSRIcon; // HG가 없으면 SR 사용
            case WeaponType.MG:
                return defaultMGIcon ?? defaultSRIcon; // MG가 없으면 SR 사용
            case WeaponType.SG:
                return defaultSGIcon ?? defaultSRIcon; // SG가 없으면 SR 사용
            case WeaponType.SMG:
                return defaultSMGIcon ?? defaultSRIcon; // SMG가 없으면 SR 사용
            case WeaponType.SR:
                return defaultSRIcon;
            default:
                Debug.LogWarning($"[InventorySlot] 알 수 없는 무기 타입: {weaponType}");
                return defaultSRIcon; // 기본값으로 SR 사용
        }
    }
    
    /// <summary>
    /// 방어구 타입별 기본 아이콘을 반환합니다.
    /// </summary>
    Sprite GetDefaultArmorIcon(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.Helmet:
                return defaultHelmetIcon ?? defaultArmorIcon;
            case ArmorType.Chest:
                return defaultChestIcon ?? defaultArmorIcon;
            case ArmorType.Legs:
                return defaultLegsIcon ?? defaultArmorIcon;
            case ArmorType.Boots:
                return defaultBootsIcon ?? defaultArmorIcon;
            case ArmorType.Shoulder:
                return defaultShoulderIcon ?? defaultArmorIcon;
            case ArmorType.Accessory:
                return defaultAccessoryIcon ?? defaultArmorIcon;
            default:
                Debug.LogWarning($"[InventorySlot] 알 수 없는 방어구 타입: {armorType}");
                return defaultArmorIcon; // 기본값으로 일반 방어구 아이콘 사용
        }
    }

    // 🎮 진짜 게임식 드래그 앤 드롭 시스템
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryManager == null) return;
        // 슬롯이 완전히 비어 있으면 드래그 불가
        if (chipsetData == null && weaponData == null && armorData == null)
            return;
        Debug.Log($"[InventorySlot] OnBeginDrag 호출됨 - 칩셋: {chipsetData}, 무기: {weaponData}, 방어구: {armorData}, 슬롯: {slotIndex}, 탭: {inventoryManager.CurrentTab}");
        switch (inventoryManager.CurrentTab)
        {
            case InventoryTab.Weapons:
                if (weaponData == null) return;
                CurrentlyDraggedWeapon = weaponData;
                CreateDraggedItemImage();
                break;
            case InventoryTab.Armors:
                if (armorData == null) return;
                CurrentlyDraggedArmor = armorData;
                CreateDraggedItemImage();
                break;
            case InventoryTab.Chipsets:
                if (chipsetData == null) return;
                CurrentlyDraggedChipset = chipsetData;
                CreateDraggedItemImage();
                break;
        }
        CurrentlyDraggingSlot = this;
        isDragging = true;
        isTemporarilyEmpty = true; // 드래그 중 슬롯 투명화
        UpdateVisuals();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"[InventorySlot] OnDrag 호출됨 - 슬롯: {slotIndex}");
        if (draggedItemImage != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out pos);
            draggedItemImage.GetComponent<RectTransform>().localPosition = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[InventorySlot] OnEndDrag 호출됨 - 슬롯: {slotIndex}");
        if (CurrentlyDraggedChipset != null)
            CurrentlyDraggedChipset = null;
        if (CurrentlyDraggedWeapon != null)
            CurrentlyDraggedWeapon = null;
        if (CurrentlyDraggedArmor != null)
            CurrentlyDraggedArmor = null;
        isDragging = false;
        isTemporarilyEmpty = false; // 드래그 종료 시 복원
        UpdateVisuals();
        if (draggedItemImage != null)
            Destroy(draggedItemImage);
        CurrentlyDraggingSlot = null;
    }
    
    // 🆕 지연된 드래그 상태 초기화
    System.Collections.IEnumerator ClearDragStateDelayed(bool itemMoved)
    {
        // ArmorSlot/WeaponSlot의 OnDrop이 처리될 시간을 줌 (2프레임 대기)
        yield return null;
        yield return null;
        
        // 🌍 전역 드래그 상태 초기화
        CurrentlyDraggedWeapon = null;
        CurrentlyDraggedArmor = null; // 🆕 방어구 드래그 상태 초기화
        CurrentlyDraggingSlot = null;
        
        // 🆕 로컬 드래그 상태 초기화
        // draggedWeaponData = null; // 더 이상 사용하지 않으므로 삭제
        // draggedArmorData = null; // 더 이상 사용하지 않으므로 삭제
        
        // 아이템이 이동했다면 원래 슬롯에서 아이템 제거
        if (itemMoved)
        {
            // 🆕 무기 또는 방어구 중 하나만 제거
            if (weaponData != null && armorData == null)
            {
                weaponData = null; // 🔥 원래 슬롯에서 무기 제거
            }
            else if (armorData != null && weaponData == null)
            {
                armorData = null; // 🆕 원래 슬롯에서 방어구 제거
            }
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
        if (draggedItemImage != null)
        {
            Destroy(draggedItemImage);
        }
        // 드래그 아이콘용 오브젝트 생성
        draggedItemImage = new GameObject("DraggedItemImage");
        draggedItemImage.transform.SetParent(canvas.transform, false);
        draggedItemImage.transform.SetAsLastSibling();
        var image = draggedItemImage.AddComponent<Image>();
        image.raycastTarget = false; // 드래그 아이콘은 Raycast 차단
        Sprite iconSprite = null;
        Color rarityColor = Color.white;
        // 드래그 상태에 따라 아이콘/색상 결정
        if (CurrentlyDraggedWeapon != null)
        {
            iconSprite = CurrentlyDraggedWeapon.icon != null ? CurrentlyDraggedWeapon.icon : GetDefaultWeaponIcon(CurrentlyDraggedWeapon.weaponType);
            rarityColor = CurrentlyDraggedWeapon.GetRarityColor();
        }
        else if (CurrentlyDraggedArmor != null)
        {
            if (CurrentlyDraggedArmor.icon != null)
            {
                iconSprite = CurrentlyDraggedArmor.icon;
            }
            else
            {
                switch (CurrentlyDraggedArmor.armorType)
                {
                    case ArmorType.Helmet: iconSprite = defaultHelmetIcon; break;
                    case ArmorType.Chest: iconSprite = defaultChestIcon; break;
                    case ArmorType.Legs: iconSprite = defaultLegsIcon; break;
                    case ArmorType.Boots: iconSprite = defaultBootsIcon; break;
                    case ArmorType.Shoulder: iconSprite = defaultShoulderIcon; break;
                    case ArmorType.Accessory: iconSprite = defaultAccessoryIcon; break;
                    default: iconSprite = defaultChestIcon; break;
                }
            }
            rarityColor = CurrentlyDraggedArmor.GetRarityColor();
        }
        else if (CurrentlyDraggedChipset != null)
        {
            if (CurrentlyDraggedChipset is WeaponChipsetData wcd)
            {
                iconSprite = defaultHGIcon;
                rarityColor = wcd.GetRarityColor();
            }
            else if (CurrentlyDraggedChipset is ArmorChipsetData acd)
            {
                iconSprite = defaultChestIcon;
                rarityColor = acd.GetRarityColor();
            }
            else if (CurrentlyDraggedChipset is PlayerChipsetData pcd)
            {
                iconSprite = defaultHelmetIcon;
                rarityColor = pcd.GetRarityColor();
            }
        }
        image.sprite = iconSprite;
        image.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.88f); // 등급색 + 투명도
        // 크기: 원본 아이콘 크기 * 0.9 (없으면 fallback)
        var rect = draggedItemImage.GetComponent<RectTransform>();
        if (iconImage != null)
        {
            rect.sizeDelta = iconImage.rectTransform.sizeDelta * 0.9f;
        }
        else
        {
            rect.sizeDelta = new Vector2(56, 56);
        }
        rect.localScale = Vector3.one;
        // 커서 위치에 바로 붙이기 (OnDrag에서 위치 갱신)
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
        rect.localPosition = pos;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // 🆕 ArmorSlot에서 드래그된 방어구 확인
        ArmorData armorSlotDraggedArmor = ArmorSlot.CurrentlyDraggedArmor;
        ArmorSlot armorSlotSource = ArmorSlot.CurrentlyDraggedSlot;
        
        if (armorSlotDraggedArmor != null && armorSlotSource != null)
        {
            // 현재 슬롯에 방어구가 있다면 ArmorSlot으로 이동
            if (armorData != null)
            {
                armorSlotSource.SetArmorData(armorData);
            }
            else
            {
                // ArmorSlot을 비움
                armorSlotSource.SetArmorData(null);
            }
            
            // 현재 슬롯에 ArmorSlot의 방어구 설정
            armorData = armorSlotDraggedArmor;
            
            // 두 슬롯 모두 시각적 업데이트
            armorSlotSource.ForceUpdateVisuals();
            UpdateVisuals();
            
            // 인벤토리 새로고침 (즉시 호출)
            if (inventoryManager != null)
            {
                inventoryManager.RefreshInventory();
            }
            
            return;
        }
        
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
            
            // 인벤토리 새로고침 (즉시 호출)
            if (inventoryManager != null)
            {
                inventoryManager.RefreshInventory();
            }
            
            return;
        }
        
        // 다른 InventorySlot에서 드래그된 아이템 확인 (무기 또는 방어구)
        WeaponData inventoryDraggedWeapon = CurrentlyDraggedWeapon;
        ArmorData inventoryDraggedArmor = CurrentlyDraggedArmor; // 🆕 방어구 드래그 확인
        InventorySlot inventorySlotSource = CurrentlyDraggingSlot;
        
        if (inventoryDraggedWeapon != null && inventorySlotSource != null && inventorySlotSource != this)
        {
            SwapItems(inventorySlotSource);
            return;
        }
        
        // 🆕 다른 InventorySlot에서 드래그된 방어구 처리
        if (inventoryDraggedArmor != null && inventorySlotSource != null && inventorySlotSource != this)
        {
            SwapItems(inventorySlotSource);
            return;
        }
    }
    
    void SwapItems(InventorySlot targetSlot)
    {
        if (targetSlot == null || inventoryManager == null) return;
        
        // 🆕 전역 드래그 상태 사용 (OnDrop에서 호출될 때)
        if (CurrentlyDraggedWeapon != null)
        {
            // 무기 교환
            WeaponData myWeapon = CurrentlyDraggedWeapon;
            WeaponData targetWeapon = targetSlot.weaponData;
            
            weaponData = targetWeapon;
            targetSlot.weaponData = myWeapon;
        }
        else if (CurrentlyDraggedArmor != null)
        {
            // 방어구 교환
            ArmorData myArmor = CurrentlyDraggedArmor;
            ArmorData targetArmor = targetSlot.armorData;
            
            armorData = targetArmor;
            targetSlot.armorData = myArmor;
        }
        else if (CurrentlyDraggedChipset != null)
        {
            // 칩셋 교환
            object myChipset = CurrentlyDraggedChipset;
            object targetChipset = targetSlot.chipsetData;
            
            chipsetData = targetChipset;
            targetSlot.chipsetData = myChipset;
        }
        
        // 두 슬롯 모두 업데이트
        isTemporarilyEmpty = false;
        targetSlot.isTemporarilyEmpty = false;
        UpdateVisuals();
        targetSlot.UpdateVisuals();
        
        // 인벤토리 매니저에 변경사항 알림 (즉시 호출)
        if (inventoryManager != null)
        {
            inventoryManager.RefreshInventory();
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
        if (isTemporarilyEmpty) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭: 선택/선택 해제
            ToggleSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭: 무기 또는 방어구 장착
            if (weaponData != null)
            {
                if (inventoryManager != null)
                {
                    inventoryManager.EquipWeapon(weaponData);
                }
            }
            else if (armorData != null)
            {
                // 🆕 방어구 우클릭: 자동 장착
                TryEquipArmor();
            }
        }
    }
    
    // 🆕 방어구 자동 장착 시도
    void TryEquipArmor()
    {
        if (armorData == null) return;
        if (inventoryManager == null) return;
        
        // ArmorSlotManager 찾기
        ArmorSlotManager armorSlotManager = FindFirstObjectByType<ArmorSlotManager>();
        if (armorSlotManager == null)
        {
            Debug.LogWarning("⚠️ [InventorySlot] ArmorSlotManager를 찾을 수 없습니다.");
            return;
        }
        
        // 해당 타입의 슬롯에 자동 장착 시도
        int slotIndex = GetSlotIndexForArmorType(armorData.armorType);
        if (slotIndex >= 0)
        {
            bool success = armorSlotManager.EquipArmorToSlot(armorData, slotIndex);
            if (success)
            {
                // Debug.Log($"🛡️ {armorData.armorName}이(가) 슬롯 {slotIndex}에 자동 장착되었습니다!");
                
                // 🆕 인벤토리 새로고침
                if (inventoryManager != null)
                {
                    inventoryManager.RefreshInventory();
                }
            }
            else
            {
                // Debug.Log($"⚠️ {armorData.armorName} 자동 장착 실패 (슬롯 {slotIndex}에 이미 방어구가 장착되어 있음)");
            }
        }
    }
    
    // 🆕 방어구 타입에 따른 슬롯 인덱스 반환
    int GetSlotIndexForArmorType(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.Helmet: return 0;    // 머리
            case ArmorType.Chest: return 1;     // 상체
            case ArmorType.Legs: return 2;      // 하체
            case ArmorType.Boots: return 3;     // 신발
            case ArmorType.Shoulder: return 4;  // 어깨
            case ArmorType.Accessory: return 5; // 악세사리
            default: return -1;
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
    
    // 🎨 아이콘 크기를 슬롯 크기에 맞춰 동적 조정
    void AdjustIconSize()
    {
        if (iconImage == null || inventoryManager == null) return;
        
        RectTransform iconRect = iconImage.GetComponent<RectTransform>();
        if (iconRect == null) return;
        
        // 🆕 아이템 타입에 따라 아이콘 크기 설정
        Vector2 iconSize;
        
        if (armorData != null)
        {
            // 방어구: 80x80 고정 크기
            iconSize = new Vector2(80f, 80f);
        }
        else
        {
            // 무기: 슬롯 크기의 85%로 설정 (기존 방식)
            Vector2 slotSize = inventoryManager.slotSize;
            iconSize = slotSize * 0.85f;
            
            // 최소/최대 크기 제한
            iconSize.x = Mathf.Clamp(iconSize.x, 20f, 150f);
            iconSize.y = Mathf.Clamp(iconSize.y, 20f, 150f);
        }
        
        iconRect.sizeDelta = iconSize;
        
        // 아이콘을 슬롯 중앙에 위치시키기
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
    }
    
    // 현재 슬롯 크기 가져오기
    Vector2 GetCurrentSlotSize()
    {
        if (inventoryManager != null)
        {
            // InventoryManager의 설정된 슬롯 크기 사용
            return inventoryManager.slotSize;
        }
        else
        {
            // InventoryManager가 없으면 자체 RectTransform 크기 사용
            RectTransform selfRect = GetComponent<RectTransform>();
            return selfRect != null ? selfRect.sizeDelta : new Vector2(70f, 70f);
        }
    }
    

} 