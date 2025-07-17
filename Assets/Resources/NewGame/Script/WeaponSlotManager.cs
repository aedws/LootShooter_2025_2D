using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class WeaponSlotManager : MonoBehaviour
{
    [Header("📋 무기 슬롯 매니저 사용법")]
    [TextArea(4, 6)]
    public string instructions = "🎯 주요 기능:\n• 3개의 무기 슬롯 관리\n• Tab키로 무기 교체 (1→2→3→1)\n• 현재 활성 슬롯 시각적 표시\n• 각 슬롯에 개별 무기 장착 가능\n• PlayerInventory와 자동 연동\n\n⚙️ 설정: weaponSlots 배열에 3개의 WeaponSlot 연결";

    [Header("🔫 Weapon Slots")]
    [Tooltip("3개의 무기 슬롯 (슬롯 1, 2, 3)")]
    public WeaponSlot[] weaponSlots = new WeaponSlot[3];
    
    [Tooltip("현재 활성화된 슬롯 인덱스 (0, 1, 2)")]
    [Range(0, 2)]
    public int currentSlotIndex = 0;
    
    [Header("🎨 Visual Feedback")]
    [Tooltip("활성 슬롯 테두리 색상")]
    public Color activeSlotColor = Color.cyan;
    
    [Tooltip("비활성 슬롯 테두리 색상")]
    public Color inactiveSlotColor = Color.gray;
    
    [Tooltip("활성 슬롯 글로우 효과 (선택사항)")]
    public GameObject[] slotGlowEffects = new GameObject[3];
    
    [Header("📊 Status Display")]
    [Tooltip("현재 슬롯 번호를 표시할 텍스트")]
    public Text currentSlotText;
    
    [Tooltip("무기 교체 안내 텍스트")]
    public Text weaponSwitchHintText;
    
    [Header("🔊 Sound Effects")]
    [Tooltip("무기 교체 사운드 (선택사항)")]
    public AudioClip weaponSwitchSound;
    
    [Tooltip("사운드 재생용 AudioSource")]
    public AudioSource audioSource;
    
    [Header("🔗 References")]
    [Tooltip("플레이어 인벤토리 (자동 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 (자동 연결됨)")]
    public InventoryManager inventoryManager;
    
    // Events
    public System.Action<int> OnWeaponSlotChanged;
    public System.Action<WeaponData> OnWeaponSwitched;
    
    // Private variables
    private WeaponData[] equippedWeapons = new WeaponData[3];
    private bool isInitialized = false;

    void Start()
    {
        InitializeSlots();
        UpdateVisuals();
        
        // UI 텍스트 초기 설정
        if (weaponSwitchHintText != null)
        {
            weaponSwitchHintText.text = "Tab키로 무기 교체";
        }
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Tab키로 무기 교체
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchToNextWeapon();
        }
        
        // 1, 2, 3 키로 직접 슬롯 선택
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchToSlot(2);
        }
    }

    void InitializeSlots()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        // 슬롯 유효성 검사
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                // Debug.LogError($"❌ [WeaponSlotManager] weaponSlots[{i}]이 연결되지 않았습니다!");
                continue;
            }
            
            // 각 슬롯에 인덱스 설정
            weaponSlots[i].name = $"WeaponSlot_{i + 1}";
            
            // 슬롯 이벤트 연결 (필요한 경우)
            int slotIndex = i; // 클로저를 위한 로컬 변수
            
            // 슬롯 초기화 완료
        }
        
        // 슬롯 번호 표시
        if (currentSlotText != null)
        {
            currentSlotText.text = $"슬롯 {currentSlotIndex + 1}";
        }
        
        isInitialized = true;
    }

    public void SwitchToNextWeapon()
    {
        int nextSlot = (currentSlotIndex + 1) % weaponSlots.Length;
        SwitchToSlot(nextSlot);
    }

    public void SwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            // Debug.LogWarning($"⚠️ [WeaponSlotManager] 잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }
        
        if (slotIndex == currentSlotIndex)
        {
            return;
        }
        
        // 이전 슬롯 비활성화
        DeactivateSlot(currentSlotIndex);
        
        // 새 슬롯 활성화
        currentSlotIndex = slotIndex;
        ActivateSlot(currentSlotIndex);
        
        // 플레이어 인벤토리에 현재 무기 설정
        WeaponData currentWeapon = GetCurrentWeapon();
        if (playerInventory != null)
        {
            playerInventory.SetEquippedWeapon(currentWeapon);
        }
        
        // 사운드 재생
        PlaySwitchSound();
        
        // 이벤트 호출
        OnWeaponSlotChanged?.Invoke(currentSlotIndex);
        if (currentWeapon != null)
        {
            OnWeaponSwitched?.Invoke(currentWeapon);
        }
        
        // UI 업데이트
        UpdateVisuals();
    }

    void ActivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        
        WeaponSlot slot = weaponSlots[slotIndex];
        if (slot == null) return;
        
        // 슬롯 시각적 활성화
        if (slot.backgroundImage != null)
        {
            slot.backgroundImage.color = activeSlotColor;
        }
        
        // 글로우 효과 활성화
        if (slotGlowEffects[slotIndex] != null)
        {
            slotGlowEffects[slotIndex].SetActive(true);
        }
        
        // 슬롯 크기 약간 확대 (선택사항)
        slot.transform.localScale = Vector3.one * 1.1f;
    }

    void DeactivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        
        WeaponSlot slot = weaponSlots[slotIndex];
        if (slot == null) return;
        
        // 슬롯 시각적 비활성화
        if (slot.backgroundImage != null)
        {
            // 무기가 있으면 장착 색상, 없으면 일반 색상
            if (slot.weaponData != null)
            {
                slot.backgroundImage.color = slot.equippedColor;
            }
            else
            {
                slot.backgroundImage.color = inactiveSlotColor;
            }
        }
        
        // 글로우 효과 비활성화
        if (slotGlowEffects[slotIndex] != null)
        {
            slotGlowEffects[slotIndex].SetActive(false);
        }
        
        // 슬롯 크기 원래대로
        slot.transform.localScale = Vector3.one;
    }

    void UpdateVisuals()
    {
        if (!isInitialized) return;
        
        // 모든 슬롯 상태 업데이트
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (i == currentSlotIndex)
            {
                ActivateSlot(i);
            }
            else
            {
                DeactivateSlot(i);
            }
        }
        
        // 현재 슬롯 텍스트 업데이트
        if (currentSlotText != null)
        {
            WeaponData currentWeapon = GetCurrentWeapon();
            if (currentWeapon != null)
            {
                currentSlotText.text = $"슬롯 {currentSlotIndex + 1}: {currentWeapon.weaponName}";
            }
            else
            {
                currentSlotText.text = $"슬롯 {currentSlotIndex + 1}: 비어있음";
            }
        }
    }

    void PlaySwitchSound()
    {
        if (audioSource != null && weaponSwitchSound != null)
        {
            audioSource.PlayOneShot(weaponSwitchSound);
        }
    }

    // 공개 메서드들
    public WeaponData GetCurrentWeapon()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < weaponSlots.Length)
        {
            return weaponSlots[currentSlotIndex]?.weaponData;
        }
        return null;
    }

    public WeaponData GetWeaponInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots.Length)
        {
            return weaponSlots[slotIndex]?.weaponData;
        }
        return null;
    }

    public bool EquipWeaponToSlot(WeaponData weapon, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            // Debug.LogWarning($"⚠️ [WeaponSlotManager] 잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        if (weaponSlots[slotIndex] == null)
        {
            // Debug.LogError($"❌ [WeaponSlotManager] weaponSlots[{slotIndex}]이 null입니다!");
            return false;
        }
        
        // 무기 장착
        weaponSlots[slotIndex].EquipWeapon(weapon);
        
        // 현재 슬롯이면 플레이어 인벤토리에도 반영
        if (slotIndex == currentSlotIndex)
        {
            if (playerInventory != null)
            {
                playerInventory.SetEquippedWeapon(weapon);
            }
        }
        
        UpdateVisuals();
        

        return true;
    }
    
    public bool UnequipWeaponFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            // Debug.LogWarning($"⚠️ [WeaponSlotManager] 잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        if (weaponSlots[slotIndex] == null)
        {
            // Debug.LogError($"❌ [WeaponSlotManager] weaponSlots[{slotIndex}]이 null입니다!");
            return false;
        }
        
        WeaponData removedWeapon = weaponSlots[slotIndex].weaponData;
        
        // 무기 해제
        weaponSlots[slotIndex].UnequipWeapon();
        
        // 현재 슬롯이면 플레이어 인벤토리에도 반영
        if (slotIndex == currentSlotIndex)
        {
            if (playerInventory != null)
            {
                playerInventory.SetEquippedWeapon(null);
            }
        }
        
        UpdateVisuals();
        

        return true;
    }

    public int GetEmptySlotIndex()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].weaponData == null)
            {
                return i;
            }
        }
        return -1; // 빈 슬롯 없음
    }

    public int GetSlotCount()
    {
        return weaponSlots.Length;
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return true;
        return weaponSlots[slotIndex]?.weaponData == null;
    }

    public bool HasWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].weaponData == weapon)
            {
                return true;
            }
        }
        return false;
    }

    public List<WeaponData> GetAllEquippedWeapons()
    {
        List<WeaponData> equippedWeapons = new List<WeaponData>();
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].weaponData != null)
            {
                equippedWeapons.Add(weaponSlots[i].weaponData);
            }
        }
        
        return equippedWeapons;
    }

    // 디버그 정보
    [ContextMenu("Debug Slot Status")]
    public void DebugSlotStatus()
    {
        // Debug.Log("=== 무기 슬롯 상태 ===");
        // Debug.Log($"현재 활성 슬롯: {currentSlotIndex + 1}");
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                string weaponName = weaponSlots[i].weaponData != null ? weaponSlots[i].weaponData.weaponName : "비어있음";
                string status = i == currentSlotIndex ? "[활성]" : "[비활성]";
                // Debug.Log($"슬롯 {i + 1} {status}: {weaponName}");
            }
            else
            {
                // Debug.Log($"슬롯 {i + 1}: [NULL - 연결 필요]");
            }
        }
    }
} 