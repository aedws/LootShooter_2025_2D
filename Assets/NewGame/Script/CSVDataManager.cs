using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// CSV 파일을 사용한 간단한 데이터 매니저
/// 구글 스프레드시트를 CSV로 내보내서 사용하는 방식
/// </summary>
public class CSVDataManager : MonoBehaviour
{
    [Header("CSV 파일 설정")]
    [Tooltip("무기 데이터 CSV 파일 (Resources 폴더 내)")]
    public TextAsset weaponsCSV;
    
    [Tooltip("방어구 데이터 CSV 파일 (Resources 폴더 내)")]
    public TextAsset armorsCSV;
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = true;
    
    // 캐시된 데이터
    private List<WeaponData> cachedWeapons;
    private List<ArmorData> cachedArmors;
    
    // 이벤트
    public static event System.Action<List<WeaponData>> OnWeaponsLoaded;
    public static event System.Action<List<ArmorData>> OnArmorsLoaded;
    public static event System.Action<string> OnError;
    
    private void Start()
    {
        LoadAllData();
    }
    
    /// <summary>
    /// 모든 데이터를 로드합니다.
    /// </summary>
    public void LoadAllData()
    {
        LoadWeaponsFromCSV();
        LoadArmorsFromCSV();
    }
    
    /// <summary>
    /// CSV에서 무기 데이터를 로드합니다.
    /// </summary>
    public void LoadWeaponsFromCSV()
    {
        if (weaponsCSV == null)
        {
            LogError("무기 CSV 파일이 설정되지 않았습니다.");
            return;
        }
        
        try
        {
            cachedWeapons = ParseWeaponCSV(weaponsCSV.text);
            LogDebug($"무기 데이터 로드 완료: {cachedWeapons.Count}개");
            OnWeaponsLoaded?.Invoke(cachedWeapons);
        }
        catch (Exception e)
        {
            LogError($"무기 CSV 파싱 오류: {e.Message}");
            OnError?.Invoke($"무기 CSV 파싱 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// CSV에서 방어구 데이터를 로드합니다.
    /// </summary>
    public void LoadArmorsFromCSV()
    {
        if (armorsCSV == null)
        {
            LogError("방어구 CSV 파일이 설정되지 않았습니다.");
            return;
        }
        
        try
        {
            cachedArmors = ParseArmorCSV(armorsCSV.text);
            LogDebug($"방어구 데이터 로드 완료: {cachedArmors.Count}개");
            OnArmorsLoaded?.Invoke(cachedArmors);
        }
        catch (Exception e)
        {
            LogError($"방어구 CSV 파싱 오류: {e.Message}");
            OnError?.Invoke($"방어구 CSV 파싱 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// CSV 텍스트를 무기 데이터로 파싱합니다.
    /// </summary>
    private List<WeaponData> ParseWeaponCSV(string csvText)
    {
        var weapons = new List<WeaponData>();
        var lines = csvText.Split('\n');
        
        if (lines.Length < 2)
        {
            LogError("CSV 파일에 데이터가 없습니다.");
            return weapons;
        }
        
        // 첫 번째 행은 헤더
        var headers = ParseCSVLine(lines[0]);
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            var values = ParseCSVLine(lines[i]);
            if (values.Length < headers.Length) continue;
            
            var weaponData = ScriptableObject.CreateInstance<WeaponData>();
            
            // 헤더와 값을 매핑
            for (int j = 0; j < headers.Length; j++)
            {
                if (j < values.Length)
                {
                    SetWeaponValue(weaponData, headers[j], values[j]);
                }
            }
            
            weapons.Add(weaponData);
        }
        
        return weapons;
    }
    
    /// <summary>
    /// CSV 텍스트를 방어구 데이터로 파싱합니다.
    /// </summary>
    private List<ArmorData> ParseArmorCSV(string csvText)
    {
        var armors = new List<ArmorData>();
        var lines = csvText.Split('\n');
        
        if (lines.Length < 2)
        {
            LogError("CSV 파일에 데이터가 없습니다.");
            return armors;
        }
        
        // 첫 번째 행은 헤더
        var headers = ParseCSVLine(lines[0]);
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            var values = ParseCSVLine(lines[i]);
            if (values.Length < headers.Length) continue;
            
            var armorData = ScriptableObject.CreateInstance<ArmorData>();
            
            // 헤더와 값을 매핑
            for (int j = 0; j < headers.Length; j++)
            {
                if (j < values.Length)
                {
                    SetArmorValue(armorData, headers[j], values[j]);
                }
            }
            
            armors.Add(armorData);
        }
        
        return armors;
    }
    
    /// <summary>
    /// CSV 라인을 파싱합니다 (쉼표로 구분, 따옴표 처리).
    /// </summary>
    private string[] ParseCSVLine(string line)
    {
        var values = new List<string>();
        var current = "";
        bool inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        
        values.Add(current.Trim());
        return values.ToArray();
    }
    
    /// <summary>
    /// 무기 데이터에 값을 설정합니다.
    /// </summary>
    private void SetWeaponValue(WeaponData weapon, string field, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        switch (field.ToLower())
        {
            case "weaponname":
                weapon.weaponName = value;
                break;
            case "weapontype":
                weapon.weaponType = ParseWeaponType(value);
                break;
            case "flavortext":
                weapon.flavorText = value;
                break;
            case "firerate":
                weapon.fireRate = ParseFloat(value);
                break;
            case "damage":
                weapon.damage = ParseInt(value);
                break;
            case "projectilespeed":
                weapon.projectileSpeed = ParseFloat(value);
                break;
            case "maxammo":
                weapon.maxAmmo = ParseInt(value);
                break;
            case "currentammo":
                weapon.currentAmmo = ParseInt(value);
                break;
            case "reloadtime":
                weapon.reloadTime = ParseFloat(value);
                break;
            case "infiniteammo":
                weapon.infiniteAmmo = ParseBool(value);
                break;
            case "basespread":
                weapon.baseSpread = ParseFloat(value);
                break;
            case "maxspread":
                weapon.maxSpread = ParseFloat(value);
                break;
            case "spreadincreaserate":
                weapon.spreadIncreaseRate = ParseFloat(value);
                break;
            case "spreaddecreaserate":
                weapon.spreadDecreaseRate = ParseFloat(value);
                break;
            case "pelletspershot":
                weapon.pelletsPerShot = ParseInt(value);
                break;
            case "shotgunspreadangle":
                weapon.shotgunSpreadAngle = ParseFloat(value);
                break;
            case "warmuptime":
                weapon.warmupTime = ParseFloat(value);
                break;
            case "maxwarmupfirerate":
                weapon.maxWarmupFireRate = ParseFloat(value);
                break;
            case "singlefireonly":
                weapon.singleFireOnly = ParseBool(value);
                break;
            case "aimingrange":
                weapon.aimingRange = ParseFloat(value);
                break;
            case "movementspeedmultiplier":
                weapon.movementSpeedMultiplier = ParseFloat(value);
                break;
            case "recoilforce":
                weapon.recoilForce = ParseFloat(value);
                break;
            case "recoilduration":
                weapon.recoilDuration = ParseFloat(value);
                break;
            case "recoilrecoveryspeed":
                weapon.recoilRecoverySpeed = ParseFloat(value);
                break;
            case "criticalchance":
                weapon.criticalChance = ParseFloat(value);
                break;
            case "criticalmultiplier":
                weapon.criticalMultiplier = ParseFloat(value);
                break;
            case "piercecount":
                weapon.pierceCount = ParseInt(value);
                break;
            case "piercedamagereduction":
                weapon.pierceDamageReduction = ParseFloat(value);
                break;
            case "hastracerrounds":
                weapon.hasTracerRounds = ParseBool(value);
                break;
            case "hasmuzzleflash":
                weapon.hasMuzzleFlash = ParseBool(value);
                break;
            case "hasexplosivekills":
                weapon.hasExplosiveKills = ParseBool(value);
                break;
            case "explosionradius":
                weapon.explosionRadius = ParseFloat(value);
                break;
        }
    }
    
    /// <summary>
    /// 방어구 데이터에 값을 설정합니다.
    /// </summary>
    private void SetArmorValue(ArmorData armor, string field, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        switch (field.ToLower())
        {
            case "armorname":
                armor.armorName = value;
                break;
            case "armortype":
                armor.armorType = ParseArmorType(value);
                break;
            case "rarity":
                armor.rarity = ParseArmorRarity(value);
                break;
            case "description":
                armor.description = value;
                break;
            case "defense":
                armor.defense = ParseInt(value);
                break;
            case "maxhealth":
                armor.maxHealth = ParseInt(value);
                break;
            case "damagereduction":
                armor.damageReduction = ParseFloat(value);
                break;
            case "movespeedbonus":
                armor.moveSpeedBonus = ParseFloat(value);
                break;
            case "jumpforcebonus":
                armor.jumpForceBonus = ParseFloat(value);
                break;
            case "dashcooldownreduction":
                armor.dashCooldownReduction = ParseFloat(value);
                break;
            case "hasregeneration":
                armor.hasRegeneration = ParseBool(value);
                break;
            case "regenerationrate":
                armor.regenerationRate = ParseFloat(value);
                break;
            case "hasinvincibilityframe":
                armor.hasInvincibilityFrame = ParseBool(value);
                break;
            case "invincibilitybonus":
                armor.invincibilityBonus = ParseFloat(value);
                break;
        }
    }
    
    // 파싱 헬퍼 메서드들
    private int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }
    
    private float ParseFloat(string value)
    {
        return float.TryParse(value, out float result) ? result : 0f;
    }
    
    private bool ParseBool(string value)
    {
        return bool.TryParse(value, out bool result) ? result : false;
    }
    
    private WeaponType ParseWeaponType(string typeString)
    {
        switch (typeString.ToUpper())
        {
            case "AR": return WeaponType.AR;
            case "HG": return WeaponType.HG;
            case "MG": return WeaponType.MG;
            case "SG": return WeaponType.SG;
            case "SMG": return WeaponType.SMG;
            case "SR": return WeaponType.SR;
            default: return WeaponType.AR;
        }
    }
    
    private ArmorType ParseArmorType(string typeString)
    {
        switch (typeString.ToLower())
        {
            case "helmet": return ArmorType.Helmet;
            case "chest": return ArmorType.Chest;
            case "legs": return ArmorType.Legs;
            case "boots": return ArmorType.Boots;
            case "shoulder": return ArmorType.Shoulder;
            case "accessory": return ArmorType.Accessory;
            default: return ArmorType.Chest;
        }
    }
    
    private ArmorRarity ParseArmorRarity(string rarityString)
    {
        switch (rarityString.ToLower())
        {
            case "common": return ArmorRarity.Common;
            case "rare": return ArmorRarity.Rare;
            case "epic": return ArmorRarity.Epic;
            case "legendary": return ArmorRarity.Legendary;
            default: return ArmorRarity.Common;
        }
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[CSVDataManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[CSVDataManager] {message}");
    }
} 