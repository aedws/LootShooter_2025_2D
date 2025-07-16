using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameDataRepository 사용 예제 스크립트
/// 데이터 로드 완료를 감지하고 게임에서 데이터를 사용하는 방법을 보여줍니다.
/// </summary>
public class GameDataExample : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI weaponsText;
    [SerializeField] private TMPro.TextMeshProUGUI armorsText;
    
    [Header("테스트 설정")]
    [SerializeField] private bool testRandomDrops = true;
    [SerializeField] private float dropTestInterval = 5f;
    
    private float dropTimer = 0f;
    
    private void Start()
    {
        // GameDataRepository 이벤트 구독
        GameDataRepository.Instance.OnAllDataLoaded += OnAllDataLoaded;
        GameDataRepository.Instance.OnWeaponsUpdated += OnWeaponsUpdated;
        GameDataRepository.Instance.OnArmorsUpdated += OnArmorsUpdated;
        GameDataRepository.Instance.OnDataLoadError += OnDataLoadError;
        
        // 초기 상태 표시
        UpdateStatusText("데이터 로드 대기 중...");
    }
    
    private void Update()
    {
        // 테스트용 랜덤 드롭
        if (testRandomDrops && GameDataRepository.Instance.IsAllDataLoaded)
        {
            dropTimer += Time.deltaTime;
            if (dropTimer >= dropTestInterval)
            {
                dropTimer = 0f;
                TestRandomDrops();
            }
        }
    }
    
    /// <summary>
    /// 모든 데이터 로드 완료 시 호출
    /// </summary>
    private void OnAllDataLoaded()
    {
        // Debug.Log("[GameDataExample] 모든 데이터 로드 완료!");
        UpdateStatusText("모든 데이터 로드 완료!");
        
        // 데이터 요약 출력
        PrintDataSummary();
    }
    
    /// <summary>
    /// 무기 데이터 업데이트 시 호출
    /// </summary>
    private void OnWeaponsUpdated(List<WeaponData> weapons)
    {
        // Debug.Log($"[GameDataExample] 무기 데이터 업데이트: {weapons.Count}개");
        UpdateWeaponsText(weapons);
    }
    
    /// <summary>
    /// 방어구 데이터 업데이트 시 호출
    /// </summary>
    private void OnArmorsUpdated(List<ArmorData> armors)
    {
        // Debug.Log($"[GameDataExample] 방어구 데이터 업데이트: {armors.Count}개");
        UpdateArmorsText(armors);
    }
    
    /// <summary>
    /// 데이터 로드 오류 시 호출
    /// </summary>
    private void OnDataLoadError(string error)
    {
        Debug.LogError($"[GameDataExample] 데이터 로드 오류: {error}");
        UpdateStatusText($"오류: {error}");
    }
    
    /// <summary>
    /// 데이터 요약을 출력합니다
    /// </summary>
    private void PrintDataSummary()
    {
        var repo = GameDataRepository.Instance;
        
        // Debug.Log("=== 게임 데이터 요약 ===");
        // Debug.Log($"무기: {repo.Weapons.Count}개");
        // Debug.Log($"방어구: {repo.Armors.Count}개");
        // Debug.Log($"보스 패턴: {repo.BossPatterns.Count}개");
        
        // 무기 타입별 통계
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            var weaponsOfType = repo.GetWeaponsByType(type);
            // Debug.Log($"  {type}: {weaponsOfType.Count}개");
        }
        
        // 방어구 타입별 통계
        foreach (ArmorType type in System.Enum.GetValues(typeof(ArmorType)))
        {
            var armorsOfType = repo.GetArmorsByType(type);
            // Debug.Log($"  {type}: {armorsOfType.Count}개");
        }
        
        // 방어구 레어리티별 통계
        foreach (ArmorRarity rarity in System.Enum.GetValues(typeof(ArmorRarity)))
        {
            var armorsOfRarity = repo.GetArmorsByRarity(rarity);
            // Debug.Log($"  {rarity}: {armorsOfRarity.Count}개");
        }
    }
    
    /// <summary>
    /// 랜덤 드롭 테스트
    /// </summary>
    private void TestRandomDrops()
    {
        var repo = GameDataRepository.Instance;
        
        // 랜덤 무기 드롭
        WeaponData randomWeapon = repo.GetRandomWeapon();
        if (randomWeapon != null)
        {
            // Debug.Log($"[랜덤 드롭] 무기: {randomWeapon.weaponName} ({randomWeapon.weaponType})");
        }
        
        // 특정 타입 랜덤 무기
        WeaponData randomAR = repo.GetRandomWeaponByType(WeaponType.AR);
        if (randomAR != null)
        {
            // Debug.Log($"[랜덤 드롭] AR: {randomAR.weaponName}");
        }
        
        // 랜덤 방어구 드롭
        ArmorData randomArmor = repo.GetRandomArmor();
        if (randomArmor != null)
        {
            // Debug.Log($"[랜덤 드롭] 방어구: {randomArmor.armorName} ({randomArmor.armorType}, {randomArmor.rarity})");
        }
        
        // 특정 레어리티 랜덤 방어구
        ArmorData randomEpic = repo.GetRandomArmorByRarity(ArmorRarity.Epic);
        if (randomEpic != null)
        {
            // Debug.Log($"[랜덤 드롭] Epic 방어구: {randomEpic.armorName}");
        }
    }
    
    /// <summary>
    /// 특정 무기 검색 예제
    /// </summary>
    [ContextMenu("무기 검색 테스트")]
    public void TestWeaponSearch()
    {
        var repo = GameDataRepository.Instance;
        
        // 이름으로 무기 찾기
        WeaponData ak47 = repo.GetWeaponByName("AK-47");
        if (ak47 != null)
        {
            // Debug.Log($"AK-47 찾음: 데미지 {ak47.damage}, 발사속도 {ak47.fireRate}");
        }
        
        // 타입별 무기 목록
        var assaultRifles = repo.GetWeaponsByType(WeaponType.AR);
        // Debug.Log($"돌격소총 {assaultRifles.Count}개:");
        // foreach (var weapon in assaultRifles)
        // {
        //     Debug.Log($"  - {weapon.weaponName}");
        // }
    }
    
    /// <summary>
    /// 특정 방어구 검색 예제
    /// </summary>
    [ContextMenu("방어구 검색 테스트")]
    public void TestArmorSearch()
    {
        var repo = GameDataRepository.Instance;
        
        // 이름으로 방어구 찾기
        ArmorData legendaryArmor = repo.GetArmorByName("전설의 갑옷");
        if (legendaryArmor != null)
        {
            // Debug.Log($"전설의 갑옷 찾음: 방어력 {legendaryArmor.defense}, 체력 +{legendaryArmor.maxHealth}");
        }
        
        // 레어리티별 방어구 목록
        var legendaryArmors = repo.GetArmorsByRarity(ArmorRarity.Legendary);
        // Debug.Log($"전설급 방어구 {legendaryArmors.Count}개:");
        // foreach (var armor in legendaryArmors)
        // {
        //     Debug.Log($"  - {armor.armorName} ({armor.armorType})");
        // }
    }
    
    /// <summary>
    /// 보스 패턴 검색 예제
    /// </summary>
    [ContextMenu("보스 패턴 검색 테스트")]
    public void TestBossPatternSearch()
    {
        var repo = GameDataRepository.Instance;
        
        // 패턴 이름으로 패턴 찾기
        BossAttackPattern bossPattern = repo.GetBossPatternByName("기본 패턴");
        if (bossPattern != null)
        {
            // Debug.Log($"기본 패턴 찾음: 공격 시퀀스 {bossPattern.attackSequences.Count}개");
        }
        
        // 모든 보스 패턴 출력
        // Debug.Log($"보스 패턴 {repo.BossPatterns.Count}개:");
        // foreach (var pattern in repo.BossPatterns)
        // {
        //     Debug.Log($"  - {pattern.patternName}: {pattern.attackSequences.Count}개 시퀀스");
        // }
    }
    
    #region UI 업데이트 메서드
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    private void UpdateWeaponsText(List<WeaponData> weapons)
    {
        if (weaponsText != null)
        {
            weaponsText.text = $"무기: {weapons.Count}개";
        }
    }
    
    private void UpdateArmorsText(List<ArmorData> armors)
    {
        if (armorsText != null)
        {
            armorsText.text = $"방어구: {armors.Count}개";
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameDataRepository.Instance != null)
        {
            GameDataRepository.Instance.OnAllDataLoaded -= OnAllDataLoaded;
            GameDataRepository.Instance.OnWeaponsUpdated -= OnWeaponsUpdated;
            GameDataRepository.Instance.OnArmorsUpdated -= OnArmorsUpdated;
            GameDataRepository.Instance.OnDataLoadError -= OnDataLoadError;
        }
    }
} 