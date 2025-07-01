using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("📋 무기 슬롯 사용법")]
    [TextArea(3, 5)]
    public string weaponSlotInstructions = "• 인벤토리에서 무기를 드래그하여 장착\n• 우클릭으로 무기 해제\n• 마우스 호버로 무기 정보 툴팁 표시\n• 무기 장착 시 녹색으로 표시\n• PlayerInventory와 자동 연동";

    [Header("🖼️ Slot Components")]
    [Tooltip("장착된 무기 아이콘을 표시할 Image")]
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
    
    [Header("🔗 References")]
    [Tooltip("플레이어 인벤토리 컴포넌트 (자동 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 컴포넌트 (자동 연결됨)")]
    public InventoryManager inventoryManager;
    
    // Properties
    public WeaponData weaponData { get; private set; }
    private bool isHovered = false;

    void Start()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        // 🔧 UI 컴포넌트 자동 생성 및 설정
        SetupUIComponents();
        
        UpdateVisuals();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 새로운 드래그앤드롭 시스템과 호환 - 간단한 방법
        WeaponData draggedWeapon = InventorySlot.CurrentlyDraggedWeapon;
        
        if (draggedWeapon != null)
        {
            EquipWeapon(draggedWeapon);
            Debug.Log($"🎯 [WeaponSlot] 드롭으로 무기 장착: {draggedWeapon.weaponName}");
        }
        else
        {
            Debug.LogWarning("⚠️ [WeaponSlot] 드래그된 무기 데이터가 없습니다!");
        }
    }
    
    // 마우스 이벤트 핸들러들
    public void OnPointerEnter(PointerEventData eventData)
    {
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
        if (eventData.button == PointerEventData.InputButton.Right && weaponData != null)
        {
            // 우클릭으로 무기 해제
            UnequipWeapon();
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
        
        // 플레이어 인벤토리에 장착 무기 설정
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(weaponData);
        
        // 🔧 인벤토리에서 장착된 무기 제거 (RefreshInventory 호출 안함)
        if (weaponData != null && inventoryManager != null)
        {
            inventoryManager.RemoveWeapon(weaponData, false); // 새로고침 없이 제거만
        }
        
        // 🏃‍♂️ 플레이어 이동속도 업데이트
        UpdatePlayerMovementSpeed();
        
        UpdateVisuals();
        
        Debug.Log($"✅ [WeaponSlot] 무기 장착 완료: {(weaponData != null ? weaponData.weaponName : "None")}");
    }

    // 🔧 새로운 메서드: 무기 해제 (우클릭용)
    public void UnequipWeapon()
    {
        if (weaponData == null) return;
        
        WeaponData oldWeapon = weaponData;
        
        // 무기 해제
        weaponData = null;
        
        // 플레이어 인벤토리에서 장착 무기 해제
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(null);
        
        // 🏃‍♂️ 플레이어 이동속도 복원 (무기 없음)
        UpdatePlayerMovementSpeed();
        
        // 인벤토리에 무기 다시 추가 및 UI 업데이트
        ReturnWeaponToInventory(oldWeapon);
        
        UpdateVisuals();
        
        Debug.Log($"🔓 [WeaponSlot] 무기 해제 완료: {oldWeapon.weaponName}");
    }

    public void ClearSlot()
    {
        if (weaponData != null)
        {
            // 무기 해제 시 인벤토리에 다시 추가
            ReturnWeaponToInventory(weaponData);
        }
        
        WeaponData oldWeapon = weaponData;
        weaponData = null;
        
        // 플레이어 인벤토리에서 장착 무기 해제
        if (playerInventory != null)
            playerInventory.SetEquippedWeapon(null);
        
        UpdateVisuals();
        
        Debug.Log($"🗑️ [WeaponSlot] 슬롯 클리어: {(oldWeapon != null ? oldWeapon.weaponName : "None")}");
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
        
        Debug.Log($"📥 [WeaponSlot] 무기가 인벤토리로 반환됨: {weapon.weaponName}");
    }
    
    void UpdateVisuals()
    {
        Debug.Log($"🔧 [WeaponSlot] UpdateVisuals 호출됨 - weaponData: {(weaponData != null ? weaponData.weaponName : "null")}");
        
        // 🧪 컴포넌트 상태 진단
        DiagnoseComponents();
        
        // 아이콘 업데이트
        if (icon != null)
        {
            if (weaponData != null)
            {
                Debug.Log($"📝 [WeaponSlot] 무기 데이터 확인: {weaponData.weaponName}, icon: {(weaponData.icon != null ? weaponData.icon.name : "null")}");
                
                if (weaponData.icon != null)
                {
                    icon.sprite = weaponData.icon;
                    icon.enabled = true;
                    icon.color = Color.white;
                    
                    // 🔧 강제 새로고침
                    icon.gameObject.SetActive(false);
                    icon.gameObject.SetActive(true);
                    
                    Debug.Log($"✅ [WeaponSlot] 아이콘 설정 완료: {weaponData.icon.name}, enabled: {icon.enabled}, color: {icon.color}");
                }
                else
                {
                    Debug.LogError($"❌ [WeaponSlot] WeaponData '{weaponData.weaponName}'의 icon이 null입니다!");
                    // 아이콘이 없어도 빈 이미지라도 표시
                    icon.sprite = null;
                    icon.enabled = true;
                    icon.color = Color.red; // 빨간색으로 표시하여 문제 있음을 알림
                }
            }
            else
            {
                icon.sprite = null;
                icon.enabled = false;
                Debug.Log("🗑️ [WeaponSlot] 무기 없음 - 아이콘 비활성화");
            }
        }
        else
        {
            Debug.LogError("❌ [WeaponSlot] icon Image 컴포넌트가 null입니다! Inspector에서 연결해주세요!");
        }
        
        // 무기 이름 표시
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponData != null ? weaponData.weaponName : "Empty";
            Debug.Log($"📝 [WeaponSlot] 무기 이름 설정: {weaponNameText.text}");
        }
        else
        {
            Debug.LogWarning("⚠️ [WeaponSlot] weaponNameText가 null입니다!");
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
            Debug.Log($"🔢 [WeaponSlot] 탄약 정보 설정: {ammoText.text}");
        }
        
        // 배경 색상 업데이트
        if (backgroundImage != null)
        {
            Color targetColor = normalColor;
            
            if (weaponData != null)
            {
                targetColor = equippedColor;
            }
            else if (isHovered)
            {
                targetColor = hoverColor;
            }
            
            backgroundImage.color = targetColor;
            Debug.Log($"🎨 [WeaponSlot] 배경 색상 설정: {targetColor}");
        }
        
        // 🔧 최종 상태 요약
        Debug.Log($"📊 [WeaponSlot] 최종 상태 - 무기: {(weaponData != null ? weaponData.weaponName : "없음")}, 아이콘 활성화: {(icon != null ? icon.enabled.ToString() : "icon이 null")}, 스프라이트: {(icon?.sprite != null ? icon.sprite.name : "없음")}");
    }
    
    // 🧪 새로운 메서드: 컴포넌트 상태 진단
    void DiagnoseComponents()
    {
        Debug.Log("🧪 [WeaponSlot] 컴포넌트 진단 시작...");
        
        // Icon Image 진단
        if (icon == null)
        {
            Debug.LogError("❌ [WeaponSlot] icon Image가 연결되지 않았습니다!");
            
            // 자동으로 Icon 컴포넌트 찾기 시도
            Image[] images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("icon") || img.name.ToLower().Contains("weapon"))
                {
                    icon = img;
                    Debug.Log($"🔧 [WeaponSlot] 자동으로 아이콘 찾음: {img.name}");
                    break;
                }
            }
            
            // 그래도 없으면 첫 번째 Image 사용
            if (icon == null && images.Length > 0)
            {
                icon = images[0];
                Debug.Log($"🔧 [WeaponSlot] 첫 번째 Image를 아이콘으로 사용: {icon.name}");
            }
        }
        else
        {
            Debug.Log($"✅ [WeaponSlot] icon Image 연결됨: {icon.name}");
            
            // Icon의 상태 확인
            Debug.Log($"   - GameObject 활성화: {icon.gameObject.activeSelf}");
            Debug.Log($"   - Component 활성화: {icon.enabled}");
            Debug.Log($"   - 현재 스프라이트: {(icon.sprite != null ? icon.sprite.name : "null")}");
            Debug.Log($"   - 색상: {icon.color}");
            Debug.Log($"   - RaycastTarget: {icon.raycastTarget}");
        }
        
        // Background Image 진단
        if (backgroundImage == null)
        {
            Debug.LogWarning("⚠️ [WeaponSlot] backgroundImage가 연결되지 않았습니다!");
        }
        else
        {
            Debug.Log($"✅ [WeaponSlot] backgroundImage 연결됨: {backgroundImage.name}");
        }
        
        // Text 컴포넌트들 진단
        if (weaponNameText == null)
        {
            Debug.LogWarning("⚠️ [WeaponSlot] weaponNameText가 연결되지 않았습니다!");
        }
        
        if (ammoText == null)
        {
            Debug.LogWarning("⚠️ [WeaponSlot] ammoText가 연결되지 않았습니다!");
        }
        
        Debug.Log("🧪 [WeaponSlot] 컴포넌트 진단 완료");
    }

    // 🛠️ 새로운 메서드: UI 컴포넌트 자동 설정
    [ContextMenu("Setup WeaponSlot UI")]
    void SetupUIComponents()
    {
        Debug.Log("🛠️ [WeaponSlot] UI 컴포넌트 자동 설정 시작...");
        
        // 1. 배경 Image 확인/생성
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
                Debug.Log("✅ [WeaponSlot] 배경 Image 컴포넌트 추가됨");
            }
            
            // 기본 배경 설정
            backgroundImage.color = normalColor;
            Debug.Log("✅ [WeaponSlot] 배경 Image 연결됨");
        }
        
        // 2. 아이콘 Image 확인/생성
        if (icon == null)
        {
            // 기존 Icon 찾기
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                icon = iconTransform.GetComponent<Image>();
                Debug.Log("✅ [WeaponSlot] 기존 Icon 찾음");
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
                
                Debug.Log("✅ [WeaponSlot] 새 Icon 생성됨");
            }
        }
        
        // 3. 무기 이름 Text 확인/생성
        if (weaponNameText == null)
        {
            // 기존 WeaponName 텍스트 찾기
            Transform nameTransform = transform.Find("WeaponName");
            if (nameTransform != null)
            {
                weaponNameText = nameTransform.GetComponent<UnityEngine.UI.Text>();
                Debug.Log("✅ [WeaponSlot] 기존 WeaponName 텍스트 찾음");
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
                
                Debug.Log("✅ [WeaponSlot] 새 WeaponName 텍스트 생성됨");
            }
        }
        
        // 4. 탄약 Text 확인/생성 (선택사항)
        if (ammoText == null)
        {
            // 기존 Ammo 텍스트 찾기
            Transform ammoTransform = transform.Find("AmmoText");
            if (ammoTransform != null)
            {
                ammoText = ammoTransform.GetComponent<UnityEngine.UI.Text>();
                Debug.Log("✅ [WeaponSlot] 기존 AmmoText 찾음");
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
                
                Debug.Log("✅ [WeaponSlot] 새 AmmoText 생성됨");
            }
        }
        
        // 5. RectTransform 크기 보정
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform.sizeDelta.x < 80 || rectTransform.sizeDelta.y < 80)
        {
            rectTransform.sizeDelta = new Vector2(80, 80);
            Debug.Log("✅ [WeaponSlot] 크기 조정됨: 80x80");
        }
        
        Debug.Log("🎯 [WeaponSlot] UI 컴포넌트 자동 설정 완료!");
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
}