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
    public event Action<DropTableData> OnDropTableLoaded;
    public event Action<List<WeaponChipsetData>> OnWeaponChipsetsLoaded;
    public event Action<List<ArmorChipsetData>> OnArmorChipsetsLoaded;
    public event Action<List<PlayerChipsetData>> OnPlayerChipsetsLoaded;
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
    
    public void LoadDropTable()
    {
        if (string.IsNullOrEmpty(config.DropTableSpreadsheetId))
        {
            OnError?.Invoke("드랍 테이블 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchDropTableData());
    }
    
    /// <summary>
    /// 모든 데이터를 로드합니다 (GameDataRepository 호환용)
    /// </summary>
    public void LoadAllData()
    {
        LoadWeapons();
        LoadArmors();
        LoadDropTable();
        LoadWeaponChipsets();
        LoadArmorChipsets();
        LoadPlayerChipsets();
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
        // Debug.Log($"[GoogleSheetsManager] 무기 데이터 요청 URL: {url}");
        
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
        // Debug.Log($"[GoogleSheetsManager] 방어구 데이터 요청 URL: {url}");
        
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
    
    private IEnumerator FetchWeaponChipsetsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.WeaponChipsetsSpreadsheetId}/values/{config.WeaponChipsetsSheetName}?key={config.ApiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseWeaponChipsetsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"무기 칩셋 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private IEnumerator FetchArmorChipsetsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.ArmorChipsetsSpreadsheetId}/values/{config.ArmorChipsetsSheetName}?key={config.ApiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseArmorChipsetsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"방어구 칩셋 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private IEnumerator FetchPlayerChipsetsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.PlayerChipsetsSpreadsheetId}/values/{config.PlayerChipsetsSheetName}?key={config.ApiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParsePlayerChipsetsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"플레이어 칩셋 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private IEnumerator FetchDropTableData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        DropTableData dropTableData = new DropTableData();
        
        // MonsterInfo 시트 로드
        yield return StartCoroutine(FetchSheetData(config.MonsterInfoSheetName, (jsonData) => {
            ParseMonsterInfoData(jsonData, dropTableData);
        }));
        
        // ItemTypeDropRates 시트 로드
        yield return StartCoroutine(FetchSheetData(config.ItemTypeDropRatesSheetName, (jsonData) => {
            ParseItemTypeDropRatesData(jsonData, dropTableData);
        }));
        
        // MonsterRarityDropRates 시트 로드
        yield return StartCoroutine(FetchSheetData(config.MonsterRarityDropRatesSheetName, (jsonData) => {
            ParseMonsterRarityDropRatesData(jsonData, dropTableData);
        }));
        
        OnDropTableLoaded?.Invoke(dropTableData);
    }
    
    public void LoadWeaponChipsets()
    {
        if (string.IsNullOrEmpty(config.WeaponChipsetsSpreadsheetId))
        {
            OnError?.Invoke("무기 칩셋 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchWeaponChipsetsData());
    }
    
    public void LoadArmorChipsets()
    {
        if (string.IsNullOrEmpty(config.ArmorChipsetsSpreadsheetId))
        {
            OnError?.Invoke("방어구 칩셋 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchArmorChipsetsData());
    }
    
    public void LoadPlayerChipsets()
    {
        if (string.IsNullOrEmpty(config.PlayerChipsetsSpreadsheetId))
        {
            OnError?.Invoke("플레이어 칩셋 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchPlayerChipsetsData());
    }
    
    private IEnumerator FetchSheetData(string sheetName, Action<string> onComplete)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.DropTableSpreadsheetId}/values/{sheetName}?key={config.GetApiKey()}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"{sheetName} 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private void ParseMonsterInfoData(string jsonData, DropTableData dropTableData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("MonsterInfo 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 7) // Level 칼럼이 추가되어 최소 7개 필요
                {
                    MonsterInfo monsterInfo = new MonsterInfo
                    {
                        MonsterID = row[0],
                        MonsterName = row[1],
                        // Level = row[2] (현재 MonsterInfo에 Level 필드가 없으므로 건너뛰기)
                        DropChance = SafeParseFloat(row[3]),
                        MinDropCount = SafeParseInt(row[4]),
                        MaxDropCount = SafeParseInt(row[5]),
                        MainRarity = row[6]
                    };
                    
                    // 확장된 필드들 파싱 (8번째 칼럼부터, Level 칼럼 때문에 인덱스 +1)
                    if (row.Count >= 8) monsterInfo.MaxHealth = SafeParseInt(row[7]);
                    if (row.Count >= 9) monsterInfo.Defense = SafeParseInt(row[8]);
                    if (row.Count >= 10) monsterInfo.MoveSpeed = SafeParseFloat(row[9]);
                    if (row.Count >= 11) monsterInfo.ExpReward = SafeParseInt(row[10]);
                    if (row.Count >= 12) monsterInfo.Damage = SafeParseInt(row[11]);
                    if (row.Count >= 13) monsterInfo.AttackRange = SafeParseFloat(row[12]);
                    if (row.Count >= 14) monsterInfo.AttackCooldown = SafeParseFloat(row[13]);
                    if (row.Count >= 15) monsterInfo.DetectionRange = SafeParseFloat(row[14]);
                    if (row.Count >= 16) monsterInfo.Acceleration = SafeParseFloat(row[15]);
                    if (row.Count >= 17) monsterInfo.MaxSpeed = SafeParseFloat(row[16]);
                    if (row.Count >= 18) monsterInfo.SeparationDistance = SafeParseFloat(row[17]);
                    
                    dropTableData.MonsterInfos.Add(monsterInfo);
                }
                else
                {
                    Debug.LogWarning($"[GoogleSheetsManager] MonsterInfo 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 7개 이상, 실제: {row.Count})");
                }
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke($"MonsterInfo 데이터 파싱 실패: {e.Message}");
        }
    }
    
    private void ParseItemTypeDropRatesData(string jsonData, DropTableData dropTableData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("ItemTypeDropRates 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 5) // ModuleDropRate 추가로 최소 5개 컬럼 필요
                {
                    ItemTypeDropRate dropRate = new ItemTypeDropRate
                    {
                        MonsterID = row[0],
                        WeaponDropRate = SafeParseFloat(row[1]),
                        ArmorDropRate = SafeParseFloat(row[2]),
                        AccessoryDropRate = SafeParseFloat(row[3]),
                        ModuleDropRate = SafeParseFloat(row[4])
                    };
                    
                    dropTableData.ItemTypeDropRates.Add(dropRate);
                }
                else
                {
                    Debug.LogWarning($"[GoogleSheetsManager] ItemTypeDropRates 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 5개 이상, 실제: {row.Count})");
                }
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke($"ItemTypeDropRates 데이터 파싱 실패: {e.Message}");
        }
    }
    
    private void ParseMonsterRarityDropRatesData(string jsonData, DropTableData dropTableData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("MonsterRarityDropRates 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 6)
                {
                    MonsterRarityDropRate rarityDropRate = new MonsterRarityDropRate
                    {
                        MonsterID = row[0],
                        CommonRate = SafeParseFloat(row[1]),
                        RareRate = SafeParseFloat(row[2]),
                        EpicRate = SafeParseFloat(row[3]),
                        LegendaryRate = SafeParseFloat(row[4]),
                        PrimordialRate = SafeParseFloat(row[5])
                    };
                    
                    dropTableData.MonsterRarityDropRates.Add(rarityDropRate);
                }
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke($"MonsterRarityDropRates 데이터 파싱 실패: {e.Message}");
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
                if (row.Count >= 33) // 모든 필드가 있는지 확인 (기본 필드 + 폭발 이펙트 + SMG 대시 효과)
                {
                    WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
                    
                    // 기본 정보
                    weapon.weaponName = row[0];
                    if (System.Enum.TryParse<WeaponType>(row[1], out WeaponType weaponType))
                        weapon.weaponType = weaponType;
                    
                    // 무기 등급 파싱
                    string weaponRarityString = row[2];
                    if (System.Enum.TryParse<WeaponRarity>(weaponRarityString, true, out WeaponRarity weaponRarity))
                    {
                        weapon.rarity = weaponRarity;
                    }
                    else
                    {
                        weapon.rarity = WeaponRarity.Common;
                    }
                    
                    weapon.flavorText = row[3];
                    
                    // 기본 스탯
                    weapon.fireRate = SafeParseFloat(row[4]);
                    weapon.damage = SafeParseInt(row[5]);
                    weapon.projectileSpeed = SafeParseFloat(row[6]);
                    weapon.maxAmmo = SafeParseInt(row[7]);
                    weapon.currentAmmo = SafeParseInt(row[8]);
                    weapon.reloadTime = SafeParseFloat(row[9]);
                    weapon.infiniteAmmo = SafeParseBool(row[10]);
                    
                    // 탄 퍼짐 설정
                    weapon.baseSpread = SafeParseFloat(row[11]);
                    weapon.maxSpread = SafeParseFloat(row[12]);
                    weapon.spreadIncreaseRate = SafeParseFloat(row[13]);
                    weapon.spreadDecreaseRate = SafeParseFloat(row[14]);
                    
                    // 샷건 설정
                    weapon.pelletsPerShot = SafeParseInt(row[15]);
                    weapon.shotgunSpreadAngle = SafeParseFloat(row[16]);
                    
                    // 머신건 설정
                    weapon.warmupTime = SafeParseFloat(row[17]);
                    weapon.maxWarmupFireRate = SafeParseFloat(row[18]);
                    
                    // MG 디버그 로그 추가
                    if (weapon.weaponType == WeaponType.MG)
                    {
                        // Debug.Log($"[MG DEBUG] {weapon.weaponName}: fireRate={weapon.fireRate}, warmupTime={weapon.warmupTime}, maxWarmupFireRate={weapon.maxWarmupFireRate}");
                    }
                    
                    // 저격총 설정
                    weapon.singleFireOnly = SafeParseBool(row[19]);
                    weapon.aimingRange = SafeParseFloat(row[20]);
                    
                    // 이동속도 영향
                    weapon.movementSpeedMultiplier = SafeParseFloat(row[21]);
                    
                    // 반동 설정
                    weapon.recoilForce = SafeParseFloat(row[22]);
                    weapon.recoilDuration = SafeParseFloat(row[23]);
                    weapon.recoilRecoverySpeed = SafeParseFloat(row[24]);
                    
                    // 특수 효과
                    weapon.criticalChance = SafeParseFloat(row[25]);
                    weapon.criticalMultiplier = SafeParseFloat(row[26]);
                    weapon.pierceCount = SafeParseInt(row[27]);
                    weapon.pierceDamageReduction = SafeParseFloat(row[28]);
                    weapon.hasTracerRounds = SafeParseBool(row[29]);
                    weapon.hasMuzzleFlash = SafeParseBool(row[30]);
                    weapon.hasExplosiveKills = SafeParseBool(row[31]);
                    weapon.explosionRadius = SafeParseFloat(row[32]);
                    
                    // SMG 대시 후 이동속도 증가 설정 파싱
                    if (row.Count > 32)
                    {
                        weapon.smgDashSpeedBonus = SafeParseFloat(row[33]);
                        weapon.smgDashSpeedDuration = SafeParseFloat(row[34]);
                    }
                    
                    // 무기 등급 설정
                    SetupWeaponAssets(weapon);
                    
                    weapons.Add(weapon);
                }
                else
                {
                    Debug.LogWarning($"무기 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 33개 이상, 실제: {row.Count})");
                }
            }
            
            // Debug.Log($"무기 데이터 로드 완료: {weapons.Count}개");
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
        string assetPath = $"Assets/Resources/NewGame/Prefab/Network/WeaponPickup_{weapon.weaponType}.prefab";
        weapon.weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (weapon.weaponPrefab == null)
        {
            // 프리팹을 찾을 수 없는 경우 조용히 처리
        }

        string projectilePath = $"Assets/Resources/NewGame/Prefab/Network/Projectile_{weapon.weaponType}.prefab";
        weapon.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath);
        if (weapon.projectilePrefab == null)
        {
            // 프리팹을 찾을 수 없는 경우 조용히 처리
        }
#else
        weapon.weaponPrefab = Resources.Load<GameObject>($"NewGame/Prefab/Network/WeaponPickup_{weapon.weaponType}");
        weapon.projectilePrefab = Resources.Load<GameObject>($"NewGame/Prefab/Network/Projectile_{weapon.weaponType}");
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
                    
                    // 등급 파싱
                    string rarityString = row[2];
                    if (System.Enum.TryParse<ArmorRarity>(rarityString, true, out ArmorRarity rarity))
                    {
                        armor.rarity = rarity;
                    }
                    else
                    {
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
            
            // Debug.Log($"방어구 데이터 로드 완료: {armors.Count}개");
            OnArmorsLoaded?.Invoke(armors);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"방어구 데이터 파싱 오류: {e.Message}");
        }
    }
    
    // 안전한 파싱 메서드들
    private bool SafeParseBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return bool.TryParse(value, out bool result) ? result : false;
    }
    
    private float SafeParseFloat(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0f;
        return float.TryParse(value, out float result) ? result : 0f;
    }
    
    private int SafeParseInt(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        return int.TryParse(value, out int result) ? result : 0;
    }
    
    private void ParseWeaponChipsetsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("무기 칩셋 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            List<WeaponChipsetData> chipsets = new List<WeaponChipsetData>();
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                
                // 컬럼 수가 부족하면 경고만 출력하고 기본값으로 처리
                if (row.Count < 16)
                {
                    Debug.LogWarning($"무기 칩셋 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 16, 실제: {row.Count}) - 기본값으로 처리합니다.");
                }
                
                    WeaponChipsetData chipset = new WeaponChipsetData();
                    
                // 기본 정보 (안전한 인덱스 접근)
                chipset.chipsetId = row.Count > 0 ? row[0] : "";
                chipset.chipsetName = row.Count > 1 ? row[1] : "";
                if (row.Count > 2 && System.Enum.TryParse<WeaponChipsetType>(row[2], out WeaponChipsetType chipsetType))
                        chipset.chipsetType = chipsetType;
                    
                if (row.Count > 3 && System.Enum.TryParse<ChipsetRarity>(row[3], out ChipsetRarity rarity))
                        chipset.rarity = rarity;
                    
                chipset.cost = row.Count > 4 ? SafeParseInt(row[4]) : 0;
                chipset.description = row.Count > 5 ? row[5] : "";
                    
                // 무기 효과 (안전한 인덱스 접근)
                chipset.damageBonus = row.Count > 6 ? SafeParseFloat(row[6]) : 0f;
                chipset.fireRateBonus = row.Count > 7 ? SafeParseFloat(row[7]) : 0f;
                chipset.accuracyBonus = row.Count > 8 ? SafeParseFloat(row[8]) : 0f;
                chipset.recoilReduction = row.Count > 9 ? SafeParseFloat(row[9]) : 0f;
                chipset.reloadSpeedBonus = row.Count > 10 ? SafeParseFloat(row[10]) : 0f;
                chipset.ammoCapacityBonus = row.Count > 11 ? SafeParseInt(row[11]) : 0;
                chipset.criticalChanceBonus = row.Count > 12 ? SafeParseFloat(row[12]) : 0f;
                chipset.criticalMultiplierBonus = row.Count > 13 ? SafeParseFloat(row[13]) : 0f;
                    
                    // 특수 효과
                chipset.specialEffectType = row.Count > 14 ? row[14] : "";
                chipset.specialEffectValue = row.Count > 15 ? SafeParseFloat(row[15]) : 0f;
                    
                    chipsets.Add(chipset);
            }
            
            Debug.Log($"무기 칩셋 데이터 로드 완료: {chipsets.Count}개");
            OnWeaponChipsetsLoaded?.Invoke(chipsets);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"무기 칩셋 데이터 파싱 오류: {e.Message}");
        }
    }
    
    private void ParseArmorChipsetsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("방어구 칩셋 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            List<ArmorChipsetData> chipsets = new List<ArmorChipsetData>();
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                
                // 컬럼 수가 부족하면 경고만 출력하고 기본값으로 처리
                if (row.Count < 17)
                {
                    Debug.LogWarning($"방어구 칩셋 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 17, 실제: {row.Count}) - 기본값으로 처리합니다.");
                }
                
                    ArmorChipsetData chipset = new ArmorChipsetData();
                    
                // 기본 정보 (안전한 인덱스 접근)
                chipset.chipsetId = row.Count > 0 ? row[0] : "";
                chipset.chipsetName = row.Count > 1 ? row[1] : "";
                if (row.Count > 2 && System.Enum.TryParse<ArmorChipsetType>(row[2], out ArmorChipsetType chipsetType))
                        chipset.chipsetType = chipsetType;
                    
                if (row.Count > 3 && System.Enum.TryParse<ChipsetRarity>(row[3], out ChipsetRarity rarity))
                        chipset.rarity = rarity;
                    
                chipset.cost = row.Count > 4 ? SafeParseInt(row[4]) : 0;
                chipset.description = row.Count > 5 ? row[5] : "";
                    
                // 방어구 효과 (안전한 인덱스 접근)
                chipset.defenseBonus = row.Count > 6 ? SafeParseFloat(row[6]) : 0f;
                chipset.healthBonus = row.Count > 7 ? SafeParseFloat(row[7]) : 0f;
                chipset.speedBonus = row.Count > 8 ? SafeParseFloat(row[8]) : 0f;
                chipset.jumpForceBonus = row.Count > 9 ? SafeParseFloat(row[9]) : 0f;
                chipset.dashCooldownReduction = row.Count > 10 ? SafeParseFloat(row[10]) : 0f;
                chipset.hasRegeneration = row.Count > 11 ? SafeParseBool(row[11]) : false;
                chipset.regenerationRate = row.Count > 12 ? SafeParseFloat(row[12]) : 0f;
                chipset.hasInvincibilityFrame = row.Count > 13 ? SafeParseBool(row[13]) : false;
                chipset.invincibilityBonus = row.Count > 14 ? SafeParseFloat(row[14]) : 0f;
                    
                    // 특수 효과
                chipset.specialEffectType = row.Count > 15 ? row[15] : "";
                chipset.specialEffectValue = row.Count > 16 ? SafeParseFloat(row[16]) : 0f;
                    
                    chipsets.Add(chipset);
            }
            
            Debug.Log($"방어구 칩셋 데이터 로드 완료: {chipsets.Count}개");
            OnArmorChipsetsLoaded?.Invoke(chipsets);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"방어구 칩셋 데이터 파싱 오류: {e.Message}");
        }
    }
    
    private void ParsePlayerChipsetsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 4)
            {
                OnError?.Invoke("플레이어 칩셋 데이터가 비어있습니다. (최소 4행 필요: 헤더 3행 + 데이터 1행)");
                return;
            }
            
            List<PlayerChipsetData> chipsets = new List<PlayerChipsetData>();
            
            // 상위 3행은 헤더이므로 건너뛰기 (4번째 행부터 데이터 시작)
            for (int i = 3; i < response.values.Count; i++)
            {
                var row = response.values[i];
                
                // 컬럼 수가 부족하면 경고만 출력하고 기본값으로 처리
                if (row.Count < 23)
                {
                    Debug.LogWarning($"플레이어 칩셋 데이터 행 {i + 1}의 컬럼 수가 부족합니다. (필요: 23, 실제: {row.Count}) - 기본값으로 처리합니다.");
                }
                
                    PlayerChipsetData chipset = new PlayerChipsetData();
                    
                // 기본 정보 (안전한 인덱스 접근)
                chipset.chipsetId = row.Count > 0 ? row[0] : "";
                chipset.chipsetName = row.Count > 1 ? row[1] : "";
                if (row.Count > 2 && System.Enum.TryParse<PlayerChipsetType>(row[2], out PlayerChipsetType chipsetType))
                        chipset.chipsetType = chipsetType;
                    
                if (row.Count > 3 && System.Enum.TryParse<ChipsetRarity>(row[3], out ChipsetRarity rarity))
                        chipset.rarity = rarity;
                    
                chipset.cost = row.Count > 4 ? SafeParseInt(row[4]) : 0;
                chipset.description = row.Count > 5 ? row[5] : "";
                    
                // 플레이어 기본 스탯 효과 (안전한 인덱스 접근)
                chipset.moveSpeedBonus = row.Count > 6 ? SafeParseFloat(row[6]) : 0f;
                chipset.jumpForceBonus = row.Count > 7 ? SafeParseFloat(row[7]) : 0f;
                chipset.dashForceBonus = row.Count > 8 ? SafeParseFloat(row[8]) : 0f;
                chipset.dashCooldownReduction = row.Count > 9 ? SafeParseFloat(row[9]) : 0f;
                chipset.maxHealthBonus = row.Count > 10 ? SafeParseFloat(row[10]) : 0f;
                chipset.damageReduction = row.Count > 11 ? SafeParseFloat(row[11]) : 0f;
                chipset.pickupRangeBonus = row.Count > 12 ? SafeParseFloat(row[12]) : 0f;
                    
                // 무기 스탯 효과 (안전한 인덱스 접근)
                chipset.weaponDamageBonus = row.Count > 13 ? SafeParseFloat(row[13]) : 0f;
                chipset.weaponFireRateBonus = row.Count > 14 ? SafeParseFloat(row[14]) : 0f;
                chipset.weaponAccuracyBonus = row.Count > 15 ? SafeParseFloat(row[15]) : 0f;
                chipset.weaponRecoilReduction = row.Count > 16 ? SafeParseFloat(row[16]) : 0f;
                chipset.weaponReloadSpeedBonus = row.Count > 17 ? SafeParseFloat(row[17]) : 0f;
                chipset.weaponAmmoCapacityBonus = row.Count > 18 ? SafeParseInt(row[18]) : 0;
                chipset.weaponCriticalChanceBonus = row.Count > 19 ? SafeParseFloat(row[19]) : 0f;
                chipset.weaponCriticalMultiplierBonus = row.Count > 20 ? SafeParseFloat(row[20]) : 0f;
                    
                    // 특수 효과
                chipset.specialEffectType = row.Count > 21 ? row[21] : "";
                chipset.specialEffectValue = row.Count > 22 ? SafeParseFloat(row[22]) : 0f;
                    
                    chipsets.Add(chipset);
            }
            
            Debug.Log($"플레이어 칩셋 데이터 로드 완료: {chipsets.Count}개");
            OnPlayerChipsetsLoaded?.Invoke(chipsets);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"플레이어 칩셋 데이터 파싱 오류: {e.Message}");
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