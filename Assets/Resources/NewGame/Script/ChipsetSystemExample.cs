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
        ShowF4KeyInfo(); // F4키 안내 출력
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
    
    /// <summary>
    /// 칩셋 UI 상태 확인
    /// </summary>
    public void CheckChipsetUIStatus()
    {
        Debug.Log("=== 칩셋 UI 상태 확인 ===");
        
        // 데이터 로드 상태 확인
        Debug.Log($"데이터 로드 완료: {GameDataRepository.Instance.IsAllDataLoaded}");
        Debug.Log($"무기 칩셋 로드: {GameDataRepository.Instance.IsWeaponChipsetsLoaded}");
        Debug.Log($"방어구 칩셋 로드: {GameDataRepository.Instance.IsArmorChipsetsLoaded}");
        Debug.Log($"플레이어 칩셋 로드: {GameDataRepository.Instance.IsPlayerChipsetsLoaded}");
        
        // 칩셋 매니저 상태 확인
        if (chipsetManager != null)
        {
            Debug.Log("칩셋 매니저: 활성화됨");
        }
        else
        {
            Debug.LogWarning("칩셋 매니저: 비활성화됨");
        }
        
        // 효과 매니저 상태 확인
        if (effectManager != null)
        {
            Debug.Log("효과 매니저: 활성화됨");
        }
        else
        {
            Debug.LogWarning("효과 매니저: 비활성화됨");
        }
        
        // 테스트 무기 상태 확인
        if (testWeapon != null)
        {
            string[] equippedChipsets = testWeapon.GetEquippedChipsetIds();
            Debug.Log($"테스트 무기: {testWeapon.weaponName}");
            Debug.Log($"장착된 칩셋 수: {equippedChipsets.Length}");
            
            for (int i = 0; i < equippedChipsets.Length; i++)
            {
                if (!string.IsNullOrEmpty(equippedChipsets[i]))
                {
                    var chipset = GameDataRepository.Instance.GetWeaponChipsetById(equippedChipsets[i]);
                    if (chipset != null)
                    {
                        Debug.Log($"  슬롯 {i}: {chipset.chipsetName} (코스트: {chipset.cost})");
                    }
                    else
                    {
                        Debug.LogWarning($"  슬롯 {i}: 칩셋을 찾을 수 없음 ({equippedChipsets[i]})");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("테스트 무기: 설정되지 않음");
        }
        
        Debug.Log("=== UI 상태 확인 완료 ===");
    }
    
    /// <summary>
    /// 칩셋 UI 강제 새로고침
    /// </summary>
    public void RefreshChipsetUI()
    {
        Debug.Log("칩셋 UI 새로고침 중...");
        
        if (chipsetManager != null)
        {
            // 현재 무기/방어구 재설정
            if (testWeapon != null)
            {
                chipsetManager.SetCurrentWeapon(testWeapon);
            }
            
            if (testArmor != null)
            {
                chipsetManager.SetCurrentArmor(testArmor);
            }
            
            Debug.Log("칩셋 UI 새로고침 완료");
        }
        else
        {
            Debug.LogWarning("칩셋 매니저가 없어서 새로고침할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 저장 테스트
    /// </summary>
    public void TestSaveChipsetInventory()
    {
        if (chipsetManager != null)
        {
            chipsetManager.SaveChipsetInventoryData();
            Debug.Log("칩셋 인벤토리 저장 테스트 완료");
        }
        else
        {
            Debug.LogWarning("칩셋 매니저가 없어서 저장할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 칩셋 인벤토리 로드 테스트
    /// </summary>
    public void TestLoadChipsetInventory()
    {
        if (chipsetManager != null)
        {
            chipsetManager.LoadChipsetInventoryData();
            Debug.Log("칩셋 인벤토리 로드 테스트 완료");
        }
        else
        {
            Debug.LogWarning("칩셋 매니저가 없어서 로드할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 랜덤 칩셋을 인벤토리에 추가 테스트
    /// </summary>
    public void TestAddRandomChipsetToInventory()
    {
        if (chipsetManager != null && GameDataRepository.Instance.IsAllDataLoaded)
        {
            // 랜덤 칩셋 선택
            var weaponChipsets = GameDataRepository.Instance.GetAllWeaponChipsets();
            var armorChipsets = GameDataRepository.Instance.GetAllArmorChipsets();
            var playerChipsets = GameDataRepository.Instance.GetAllPlayerChipsets();
            
            int totalChipsets = weaponChipsets.Count + armorChipsets.Count + playerChipsets.Count;
            if (totalChipsets > 0)
            {
                int randomIndex = Random.Range(0, totalChipsets);
                object randomChipset = null;
                
                if (randomIndex < weaponChipsets.Count)
                {
                    randomChipset = weaponChipsets[randomIndex];
                }
                else if (randomIndex < weaponChipsets.Count + armorChipsets.Count)
                {
                    randomChipset = armorChipsets[randomIndex - weaponChipsets.Count];
                }
                else
                {
                    randomChipset = playerChipsets[randomIndex - weaponChipsets.Count - armorChipsets.Count];
                }
                
                if (randomChipset != null)
                {
                    chipsetManager.AddChipsetToInventory(randomChipset);
                    Debug.Log($"랜덤 칩셋 추가 테스트 완료");
                }
            }
            else
            {
                Debug.LogWarning("추가할 칩셋이 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("칩셋 매니저가 없거나 데이터가 로드되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// F4키 안내 출력
    /// </summary>
    public void ShowF4KeyInfo()
    {
        Debug.Log("=== F4키 사용법 ===");
        Debug.Log("F4키를 누르면 랜덤 칩셋이 필드에 소환됩니다.");
        Debug.Log("플레이어가 칩셋에 접근하면 자동으로 인벤토리에 추가됩니다.");
        Debug.Log("칩셋 인벤토리는 자동으로 저장/로드됩니다.");
        Debug.Log("==================");
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