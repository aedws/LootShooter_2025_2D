using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ArmorSlotManager : MonoBehaviour
{
    [Header("📋 방어구 슬롯 매니저 사용법")]
    [TextArea(4, 6)]
    public string instructions = "🛡️ 방어구 슬롯 매니저:\n• 6개의 방어구 슬롯 관리 (3x2 그리드)\n• 각 슬롯은 특정 타입의 방어구만 장착 가능\n• 슬롯 순서: 머리, 상체, 하체, 신발, 어깨, 악세사리\n• 시각적 피드백: 레어리티별 색상 표시\n• 자동 능력치 계산 및 적용";

    [Header("🛡️ Armor Slots")]
    [Tooltip("6개의 방어구 슬롯 (3x2 그리드)")]
    public ArmorSlot[] armorSlots = new ArmorSlot[6];
    
    [Header("🎨 Visual Feedback")]
    [Tooltip("장착된 슬롯 테두리 색상")]
    public Color equippedSlotColor = Color.cyan;
    
    [Tooltip("빈 슬롯 테두리 색상")]
    public Color emptySlotColor = Color.gray;
    
    [Tooltip("슬롯 글로우 효과 (선택사항)")]
    public GameObject[] slotGlowEffects = new GameObject[6];
    
    [Header("📊 Status Display")]
    [Tooltip("총 방어력 표시 텍스트")]
    public Text totalDefenseText;
    
    [Tooltip("총 체력 보너스 표시 텍스트")]
    public Text totalHealthBonusText;
    
    [Tooltip("총 이동속도 보너스 표시 텍스트")]
    public Text totalSpeedBonusText;
    
    [Header("🔊 Sound Effects")]
    [Tooltip("방어구 장착 사운드 (선택사항)")]
    public AudioClip equipSound;
    
    [Tooltip("사운드 재생용 AudioSource")]
    public AudioSource audioSource;
    
    [Header("🔗 References")]
    [Tooltip("플레이어 인벤토리 (자동 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 (자동 연결됨)")]
    public InventoryManager inventoryManager;
    
    // Events
    public System.Action<int> OnArmorSlotChanged;
    public System.Action<ArmorData> OnArmorEquipped;
    public System.Action<ArmorData> OnArmorUnequipped;
    
    // Private variables
    private Dictionary<ArmorType, ArmorData> equippedArmors = new Dictionary<ArmorType, ArmorData>();
    
    void Awake()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        // AudioSource 자동 찾기
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // 슬롯 초기화
        InitializeSlots();
    }
    
    void Start()
    {
        // 슬롯 이벤트 연결
        ConnectSlotEvents();
        
        // 초기 UI 업데이트
        UpdateVisuals();
        UpdateStatsDisplay();
    }
    
    void InitializeSlots()
    {
        // 슬롯이 설정되지 않았다면 자동으로 찾기
        if (armorSlots[0] == null)
        {
            ArmorSlot[] foundSlots = GetComponentsInChildren<ArmorSlot>();
            for (int i = 0; i < Mathf.Min(foundSlots.Length, armorSlots.Length); i++)
            {
                armorSlots[i] = foundSlots[i];
            }
        }
        
        // 슬롯 타입 자동 설정 (순서대로)
        ArmorType[] slotTypes = {
            ArmorType.Helmet,    // 머리
            ArmorType.Chest,     // 상체
            ArmorType.Legs,      // 하체
            ArmorType.Boots,     // 신발
            ArmorType.Shoulder,  // 어깨
            ArmorType.Accessory  // 악세사리
        };
        
        for (int i = 0; i < armorSlots.Length; i++)
        {
            if (armorSlots[i] != null)
            {
                armorSlots[i].allowedArmorType = slotTypes[i];
                armorSlots[i].slotName = GetSlotName(slotTypes[i]);
            }
        }
    }
    
    void ConnectSlotEvents()
    {
        for (int i = 0; i < armorSlots.Length; i++)
        {
            if (armorSlots[i] != null)
            {
                int slotIndex = i; // 클로저를 위한 로컬 변수
                
                armorSlots[i].OnArmorEquipped += (armor) => {
                    OnArmorEquippedInSlot(slotIndex, armor);
                };
                
                armorSlots[i].OnArmorUnequipped += (armor) => {
                    OnArmorUnequippedInSlot(slotIndex, armor);
                };
            }
        }
    }
    
    // 슬롯 이름 반환
    string GetSlotName(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.Helmet: return "머리";
            case ArmorType.Chest: return "상체";
            case ArmorType.Legs: return "하체";
            case ArmorType.Boots: return "신발";
            case ArmorType.Shoulder: return "어깨";
            case ArmorType.Accessory: return "악세사리";
            default: return "알 수 없음";
        }
    }
    
    // 슬롯에서 방어구 장착됨
    void OnArmorEquippedInSlot(int slotIndex, ArmorData armor)
    {
        equippedArmors[armor.armorType] = armor;
        
        // 사운드 재생
        PlayEquipSound();
        
        // 이벤트 호출
        OnArmorSlotChanged?.Invoke(slotIndex);
        OnArmorEquipped?.Invoke(armor);
        
        // UI 업데이트
        UpdateVisuals();
        UpdateStatsDisplay();
        
        Debug.Log($"🛡️ 슬롯 {slotIndex}에 {armor.armorName} 장착됨");
    }
    
    // 슬롯에서 방어구 해제됨
    void OnArmorUnequippedInSlot(int slotIndex, ArmorData armor)
    {
        equippedArmors.Remove(armor.armorType);
        
        // 이벤트 호출
        OnArmorSlotChanged?.Invoke(slotIndex);
        OnArmorUnequipped?.Invoke(armor);
        
        // UI 업데이트
        UpdateVisuals();
        UpdateStatsDisplay();
        
        Debug.Log($"🛡️ 슬롯 {slotIndex}에서 {armor.armorName} 해제됨");
    }
    
    // 시각적 업데이트
    void UpdateVisuals()
    {
        for (int i = 0; i < armorSlots.Length; i++)
        {
            if (armorSlots[i] != null)
            {
                // 슬롯 상태에 따른 시각적 피드백
                if (armorSlots[i].IsEquipped())
                {
                    ActivateSlot(i);
                }
                else
                {
                    DeactivateSlot(i);
                }
            }
        }
    }
    
    // 슬롯 활성화
    void ActivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= armorSlots.Length) return;
        
        ArmorSlot slot = armorSlots[slotIndex];
        if (slot == null) return;
        
        // 글로우 효과 활성화
        if (slotGlowEffects[slotIndex] != null)
        {
            slotGlowEffects[slotIndex].SetActive(true);
        }
        
        // 슬롯 크기 약간 확대 (선택사항)
        slot.transform.localScale = Vector3.one * 1.05f;
    }
    
    // 슬롯 비활성화
    void DeactivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= armorSlots.Length) return;
        
        ArmorSlot slot = armorSlots[slotIndex];
        if (slot == null) return;
        
        // 글로우 효과 비활성화
        if (slotGlowEffects[slotIndex] != null)
        {
            slotGlowEffects[slotIndex].SetActive(false);
        }
        
        // 슬롯 크기 원래대로
        slot.transform.localScale = Vector3.one;
    }
    
    // 통계 표시 업데이트
    void UpdateStatsDisplay()
    {
        int totalDefense = 0;
        int totalHealthBonus = 0;
        float totalSpeedBonus = 0f;
        
        foreach (var armor in equippedArmors.Values)
        {
            totalDefense += armor.defense;
            totalHealthBonus += armor.maxHealth;
            totalSpeedBonus += armor.moveSpeedBonus;
        }
        
        // UI 업데이트
        if (totalDefenseText != null)
            totalDefenseText.text = $"총 방어력: {totalDefense}";
        
        if (totalHealthBonusText != null)
            totalHealthBonusText.text = $"체력 보너스: +{totalHealthBonus}";
        
        if (totalSpeedBonusText != null)
            totalSpeedBonusText.text = $"이동속도 보너스: +{totalSpeedBonus:F1}";
    }
    
    // 사운드 재생
    void PlayEquipSound()
    {
        if (audioSource != null && equipSound != null)
        {
            audioSource.PlayOneShot(equipSound);
        }
    }
    
    // 공개 메서드들
    public ArmorData GetArmorInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < armorSlots.Length)
        {
            return armorSlots[slotIndex]?.GetArmorData();
        }
        return null;
    }
    
    public bool EquipArmorToSlot(ArmorData armor, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= armorSlots.Length)
        {
            Debug.LogWarning($"⚠️ [ArmorSlotManager] 잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        if (armorSlots[slotIndex] == null)
        {
            Debug.LogError($"❌ [ArmorSlotManager] armorSlots[{slotIndex}]이 null입니다!");
            return false;
        }
        
        // 방어구 장착
        armorSlots[slotIndex].EquipArmor(armor);
        
        return true;
    }
    
    public void UnequipArmorFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= armorSlots.Length) return;
        
        if (armorSlots[slotIndex] != null)
        {
            armorSlots[slotIndex].UnequipArmor();
        }
    }
    
    public Dictionary<ArmorType, ArmorData> GetAllEquippedArmors()
    {
        return new Dictionary<ArmorType, ArmorData>(equippedArmors);
    }
    
    public int GetEquippedArmorCount()
    {
        return equippedArmors.Count;
    }
    
    public bool IsArmorEquipped(ArmorData armor)
    {
        return equippedArmors.ContainsValue(armor);
    }
    
    public bool IsArmorTypeEquipped(ArmorType armorType)
    {
        return equippedArmors.ContainsKey(armorType);
    }
    
    // 통계 계산 메서드들
    public int GetTotalDefense()
    {
        return equippedArmors.Values.Sum(armor => armor.defense);
    }
    
    public int GetTotalHealthBonus()
    {
        return equippedArmors.Values.Sum(armor => armor.maxHealth);
    }
    
    public float GetTotalSpeedBonus()
    {
        return equippedArmors.Values.Sum(armor => armor.moveSpeedBonus);
    }
    
    public float GetTotalDamageReduction()
    {
        float totalReduction = 0f;
        foreach (var armor in equippedArmors.Values)
        {
            totalReduction += armor.damageReduction;
        }
        return Mathf.Clamp01(totalReduction); // 최대 100% 제한
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        OnArmorSlotChanged = null;
        OnArmorEquipped = null;
        OnArmorUnequipped = null;
    }
} 