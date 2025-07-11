using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;

[System.Serializable]
public class SimpleGoogleSheetsConfig
{
    [Header("구글 스프레드시트 설정")]
    [Tooltip("구글 스프레드시트 ID (URL에서 추출)")]
    public string spreadsheetId = "";
    
    [Tooltip("API 키 (구글 클라우드 콘솔에서 생성)")]
    public string apiKey = "";
    
    [Header("시트 이름")]
    [Tooltip("무기 데이터가 있는 시트 이름")]
    public string weaponsSheetName = "Weapons";
    
    [Tooltip("방어구 데이터가 있는 시트 이름")]
    public string armorsSheetName = "Armors";
}

[System.Serializable]
public class SimpleGoogleSheetsResponse
{
    public string range;
    public string majorDimension;
    public List<List<string>> values;
}

public class SimpleGoogleSheetsManager : MonoBehaviour
{
    [Header("설정")]
    public SimpleGoogleSheetsConfig config;
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = true;
    
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
        StartCoroutine(LoadWeaponsData());
        StartCoroutine(LoadArmorsData());
    }
    
    /// <summary>
    /// 무기 데이터를 구글 스프레드시트에서 로드합니다.
    /// </summary>
    public IEnumerator LoadWeaponsData()
    {
        if (string.IsNullOrEmpty(config.spreadsheetId) || string.IsNullOrEmpty(config.apiKey))
        {
            LogError("스프레드시트 ID 또는 API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.spreadsheetId}/values/{config.weaponsSheetName}?key={config.apiKey}";
        LogDebug($"무기 데이터 요청 URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    LogDebug($"무기 API 응답: {responseText}");
                    
                    // 응답이 비어있는지 확인
                    if (string.IsNullOrEmpty(responseText))
                    {
                        LogError("API 응답이 비어있습니다.");
                        yield break;
                    }
                    
                    // Newtonsoft.Json 사용
                    var response = JsonConvert.DeserializeObject<SimpleGoogleSheetsResponse>(responseText);
                    
                    if (response == null)
                    {
                        LogError("JSON 파싱 실패. 응답 형식을 확인하세요.");
                        yield break;
                    }
                    
                    LogDebug($"파싱된 응답 - range: {response.range}, majorDimension: {response.majorDimension}");
                    
                    if (response.values == null)
                    {
                        LogError("values 필드가 null입니다. 스프레드시트에 데이터가 있는지 확인하세요.");
                        LogDebug("가능한 원인:");
                        LogDebug("1. 시트 이름이 잘못되었습니다 (현재: " + config.weaponsSheetName + ")");
                        LogDebug("2. 스프레드시트가 공유되지 않았습니다");
                        LogDebug("3. 스프레드시트에 데이터가 없습니다");
                        yield break;
                    }
                    
                    LogDebug($"values 배열 크기: {response.values.Count}");
                    if (response.values.Count > 0)
                    {
                        LogDebug($"첫 번째 행 (헤더): {string.Join(", ", response.values[0])}");
                    }
                    
                    var weaponList = ParseWeaponData(response);
                    LogDebug($"무기 데이터 로드 완료: {weaponList.Count}개");
                    OnWeaponsLoaded?.Invoke(weaponList);
                }
                catch (Exception e)
                {
                    LogError($"무기 데이터 파싱 오류: {e.Message}");
                    LogError($"스택 트레이스: {e.StackTrace}");
                    OnError?.Invoke($"무기 데이터 파싱 오류: {e.Message}");
                }
            }
            else
            {
                LogError($"무기 데이터 로드 실패: {request.error}");
                LogError($"HTTP 상태 코드: {request.responseCode}");
                LogError($"응답 헤더: {request.GetResponseHeader("content-type")}");
                OnError?.Invoke($"무기 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// 방어구 데이터를 구글 스프레드시트에서 로드합니다.
    /// </summary>
    public IEnumerator LoadArmorsData()
    {
        if (string.IsNullOrEmpty(config.spreadsheetId) || string.IsNullOrEmpty(config.apiKey))
        {
            LogError("스프레드시트 ID 또는 API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.spreadsheetId}/values/{config.armorsSheetName}?key={config.apiKey}";
        LogDebug($"방어구 데이터 요청 URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    LogDebug($"방어구 API 응답: {responseText}");
                    
                    // 응답이 비어있는지 확인
                    if (string.IsNullOrEmpty(responseText))
                    {
                        LogError("API 응답이 비어있습니다.");
                        yield break;
                    }
                    
                    // Newtonsoft.Json 사용
                    var response = JsonConvert.DeserializeObject<SimpleGoogleSheetsResponse>(responseText);
                    
                    if (response == null)
                    {
                        LogError("JSON 파싱 실패. 응답 형식을 확인하세요.");
                        yield break;
                    }
                    
                    LogDebug($"파싱된 응답 - range: {response.range}, majorDimension: {response.majorDimension}");
                    
                    if (response.values == null)
                    {
                        LogError("values 필드가 null입니다. 스프레드시트에 데이터가 있는지 확인하세요.");
                        LogDebug("가능한 원인:");
                        LogDebug("1. 시트 이름이 잘못되었습니다 (현재: " + config.armorsSheetName + ")");
                        LogDebug("2. 스프레드시트가 공유되지 않았습니다");
                        LogDebug("3. 스프레드시트에 데이터가 없습니다");
                        yield break;
                    }
                    
                    LogDebug($"values 배열 크기: {response.values.Count}");
                    if (response.values.Count > 0)
                    {
                        LogDebug($"첫 번째 행 (헤더): {string.Join(", ", response.values[0])}");
                    }
                    
                    var armorList = ParseArmorData(response);
                    LogDebug($"방어구 데이터 로드 완료: {armorList.Count}개");
                    OnArmorsLoaded?.Invoke(armorList);
                }
                catch (Exception e)
                {
                    LogError($"방어구 데이터 파싱 오류: {e.Message}");
                    LogError($"스택 트레이스: {e.StackTrace}");
                    OnError?.Invoke($"방어구 데이터 파싱 오류: {e.Message}");
                }
            }
            else
            {
                LogError($"방어구 데이터 로드 실패: {request.error}");
                LogError($"HTTP 상태 코드: {request.responseCode}");
                LogError($"응답 헤더: {request.GetResponseHeader("content-type")}");
                OnError?.Invoke($"방어구 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// 구글 스프레드시트 응답을 무기 데이터로 파싱합니다.
    /// </summary>
    private List<WeaponData> ParseWeaponData(SimpleGoogleSheetsResponse response)
    {
        var weaponList = new List<WeaponData>();
        
        if (response.values == null || response.values.Count < 2)
        {
            LogError("스프레드시트에 데이터가 없습니다.");
            return weaponList;
        }
        
        // 첫 번째 행은 헤더
        var headers = response.values[0];
        LogDebug($"헤더 컬럼 수: {headers.Count}");
        
        for (int i = 1; i < response.values.Count; i++)
        {
            var row = response.values[i];
            if (row == null || row.Count < headers.Count) 
            {
                LogDebug($"행 {i} 건너뜀: null이거나 헤더보다 적은 컬럼");
                continue;
            }
            
            var weaponData = ScriptableObject.CreateInstance<WeaponData>();
            
            // 헤더와 값을 매핑
            for (int j = 0; j < headers.Count; j++)
            {
                if (j < row.Count)
                {
                    SetWeaponValue(weaponData, headers[j], row[j]);
                }
            }
            
            weaponList.Add(weaponData);
            LogDebug($"무기 추가: {weaponData.weaponName}");
        }
        
        return weaponList;
    }
    
    /// <summary>
    /// 구글 스프레드시트 응답을 방어구 데이터로 파싱합니다.
    /// </summary>
    private List<ArmorData> ParseArmorData(SimpleGoogleSheetsResponse response)
    {
        var armorList = new List<ArmorData>();
        
        if (response.values == null || response.values.Count < 2)
        {
            LogError("스프레드시트에 데이터가 없습니다.");
            return armorList;
        }
        
        // 첫 번째 행은 헤더
        var headers = response.values[0];
        LogDebug($"헤더 컬럼 수: {headers.Count}");
        
        for (int i = 1; i < response.values.Count; i++)
        {
            var row = response.values[i];
            if (row == null || row.Count < headers.Count) 
            {
                LogDebug($"행 {i} 건너뜀: null이거나 헤더보다 적은 컬럼");
                continue;
            }
            
            var armorData = ScriptableObject.CreateInstance<ArmorData>();
            
            // 헤더와 값을 매핑
            for (int j = 0; j < headers.Count; j++)
            {
                if (j < row.Count)
                {
                    SetArmorValue(armorData, headers[j], row[j]);
                }
            }
            
            armorList.Add(armorData);
            LogDebug($"방어구 추가: {armorData.armorName}");
        }
        
        return armorList;
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
            case "rarity":
                weapon.rarity = ParseWeaponRarity(value);
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
            case "smgdashspeedbonus":
                weapon.smgDashSpeedBonus = ParseFloat(value);
                break;
            case "smgdashspeedduration":
                weapon.smgDashSpeedDuration = ParseFloat(value);
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
    
    private WeaponRarity ParseWeaponRarity(string rarityString)
    {
        switch (rarityString.ToLower())
        {
            case "primordial": return WeaponRarity.Primordial;
            case "common": return WeaponRarity.Common;
            case "rare": return WeaponRarity.Rare;
            case "epic": return WeaponRarity.Epic;
            case "legendary": return WeaponRarity.Legendary;
            default: return WeaponRarity.Common;
        }
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[SimpleGoogleSheetsManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[SimpleGoogleSheetsManager] {message}");
    }
}