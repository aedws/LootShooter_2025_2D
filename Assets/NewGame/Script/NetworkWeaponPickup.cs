using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 네트워크 무기 픽업 프리팹용 스크립트
/// GoogleSheets에서 로드된 무기 데이터를 동적으로 설정합니다.
/// </summary>
public class NetworkWeaponPickup : MonoBehaviour, IItemPickup
{
    [Header("🔧 네트워크 무기 픽업 설정")]
    [Tooltip("이 픽업이 생성할 무기 타입")]
    public WeaponType weaponType;
    
    [Tooltip("무기 등급 (1, 2, 3)")]
    [Range(1, 3)]
    public int weaponTier = 1;
    
    [Header("디버그")]
    [Tooltip("디버그 모드 활성화")]
    public bool debugMode = false;
    
    // 동적으로 설정될 무기 데이터
    private WeaponData weaponData;
    
    void Start()
    {
        // GoogleSheets에서 로드된 무기 데이터를 찾아서 설정
        SetupWeaponData();
    }
    
    /// <summary>
    /// GoogleSheets에서 로드된 무기 데이터를 찾아서 설정합니다.
    /// </summary>
    void SetupWeaponData()
    {
        // GameDataRepository에서 무기 데이터 가져오기
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null)
        {
            Debug.LogError("[NetworkWeaponPickup] GameDataRepository를 찾을 수 없습니다!");
            return;
        }
        
        // 무기 데이터가 로드될 때까지 대기
        if (!gameDataRepo.IsWeaponsLoaded)
        {
            Debug.LogWarning("[NetworkWeaponPickup] 무기 데이터가 아직 로드되지 않았습니다. 이벤트를 구독합니다.");
            gameDataRepo.OnWeaponsUpdated += OnWeaponsLoaded;
            return;
        }
        
        // 무기 데이터 찾기
        FindAndSetWeaponData();
    }
    
    /// <summary>
    /// 무기 데이터가 로드되었을 때 호출됩니다.
    /// </summary>
    void OnWeaponsLoaded(List<WeaponData> weapons)
    {
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] 무기 데이터 로드됨: {weapons.Count}개");
        
        FindAndSetWeaponData();
        
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnWeaponsUpdated -= OnWeaponsLoaded;
        }
    }
    
    /// <summary>
    /// 무기 타입과 등급에 맞는 무기 데이터를 찾아서 설정합니다.
    /// </summary>
    void FindAndSetWeaponData()
    {
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null || !gameDataRepo.IsWeaponsLoaded)
        {
            Debug.LogError("[NetworkWeaponPickup] GameDataRepository가 없거나 무기 데이터가 로드되지 않았습니다!");
            return;
        }
        
        // 무기 타입과 등급에 맞는 무기 찾기
        string targetWeaponName = $"{weaponType}_{weaponTier}";
        
        foreach (var weapon in gameDataRepo.Weapons)
        {
            if (weapon.weaponName == targetWeaponName)
            {
                weaponData = weapon;
                if (debugMode)
                    Debug.Log($"[NetworkWeaponPickup] 무기 데이터 설정 완료: {weaponData.weaponName}");
                return;
            }
        }
        
        // 정확한 이름을 찾지 못한 경우, 무기 타입만으로 찾기
        foreach (var weapon in gameDataRepo.Weapons)
        {
            if (weapon.weaponType == weaponType)
            {
                weaponData = weapon;
                if (debugMode)
                    Debug.LogWarning($"[NetworkWeaponPickup] 정확한 등급을 찾지 못해 첫 번째 {weaponType} 무기를 사용: {weaponData.weaponName}");
                return;
            }
        }
        
        Debug.LogError($"[NetworkWeaponPickup] {weaponType} 타입의 무기를 찾을 수 없습니다!");
    }
    
    public void OnPickup(GameObject player)
    {
        if (weaponData == null)
        {
            Debug.LogError("[NetworkWeaponPickup] weaponData가 null입니다! 무기 데이터 설정을 확인하세요.");
            return;
        }
        
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] 무기 픽업: {weaponData.weaponName}");
        
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddWeapon(weaponData);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("[NetworkWeaponPickup] PlayerInventory 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnWeaponsUpdated -= OnWeaponsLoaded;
        }
    }
} 