using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("📊 플레이어 스탯 UI")]
    [TextArea(3, 5)]
    public string instructions = "🎯 플레이어 스탯 UI:\n• 좌상단에 상시 표시\n• 실시간 스탯 업데이트\n• 호버 시 상세 설명\n• 클릭 시 확장/축소";

    [Header("🎨 UI References")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;
    
    [Header("📋 스탯 카테고리")]
    [SerializeField] private Transform basicStatsContainer;
    [SerializeField] private Transform defenseStatsContainer;
    [SerializeField] private Transform weaponStatsContainer;
    [SerializeField] private Transform chipsetStatsContainer;
    
    [Header("🔧 설정")]
    [SerializeField] private float updateInterval = 0.2f;
    [SerializeField] private bool startExpanded = false;
    
    [Header("🎨 시각적 설정")]
    [SerializeField] private Color basicStatsColor = Color.blue;
    [SerializeField] private Color defenseStatsColor = Color.green;
    [SerializeField] private Color weaponStatsColor = Color.red;
    [SerializeField] private Color chipsetStatsColor = new Color(0.5f, 0f, 1f); // 보라색
    
    // References
    private PlayerInventory playerInventory;
    private PlayerController playerController;
    private ChipsetManager chipsetManager;
    
    // UI Components
    private List<StatDisplay> statDisplays = new List<StatDisplay>();
    private bool isExpanded = false;
    private float lastUpdateTime = 0f;
    
    // 스탯 데이터
    private Dictionary<string, float> currentStats = new Dictionary<string, float>();
    private Dictionary<string, string> statDescriptions = new Dictionary<string, string>();
    
    void Awake()
    {
        InitializeReferences();
        InitializeStatDescriptions();
        SetupUI();
    }
    
    void Start()
    {
        isExpanded = startExpanded;
        UpdatePanelVisibility();
        CreateStatDisplays();
    }
    
    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAllStats();
            lastUpdateTime = Time.time;
        }
    }
    
    void InitializeReferences()
    {
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();
        
        if (chipsetManager == null)
            chipsetManager = FindAnyObjectByType<ChipsetManager>();
    }
    
    void InitializeStatDescriptions()
    {
        // 기본 스탯 설명
        statDescriptions["MoveSpeed"] = "플레이어의 이동 속도입니다. 높을수록 빠르게 움직입니다.";
        statDescriptions["JumpForce"] = "플레이어의 점프력입니다. 높을수록 더 높이 점프합니다.";
        statDescriptions["DashCooldown"] = "대시 스킬의 재사용 대기시간입니다. 낮을수록 자주 사용할 수 있습니다.";
        
        // 방어 스탯 설명
        statDescriptions["Defense"] = "총 방어력입니다. 높을수록 받는 데미지가 감소합니다.";
        statDescriptions["DamageReduction"] = "데미지 감소율입니다. 높을수록 더 많은 데미지를 무시합니다.";
        statDescriptions["HealthRegen"] = "초당 체력 재생량입니다. 높을수록 빠르게 체력을 회복합니다.";
        statDescriptions["InvincibilityTime"] = "피격 후 무적 시간입니다. 높을수록 더 오래 무적 상태를 유지합니다.";
        
        // 무기 스탯 설명
        statDescriptions["CurrentWeapon"] = "현재 장착된 무기입니다.";
        statDescriptions["WeaponDamage"] = "무기의 기본 공격력입니다. 높을수록 더 강한 데미지를 줍니다.";
        statDescriptions["FireRate"] = "무기의 발사 속도입니다. 높을수록 빠르게 공격합니다.";
        statDescriptions["ReloadTime"] = "무기의 재장전 시간입니다. 낮을수록 빠르게 재장전합니다.";
        
        // 칩셋 스탯 설명
        statDescriptions["WeaponChipsets"] = "무기에 장착된 칩셋 개수입니다.";
        statDescriptions["ArmorChipsets"] = "방어구에 장착된 칩셋 개수입니다.";
        statDescriptions["PlayerChipsets"] = "플레이어에 장착된 칩셋 개수입니다.";
    }
    
    void SetupUI()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }
        
        UpdateToggleButtonText();
    }
    
    void CreateStatDisplays()
    {
        // 기본 스탯
        CreateStatDisplay("MoveSpeed", "이동속도", basicStatsContainer, basicStatsColor);
        CreateStatDisplay("JumpForce", "점프력", basicStatsContainer, basicStatsColor);
        CreateStatDisplay("DashCooldown", "대시 쿨다운", basicStatsContainer, basicStatsColor);
        
        // 방어 스탯
        CreateStatDisplay("Defense", "방어력", defenseStatsContainer, defenseStatsColor);
        CreateStatDisplay("DamageReduction", "데미지 감소율", defenseStatsContainer, defenseStatsColor);
        CreateStatDisplay("HealthRegen", "체력 재생", defenseStatsContainer, defenseStatsColor);
        CreateStatDisplay("InvincibilityTime", "무적 시간", defenseStatsContainer, defenseStatsColor);
        
        // 무기 스탯
        CreateStatDisplay("CurrentWeapon", "현재 무기", weaponStatsContainer, weaponStatsColor);
        CreateStatDisplay("WeaponDamage", "공격력", weaponStatsContainer, weaponStatsColor);
        CreateStatDisplay("FireRate", "공격속도", weaponStatsContainer, weaponStatsColor);
        CreateStatDisplay("ReloadTime", "재장전 시간", weaponStatsContainer, weaponStatsColor);
        
        // 칩셋 스탯
        CreateStatDisplay("WeaponChipsets", "무기 칩셋", chipsetStatsContainer, chipsetStatsColor);
        CreateStatDisplay("ArmorChipsets", "방어구 칩셋", chipsetStatsContainer, chipsetStatsColor);
        CreateStatDisplay("PlayerChipsets", "플레이어 칩셋", chipsetStatsContainer, chipsetStatsColor);
    }
    
    void CreateStatDisplay(string statKey, string statName, Transform container, Color color)
    {
        if (container == null) return;
        
        // StatDisplay 프리팹 생성 또는 기존 오브젝트 찾기
        GameObject statDisplayObj = new GameObject($"StatDisplay_{statKey}");
        statDisplayObj.transform.SetParent(container, false);
        
        // StatDisplay 컴포넌트 추가
        StatDisplay statDisplay = statDisplayObj.AddComponent<StatDisplay>();
        statDisplay.Initialize(statKey, statName, color, GetStatDescription(statKey));
        
        statDisplays.Add(statDisplay);
    }
    
    string GetStatDescription(string statKey)
    {
        return statDescriptions.ContainsKey(statKey) ? statDescriptions[statKey] : "설명이 없습니다.";
    }
    
    void UpdateAllStats()
    {
        // 기본 스탯 업데이트
        UpdateBasicStats();
        
        // 방어 스탯 업데이트
        UpdateDefenseStats();
        
        // 무기 스탯 업데이트
        UpdateWeaponStats();
        
        // 칩셋 스탯 업데이트
        UpdateChipsetStats();
        
        // UI 업데이트
        UpdateStatDisplays();
    }
    
    void UpdateBasicStats()
    {
        if (playerController != null)
        {
            // 이동속도 (기본 + 보너스)
            float baseMoveSpeed = playerController.GetBaseMoveSpeed();
            float moveSpeedBonus = playerInventory != null ? playerInventory.GetTotalMoveSpeedBonus() : 0f;
            currentStats["MoveSpeed"] = baseMoveSpeed + moveSpeedBonus;
            
            // 점프력 (기본 + 보너스)
            float baseJumpForce = playerController.GetBaseJumpForce();
            float jumpForceBonus = playerInventory != null ? playerInventory.GetTotalJumpForceBonus() : 0f;
            currentStats["JumpForce"] = baseJumpForce + jumpForceBonus;
            
            // 대시 쿨다운 (기본 - 감소량)
            float baseDashCooldown = playerController.GetBaseDashCooldown();
            float dashCooldownReduction = playerInventory != null ? playerInventory.GetTotalDashCooldownReduction() : 0f;
            currentStats["DashCooldown"] = Mathf.Max(0.1f, baseDashCooldown - dashCooldownReduction);
        }
    }
    
    void UpdateDefenseStats()
    {
        if (playerInventory != null)
        {
            currentStats["Defense"] = playerInventory.GetTotalDefense();
            currentStats["DamageReduction"] = playerInventory.GetTotalDamageReduction() * 100f; // 퍼센트로 표시
            currentStats["HealthRegen"] = playerInventory.GetTotalHealthRegeneration();
            currentStats["InvincibilityTime"] = playerInventory.GetTotalInvincibilityTime();
        }
    }
    
    void UpdateWeaponStats()
    {
        if (playerInventory != null)
        {
            WeaponData currentWeapon = playerInventory.GetEquippedWeapon();
            if (currentWeapon != null)
            {
                currentStats["CurrentWeapon"] = 1f; // 무기가 있음을 표시
                currentStats["WeaponDamage"] = currentWeapon.damage;
                currentStats["FireRate"] = currentWeapon.fireRate;
                currentStats["ReloadTime"] = currentWeapon.reloadTime;
            }
            else
            {
                currentStats["CurrentWeapon"] = 0f; // 무기가 없음을 표시
                currentStats["WeaponDamage"] = 0f;
                currentStats["FireRate"] = 0f;
                currentStats["ReloadTime"] = 0f;
            }
        }
    }
    
    void UpdateChipsetStats()
    {
        if (chipsetManager != null)
        {
            currentStats["WeaponChipsets"] = chipsetManager.GetWeaponChipsetCount();
            currentStats["ArmorChipsets"] = chipsetManager.GetArmorChipsetCount();
            currentStats["PlayerChipsets"] = chipsetManager.GetPlayerChipsetCount();
        }
    }
    
    void UpdateStatDisplays()
    {
        foreach (var statDisplay in statDisplays)
        {
            if (currentStats.ContainsKey(statDisplay.StatKey))
            {
                statDisplay.UpdateValue(currentStats[statDisplay.StatKey]);
            }
        }
    }
    
    public void TogglePanel()
    {
        isExpanded = !isExpanded;
        UpdatePanelVisibility();
        UpdateToggleButtonText();
    }
    
    void UpdatePanelVisibility()
    {
        if (statsPanel != null)
        {
            // 확장된 상태에서는 모든 스탯 표시, 축소된 상태에서는 요약만 표시
            // 실제 구현에서는 애니메이션과 함께 처리
            statsPanel.SetActive(isExpanded);
        }
    }
    
    void UpdateToggleButtonText()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isExpanded ? "▼" : "▲";
        }
    }
    
    // Public methods for external access
    public void ForceUpdate()
    {
        UpdateAllStats();
    }
    
    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        UpdatePanelVisibility();
        UpdateToggleButtonText();
    }
    
    public bool IsExpanded() => isExpanded;
} 