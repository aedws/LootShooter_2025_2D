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
    
    [Tooltip("무기 등급 (1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Primordial)")]
    [Range(1, 5)]
    public int weaponTier = 1;
    
    [Tooltip("런타임에 등급을 랜덤하게 설정할지 여부")]
    public bool useRandomTier = true;
    
    [Tooltip("랜덤 등급 사용시 최소 등급")]
    [Range(1, 5)]
    public int minTier = 1;
    
    [Tooltip("랜덤 등급 사용시 최대 등급")]
    [Range(1, 5)]
    public int maxTier = 5;
    
    [Header("디버그")]
    [Tooltip("디버그 모드 활성화")]
    public bool debugMode = false;
    
    // 동적으로 설정될 무기 데이터
    private WeaponData weaponData;
    
    void Start()
    {
        // 런타임에 랜덤 등급 설정
        if (useRandomTier)
        {
            weaponTier = UnityEngine.Random.Range(minTier, maxTier + 1);
            if (debugMode)
                Debug.Log($"[NetworkWeaponPickup] 랜덤 등급 설정: {weaponTier} (범위: {minTier}-{maxTier})");
        }
        
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
        
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] {weaponType} 타입, {weaponTier} 등급 무기 찾기 시작");
        
        // 해당 타입의 모든 무기 찾기
        var weaponsOfType = gameDataRepo.GetWeaponsByType(weaponType);
        
        if (debugMode)
        {
            Debug.Log($"[NetworkWeaponPickup] {weaponType} 타입 무기들:");
            foreach (var weapon in weaponsOfType)
            {
                Debug.Log($"[NetworkWeaponPickup]   - {weapon.weaponName} (등급: {weapon.rarity})");
            }
        }
        
        if (weaponsOfType.Count == 0)
        {
            Debug.LogError($"[NetworkWeaponPickup] {weaponType} 타입의 무기가 없습니다!");
            return;
        }
        
        // 등급에 따른 무기 선택
        WeaponData selectedWeapon = null;
        
        // 등급별로 무기 분류
        var weaponsByRarity = new Dictionary<WeaponRarity, List<WeaponData>>();
        foreach (var weapon in weaponsOfType)
        {
            if (!weaponsByRarity.ContainsKey(weapon.rarity))
            {
                weaponsByRarity[weapon.rarity] = new List<WeaponData>();
            }
            weaponsByRarity[weapon.rarity].Add(weapon);
        }
        
        // 등급별 가중치 설정 (1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Primordial)
        var rarityWeights = new Dictionary<WeaponRarity, float>
        {
            { WeaponRarity.Common, 1.0f },
            { WeaponRarity.Rare, 1.5f },
            { WeaponRarity.Epic, 2.0f },
            { WeaponRarity.Legendary, 3.0f },
            { WeaponRarity.Primordial, 4.0f }
        };
        
        // weaponTier에 따른 등급 선택
        WeaponRarity targetRarity = WeaponRarity.Common; // 기본값
        
        switch (weaponTier)
        {
            case 1: targetRarity = WeaponRarity.Common; break;
            case 2: targetRarity = WeaponRarity.Rare; break;
            case 3: targetRarity = WeaponRarity.Epic; break;
            case 4: targetRarity = WeaponRarity.Legendary; break;
            case 5: targetRarity = WeaponRarity.Primordial; break;
            default: targetRarity = WeaponRarity.Common; break;
        }
        
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] 목표 등급: {targetRarity} (Tier: {weaponTier})");
        
        // 목표 등급의 무기가 있으면 선택, 없으면 다른 등급에서 선택
        if (weaponsByRarity.ContainsKey(targetRarity) && weaponsByRarity[targetRarity].Count > 0)
        {
            // 목표 등급에서 랜덤 선택
            selectedWeapon = weaponsByRarity[targetRarity][UnityEngine.Random.Range(0, weaponsByRarity[targetRarity].Count)];
            if (debugMode)
                Debug.Log($"[NetworkWeaponPickup] 목표 등급에서 선택: {selectedWeapon.weaponName}");
        }
        else
        {
            // 목표 등급이 없으면 가중치 기반으로 선택
            var candidates = new List<WeaponData>();
            foreach (var rarity in weaponsByRarity.Keys)
            {
                if (weaponsByRarity[rarity].Count > 0)
                {
                    // 해당 등급에서 랜덤 선택
                    var randomWeapon = weaponsByRarity[rarity][UnityEngine.Random.Range(0, weaponsByRarity[rarity].Count)];
                    candidates.Add(randomWeapon);
                }
            }
            
            if (candidates.Count > 0)
            {
                // 가중치 기반 최종 선택
                selectedWeapon = SelectWeaponByWeight(candidates);
                if (debugMode)
                    Debug.Log($"[NetworkWeaponPickup] 가중치 기반 선택: {selectedWeapon.weaponName}");
            }
            else
            {
                // 폴백: 첫 번째 무기 선택
                selectedWeapon = weaponsOfType[0];
                if (debugMode)
                    Debug.Log($"[NetworkWeaponPickup] 폴백 선택: {selectedWeapon.weaponName}");
            }
        }
        
        weaponData = selectedWeapon;
        
        // 🎨 무기 아이콘 및 색상 설정
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (weaponData != null && spriteRenderer != null)
        {
            if (weaponData.icon != null)
                spriteRenderer.sprite = weaponData.icon;
            spriteRenderer.color = weaponData.GetRarityColor();
            
            if (debugMode)
                Debug.Log($"[NetworkWeaponPickup] 무기 시각적 설정 완료: {weaponData.weaponName} -> 색상: {weaponData.GetRarityColor()}");
        }
        
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] 최종 선택된 무기: {weaponData.weaponName} (등급: {weaponData.rarity})");
    }
    
    /// <summary>
    /// 가중치 기반으로 무기를 선택합니다
    /// </summary>
    private WeaponData SelectWeaponByWeight(List<WeaponData> weapons)
    {
        if (weapons.Count == 0) return null;
        if (weapons.Count == 1) return weapons[0];
        
        // 등급별 가중치 설정
        var weights = new Dictionary<WeaponRarity, float>
        {
            { WeaponRarity.Common, 1.0f },
            { WeaponRarity.Rare, 1.5f },
            { WeaponRarity.Epic, 2.0f },
            { WeaponRarity.Legendary, 3.0f },
            { WeaponRarity.Primordial, 4.0f }
        };
        
        // 각 무기의 가중치 계산
        var weaponWeights = new List<float>();
        foreach (var weapon in weapons)
        {
            float weight = weights.ContainsKey(weapon.rarity) ? weights[weapon.rarity] : 1.0f;
            weaponWeights.Add(weight);
            
            if (debugMode)
                Debug.Log($"[NetworkWeaponPickup] 무기 '{weapon.weaponName}' 가중치: {weight}");
        }
        
        // 가중치 기반 랜덤 선택
        float totalWeight = 0f;
        foreach (float weight in weaponWeights)
        {
            totalWeight += weight;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        for (int i = 0; i < weapons.Count; i++)
        {
            currentWeight += weaponWeights[i];
            if (randomValue <= currentWeight)
            {
                return weapons[i];
            }
        }
        
        // 폴백
        return weapons[weapons.Count - 1];
    }
    
    public void OnPickup(GameObject player)
    {
        if (weaponData == null)
        {
            Debug.LogError("[NetworkWeaponPickup] weaponData가 null입니다! 무기 데이터 설정을 확인하세요.");
            return;
        }
        
        if (debugMode)
            Debug.Log($"[NetworkWeaponPickup] 무기 픽업: {weaponData.weaponName} (등급: {weaponData.rarity}, Tier: {weaponTier})");
        
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