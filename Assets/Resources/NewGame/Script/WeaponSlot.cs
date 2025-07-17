using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("📋 무기 슬롯 사용법")]
    [TextArea(3, 5)]
    [Tooltip("우클릭: 무기 해제 | 드래그: 무기 이동/교체")]
    public string usageInfo = "• 인벤토리에서 무기를 드래그해서 장착\n• 우클릭으로 무기 해제\n• 슬롯 간 드래그로 무기 교체\n• 슬롯에서 인벤토리로 드래그해서 반환";

    [Header("🖼️ Slot Components")]
    [Tooltip("무기 아이콘을 표시할 Image")]
    public Image icon;
    
    [Tooltip("슬롯 배경 이미지 (상태별 색상 변경)")]
    public Image backgroundImage;
    
    [Tooltip("장착된 무기 이름을 표시할 Text")]
    public Text weaponNameText;
    
    [Tooltip("무기의 탄약 정보를 표시할 Text")]
    public Text ammoText;
    
    [Header("🎨 Visual States")]
    [Tooltip("기본 빈 슬롯 색상")]
    public Color normalColor = Color.white;
    
    [Tooltip("마우스 호버 시 색상")]
    public Color hoverColor = Color.yellow;
    
    [Tooltip("무기 장착 시 색상")]
    public Color equippedColor = Color.green;
    
    [Tooltip("드래그 중일 때 색상")]
    public Color draggingColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("🔗 References")]
    [Tooltip("플레이어 인벤토리 컴포넌트 (자동 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 컴포넌트 (자동 연결됨)")]
    public InventoryManager inventoryManager;
    
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
    
    [Header("디버그")]
    [Tooltip("디버그 모드 활성화")]
    public bool debugMode = false;
    
    // Properties
    public WeaponData weaponData { get; private set; }
    private bool isHovered = false;
    private bool isDragging = false;
    
    // 🆕 드래그 관련 static 변수
    public static WeaponData CurrentlyDraggedWeapon { get; private set; } = null;
    public static WeaponSlot CurrentlyDraggedSlot { get; private set; } = null;

    public event System.Action<WeaponData> OnWeaponChanged;

    void Start()
    {
        // Debug.Log($"🔧 [WeaponSlot] Start() 호출됨 - {gameObject.name}");
        
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        // 🔧 UI 컴포넌트 자동 생성 및 설정
        SetupUIComponents();
        
        // 🆕 EventSystem 등록 강제 (중요!)
        ForceEventSystemRegistration();
        
        // 🆕 UI 레이어 충돌 자동 해결 (게임 시작 시)
        StartCoroutine(AutoFixUILayerConflictsOnStart());
        
        UpdateVisuals();
        
        // 마우스 이벤트 인터페이스 확인
        // Debug.Log($"🔧 [WeaponSlot] IPointerClickHandler 구현: {this is IPointerClickHandler}");
        // Debug.Log($"🔧 [WeaponSlot] raycastTarget: {(backgroundImage != null ? backgroundImage.raycastTarget.ToString() : "backgroundImage null")}");
    }

    // 🆕 게임 시작 시 UI 레이어 충돌 자동 해결
    System.Collections.IEnumerator AutoFixUILayerConflictsOnStart()
    {
        // Debug.Log($"🚀 [WeaponSlot] 게임 시작 시 UI 레이어 충돌 자동 해결 시작... - {gameObject.name}");
        
        // EventSystem과 UI가 완전히 초기화될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);
        
        // WeaponSlot 위치를 기준으로 자동 해결
        AutoFixUILayerConflicts();
        
        // Debug.Log($"✅ [WeaponSlot] 게임 시작 시 UI 레이어 충돌 자동 해결 완료! - {gameObject.name}");
    }
    
    // 🆕 마우스 위치에 의존하지 않는 자동 UI 충돌 해결
    void AutoFixUILayerConflicts()
    {
        // Debug.Log($"🔧 [WeaponSlot] UI 레이어 충돌 자동 해결 (마우스 위치 무관) - {gameObject.name}");
        
        // WeaponSlot의 중심 위치를 스크린 좌표로 변환
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector3 worldPosition = rectTransform.position;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
        
        // Debug.Log($"📍 WeaponSlot 스크린 위치: {screenPosition}");
        
        // 해당 위치에서 레이캐스트 수행
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) 
        {
            // Debug.LogWarning("⚠️ EventSystem이 없어서 자동 해결을 건너뜁니다.");
            return;
        }
        
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = screenPosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, raycastResults);
        
        // Debug.Log($"🎯 WeaponSlot 위치에서 감지된 UI 요소 수: {raycastResults.Count}");
        
        bool weaponSlotFound = false;
        int weaponSlotIndex = -1;
        
        // WeaponSlot의 위치 찾기
        for (int i = 0; i < raycastResults.Count; i++)
        {
            // Debug.Log($"  {i}: {raycastResults[i].gameObject.name}");
            
            if (raycastResults[i].gameObject == gameObject)
            {
                weaponSlotFound = true;
                weaponSlotIndex = i;
                // Debug.Log($"    ⭐ WeaponSlot 발견! (인덱스: {i})");
            }
        }
        
        if (!weaponSlotFound)
        {
            // Debug.LogWarning($"⚠️ WeaponSlot이 해당 위치에서 감지되지 않습니다! - {gameObject.name}");
            return;
        }
        
        if (weaponSlotIndex == 0)
        {
            // Debug.Log($"✅ WeaponSlot이 이미 최상위에 있습니다! - {gameObject.name}");
            return;
        }
        
        // WeaponSlot을 덮고 있는 UI 요소들 자동 처리
        // Debug.Log($"🔧 WeaponSlot을 덮고 있는 {weaponSlotIndex}개의 UI 요소를 자동 처리합니다...");
        
        int fixedCount = 0;
        
        for (int i = 0; i < weaponSlotIndex; i++)
        {
            var blockingUI = raycastResults[i].gameObject;
            // Debug.Log($"  {i + 1}. {blockingUI.name} 자동 처리 중...");
            
            // 자동 해결: 특정 UI 요소들의 raycastTarget 비활성화
            if (blockingUI.name.Contains("InventoryPanel") || 
                blockingUI.name.Contains("Panel") ||
                blockingUI.name.Contains("Background"))
            {
                Image img = blockingUI.GetComponent<Image>();
                if (img != null && img.raycastTarget)
                {
                    img.raycastTarget = false;
                    fixedCount++;
                    // Debug.Log($"    ✅ {blockingUI.name}의 raycastTarget을 false로 설정");
                }
            }
            // 투명한 UI 요소 자동 처리
            else
            {
                Image img = blockingUI.GetComponent<Image>();
                if (img != null && img.color.a < 0.1f && img.raycastTarget)
                {
                    img.raycastTarget = false;
                    fixedCount++;
                    // Debug.Log($"    ✅ 투명한 UI {blockingUI.name}의 raycastTarget을 false로 설정");
                }
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"✅ {fixedCount}개의 UI 요소를 자동으로 수정했습니다! WeaponSlot 우클릭이 이제 작동해야 합니다!");
        }
        // else
        // {
        //     Debug.LogWarning($"⚠️ 자동으로 수정할 수 있는 UI 요소가 없습니다. 수동 확인이 필요할 수 있습니다. - {gameObject.name}");
        // }
    }

    void Update()
    {
        // 🎮 키보드 단축키로 WeaponSlot 테스트
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestWeaponSlotMouseEvents();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ForceEventSystemRegistration();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            FixMouseEventIssues();
        }
        
        // 🆕 F8: Canvas 레이어 및 렌더링 문제 진단
        if (Input.GetKeyDown(KeyCode.F8))
        {
            DiagnoseCanvasLayerIssues();
        }
        
        // 🆕 F4: UI 레이어 충돌 문제 자동 해결
        if (Input.GetKeyDown(KeyCode.F4))
        {
            FixUILayerConflicts();
        }
    }

    // 🆕 EventSystem 등록 강제
    void ForceEventSystemRegistration()
    {
        // Debug.Log($"🔧 [WeaponSlot] EventSystem 등록 강제 시작 - {gameObject.name}");
        
        // EventSystem 확인
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("❌ [WeaponSlot] EventSystem이 없습니다!");
            return;
        }
        
        // GraphicRaycaster 확인
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log($"✅ [WeaponSlot] GraphicRaycaster 추가 - {canvas.name}");
            }
        }
        
        // 컴포넌트 강제 활성화/비활성화로 EventSystem 재등록
        this.enabled = false;
        this.enabled = true;
        
        // GameObject 강제 활성화/비활성화
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        
        Debug.Log($"✅ [WeaponSlot] EventSystem 재등록 완료 - {gameObject.name}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 1. 인벤토리에서 드래그된 무기 처리
        WeaponData inventoryDraggedWeapon = InventorySlot.CurrentlyDraggedWeapon;
        if (inventoryDraggedWeapon != null)
        {
            EquipWeapon(inventoryDraggedWeapon);
            return;
        }
        
        // 2. 다른 WeaponSlot에서 드래그된 무기 처리 (슬롯 간 교체)
        WeaponData slotDraggedWeapon = CurrentlyDraggedWeapon;
        WeaponSlot draggedFromSlot = CurrentlyDraggedSlot;
        
        if (slotDraggedWeapon != null && draggedFromSlot != null && draggedFromSlot != this)
        {
            // 현재 슬롯의 무기 (교체될 무기)
            WeaponData currentWeapon = weaponData;
            
            // 드래그 시작 슬롯의 무기 제거 (인벤토리로 반환하지 않음)
            draggedFromSlot.SetWeaponData(null);
            
            // 현재 슬롯에 드래그된 무기 장착
            SetWeaponData(slotDraggedWeapon);
            
            // 현재 슬롯에 있던 무기를 드래그 시작 슬롯으로 이동 (있다면)
            if (currentWeapon != null)
            {
                draggedFromSlot.SetWeaponData(currentWeapon);
            }
            return;
        }
    }
    
    // 마우스 이벤트 핸들러들
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("🖱️ [WeaponSlot] OnPointerEnter 호출됨");
        isHovered = true;
        UpdateVisuals();
        
        // 툴팁 표시
        if (weaponData != null && inventoryManager != null)
        {
            Vector3 tooltipPosition = transform.position + new Vector3(100, 0, 0);
            inventoryManager.ShowTooltip(weaponData, tooltipPosition);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("🖱️ [WeaponSlot] OnPointerExit 호출됨");
        isHovered = false;
        UpdateVisuals();
        
        // 툴팁 숨기기
        if (inventoryManager != null)
        {
            inventoryManager.HideTooltip();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log($"🖱️ [WeaponSlot] OnPointerClick 호출됨 - 버튼: {eventData.button}");
        
        if (eventData.button == PointerEventData.InputButton.Right && weaponData != null)
        {
            // Debug.Log($"✅ [WeaponSlot] 우클릭으로 무기 해제: {weaponData.weaponName}");
            // 우클릭으로 무기 해제
            UnequipWeapon();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && weaponData == null)
        {
            // Debug.Log("⚠️ [WeaponSlot] 우클릭했지만 무기가 없음");
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Debug.Log("🖱️ [WeaponSlot] 좌클릭 감지됨");
        }
    }

    public void EquipWeapon(WeaponData newWeaponData)
    {
        // 기존 무기가 있다면 인벤토리로 돌려보내기
        if (weaponData != null)
        {
            ReturnWeaponToInventory(weaponData);
        }
        
        // 새 무기 장착
        weaponData = newWeaponData;
        
        // PlayerInventory의 현재 무기와 다를 때만 실제 오브젝트 변경
        if (playerInventory != null && playerInventory.GetEquippedWeapon() != weaponData)
            playerInventory.SetEquippedWeapon(weaponData);
        
        // 🔧 인벤토리에서 장착된 무기 제거
        if (weaponData != null && inventoryManager != null)
        {
            inventoryManager.RemoveWeapon(weaponData, false); // 새로고침 없이 제거만
            inventoryManager.RefreshInventory(); // 무기 장착 시 인벤토리 자동 리프레시
        }
        
        // 🏃‍♂️ 플레이어 이동속도 업데이트
        UpdatePlayerMovementSpeed();
        
        OnWeaponChanged?.Invoke(weaponData);
        UpdateVisuals();
    }

    // 🔧 새로운 메서드: 무기 해제 (우클릭용)
    public void UnequipWeapon()
    {
        if (weaponData == null) return;
        
        WeaponData oldWeapon = weaponData;
        
        // 무기 해제
        weaponData = null;
        
        // PlayerInventory의 현재 무기와 일치할 때만 해제
        if (playerInventory != null && playerInventory.GetEquippedWeapon() == oldWeapon)
            playerInventory.SetEquippedWeapon(null);
        
        // 🏃‍♂️ 플레이어 이동속도 복원 (무기 없음)
        UpdatePlayerMovementSpeed();
        
        // 인벤토리에 무기 다시 추가 및 UI 업데이트
        ReturnWeaponToInventory(oldWeapon);
        
        OnWeaponChanged?.Invoke(weaponData);
        UpdateVisuals();
    }

    public void ClearSlot()
    {
        if (weaponData != null)
        {
            // 무기 해제 시 인벤토리에 다시 추가
            ReturnWeaponToInventory(weaponData);
        }
        
        // PlayerInventory의 현재 무기와 일치할 때만 해제
        if (playerInventory != null && playerInventory.GetEquippedWeapon() == weaponData)
            playerInventory.SetEquippedWeapon(null);
        
        weaponData = null;
        
        OnWeaponChanged?.Invoke(weaponData);
        UpdateVisuals();
    }
    
    // 🔄 새로운 메서드: 무기를 인벤토리로 반환하고 UI 업데이트
    void ReturnWeaponToInventory(WeaponData weapon)
    {
        if (weapon == null) return;
        
        // PlayerInventory와 InventoryManager 모두에 추가
        if (playerInventory != null)
        {
            playerInventory.AddWeapon(weapon);
        }
        
        if (inventoryManager != null)
        {
            inventoryManager.AddWeapon(weapon);
            inventoryManager.RefreshInventory(); // 🔥 UI 업데이트!
        }
    }
    
    // 🆕 외부에서 weaponData를 직접 설정할 수 있는 public 메서드
    public void SetWeaponData(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;
        OnWeaponChanged?.Invoke(weaponData);
        UpdateVisuals();
    }
    
    // 🆕 외부에서 시각적 업데이트를 강제할 수 있는 public 메서드
    public void ForceUpdateVisuals()
    {
        UpdateVisuals();
    }
    
    // 🆕 무기가 장착되어 있는지 확인하는 메서드
    public bool IsEquipped()
    {
        return weaponData != null;
    }
    
    void UpdateVisuals()
    {
        // 🧪 컴포넌트 상태 진단 (필요시에만)
        if (icon == null || backgroundImage == null)
        {
            DiagnoseComponents();
        }
        
        // 아이콘 업데이트
        if (icon != null)
        {
            if (weaponData != null)
            {
                // 🎯 아이콘이 null이면 무기 타입별 기본 아이콘 사용
                if (weaponData.icon != null)
                {
                    icon.sprite = weaponData.icon;
                    icon.enabled = true;
                    icon.color = weaponData.GetRarityColor();
                    
                    // 🔧 강제 새로고침
                    icon.gameObject.SetActive(false);
                    icon.gameObject.SetActive(true);
                }
                else
                {
                    // 무기 타입별 기본 아이콘 사용
                    icon.sprite = GetDefaultWeaponIcon(weaponData.weaponType);
                    icon.enabled = true;
                    icon.color = weaponData.GetRarityColor();
                    
                    if (debugMode)
                        Debug.LogWarning($"[WeaponSlot] WeaponData '{weaponData.weaponName}'의 icon이 null이어서 기본 아이콘을 사용합니다.");
                }
            }
            else
            {
                icon.sprite = null;
                icon.enabled = false;
            }
        }
        else
        {
            Debug.LogError("❌ [WeaponSlot] icon Image 컴포넌트가 null입니다!");
        }
        
        // 무기 이름 표시
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponData != null ? weaponData.weaponName : "Empty";
        }
        
        // 탄약 정보 표시
        if (ammoText != null)
        {
            if (weaponData != null)
            {
                if (weaponData.infiniteAmmo)
                    ammoText.text = "∞";
                else
                    ammoText.text = $"{weaponData.currentAmmo}/{weaponData.maxAmmo}";
            }
            else
            {
                ammoText.text = "";
            }
        }
        
        // 배경 색상 업데이트 (드래그 상태 우선 적용)
        if (backgroundImage != null)
        {
            Color targetColor = normalColor;
            
            if (isDragging)
            {
                targetColor = draggingColor; // 드래그 중일 때 반투명
            }
            else if (weaponData != null)
            {
                targetColor = equippedColor; // 무기 장착 시 녹색
            }
            else if (isHovered)
            {
                targetColor = hoverColor; // 마우스 호버 시 노란색
            }
            
            backgroundImage.color = targetColor;
        }
    }
    
    // 🧪 새로운 메서드: 컴포넌트 상태 진단
    void DiagnoseComponents()
    {
        // Icon Image 진단
        if (icon == null)
        {
            // 자동으로 Icon 컴포넌트 찾기 시도
            Image[] images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("icon") || img.name.ToLower().Contains("weapon"))
                {
                    icon = img;
                    break;
                }
            }
            
            // 그래도 없으면 첫 번째 Image 사용
            if (icon == null && images.Length > 0)
            {
                icon = images[0];
            }
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
                if (debugMode)
                    Debug.LogWarning($"[WeaponSlot] 알 수 없는 무기 타입: {weaponType}");
                return defaultSRIcon; // 기본값으로 SR 사용
        }
    }

    // 🏃‍♂️ 플레이어 이동속도 업데이트 메서드
    void UpdatePlayerMovementSpeed()
    {
        // PlayerController 찾기 (자동 연결)
        if (playerInventory == null) return;
        
        PlayerController playerController = playerInventory.GetComponent<PlayerController>();
        if (playerController == null)
        {
            // 혹시 PlayerController가 다른 오브젝트에 있다면 찾기
            playerController = FindAnyObjectByType<PlayerController>();
        }
        
        if (playerController != null)
        {
            playerController.UpdateMovementSpeed(weaponData);
        }
        else
        {
            Debug.LogWarning("⚠️ [WeaponSlot] PlayerController를 찾을 수 없어 이동속도를 업데이트할 수 없습니다!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 무기가 없으면 드래그 시작하지 않음
        if (weaponData == null) return;
        
        CurrentlyDraggedWeapon = weaponData;
        CurrentlyDraggedSlot = this;
        isDragging = true;
        
        UpdateVisuals();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중 상태 유지 (추가 로직이 필요하면 여기에)
        if (weaponData != null && isDragging)
        {
            // 필요시 드래그 중 시각적 효과 추가 가능
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        // 드래그가 성공적으로 완료되지 않았다면 (아무곳에도 드롭되지 않음)
        // 원래 상태로 복구는 자동으로 됨 (weaponData는 그대로 유지)
        
        // static 변수 초기화
        CurrentlyDraggedWeapon = null;
        CurrentlyDraggedSlot = null;
        
        UpdateVisuals();
    }

    // 🆕 드래그앤드롭 문제 진단 도구
    [ContextMenu("Diagnose Drag Drop Issues")]
    public void DiagnoseDragDropIssues()
    {
        // Debug.Log("🔍 [WeaponSlot] 드래그앤드롭 문제 진단 시작...");
        
        // 1. Image 컴포넌트 raycastTarget 확인
        if (backgroundImage != null)
        {
            // Debug.Log($"📋 backgroundImage.raycastTarget: {backgroundImage.raycastTarget}");
            if (!backgroundImage.raycastTarget)
            {
                // Debug.LogWarning("⚠️ backgroundImage.raycastTarget이 false입니다! 드래그앤드롭이 작동하지 않을 수 있습니다.");
                backgroundImage.raycastTarget = true;
                // Debug.Log("🔧 backgroundImage.raycastTarget을 true로 수정했습니다.");
            }
        }
        else
        {
            // Debug.LogError("❌ backgroundImage가 null입니다!");
        }
        
        // 2. Canvas 및 GraphicRaycaster 확인
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
        }
        else
        {
            // Debug.Log($"✅ Canvas 찾음: {canvas.name}");
            
            UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                // Debug.LogError("❌ GraphicRaycaster가 없습니다!");
                raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                // Debug.Log("🔧 GraphicRaycaster를 추가했습니다.");
            }
            else
            {
                // Debug.Log("✅ GraphicRaycaster 확인 완료");
            }
        }
        
        // 3. IDropHandler 인터페이스 확인
        IDropHandler dropHandler = GetComponent<IDropHandler>();
        if (dropHandler == null)
        {
            // Debug.LogError("❌ IDropHandler 인터페이스가 구현되지 않았습니다!");
        }
        else
        {
            // Debug.Log("✅ IDropHandler 인터페이스 구현 확인");
        }
        
        // 4. GameObject 활성화 상태 확인
        // Debug.Log($"📋 GameObject 활성화: {gameObject.activeInHierarchy}");
        // Debug.Log($"📋 Component 활성화: {enabled}");
        
        // 5. 레이어 및 위치 확인
        // Debug.Log($"📋 Layer: {gameObject.layer}");
        // Debug.Log($"📋 Position: {transform.position}");
        // Debug.Log($"📋 Local Position: {transform.localPosition}");
        
        // 6. 드래그 상태 확인
        // Debug.Log($"📋 InventorySlot.CurrentlyDraggedWeapon: {(InventorySlot.CurrentlyDraggedWeapon != null ? InventorySlot.CurrentlyDraggedWeapon.weaponName : "null")}");
        
        // Debug.Log("🔍 [WeaponSlot] 드래그앤드롭 문제 진단 완료!");
    }
    
    // 🆕 마우스 이벤트 진단 도구 (새로 추가)
    [ContextMenu("Diagnose Mouse Events")]
    public void DiagnoseMouseEvents()
    {
        Debug.Log("🖱️ [WeaponSlot] 마우스 이벤트 진단 시작...");
        
        // 1. 인터페이스 구현 확인
        bool hasPointerClickHandler = this is IPointerClickHandler;
        bool hasPointerEnterHandler = this is IPointerEnterHandler;
        bool hasPointerExitHandler = this is IPointerExitHandler;
        
        Debug.Log($"📋 IPointerClickHandler 구현: {hasPointerClickHandler}");
        Debug.Log($"📋 IPointerEnterHandler 구현: {hasPointerEnterHandler}");
        Debug.Log($"📋 IPointerExitHandler 구현: {hasPointerExitHandler}");
        
        // 2. raycastTarget 설정 확인 (모든 자식 요소 포함)
        Image[] allImages = GetComponentsInChildren<Image>();
        Debug.Log($"📋 총 Image 컴포넌트 개수: {allImages.Length}");
        for (int i = 0; i < allImages.Length; i++)
        {
            Debug.Log($"  - {allImages[i].name}: raycastTarget={allImages[i].raycastTarget}");
        }
        
        Text[] allTexts = GetComponentsInChildren<Text>();
        Debug.Log($"📋 총 Text 컴포넌트 개수: {allTexts.Length}");
        for (int i = 0; i < allTexts.Length; i++)
        {
            Debug.Log($"  - {allTexts[i].name}: raycastTarget={allTexts[i].raycastTarget}");
        }
        
        // 3. EventSystem 확인
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem이 씬에 없습니다!");
        }
        else
        {
            Debug.Log($"✅ EventSystem 확인: {eventSystem.name}");
        }
        
        // 4. Canvas 설정 확인
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"📋 Canvas RenderMode: {canvas.renderMode}");
            Debug.Log($"📋 Canvas sortingOrder: {canvas.sortingOrder}");
            Debug.Log($"📋 Canvas worldCamera: {(canvas.worldCamera != null ? canvas.worldCamera.name : "null")}");
        }
        
        // 5. 충돌 가능한 요소들 확인
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>();
        Debug.Log($"📋 3D Collider 개수: {colliders.Length}");
        Debug.Log($"📋 2D Collider 개수: {colliders2D.Length}");
        
        if (colliders.Length > 0 || colliders2D.Length > 0)
        {
            Debug.LogWarning("⚠️ WeaponSlot에 Collider가 있습니다! UI 이벤트와 충돌할 수 있습니다.");
        }
        
        // 6. 테스트용 마우스 이벤트 시뮬레이션
        Debug.Log("🧪 마우스 이벤트 테스트를 위해 가상 이벤트를 발생시킵니다...");
        
        // 가상 PointerEventData 생성
        PointerEventData fakeEventData = new PointerEventData(EventSystem.current);
        fakeEventData.button = PointerEventData.InputButton.Right;
        fakeEventData.position = Input.mousePosition;
        
        Debug.Log("🖱️ OnPointerClick 직접 호출 테스트...");
        OnPointerClick(fakeEventData);
        
        Debug.Log("🖱️ [WeaponSlot] 마우스 이벤트 진단 완료!");
    }
    
    // 🆕 UI 설정 자동 복구 도구
    [ContextMenu("Fix Mouse Event Issues")]
    public void FixMouseEventIssues()
    {
        Debug.Log("🔧 [WeaponSlot] 마우스 이벤트 문제 자동 복구 시작...");
        
        // 1. raycastTarget 설정 수정
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            Debug.Log("✅ backgroundImage.raycastTarget = true 설정");
        }
        
        if (icon != null)
        {
            icon.raycastTarget = false;
            Debug.Log("✅ icon.raycastTarget = false 설정");
        }
        
        if (weaponNameText != null)
        {
            weaponNameText.raycastTarget = false;
            Debug.Log("✅ weaponNameText.raycastTarget = false 설정");
        }
        
        if (ammoText != null)
        {
            ammoText.raycastTarget = false;
            Debug.Log("✅ ammoText.raycastTarget = false 설정");
        }
        
        // 2. Canvas/GraphicRaycaster 확인 및 추가
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ GraphicRaycaster 추가");
            }
        }
        
        // 3. EventSystem 확인 및 추가
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ EventSystem 추가");
        }
        
        Debug.Log("🔧 [WeaponSlot] 마우스 이벤트 문제 자동 복구 완료!");
    }

    [ContextMenu("Test WeaponSlot Mouse Events")]
    public void TestWeaponSlotMouseEvents()
    {
        Debug.Log($"🧪 [WeaponSlot] 마우스 이벤트 테스트 시작 - {gameObject.name}");
        
        // 1. EventSystem 상태 확인
        EventSystem eventSystem = EventSystem.current;
        Debug.Log($"EventSystem.current: {(eventSystem != null ? "✅ 존재" : "❌ null")}");
        
        if (eventSystem != null)
        {
            Debug.Log($"EventSystem GameObject: {eventSystem.gameObject.name}");
            Debug.Log($"EventSystem enabled: {eventSystem.enabled}");
            Debug.Log($"Current Input Module: {(eventSystem.currentInputModule != null ? eventSystem.currentInputModule.GetType().Name : "❌ null")}");
        }
        
        // 2. Canvas 및 GraphicRaycaster 확인
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"Canvas: {canvas.name}, GraphicRaycaster: {(raycaster != null ? "✅ 존재" : "❌ 없음")}");
        }
        else
        {
            Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
        }
        
        // 3. WeaponSlot 컴포넌트 상태 확인
        Debug.Log($"WeaponSlot enabled: {this.enabled}");
        Debug.Log($"GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"backgroundImage: {(backgroundImage != null ? "✅ 존재" : "❌ null")}");
        
        if (backgroundImage != null)
        {
            Debug.Log($"backgroundImage.raycastTarget: {backgroundImage.raycastTarget}");
            Debug.Log($"backgroundImage.enabled: {backgroundImage.enabled}");
        }
        
        // 4. 인터페이스 구현 확인
        Debug.Log($"IPointerClickHandler: {(this is IPointerClickHandler ? "✅ 구현" : "❌ 미구현")}");
        Debug.Log($"IPointerEnterHandler: {(this is IPointerEnterHandler ? "✅ 구현" : "❌ 미구현")}");
        Debug.Log($"IPointerExitHandler: {(this is IPointerExitHandler ? "✅ 구현" : "❌ 미구현")}");
        
        // 5. 레이캐스트 시뮬레이션 테스트
        TestRaycast();
        
        Debug.Log($"🧪 [WeaponSlot] 마우스 이벤트 테스트 완료 - {gameObject.name}");
    }
    
    void TestRaycast()
    {
        Debug.Log("🎯 [WeaponSlot] 레이캐스트 시뮬레이션 테스트...");
        
        // 현재 마우스 위치에서 레이캐스트 테스트
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) 
        {
            Debug.LogError("❌ EventSystem.current가 null입니다!");
            return;
        }
        
        Debug.Log($"✅ EventSystem: {eventSystem.gameObject.name}");
        Debug.Log($"✅ Input Module: {(eventSystem.currentInputModule != null ? eventSystem.currentInputModule.GetType().Name : "❌ null")}");
        
        // 마우스 위치 확인
        Vector2 mousePosition = Input.mousePosition;
        Debug.Log($"🖱️ 마우스 위치: {mousePosition}");
        
        // 스크린 크기 확인
        Debug.Log($"📺 스크린 크기: {Screen.width}x{Screen.height}");
        
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = mousePosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        
        // 🆕 모든 GraphicRaycaster 수동 확인
        GraphicRaycaster[] allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        Debug.Log($"🔍 발견된 GraphicRaycaster 수: {allRaycasters.Length}");
        
        foreach (var raycaster in allRaycasters)
        {
            Debug.Log($"  - {raycaster.gameObject.name} (활성화: {raycaster.enabled}, Canvas: {raycaster.GetComponent<Canvas>()?.name})");
            
            // 각 레이캐스터별로 개별 테스트
            var individualResults = new System.Collections.Generic.List<RaycastResult>();
            raycaster.Raycast(pointerData, individualResults);
            Debug.Log($"    → 개별 레이캐스트 결과: {individualResults.Count}개");
            
            foreach (var result in individualResults)
            {
                Debug.Log($"      * {result.gameObject.name} (거리: {result.distance})");
            }
        }
        
        // 전체 레이캐스트 실행
        eventSystem.RaycastAll(pointerData, raycastResults);
        Debug.Log($"레이캐스트 결과 수: {raycastResults.Count}");
        
        if (raycastResults.Count == 0)
        {
            Debug.LogWarning("⚠️ 레이캐스트 결과가 0개입니다. 추가 진단을 실행합니다...");
            DiagnoseRaycastIssues();
        }
        
        bool foundThisWeaponSlot = false;
        foreach (var result in raycastResults)
        {
            Debug.Log($"- {result.gameObject.name} (거리: {result.distance})");
            
            if (result.gameObject == gameObject)
            {
                foundThisWeaponSlot = true;
                Debug.Log($"✅ 현재 WeaponSlot이 레이캐스트에서 감지됨!");
            }
        }
        
        if (!foundThisWeaponSlot && raycastResults.Count > 0)
        {
            Debug.LogWarning($"⚠️ 현재 WeaponSlot이 레이캐스트에서 감지되지 않음. 다른 UI가 가리고 있을 수 있음.");
        }
    }
    
    // 🆕 레이캐스트 문제 세부 진단
    void DiagnoseRaycastIssues()
    {
        Debug.Log("🔬 [WeaponSlot] 레이캐스트 문제 세부 진단...");
        
        // 1. Canvas 관련 진단
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("❌ 부모 Canvas를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"✅ 부모 Canvas: {parentCanvas.name}");
        Debug.Log($"  - Canvas.enabled: {parentCanvas.enabled}");
        Debug.Log($"  - Canvas.gameObject.activeInHierarchy: {parentCanvas.gameObject.activeInHierarchy}");
        Debug.Log($"  - Canvas.renderMode: {parentCanvas.renderMode}");
        Debug.Log($"  - Canvas.sortingOrder: {parentCanvas.sortingOrder}");
        
        // 2. GraphicRaycaster 진단
        GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("❌ Canvas에 GraphicRaycaster가 없습니다!");
            raycaster = parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("✅ GraphicRaycaster를 자동으로 추가했습니다.");
        }
        else
        {
            Debug.Log($"✅ GraphicRaycaster: {raycaster.name}");
            Debug.Log($"  - GraphicRaycaster.enabled: {raycaster.enabled}");
        }
        
        // 3. WeaponSlot GameObject 진단
        Debug.Log($"📦 WeaponSlot GameObject 상태:");
        Debug.Log($"  - gameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"  - gameObject.activeSelf: {gameObject.activeSelf}");
        Debug.Log($"  - transform.position: {transform.position}");
        
        // 4. RectTransform 진단
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"📐 RectTransform 상태:");
            Debug.Log($"  - sizeDelta: {rectTransform.sizeDelta}");
            Debug.Log($"  - anchoredPosition: {rectTransform.anchoredPosition}");
            Debug.Log($"  - rect: {rectTransform.rect}");
            
            // 월드 포지션을 스크린 포지션으로 변환
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            Debug.Log($"  - 스크린 포지션: {screenPos}");
        }
        
        // 5. Image 컴포넌트 진단
        if (backgroundImage != null)
        {
            Debug.Log($"🖼️ backgroundImage 상태:");
            Debug.Log($"  - enabled: {backgroundImage.enabled}");
            Debug.Log($"  - raycastTarget: {backgroundImage.raycastTarget}");
            Debug.Log($"  - color.a (투명도): {backgroundImage.color.a}");
            Debug.Log($"  - sprite: {(backgroundImage.sprite != null ? backgroundImage.sprite.name : "null")}");
        }
        else
        {
            Debug.LogError("❌ backgroundImage가 null입니다!");
        }
        
        // 6. 마우스 위치가 WeaponSlot 영역 안에 있는지 확인
        CheckMouseOverWeaponSlot();
    }
    
    // 🆕 마우스가 WeaponSlot 위에 있는지 확인
    void CheckMouseOverWeaponSlot()
    {
        Debug.Log("🖱️ [WeaponSlot] 마우스 위치 vs WeaponSlot 영역 확인...");
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector2 mousePosition = Input.mousePosition;
        
        // 마우스 위치를 로컬 좌표로 변환
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, mousePosition, null, out localMousePosition))
        {
            Debug.Log($"🖱️ 로컬 마우스 위치: {localMousePosition}");
            Debug.Log($"📐 WeaponSlot Rect: {rectTransform.rect}");
            
            bool isInside = rectTransform.rect.Contains(localMousePosition);
            Debug.Log($"🎯 마우스가 WeaponSlot 안에 있음: {(isInside ? "✅ 예" : "❌ 아니오")}");
            
            if (!isInside)
            {
                Debug.LogWarning("⚠️ 마우스가 WeaponSlot 영역 밖에 있습니다! WeaponSlot 위에서 테스트하세요.");
            }
        }
        else
        {
            Debug.LogError("❌ 마우스 위치를 로컬 좌표로 변환할 수 없습니다!");
        }
    }

    // 🛠️ UI 컴포넌트 자동 설정
    [ContextMenu("Setup WeaponSlot UI")]
    void SetupUIComponents()
    {
        // 1. 배경 Image 확인/생성
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            
            // 기본 배경 설정
            backgroundImage.color = normalColor;
        }
        
        // 🚨 드래그앤드롭을 위한 중요한 설정: raycastTarget 활성화
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            Debug.Log($"✅ [WeaponSlot] backgroundImage.raycastTarget = true 설정 완료 - {gameObject.name}");
        }
        else
        {
            Debug.LogError($"❌ [WeaponSlot] backgroundImage가 여전히 null입니다! - {gameObject.name}");
        }
        
        // 2. 아이콘 Image 확인/생성
        if (icon == null)
        {
            // 기존 Icon 찾기
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                icon = iconTransform.GetComponent<Image>();
            }
            else
            {
                // 새 Icon 생성
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform, false);
                
                // RectTransform 설정
                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(5, 5);    // 5픽셀 여백
                iconRect.offsetMax = new Vector2(-5, -5);  // 5픽셀 여백
                
                // Image 컴포넌트 추가
                icon = iconObj.AddComponent<Image>();
                icon.preserveAspect = true;
                icon.enabled = false; // 처음에는 비활성화
            }
        }
        
        // 🚨 Icon은 레이캐스트를 방해하지 않도록 설정
        if (icon != null)
        {
            icon.raycastTarget = false;
        }
        
        // 3. 무기 이름 Text 확인/생성
        if (weaponNameText == null)
        {
            // 기존 WeaponName 텍스트 찾기
            Transform nameTransform = transform.Find("WeaponName");
            if (nameTransform != null)
            {
                weaponNameText = nameTransform.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                // 새 WeaponName 텍스트 생성
                GameObject nameObj = new GameObject("WeaponName");
                nameObj.transform.SetParent(transform, false);
                
                // RectTransform 설정 (하단)
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0.3f);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;
                
                // Text 컴포넌트 추가
                weaponNameText = nameObj.AddComponent<UnityEngine.UI.Text>();
                weaponNameText.text = "Empty";
                weaponNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                weaponNameText.fontSize = 12;
                weaponNameText.alignment = TextAnchor.MiddleCenter;
                weaponNameText.color = Color.white;
            }
        }
        
        // 🚨 Text도 레이캐스트를 방해하지 않도록 설정
        if (weaponNameText != null)
        {
            weaponNameText.raycastTarget = false;
        }
        
        // 4. 탄약 Text 확인/생성 (선택사항)
        if (ammoText == null)
        {
            // 기존 Ammo 텍스트 찾기
            Transform ammoTransform = transform.Find("AmmoText");
            if (ammoTransform != null)
            {
                ammoText = ammoTransform.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                // 새 Ammo 텍스트 생성
                GameObject ammoObj = new GameObject("AmmoText");
                ammoObj.transform.SetParent(transform, false);
                
                // RectTransform 설정 (우상단)
                RectTransform ammoRect = ammoObj.AddComponent<RectTransform>();
                ammoRect.anchorMin = new Vector2(0.7f, 0.7f);
                ammoRect.anchorMax = new Vector2(1, 1);
                ammoRect.offsetMin = Vector2.zero;
                ammoRect.offsetMax = Vector2.zero;
                
                // Text 컴포넌트 추가
                ammoText = ammoObj.AddComponent<UnityEngine.UI.Text>();
                ammoText.text = "";
                ammoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                ammoText.fontSize = 10;
                ammoText.alignment = TextAnchor.MiddleCenter;
                ammoText.color = Color.yellow;
            }
        }
        
        // 🚨 Ammo Text도 레이캐스트를 방해하지 않도록 설정
        if (ammoText != null)
        {
            ammoText.raycastTarget = false;
        }
        
        // 5. Canvas와 GraphicRaycaster 확인
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ [WeaponSlot] Canvas를 찾을 수 없습니다! WeaponSlot이 Canvas 하위에 있어야 합니다.");
        }
        else
        {
            UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning("⚠️ [WeaponSlot] Canvas에 GraphicRaycaster가 없습니다! 자동으로 추가합니다.");
                raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
    }

    // 🆕 Canvas 레이어 및 렌더링 문제 진단
    [ContextMenu("Diagnose Canvas Layer Issues")]
    void DiagnoseCanvasLayerIssues()
    {
        Debug.Log("🎨 [WeaponSlot] Canvas 레이어 및 렌더링 문제 진단...");
        
        // 1. WeaponSlot의 Canvas 구조 분석
        Canvas weaponSlotCanvas = GetComponentInParent<Canvas>();
        if (weaponSlotCanvas == null)
        {
            Debug.LogError("❌ WeaponSlot의 부모 Canvas를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"🎨 WeaponSlot Canvas: {weaponSlotCanvas.name}");
        Debug.Log($"  - renderMode: {weaponSlotCanvas.renderMode}");
        Debug.Log($"  - sortingOrder: {weaponSlotCanvas.sortingOrder}");
        Debug.Log($"  - sortingLayerName: {weaponSlotCanvas.sortingLayerName}");
        Debug.Log($"  - overrideSorting: {weaponSlotCanvas.overrideSorting}");
        
        // 2. 인벤토리 슬롯의 Canvas와 비교
        InventorySlot[] inventorySlots = FindObjectsByType<InventorySlot>(FindObjectsSortMode.None);
        if (inventorySlots.Length > 0)
        {
            Canvas inventoryCanvas = inventorySlots[0].GetComponentInParent<Canvas>();
            if (inventoryCanvas != null)
            {
                Debug.Log($"📦 InventorySlot Canvas: {inventoryCanvas.name}");
                Debug.Log($"  - renderMode: {inventoryCanvas.renderMode}");
                Debug.Log($"  - sortingOrder: {inventoryCanvas.sortingOrder}");
                Debug.Log($"  - sortingLayerName: {inventoryCanvas.sortingLayerName}");
                Debug.Log($"  - overrideSorting: {inventoryCanvas.overrideSorting}");
                
                // Canvas 설정 비교
                if (weaponSlotCanvas.sortingOrder != inventoryCanvas.sortingOrder)
                {
                    Debug.LogWarning($"⚠️ Canvas sortingOrder가 다릅니다! WeaponSlot: {weaponSlotCanvas.sortingOrder}, Inventory: {inventoryCanvas.sortingOrder}");
                }
                
                if (weaponSlotCanvas.renderMode != inventoryCanvas.renderMode)
                {
                    Debug.LogWarning($"⚠️ Canvas renderMode가 다릅니다! WeaponSlot: {weaponSlotCanvas.renderMode}, Inventory: {inventoryCanvas.renderMode}");
                }
            }
        }
        
        // 3. CanvasGroup 확인 (투명도 문제)
        CanvasGroup[] canvasGroups = GetComponentsInParent<CanvasGroup>();
        Debug.Log($"🔍 WeaponSlot의 부모 CanvasGroup 수: {canvasGroups.Length}");
        
        float totalAlpha = 1f;
        bool blocksRaycasts = true;
        
        foreach (var canvasGroup in canvasGroups)
        {
            Debug.Log($"  - {canvasGroup.gameObject.name}: alpha={canvasGroup.alpha}, blocksRaycasts={canvasGroup.blocksRaycasts}, interactable={canvasGroup.interactable}");
            totalAlpha *= canvasGroup.alpha;
            
            if (!canvasGroup.blocksRaycasts)
            {
                blocksRaycasts = false;
            }
        }
        
        Debug.Log($"📊 총 Alpha 값: {totalAlpha}");
        Debug.Log($"📊 레이캐스트 차단: {blocksRaycasts}");
        
        if (totalAlpha < 0.9f)
        {
            Debug.LogWarning($"⚠️ 총 Alpha 값이 {totalAlpha}으로 낮습니다! 이것이 색상이 어둡게 보이는 원인일 수 있습니다.");
        }
        
        if (!blocksRaycasts)
        {
            Debug.LogError($"❌ CanvasGroup이 레이캐스트를 차단하고 있습니다! 마우스 이벤트가 작동하지 않을 수 있습니다.");
        }
        
        // 4. 마우스 위치에서 모든 UI 요소 확인
        CheckUIElementsAtMousePosition();
        
        // 5. WeaponSlot의 시각적 레이어 순서 확인
        CheckVisualLayerOrder();
    }
    
    // 🆕 마우스 위치의 모든 UI 요소 확인
    void CheckUIElementsAtMousePosition()
    {
        Debug.Log("🖱️ [WeaponSlot] 마우스 위치의 모든 UI 요소 확인...");
        
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;
        
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, raycastResults);
        
        Debug.Log($"🎯 마우스 위치의 UI 요소들 (총 {raycastResults.Count}개):");
        
        for (int i = 0; i < raycastResults.Count; i++)
        {
            var result = raycastResults[i];
            string layerInfo = $"레이어 {i+1}";
            
            if (result.gameObject == gameObject)
            {
                layerInfo += " ⭐ (이것이 WeaponSlot!)";
            }
            
            Debug.Log($"  {layerInfo}: {result.gameObject.name} (거리: {result.distance})");
            
            // 각 요소의 Canvas 정보
            Canvas canvas = result.gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"    → Canvas: {canvas.name}, sortingOrder: {canvas.sortingOrder}");
            }
            
            // Image 컴포넌트의 raycastTarget 확인
            Image img = result.gameObject.GetComponent<Image>();
            if (img != null)
            {
                Debug.Log($"    → Image raycastTarget: {img.raycastTarget}, alpha: {img.color.a}");
            }
        }
        
        // WeaponSlot이 첫 번째가 아니라면 경고
        if (raycastResults.Count > 0 && raycastResults[0].gameObject != gameObject)
        {
            Debug.LogWarning($"⚠️ 마우스 위치에서 WeaponSlot이 첫 번째 요소가 아닙니다! '{raycastResults[0].gameObject.name}'이 WeaponSlot을 덮고 있습니다.");
        }
    }
    
    // 🆕 시각적 레이어 순서 확인
    void CheckVisualLayerOrder()
    {
        Debug.Log("📋 [WeaponSlot] 시각적 레이어 순서 확인...");
        
        // WeaponSlot의 sibling index 확인
        int siblingIndex = transform.GetSiblingIndex();
        Debug.Log($"📍 WeaponSlot sibling index: {siblingIndex}");
        
        // 같은 부모의 다른 자식들 확인
        Transform parent = transform.parent;
        if (parent != null)
        {
            Debug.Log($"📂 부모: {parent.name}, 총 자식 수: {parent.childCount}");
            
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string info = $"  {i}: {child.name}";
                
                if (child == transform)
                {
                    info += " ⭐ (WeaponSlot)";
                }
                
                // 활성화 상태 확인
                if (!child.gameObject.activeInHierarchy)
                {
                    info += " (비활성화)";
                }
                
                Debug.Log(info);
            }
            
            // WeaponSlot이 마지막이 아니라면 (다른 요소가 위에 있다면) 경고
            if (siblingIndex < parent.childCount - 1)
            {
                Debug.LogWarning($"⚠️ WeaponSlot 위에 {parent.childCount - 1 - siblingIndex}개의 UI 요소가 있습니다! 이들이 마우스 이벤트를 가로챌 수 있습니다.");
            }
        }
    }

    // 🆕 UI 레이어 충돌 문제 자동 해결
    [ContextMenu("Fix UI Layer Conflicts")]
    void FixUILayerConflicts()
    {
        Debug.Log("🔧 [WeaponSlot] UI 레이어 충돌 문제 자동 해결 시작...");
        
        // 마우스 위치에서 WeaponSlot을 덮고 있는 UI 요소들 찾기
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;
        
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, raycastResults);
        
        bool weaponSlotFound = false;
        int weaponSlotIndex = -1;
        
        // WeaponSlot의 위치 찾기
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject == gameObject)
            {
                weaponSlotFound = true;
                weaponSlotIndex = i;
                break;
            }
        }
        
        if (!weaponSlotFound)
        {
            Debug.LogWarning("⚠️ WeaponSlot이 마우스 위치에서 감지되지 않습니다!");
            return;
        }
        
        if (weaponSlotIndex == 0)
        {
            Debug.Log("✅ WeaponSlot이 이미 최상위에 있습니다!");
            return;
        }
        
        // WeaponSlot을 덮고 있는 UI 요소들 처리
        Debug.Log($"🔧 WeaponSlot을 덮고 있는 {weaponSlotIndex}개의 UI 요소를 처리합니다...");
        
        for (int i = 0; i < weaponSlotIndex; i++)
        {
            var blockingUI = raycastResults[i].gameObject;
            Debug.Log($"  {i + 1}. {blockingUI.name} 처리 중...");
            
            // 해결 방법 1: 특정 UI 요소들의 raycastTarget 비활성화
            if (blockingUI.name.Contains("InventoryPanel") || 
                blockingUI.name.Contains("Panel") ||
                blockingUI.name.Contains("Background"))
            {
                Image img = blockingUI.GetComponent<Image>();
                if (img != null && img.raycastTarget)
                {
                    img.raycastTarget = false;
                    Debug.Log($"    ✅ {blockingUI.name}의 raycastTarget을 false로 설정");
                }
            }
            
            // 해결 방법 2: WeaponSlot 영역에서만 raycastTarget 비활성화
            else if (IsOverlappingWithWeaponSlot(blockingUI))
            {
                // WeaponSlot과 겹치는 영역에 있는 UI 요소 처리
                ProcessOverlappingUI(blockingUI);
            }
        }
        
        // 해결 후 재테스트
        Debug.Log("🔄 해결 후 재테스트...");
        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
            CheckUIElementsAtMousePosition();
        });
        
        Debug.Log("✅ UI 레이어 충돌 문제 해결 완료!");
    }
    
    // 🆕 UI 요소가 WeaponSlot과 겹치는지 확인
    bool IsOverlappingWithWeaponSlot(GameObject uiElement)
    {
        RectTransform weaponSlotRect = GetComponent<RectTransform>();
        RectTransform uiRect = uiElement.GetComponent<RectTransform>();
        
        if (weaponSlotRect == null || uiRect == null) return false;
        
        // 두 RectTransform이 겹치는지 확인
        Rect weaponSlotWorldRect = GetWorldRect(weaponSlotRect);
        Rect uiWorldRect = GetWorldRect(uiRect);
        
        return weaponSlotWorldRect.Overlaps(uiWorldRect);
    }
    
    // 🆕 RectTransform의 월드 좌표 Rect 계산
    Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        float xMin = corners[0].x;
        float xMax = corners[2].x;
        float yMin = corners[0].y;
        float yMax = corners[2].y;
        
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
    
    // 🆕 겹치는 UI 요소 처리
    void ProcessOverlappingUI(GameObject overlappingUI)
    {
        Debug.Log($"🔧 겹치는 UI 요소 처리: {overlappingUI.name}");
        
        // 1. raycastTarget 비활성화 시도
        Image img = overlappingUI.GetComponent<Image>();
        if (img != null)
        {
            // 투명하거나 거의 투명한 이미지의 raycastTarget 비활성화
            if (img.color.a < 0.1f)
            {
                img.raycastTarget = false;
                Debug.Log($"    ✅ 투명한 이미지 {overlappingUI.name}의 raycastTarget 비활성화");
                return;
            }
        }
        
        // 2. 특별한 컴포넌트가 없는 빈 UI 오브젝트의 경우
        if (overlappingUI.GetComponents<Component>().Length <= 2) // Transform + RectTransform만 있는 경우
        {
            if (img != null)
            {
                img.raycastTarget = false;
                Debug.Log($"    ✅ 빈 UI 오브젝트 {overlappingUI.name}의 raycastTarget 비활성화");
            }
        }
        
        // 3. WeaponSlot보다 뒤로 이동 (마지막 수단)
        else
        {
            Debug.LogWarning($"    ⚠️ {overlappingUI.name}을 처리할 수 없습니다. 수동으로 확인이 필요합니다.");
        }
    }
}