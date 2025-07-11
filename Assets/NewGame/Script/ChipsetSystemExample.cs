using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 칩셋 시스템 사용 예시
/// 게임에서 칩셋 시스템을 어떻게 사용하는지 보여주는 예시 스크립트
/// </summary>
public class ChipsetSystemExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ChipsetManager chipsetManager;
    [SerializeField] private ChipsetEffectManager effectManager;
    [SerializeField] private Button openChipsetButton;
    [SerializeField] private Button closeChipsetButton;
    [SerializeField] private Button weaponPanelButton;
    [SerializeField] private Button armorPanelButton;
    [SerializeField] private Button playerPanelButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private TextMeshProUGUI effectsSummaryText;
    
    [Header("Test Data")]
    [SerializeField] private WeaponData testWeapon;
    [SerializeField] private ArmorData testArmor;
    
    private void Start()
    {
        SetupUI();
        LoadTestData();
    }
    
    /// <summary>
    /// UI 설정
    /// </summary>
    private void SetupUI()
    {
        if (openChipsetButton != null)
            openChipsetButton.onClick.AddListener(OpenChipsetSystem);
        
        if (closeChipsetButton != null)
            closeChipsetButton.onClick.AddListener(CloseChipsetSystem);
        
        if (weaponPanelButton != null)
            weaponPanelButton.onClick.AddListener(() => chipsetManager?.ShowWeaponPanel());
        
        if (armorPanelButton != null)
            armorPanelButton.onClick.AddListener(() => chipsetManager?.ShowArmorPanel());
        
        if (playerPanelButton != null)
            playerPanelButton.onClick.AddListener(() => chipsetManager?.ShowPlayerPanel());
        
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(() => chipsetManager?.ShowInventoryPanel());
    }
    
    /// <summary>
    /// 테스트 데이터 로드
    /// </summary>
    private void LoadTestData()
    {
        // 테스트 무기 설정
        if (testWeapon != null && chipsetManager != null)
        {
            chipsetManager.SetCurrentWeapon(testWeapon);
        }
        
        // 테스트 방어구 설정
        if (testArmor != null && chipsetManager != null)
        {
            chipsetManager.SetCurrentArmor(testArmor);
        }
    }
    
    /// <summary>
    /// 칩셋 시스템 열기
    /// </summary>
    public void OpenChipsetSystem()
    {
        if (chipsetManager != null)
        {
            chipsetManager.gameObject.SetActive(true);
            chipsetManager.ShowWeaponPanel(); // 기본적으로 무기 패널 표시
        }
    }
    
    /// <summary>
    /// 칩셋 시스템 닫기
    /// </summary>
    public void CloseChipsetSystem()
    {
        if (chipsetManager != null)
        {
            chipsetManager.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 효과 요약 업데이트
    /// </summary>
    public void UpdateEffectsSummary()
    {
        if (effectsSummaryText != null && chipsetManager != null)
        {
            effectsSummaryText.text = chipsetManager.GetEffectsSummary();
        }
    }
    
    /// <summary>
    /// 테스트용 칩셋 장착
    /// </summary>
    public void TestEquipChipset()
    {
        // 예시: 무기 칩셋 장착
        var weaponChipset = GameDataRepository.Instance.GetWeaponChipsetById("weapon_damage_01");
        if (weaponChipset != null && testWeapon != null)
        {
            // 첫 번째 슬롯에 장착
            string[] currentChipsets = testWeapon.GetEquippedChipsetIds();
            if (currentChipsets.Length < 3)
            {
                string[] newChipsets = new string[3];
                currentChipsets.CopyTo(newChipsets, 0);
                currentChipsets = newChipsets;
            }
            
            currentChipsets[0] = weaponChipset.chipsetId;
            testWeapon.SetEquippedChipsetIds(currentChipsets);
            
            // 칩셋 매니저에 적용
            if (chipsetManager != null)
            {
                chipsetManager.SetCurrentWeapon(testWeapon);
            }
            
            Debug.Log($"칩셋 장착: {weaponChipset.chipsetName}");
        }
    }
    
    /// <summary>
    /// 테스트용 칩셋 해제
    /// </summary>
    public void TestUnequipChipset()
    {
        if (testWeapon != null)
        {
            string[] currentChipsets = testWeapon.GetEquippedChipsetIds();
            if (currentChipsets.Length > 0)
            {
                currentChipsets[0] = null;
                testWeapon.SetEquippedChipsetIds(currentChipsets);
            }
            
            if (chipsetManager != null)
            {
                chipsetManager.SetCurrentWeapon(testWeapon);
            }
            
            Debug.Log("칩셋 해제 완료");
        }
    }
    
    /// <summary>
    /// 모든 칩셋 효과 초기화
    /// </summary>
    public void ClearAllEffects()
    {
        if (effectManager != null)
        {
            effectManager.ClearAllEffects();
            Debug.Log("모든 칩셋 효과 초기화 완료");
        }
    }
    
    /// <summary>
    /// 칩셋 데이터 로드 상태 확인
    /// </summary>
    public void CheckChipsetDataStatus()
    {
        var weaponChipsets = GameDataRepository.Instance.GetAllWeaponChipsets();
        var armorChipsets = GameDataRepository.Instance.GetAllArmorChipsets();
        var playerChipsets = GameDataRepository.Instance.GetAllPlayerChipsets();
        
        Debug.Log($"무기 칩셋: {weaponChipsets.Count}개");
        Debug.Log($"방어구 칩셋: {armorChipsets.Count}개");
        Debug.Log($"플레이어 칩셋: {playerChipsets.Count}개");
        
        // 첫 번째 칩셋 정보 출력
        if (weaponChipsets.Count > 0)
        {
            var firstChipset = weaponChipsets[0];
            Debug.Log($"첫 번째 무기 칩셋: {firstChipset.chipsetName} (코스트: {firstChipset.cost})");
        }
    }
    
    /// <summary>
    /// 코스트 계산 테스트
    /// </summary>
    public void TestCostCalculation()
    {
        if (testWeapon != null)
        {
            string[] equippedChipsets = testWeapon.GetEquippedChipsetIds();
            int totalCost = 0;
            int equippedCount = 0;
            
            foreach (var chipsetId in equippedChipsets)
            {
                if (!string.IsNullOrEmpty(chipsetId))
                {
                    var chipset = GameDataRepository.Instance.GetWeaponChipsetById(chipsetId);
                    if (chipset != null)
                    {
                        totalCost += chipset.cost;
                        equippedCount++;
                    }
                }
            }
            
            Debug.Log($"장착된 칩셋: {equippedCount}개, 총 코스트: {totalCost}");
        }
    }
    
    private void Update()
    {
        // 키보드 단축키
        if (Input.GetKeyDown(KeyCode.C))
        {
            OpenChipsetSystem();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChipsetSystem();
        }
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheckChipsetDataStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestEquipChipset();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestUnequipChipset();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestCostCalculation();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            UpdateEffectsSummary();
        }
    }
} 