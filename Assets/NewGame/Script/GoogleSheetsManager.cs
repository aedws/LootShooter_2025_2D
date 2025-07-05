using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class GoogleSheetsManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GoogleSheetsConfig config;
    
    // 이벤트
    public event Action<List<WeaponData>> OnWeaponsLoaded;
    public event Action<List<ArmorData>> OnArmorsLoaded;
    public event Action<string> OnError;
    
    private void Start()
    {
        // 설정이 없으면 자동으로 로드
        if (config == null)
        {
            config = GoogleSheetsConfig.Instance;
        }
        
        if (config == null)
        {
            Debug.LogError("GoogleSheetsConfig를 찾을 수 없습니다!");
            return;
        }
    }
    
    public void LoadWeapons()
    {
        if (string.IsNullOrEmpty(config.WeaponsSpreadsheetId))
        {
            OnError?.Invoke("무기 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchWeaponsData());
    }
    
    public void LoadArmors()
    {
        if (string.IsNullOrEmpty(config.ArmorsSpreadsheetId))
        {
            OnError?.Invoke("방어구 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchArmorsData());
    }
    
    /// <summary>
    /// 모든 데이터를 로드합니다 (GameDataRepository 호환용)
    /// </summary>
    public void LoadAllData()
    {
        LoadWeapons();
        LoadArmors();
    }
    
    private IEnumerator FetchWeaponsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.WeaponsSpreadsheetId}/values/{config.WeaponsSheetName}?key={config.ApiKey}";
        Debug.Log($"[GoogleSheetsManager] 무기 데이터 요청 URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseWeaponsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"무기 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private IEnumerator FetchArmorsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.ArmorsSpreadsheetId}/values/{config.ArmorsSheetName}?key={config.ApiKey}";
        Debug.Log($"[GoogleSheetsManager] 방어구 데이터 요청 URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseArmorsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"방어구 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private void ParseWeaponsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 2)
            {
                OnError?.Invoke("무기 데이터가 비어있습니다.");
                return;
            }
            
            List<WeaponData> weapons = new List<WeaponData>();
            
            // 첫 번째 행은 헤더이므로 건너뛰기
            for (int i = 1; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 31) // 모든 필드가 있는지 확인
                {
                    WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
                    
                    // 기본 정보
                    weapon.weaponName = row[0];
                    if (System.Enum.TryParse<WeaponType>(row[1], out WeaponType weaponType))
                        weapon.weaponType = weaponType;
                    weapon.flavorText = row[2];
                    
                    // 기본 스탯
                    weapon.fireRate = float.Parse(row[3]);
                    weapon.damage = int.Parse(row[4]);
                    weapon.projectileSpeed = float.Parse(row[5]);
                    weapon.maxAmmo = int.Parse(row[6]);
                    weapon.currentAmmo = int.Parse(row[7]);
                    weapon.reloadTime = float.Parse(row[8]);
                    weapon.infiniteAmmo = bool.Parse(row[9]);
                    
                    // 탄 퍼짐 설정
                    weapon.baseSpread = float.Parse(row[10]);
                    weapon.maxSpread = float.Parse(row[11]);
                    weapon.spreadIncreaseRate = float.Parse(row[12]);
                    weapon.spreadDecreaseRate = float.Parse(row[13]);
                    
                    // 샷건 설정
                    weapon.pelletsPerShot = int.Parse(row[14]);
                    weapon.shotgunSpreadAngle = float.Parse(row[15]);
                    
                    // 머신건 설정
                    weapon.warmupTime = float.Parse(row[16]);
                    weapon.maxWarmupFireRate = float.Parse(row[17]);
                    
                    // 저격총 설정
                    weapon.singleFireOnly = bool.Parse(row[18]);
                    weapon.aimingRange = float.Parse(row[19]);
                    
                    // 이동속도 영향
                    weapon.movementSpeedMultiplier = float.Parse(row[20]);
                    
                    // 반동 설정
                    weapon.recoilForce = float.Parse(row[21]);
                    weapon.recoilDuration = float.Parse(row[22]);
                    weapon.recoilRecoverySpeed = float.Parse(row[23]);
                    
                    // 특수 효과
                    weapon.criticalChance = float.Parse(row[24]);
                    weapon.criticalMultiplier = float.Parse(row[25]);
                    weapon.pierceCount = int.Parse(row[26]);
                    weapon.pierceDamageReduction = float.Parse(row[27]);
                    weapon.hasTracerRounds = bool.Parse(row[28]);
                    weapon.hasMuzzleFlash = bool.Parse(row[29]);
                    weapon.hasExplosiveKills = bool.Parse(row[30]);
                    weapon.explosionRadius = float.Parse(row[31]);
                    
                    weapons.Add(weapon);
                }
                else
                {
                    Debug.LogWarning($"무기 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 32, 실제: {row.Count})");
                }
            }
            
            Debug.Log($"무기 데이터 로드 완료: {weapons.Count}개");
            OnWeaponsLoaded?.Invoke(weapons);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"무기 데이터 파싱 오류: {e.Message}");
        }
    }
    
    private void ParseArmorsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 2)
            {
                OnError?.Invoke("방어구 데이터가 비어있습니다.");
                return;
            }
            
            List<ArmorData> armors = new List<ArmorData>();
            
            // 첫 번째 행은 헤더이므로 건너뛰기
            for (int i = 1; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 14) // 모든 필드가 있는지 확인
                {
                    ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
                    
                    // 기본 정보
                    armor.armorName = row[0];
                    if (System.Enum.TryParse<ArmorType>(row[1], out ArmorType armorType))
                        armor.armorType = armorType;
                    if (System.Enum.TryParse<ArmorRarity>(row[2], out ArmorRarity rarity))
                        armor.rarity = rarity;
                    armor.description = row[3];
                    
                    // 방어 능력
                    armor.defense = int.Parse(row[4]);
                    armor.maxHealth = int.Parse(row[5]);
                    armor.damageReduction = float.Parse(row[6]);
                    
                    // 추가 능력
                    armor.moveSpeedBonus = float.Parse(row[7]);
                    armor.jumpForceBonus = float.Parse(row[8]);
                    armor.dashCooldownReduction = float.Parse(row[9]);
                    
                    // 특수 효과
                    armor.hasRegeneration = bool.Parse(row[10]);
                    armor.regenerationRate = float.Parse(row[11]);
                    armor.hasInvincibilityFrame = bool.Parse(row[12]);
                    armor.invincibilityBonus = float.Parse(row[13]);
                    
                    armors.Add(armor);
                }
                else
                {
                    Debug.LogWarning($"방어구 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 14, 실제: {row.Count})");
                }
            }
            
            Debug.Log($"방어구 데이터 로드 완료: {armors.Count}개");
            OnArmorsLoaded?.Invoke(armors);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"방어구 데이터 파싱 오류: {e.Message}");
        }
    }
}

[System.Serializable]
public class GoogleSheetsResponse
{
    public string range;
    public string majorDimension;
    public List<List<string>> values;
} 