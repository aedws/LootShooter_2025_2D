using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 칩셋 효과를 실제 게임에 적용하는 매니저
/// 무기, 방어구, 플레이어의 칩셋 효과를 계산하고 적용
/// </summary>
public class ChipsetEffectManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Weapon weaponController;
    [SerializeField] private PlayerInventory playerInventory; // 추가
    [SerializeField] private ChipsetManager chipsetManager; // 추가
    
    // 현재 적용된 칩셋 효과들
    private Dictionary<string, float> weaponEffects = new Dictionary<string, float>();
    private Dictionary<string, float> armorEffects = new Dictionary<string, float>();
    private Dictionary<string, float> playerEffects = new Dictionary<string, float>();
    
    // 장착된 플레이어 칩셋 데이터
    private List<PlayerChipsetData> playerEquippedChipsets = new List<PlayerChipsetData>();
    
    // 이벤트
    public System.Action OnEffectsUpdated;
    
    private void Awake()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();
        
        if (weaponController == null)
            weaponController = FindAnyObjectByType<Weapon>();
            
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
            
        if (chipsetManager == null)
            chipsetManager = FindAnyObjectByType<ChipsetManager>();
    }
    
    private void Start()
    {
        // PlayerInventory의 무기 변경 이벤트 구독
        if (playerInventory != null)
        {
            playerInventory.OnWeaponChanged += OnWeaponChanged;
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerInventory != null)
        {
            playerInventory.OnWeaponChanged -= OnWeaponChanged;
        }
    }
    
    /// <summary>
    /// 무기가 변경될 때 호출되는 이벤트 핸들러
    /// </summary>
    private void OnWeaponChanged(WeaponData newWeapon, WeaponData oldWeapon)
    {
        Debug.Log($"🔄 [ChipsetEffectManager] 무기 변경 감지: {(oldWeapon != null ? oldWeapon.weaponName : "없음")} → {(newWeapon != null ? newWeapon.weaponName : "없음")}");
        
        if (newWeapon != null)
        {
            // 새 무기의 칩셋 효과 계산 및 적용
            CalculateWeaponEffects(newWeapon);
            
            // 플레이어 칩셋 효과도 새 무기에 재적용
            ApplyPlayerChipsetEffectsToWeapons();
        }
        else
        {
            // 무기가 없으면 무기 효과 초기화
            weaponEffects.Clear();
            ApplyAllEffects();
        }
    }
    
    /// <summary>
    /// 무기 칩셋 효과 계산 및 적용
    /// </summary>
    public void CalculateWeaponEffects(WeaponData weapon)
    {
        weaponEffects.Clear();
        
        string[] equippedChipsets = weapon.GetEquippedChipsetIds();
        if (equippedChipsets == null || equippedChipsets.Length == 0) return;
        
        foreach (var chipsetId in equippedChipsets)
        {
            if (string.IsNullOrEmpty(chipsetId)) continue;
            
            var chipset = GameDataRepository.Instance.GetWeaponChipsetById(chipsetId);
            if (chipset == null) continue;
            
            ApplyWeaponChipsetEffects(chipset);
        }
        
        ApplyWeaponEffectsToWeapons();
        OnEffectsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 방어구 칩셋 효과 계산 및 적용
    /// </summary>
    public void CalculateArmorEffects(ArmorData armor)
    {
        armorEffects.Clear();
        
        string[] equippedChipsets = armor.GetEquippedChipsetIds();
        if (equippedChipsets == null || equippedChipsets.Length == 0) return;
        
        foreach (var chipsetId in equippedChipsets)
        {
            if (string.IsNullOrEmpty(chipsetId)) continue;
            
            var chipset = GameDataRepository.Instance.GetArmorChipsetById(chipsetId);
            if (chipset == null) continue;
            
            ApplyArmorChipsetEffects(chipset);
        }
        
        ApplyArmorEffectsToPlayer();
        OnEffectsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 플레이어 칩셋 효과 계산 및 적용
    /// </summary>
    public void CalculatePlayerEffects(string[] playerChipsetIds)
    {
        playerEffects.Clear();
        playerEquippedChipsets.Clear();
        
        if (playerChipsetIds == null) return;
        
        foreach (var chipsetId in playerChipsetIds)
        {
            if (string.IsNullOrEmpty(chipsetId)) continue;
            
            var chipset = GameDataRepository.Instance.GetPlayerChipsetById(chipsetId);
            if (chipset == null) continue;
            
            playerEquippedChipsets.Add(chipset);
            ApplyPlayerChipsetEffects(chipset);
        }
        
        ApplyPlayerEffectsToPlayer();
        OnEffectsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 무기 칩셋 효과 적용
    /// </summary>
    private void ApplyWeaponChipsetEffects(WeaponChipsetData chipset)
    {
        switch (chipset.chipsetType)
        {
            case WeaponChipsetType.Damage:
                AddEffect(weaponEffects, "damage", chipset.effectValue);
                break;
            case WeaponChipsetType.FireRate:
                AddEffect(weaponEffects, "fireRate", chipset.effectValue);
                break;
            case WeaponChipsetType.Accuracy:
                AddEffect(weaponEffects, "accuracy", chipset.effectValue);
                break;
            case WeaponChipsetType.Stability:
                AddEffect(weaponEffects, "stability", chipset.effectValue);
                break;
            case WeaponChipsetType.Capacity:
                AddEffect(weaponEffects, "magazineSize", (int)chipset.effectValue);
                break;
            case WeaponChipsetType.Reload:
                AddEffect(weaponEffects, "reloadSpeed", chipset.effectValue);
                break;
            case WeaponChipsetType.Critical:
                AddEffect(weaponEffects, "critical", chipset.effectValue);
                break;
            case WeaponChipsetType.Utility:
                AddEffect(weaponEffects, "utility", chipset.effectValue);
                break;
            case WeaponChipsetType.Synergy:
                AddEffect(weaponEffects, "synergy", chipset.effectValue);
                break;
            case WeaponChipsetType.Hybrid:
                AddEffect(weaponEffects, "hybrid", chipset.effectValue);
                break;
        }
    }
    
    /// <summary>
    /// 방어구 칩셋 효과 적용
    /// </summary>
    private void ApplyArmorChipsetEffects(ArmorChipsetData chipset)
    {
        switch (chipset.chipsetType)
        {
            case ArmorChipsetType.Defense:
                AddEffect(armorEffects, "defense", chipset.effectValue);
                break;
            case ArmorChipsetType.Health:
                AddEffect(armorEffects, "health", chipset.effectValue);
                break;
            case ArmorChipsetType.Speed:
                AddEffect(armorEffects, "movementSpeed", chipset.effectValue);
                break;
            case ArmorChipsetType.JumpForce:
                AddEffect(armorEffects, "jumpForce", chipset.effectValue);
                break;
            case ArmorChipsetType.DashCooldown:
                AddEffect(armorEffects, "dashCooldown", chipset.effectValue);
                break;
            case ArmorChipsetType.Regeneration:
                AddEffect(armorEffects, "regeneration", chipset.effectValue);
                break;
            case ArmorChipsetType.Resistance:
                AddEffect(armorEffects, "elementalResistance", chipset.effectValue);
                break;
            case ArmorChipsetType.Utility:
                AddEffect(armorEffects, "utility", chipset.effectValue);
                break;
            case ArmorChipsetType.Synergy:
                AddEffect(armorEffects, "synergy", chipset.effectValue);
                break;
            case ArmorChipsetType.Hybrid:
                AddEffect(armorEffects, "hybrid", chipset.effectValue);
                break;
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 효과 적용
    /// </summary>
    private void ApplyPlayerChipsetEffects(PlayerChipsetData chipset)
    {
        switch (chipset.chipsetType)
        {
            case PlayerChipsetType.WeaponMastery:
                AddEffect(playerEffects, "weaponMastery", chipset.effectValue);
                break;
            case PlayerChipsetType.CombatExpert:
                AddEffect(playerEffects, "combatExpert", chipset.effectValue);
                break;
            case PlayerChipsetType.Survivor:
                AddEffect(playerEffects, "survivor", chipset.effectValue);
                break;
            case PlayerChipsetType.Speedster:
                AddEffect(playerEffects, "speedster", chipset.effectValue);
                break;
            case PlayerChipsetType.Tactical:
                AddEffect(playerEffects, "tactical", chipset.effectValue);
                break;
            case PlayerChipsetType.Synergy:
                AddEffect(playerEffects, "synergy", chipset.effectValue);
                break;
            case PlayerChipsetType.Hybrid:
                AddEffect(playerEffects, "hybrid", chipset.effectValue);
                break;
            case PlayerChipsetType.Ultimate:
                AddEffect(playerEffects, "ultimate", chipset.effectValue);
                break;
        }
    }
    
    /// <summary>
    /// 효과 딕셔너리에 값 추가
    /// </summary>
    private void AddEffect(Dictionary<string, float> effects, string key, float value)
    {
        if (effects.ContainsKey(key))
        {
            effects[key] += value;
        }
        else
        {
            effects[key] = value;
        }
    }
    
    /// <summary>
    /// 무기 효과를 무기에 적용
    /// </summary>
    private void ApplyWeaponEffectsToWeapons()
    {
        // 현재 활성화된 모든 무기들을 찾아서 효과 적용
        var activeWeapons = FindObjectsByType<Weapon>(FindObjectsSortMode.None);
        
        foreach (var weapon in activeWeapons)
        {
            // 데미지 증가
            if (weaponEffects.ContainsKey("damage"))
            {
                weapon.SetDamageMultiplier(1f + weaponEffects["damage"]);
            }
            
            // 발사 속도 증가
            if (weaponEffects.ContainsKey("fireRate"))
            {
                weapon.SetFireRateMultiplier(1f + weaponEffects["fireRate"]);
            }
            
            // 사거리 증가
            if (weaponEffects.ContainsKey("range"))
            {
                weapon.SetRangeMultiplier(1f + weaponEffects["range"]);
            }
            
            // 정확도 증가
            if (weaponEffects.ContainsKey("accuracy"))
            {
                weapon.SetAccuracyMultiplier(1f + weaponEffects["accuracy"]);
            }
            
            // 재장전 속도 증가
            if (weaponEffects.ContainsKey("reloadSpeed"))
            {
                weapon.SetReloadSpeedMultiplier(1f + weaponEffects["reloadSpeed"]);
            }
            
            // 탄창 크기 증가
            if (weaponEffects.ContainsKey("magazineSize"))
            {
                weapon.SetMagazineSizeBonus((int)weaponEffects["magazineSize"]);
            }
            
            // 관통력 증가
            if (weaponEffects.ContainsKey("penetration"))
            {
                weapon.SetPenetrationBonus(weaponEffects["penetration"]);
            }
            
            // 폭발 효과
            if (weaponEffects.ContainsKey("explosive"))
            {
                weapon.SetExplosiveEffect(weaponEffects["explosive"]);
            }
        }
    }
    
    /// <summary>
    /// 방어구 효과를 플레이어에 적용
    /// </summary>
    private void ApplyArmorEffectsToPlayer()
    {
        if (playerController == null) return;
        
        // 방어력 증가
        if (armorEffects.ContainsKey("defense"))
        {
            playerController.SetDefenseBonus(armorEffects["defense"]);
        }
        
        // 체력 증가
        if (armorEffects.ContainsKey("health"))
        {
            playerController.SetHealthBonus(armorEffects["health"]);
        }
        
        // 이동 속도 증가
        if (armorEffects.ContainsKey("movementSpeed"))
        {
            playerController.SetMovementSpeedMultiplier(1f + armorEffects["movementSpeed"]);
        }
        
        // 회피 확률 증가
        if (armorEffects.ContainsKey("dodgeChance"))
        {
            playerController.SetDodgeChanceBonus(armorEffects["dodgeChance"]);
        }
        
        // 블록 확률 증가
        if (armorEffects.ContainsKey("blockChance"))
        {
            playerController.SetBlockChanceBonus(armorEffects["blockChance"]);
        }
        
        // 체력 재생 증가
        if (armorEffects.ContainsKey("regeneration"))
        {
            playerController.SetRegenerationBonus(armorEffects["regeneration"]);
        }
        
        // 원소 저항 증가
        if (armorEffects.ContainsKey("elementalResistance"))
        {
            playerController.SetElementalResistanceBonus(armorEffects["elementalResistance"]);
        }
        
        // 무게 감소
        if (armorEffects.ContainsKey("weightReduction"))
        {
            playerController.SetWeightReductionBonus(armorEffects["weightReduction"]);
        }
    }
    
    /// <summary>
    /// 플레이어 효과를 플레이어에 적용
    /// </summary>
    private void ApplyPlayerEffectsToPlayer()
    {
        if (playerController == null) return;
        
        // 경험치 획득 증가
        if (playerEffects.ContainsKey("experienceGain"))
        {
            playerController.SetExperienceGainMultiplier(1f + playerEffects["experienceGain"]);
        }
        
        // 행운 증가
        if (playerEffects.ContainsKey("luck"))
        {
            playerController.SetLuckBonus(playerEffects["luck"]);
        }
        
        // 크리티컬 확률 증가
        if (playerEffects.ContainsKey("criticalChance"))
        {
            playerController.SetCriticalChanceBonus(playerEffects["criticalChance"]);
        }
        
        // 크리티컬 데미지 증가
        if (playerEffects.ContainsKey("criticalDamage"))
        {
            playerController.SetCriticalDamageMultiplier(1f + playerEffects["criticalDamage"]);
        }
        
        // 스킬 쿨다운 감소
        if (playerEffects.ContainsKey("skillCooldown"))
        {
            playerController.SetSkillCooldownMultiplier(1f - playerEffects["skillCooldown"]);
        }
        
        // 자원 획득 증가
        if (playerEffects.ContainsKey("resourceGain"))
        {
            playerController.SetResourceGainMultiplier(1f + playerEffects["resourceGain"]);
        }
        
        // 특수 능력
        if (playerEffects.ContainsKey("specialAbility"))
        {
            playerController.SetSpecialAbilityBonus(playerEffects["specialAbility"]);
        }
        
        // 유틸리티
        if (playerEffects.ContainsKey("utility"))
        {
            playerController.SetUtilityBonus(playerEffects["utility"]);
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋 효과를 무기에도 적용
    /// </summary>
    private void ApplyPlayerChipsetEffectsToWeapons()
    {
        // 현재 활성화된 모든 무기들을 찾아서 효과 적용
        var activeWeapons = FindObjectsByType<Weapon>(FindObjectsSortMode.None);
        
        foreach (var weapon in activeWeapons)
        {
            // 플레이어 칩셋의 무기 관련 효과들을 적용
            ApplyPlayerWeaponBonuses(weapon);
        }
    }
    
    /// <summary>
    /// 플레이어 칩셋의 무기 보너스를 특정 무기에 적용
    /// </summary>
    private void ApplyPlayerWeaponBonuses(Weapon weapon)
    {
        if (weapon == null) return;
        
        // 플레이어 칩셋 데이터에서 무기 보너스 가져오기
        float weaponDamageBonus = 0f;
        float weaponFireRateBonus = 0f;
        float weaponAccuracyBonus = 0f;
        float weaponReloadSpeedBonus = 0f;
        float weaponCriticalChanceBonus = 0f;
        float weaponCriticalMultiplierBonus = 0f;
        
        // 장착된 플레이어 칩셋들을 순회하며 무기 보너스 합산
        foreach (var chipsetData in playerEquippedChipsets)
        {
            if (chipsetData != null)
            {
                weaponDamageBonus += chipsetData.weaponDamageBonus;
                weaponFireRateBonus += chipsetData.weaponFireRateBonus;
                weaponAccuracyBonus += chipsetData.weaponAccuracyBonus;
                weaponReloadSpeedBonus += chipsetData.weaponReloadSpeedBonus;
                weaponCriticalChanceBonus += chipsetData.weaponCriticalChanceBonus;
                weaponCriticalMultiplierBonus += chipsetData.weaponCriticalMultiplierBonus;
            }
        }
        
        // 무기에 플레이어 칩셋 보너스 적용 (기존 무기 칩셋 효과에 추가)
        float currentDamageMultiplier = weapon.GetDamageMultiplier();
        weapon.SetDamageMultiplier(currentDamageMultiplier + weaponDamageBonus);
        
        float currentFireRateMultiplier = weapon.GetFireRateMultiplier();
        weapon.SetFireRateMultiplier(currentFireRateMultiplier + weaponFireRateBonus);
        
        float currentAccuracyMultiplier = weapon.GetAccuracyMultiplier();
        weapon.SetAccuracyMultiplier(currentAccuracyMultiplier + weaponAccuracyBonus);
        
        float currentReloadSpeedMultiplier = weapon.GetReloadSpeedMultiplier();
        weapon.SetReloadSpeedMultiplier(currentReloadSpeedMultiplier + weaponReloadSpeedBonus);
        
        // 크리티컬 관련 효과는 WeaponData에 직접 추가
        if (weapon.weaponData != null && (weaponCriticalChanceBonus > 0 || weaponCriticalMultiplierBonus > 0))
        {
            // 플레이어 칩셋의 크리티컬 보너스를 임시로 저장
            weapon.SetPlayerCriticalBonus(weaponCriticalChanceBonus, weaponCriticalMultiplierBonus);
        }
    }
    
    /// <summary>
    /// 특정 효과 값 가져오기
    /// </summary>
    public float GetWeaponEffect(string effectName)
    {
        return weaponEffects.ContainsKey(effectName) ? weaponEffects[effectName] : 0f;
    }
    
    public float GetArmorEffect(string effectName)
    {
        return armorEffects.ContainsKey(effectName) ? armorEffects[effectName] : 0f;
    }
    
    public float GetPlayerEffect(string effectName)
    {
        return playerEffects.ContainsKey(effectName) ? playerEffects[effectName] : 0f;
    }
    
    /// <summary>
    /// 모든 효과 초기화
    /// </summary>
    public void ClearAllEffects()
    {
        weaponEffects.Clear();
        armorEffects.Clear();
        playerEffects.Clear();
        
        var activeWeapons = FindObjectsByType<Weapon>(FindObjectsSortMode.None);
        foreach (var weapon in activeWeapons)
        {
            weapon.ResetAllMultipliers();
        }
        
        if (playerController != null)
        {
            playerController.ResetAllMultipliers();
        }
        
        OnEffectsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 모든 효과를 적용
    /// </summary>
    private void ApplyAllEffects()
    {
        Debug.Log("🔧 [ChipsetEffectManager] 모든 칩셋 효과 적용 시작");
        
        // 무기 효과 적용
        ApplyWeaponEffectsToWeapons();
        
        // 방어구 효과 적용
        ApplyArmorEffectsToPlayer();
        
        // 플레이어 효과 적용
        ApplyPlayerEffectsToPlayer();
        
        // 플레이어 칩셋 효과를 무기에도 적용
        ApplyPlayerChipsetEffectsToWeapons();
        
        Debug.Log("✅ [ChipsetEffectManager] 모든 칩셋 효과 적용 완료");
    }
    
    /// <summary>
    /// 효과 요약 정보 반환
    /// </summary>
    public string GetEffectsSummary()
    {
        var summary = "=== 칩셋 효과 요약 ===\n";
        
        summary += "\n[무기 효과]\n";
        foreach (var effect in weaponEffects)
        {
            summary += $"{effect.Key}: +{effect.Value:F2}\n";
        }
        
        summary += "\n[방어구 효과]\n";
        foreach (var effect in armorEffects)
        {
            summary += $"{effect.Key}: +{effect.Value:F2}\n";
        }
        
        summary += "\n[플레이어 효과]\n";
        foreach (var effect in playerEffects)
        {
            summary += $"{effect.Key}: +{effect.Value:F2}\n";
        }
        
        return summary;
    }
} 