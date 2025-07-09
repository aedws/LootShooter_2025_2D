using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("무기 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            List<WeaponData> weapons = new List<WeaponData>();
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 31) // 모든 필드가 있는지 확인
                {
                    WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
                    
                    // 기본 정보
                    weapon.weaponName = row[0];
                    if (System.Enum.TryParse<WeaponType>(row[1], out WeaponType weaponType))
                        weapon.weaponType = weaponType;
                    
                    // 무기 등급 파싱 디버그
                    string weaponRarityString = row[2];
                    Debug.Log($"[GoogleSheetsManager] 무기 등급 파싱 시도: '{weaponRarityString}' (행 {i + 1})");
                    
                    if (System.Enum.TryParse<WeaponRarity>(weaponRarityString, true, out WeaponRarity weaponRarity))
                    {
                        weapon.rarity = weaponRarity;
                        Debug.Log($"[GoogleSheetsManager] 무기 등급 파싱 성공: {weaponRarityString} -> {weaponRarity}");
                    }
                    else
                    {
                        Debug.LogError($"[GoogleSheetsManager] 무기 등급 파싱 실패: '{weaponRarityString}' -> Common으로 설정됨");
                        weapon.rarity = WeaponRarity.Common;
                    }
                    
                    weapon.flavorText = row[3];
                    
                    // 기본 스탯
                    weapon.fireRate = float.Parse(row[4]);
                    weapon.damage = int.Parse(row[5]);
                    weapon.projectileSpeed = float.Parse(row[6]);
                    weapon.maxAmmo = int.Parse(row[7]);
                    weapon.currentAmmo = int.Parse(row[8]);
                    weapon.reloadTime = float.Parse(row[9]);
                    weapon.infiniteAmmo = bool.Parse(row[10]);
                    
                    // 탄 퍼짐 설정
                    weapon.baseSpread = float.Parse(row[11]);
                    weapon.maxSpread = float.Parse(row[12]);
                    weapon.spreadIncreaseRate = float.Parse(row[13]);
                    weapon.spreadDecreaseRate = float.Parse(row[14]);
                    
                    // 샷건 설정
                    weapon.pelletsPerShot = int.Parse(row[15]);
                    weapon.shotgunSpreadAngle = float.Parse(row[16]);
                    
                    // 머신건 설정
                    weapon.warmupTime = float.Parse(row[17]);
                    weapon.maxWarmupFireRate = float.Parse(row[18]);
                    
                    // 저격총 설정
                    weapon.singleFireOnly = bool.Parse(row[19]);
                    weapon.aimingRange = float.Parse(row[20]);
                    
                    // 이동속도 영향
                    weapon.movementSpeedMultiplier = float.Parse(row[21]);
                    
                    // 반동 설정
                    weapon.recoilForce = float.Parse(row[22]);
                    weapon.recoilDuration = float.Parse(row[23]);
                    weapon.recoilRecoverySpeed = float.Parse(row[24]);
                    
                    // 특수 효과
                    weapon.criticalChance = float.Parse(row[25]);
                    weapon.criticalMultiplier = float.Parse(row[26]);
                    weapon.pierceCount = int.Parse(row[27]);
                    weapon.pierceDamageReduction = float.Parse(row[28]);
                    weapon.hasTracerRounds = bool.Parse(row[29]);
                    weapon.hasMuzzleFlash = bool.Parse(row[30]);
                    weapon.hasExplosiveKills = bool.Parse(row[31]);
                    weapon.explosionRadius = float.Parse(row[32]);
                    
                    // 🆕 누락된 필드들 처리
                    SetupWeaponAssets(weapon);
                    
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
    
    /// <summary>
    /// 무기 타입별로 아이콘과 프리팹을 설정합니다.
    /// </summary>
    private void SetupWeaponAssets(WeaponData weapon)
    {
        // 반동 방향 설정 (무기 타입별로 다름)
        switch (weapon.weaponType)
        {
            case WeaponType.AR:
                weapon.recoilDirection = new Vector2(0.1f, 1f); // 약간 오른쪽 위로
                break;
            case WeaponType.HG:
                weapon.recoilDirection = new Vector2(0f, 1f); // 수직 위로
                break;
            case WeaponType.MG:
                weapon.recoilDirection = new Vector2(0.2f, 1f); // 더 많이 오른쪽 위로
                break;
            case WeaponType.SG:
                weapon.recoilDirection = new Vector2(0f, 1.2f); // 강한 수직 반동
                break;
            case WeaponType.SMG:
                weapon.recoilDirection = new Vector2(0.15f, 0.8f); // 약한 반동
                break;
            case WeaponType.SR:
                weapon.recoilDirection = new Vector2(0f, 1.5f); // 매우 강한 수직 반동
                break;
            default:
                weapon.recoilDirection = Vector2.up;
                break;
        }
        
        weapon.icon = null;

#if UNITY_EDITOR
        // 무기 프리팹 자동 할당
        string assetPath = $"Assets/NewGame/Prefab/Network/WeaponPickup_{weapon.weaponType}.prefab";
        weapon.weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (weapon.weaponPrefab == null)
        {
            Debug.LogWarning($"[GoogleSheetsManager] 네트워크 무기 프리팹을 찾을 수 없습니다: {assetPath}");
        }
        else
        {
            Debug.Log($"[GoogleSheetsManager] 네트워크 무기 프리팹 자동 할당 성공: {assetPath}");
        }

        // 투사체 프리팹 자동 할당
        string projectilePath = $"Assets/NewGame/Prefab/Network/Projectile_{weapon.weaponType}.prefab";
        weapon.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath);
        if (weapon.projectilePrefab == null)
        {
            Debug.LogWarning($"[GoogleSheetsManager] 투사체 프리팹을 찾을 수 없습니다: {projectilePath}");
        }
        else
        {
            Debug.Log($"[GoogleSheetsManager] 투사체 프리팹 자동 할당 성공: {projectilePath}");
        }
#else
        weapon.weaponPrefab = null;
        weapon.projectilePrefab = null;
#endif
    }
    
    private void ParseArmorsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("방어구 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            List<ArmorData> armors = new List<ArmorData>();
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 14) // 모든 필드가 있는지 확인
                {
                    ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
                    
                    // 기본 정보
                    armor.armorName = row[0];
                    if (System.Enum.TryParse<ArmorType>(row[1], out ArmorType armorType))
                        armor.armorType = armorType;
                    
                    // 등급 파싱 디버그
                    string rarityString = row[2];
                    Debug.Log($"[GoogleSheetsManager] 방어구 등급 파싱 시도: '{rarityString}' (행 {i + 1})");
                    
                    if (System.Enum.TryParse<ArmorRarity>(rarityString, true, out ArmorRarity rarity))
                    {
                        armor.rarity = rarity;
                        Debug.Log($"[GoogleSheetsManager] 방어구 등급 파싱 성공: {rarityString} -> {rarity}");
                    }
                    else
                    {
                        Debug.LogError($"[GoogleSheetsManager] 방어구 등급 파싱 실패: '{rarityString}' -> Common으로 설정됨");
                        armor.rarity = ArmorRarity.Common;
                    }
                    
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