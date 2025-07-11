using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// 게임의 모든 데이터를 중앙에서 관리하는 Repository 클래스
/// 싱글톤 패턴으로 구현되어 어디서든 접근 가능
/// </summary>
public class GameDataRepository : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameDataRepository _instance;
    public static GameDataRepository Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameDataRepository>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameDataRepository");
                    _instance = go.AddComponent<GameDataRepository>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("데이터 소스")]
    [SerializeField] private GoogleSheetsManager googleSheetsManager;
    [SerializeField] private bool loadDataOnStart = true;
    [SerializeField] private bool useLocalDataAsFallback = true;

    [Header("로컬 데이터 (폴백용)")]
    [SerializeField] private List<WeaponData> localWeaponData = new List<WeaponData>();
    [SerializeField] private List<ArmorData> localArmorData = new List<ArmorData>();

    // 저장된 데이터
    private List<WeaponData> _weapons = new List<WeaponData>();
    private List<ArmorData> _armors = new List<ArmorData>();
    private List<BossAttackPattern> _bossPatterns = new List<BossAttackPattern>();
    private List<WeaponChipsetData> _weaponChipsets = new List<WeaponChipsetData>();
    private List<ArmorChipsetData> _armorChipsets = new List<ArmorChipsetData>();
    private List<PlayerChipsetData> _playerChipsets = new List<PlayerChipsetData>();

    // 데이터 로드 상태
    private bool _weaponsLoaded = false;
    private bool _armorsLoaded = false;
    private bool _bossPatternsLoaded = false;
    private bool _weaponChipsetsLoaded = false;
    private bool _armorChipsetsLoaded = false;
    private bool _playerChipsetsLoaded = false;

    // 이벤트
    public event Action OnAllDataLoaded;
    public event Action<List<WeaponData>> OnWeaponsUpdated;
    public event Action<List<ArmorData>> OnArmorsUpdated;
    public event Action<List<BossAttackPattern>> OnBossPatternsUpdated;
    public event Action<List<WeaponChipsetData>> OnWeaponChipsetsUpdated;
    public event Action<List<ArmorChipsetData>> OnArmorChipsetsUpdated;
    public event Action<List<PlayerChipsetData>> OnPlayerChipsetsUpdated;
    public event Action<string> OnDataLoadError;

    // 프로퍼티
    public List<WeaponData> Weapons => _weapons;
    public List<ArmorData> Armors => _armors;
    public List<BossAttackPattern> BossPatterns => _bossPatterns;
    public List<WeaponChipsetData> WeaponChipsets => _weaponChipsets;
    public List<ArmorChipsetData> ArmorChipsets => _armorChipsets;
    public List<PlayerChipsetData> PlayerChipsets => _playerChipsets;
    
    public bool IsWeaponsLoaded => _weaponsLoaded;
    public bool IsArmorsLoaded => _armorsLoaded;
    public bool IsBossPatternsLoaded => _bossPatternsLoaded;
    public bool IsWeaponChipsetsLoaded => _weaponChipsetsLoaded;
    public bool IsArmorChipsetsLoaded => _armorChipsetsLoaded;
    public bool IsPlayerChipsetsLoaded => _playerChipsetsLoaded;
    public bool IsAllDataLoaded => _weaponsLoaded && _armorsLoaded && _bossPatternsLoaded && 
                                   _weaponChipsetsLoaded && _armorChipsetsLoaded && _playerChipsetsLoaded;

    private void Awake()
    {
        // 싱글톤 설정
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // GoogleSheetsManager 자동 찾기
        if (googleSheetsManager == null)
        {
            googleSheetsManager = FindFirstObjectByType<GoogleSheetsManager>();
        }
    }

    private void Start()
    {
        if (loadDataOnStart)
        {
            LoadAllData();
        }
    }

    /// <summary>
    /// 모든 데이터를 로드합니다.
    /// </summary>
    public void LoadAllData()
    {
        // 모든 데이터 로드 시작
        
        // Google Sheets에서 데이터 로드
        if (googleSheetsManager != null)
        {
            // 이벤트 구독
            googleSheetsManager.OnWeaponsLoaded += OnWeaponsLoadedFromSheets;
            googleSheetsManager.OnArmorsLoaded += OnArmorsLoadedFromSheets;
            googleSheetsManager.OnWeaponChipsetsLoaded += OnWeaponChipsetsLoadedFromSheets;
            googleSheetsManager.OnArmorChipsetsLoaded += OnArmorChipsetsLoadedFromSheets;
            googleSheetsManager.OnPlayerChipsetsLoaded += OnPlayerChipsetsLoadedFromSheets;
            googleSheetsManager.OnError += OnSheetsError;

            // 데이터 로드 시작
            googleSheetsManager.LoadWeapons();
            googleSheetsManager.LoadArmors();
            googleSheetsManager.LoadWeaponChipsets();
            googleSheetsManager.LoadArmorChipsets();
            googleSheetsManager.LoadPlayerChipsets();
        }
        else
        {
            Debug.LogWarning("[GameDataRepository] GoogleSheetsManager를 찾을 수 없습니다. 로컬 데이터를 사용합니다.");
            LoadLocalData();
        }

        // 보스 패턴 데이터 로드 (로컬에서)
        LoadBossPatterns();
    }

    /// <summary>
    /// Google Sheets에서 무기 데이터가 로드되었을 때 호출
    /// </summary>
    private void OnWeaponsLoadedFromSheets(List<WeaponData> weapons)
    {
        _weapons = weapons;
        _weaponsLoaded = true;
                    // 무기 데이터 로드 완료
        
        OnWeaponsUpdated?.Invoke(_weapons);
        CheckAllDataLoaded();
    }

    /// <summary>
    /// Google Sheets에서 방어구 데이터가 로드되었을 때 호출
    /// </summary>
    private void OnArmorsLoadedFromSheets(List<ArmorData> armors)
    {
        _armors = armors;
        _armorsLoaded = true;
                    // 방어구 데이터 로드 완료
        
        OnArmorsUpdated?.Invoke(_armors);
        CheckAllDataLoaded();
    }
    
    /// <summary>
    /// Google Sheets에서 무기 칩셋 데이터가 로드되었을 때 호출
    /// </summary>
    private void OnWeaponChipsetsLoadedFromSheets(List<WeaponChipsetData> chipsets)
    {
        _weaponChipsets = chipsets;
        _weaponChipsetsLoaded = true;
        Debug.Log($"[GameDataRepository] 무기 칩셋 데이터 로드: {_weaponChipsets.Count}개");
        
        OnWeaponChipsetsUpdated?.Invoke(_weaponChipsets);
        CheckAllDataLoaded();
    }
    
    /// <summary>
    /// Google Sheets에서 방어구 칩셋 데이터가 로드되었을 때 호출
    /// </summary>
    private void OnArmorChipsetsLoadedFromSheets(List<ArmorChipsetData> chipsets)
    {
        _armorChipsets = chipsets;
        _armorChipsetsLoaded = true;
        Debug.Log($"[GameDataRepository] 방어구 칩셋 데이터 로드: {_armorChipsets.Count}개");
        
        OnArmorChipsetsUpdated?.Invoke(_armorChipsets);
        CheckAllDataLoaded();
    }
    
    /// <summary>
    /// Google Sheets에서 플레이어 칩셋 데이터가 로드되었을 때 호출
    /// </summary>
    private void OnPlayerChipsetsLoadedFromSheets(List<PlayerChipsetData> chipsets)
    {
        _playerChipsets = chipsets;
        _playerChipsetsLoaded = true;
        Debug.Log($"[GameDataRepository] 플레이어 칩셋 데이터 로드: {_playerChipsets.Count}개");
        
        OnPlayerChipsetsUpdated?.Invoke(_playerChipsets);
        CheckAllDataLoaded();
    }

    /// <summary>
    /// Google Sheets 오류 발생 시 호출
    /// </summary>
    private void OnSheetsError(string error)
    {
        Debug.LogError($"[GameDataRepository] Google Sheets 오류: {error}");
        
        if (useLocalDataAsFallback)
        {
            Debug.Log("[GameDataRepository] 로컬 데이터를 폴백으로 사용합니다.");
            LoadLocalData();
        }
        
        OnDataLoadError?.Invoke(error);
    }

    /// <summary>
    /// 로컬 데이터를 로드합니다 (폴백용)
    /// </summary>
    private void LoadLocalData()
    {
        // 무기 데이터
        if (localWeaponData.Count > 0)
        {
            _weapons = new List<WeaponData>(localWeaponData);
            _weaponsLoaded = true;
            // Debug.Log($"[GameDataRepository] 로컬 무기 데이터 로드: {_weapons.Count}개");
            OnWeaponsUpdated?.Invoke(_weapons);
        }

        // 방어구 데이터
        if (localArmorData.Count > 0)
        {
            _armors = new List<ArmorData>(localArmorData);
            _armorsLoaded = true;
            // Debug.Log($"[GameDataRepository] 로컬 방어구 데이터 로드: {_armors.Count}개");
            OnArmorsUpdated?.Invoke(_armors);
        }

        CheckAllDataLoaded();
    }

    /// <summary>
    /// 보스 패턴 데이터를 로드합니다
    /// </summary>
    private void LoadBossPatterns()
    {
        // Resources 폴더에서 보스 패턴 데이터 로드
        BossAttackPattern[] patterns = Resources.LoadAll<BossAttackPattern>("BossPatterns");
        _bossPatterns = new List<BossAttackPattern>(patterns);
        _bossPatternsLoaded = true;
        
        // Debug.Log($"[GameDataRepository] 보스 패턴 데이터 로드: {_bossPatterns.Count}개");
        OnBossPatternsUpdated?.Invoke(_bossPatterns);
        CheckAllDataLoaded();
    }

    /// <summary>
    /// 모든 데이터가 로드되었는지 확인하고 이벤트 발생
    /// </summary>
    private void CheckAllDataLoaded()
    {
        if (IsAllDataLoaded)
        {
            // 모든 데이터 로드 완료
            OnAllDataLoaded?.Invoke();
        }
    }

    #region 데이터 조회 메서드

    /// <summary>
    /// 무기 타입별로 무기를 조회합니다
    /// </summary>
    public List<WeaponData> GetWeaponsByType(WeaponType type)
    {
        return _weapons.FindAll(w => w.weaponType == type);
    }

    /// <summary>
    /// 무기 이름으로 무기를 찾습니다
    /// </summary>
    public WeaponData GetWeaponByName(string weaponName)
    {
        return _weapons.Find(w => w.weaponName.Equals(weaponName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 방어구 타입별로 방어구를 조회합니다
    /// </summary>
    public List<ArmorData> GetArmorsByType(ArmorType type)
    {
        return _armors.FindAll(a => a.armorType == type);
    }

    /// <summary>
    /// 방어구 레어리티별로 방어구를 조회합니다
    /// </summary>
    public List<ArmorData> GetArmorsByRarity(ArmorRarity rarity)
    {
        return _armors.FindAll(a => a.rarity == rarity);
    }

    /// <summary>
    /// 방어구 이름으로 방어구를 찾습니다
    /// </summary>
    public ArmorData GetArmorByName(string armorName)
    {
        return _armors.Find(a => a.armorName.Equals(armorName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 패턴 이름으로 보스 패턴을 찾습니다
    /// </summary>
    public BossAttackPattern GetBossPatternByName(string patternName)
    {
        return _bossPatterns.Find(b => b.patternName.Equals(patternName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 랜덤 무기를 반환합니다 (가중치 기반)
    /// </summary>
    public WeaponData GetRandomWeapon()
    {
        if (_weapons.Count == 0) return null;
        
        // 디버그: 모든 무기 출력
        // 총 무기 수 확인
        
        // 가중치 기반 선택을 위해 무기 그룹별로 분류
        var weaponGroups = new Dictionary<string, List<WeaponData>>();
        
        foreach (var weapon in _weapons)
        {
            string baseName = GetBaseWeaponName(weapon.weaponName);
            if (!weaponGroups.ContainsKey(baseName))
            {
                weaponGroups[baseName] = new List<WeaponData>();
            }
            weaponGroups[baseName].Add(weapon);
        }
        
        // 무기 그룹별 분류 완료
        
        // 각 그룹에서 하나씩 선택하여 후보 목록 생성
        var candidates = new List<WeaponData>();
        foreach (var group in weaponGroups.Values)
        {
            if (group.Count > 0)
            {
                // 그룹 내에서 가중치 기반 선택
                WeaponData selected = SelectWeaponByWeight(group);
                candidates.Add(selected);
                // 그룹에서 무기 선택 완료
            }
        }
        
        // 후보 목록에서 최종 선택
        if (candidates.Count > 0)
        {
            WeaponData finalSelected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            // 최종 무기 선택 완료
            return finalSelected;
        }
        
        // 폴백: 기존 방식
        WeaponData fallbackSelected = _weapons[UnityEngine.Random.Range(0, _weapons.Count)];
        // 폴백 무기 선택 완료
        return fallbackSelected;
    }
    
    /// <summary>
    /// 무기 이름에서 기본 이름을 추출합니다 (등급 제거)
    /// </summary>
    private string GetBaseWeaponName(string weaponName)
    {
        // 등급 접미사 제거
        string[] raritySuffixes = { " (Common)", " (Rare)", " (Epic)", " (Legendary)", " (Primordial)" };
        string baseName = weaponName;
        
        foreach (string suffix in raritySuffixes)
        {
            if (baseName.EndsWith(suffix))
            {
                baseName = baseName.Substring(0, baseName.Length - suffix.Length);
                break;
            }
        }
        
        return baseName;
    }
    
    /// <summary>
    /// 가중치 기반으로 무기를 선택합니다
    /// </summary>
    private WeaponData SelectWeaponByWeight(List<WeaponData> weapons)
    {
        if (weapons.Count == 0) return null;
        if (weapons.Count == 1) return weapons[0];
        
        // 등급별 가중치 설정 (더 균형잡힌 가중치로 조정)
        var weights = new Dictionary<string, float>
        {
            { "Common", 1.0f },
            { "Rare", 1.5f },
            { "Epic", 2.0f },
            { "Legendary", 3.0f },
            { "Primordial", 4.0f }
        };
        
        // 가중치 선택 시작
        
        // 각 무기의 가중치 계산
        var weaponWeights = new List<float>();
        foreach (var weapon in weapons)
        {
            float weight = 1.0f; // 기본 가중치
            foreach (var rarity in weights.Keys)
            {
                if (weapon.weaponName.Contains($"({rarity})"))
                {
                    weight = weights[rarity];
                    break;
                }
            }
            weaponWeights.Add(weight);
            // 무기 가중치 계산
        }
        
        // 가중치 기반 랜덤 선택
        float totalWeight = 0f;
        foreach (float weight in weaponWeights)
        {
            totalWeight += weight;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // 총 가중치 계산 완료
        
        for (int i = 0; i < weapons.Count; i++)
        {
            currentWeight += weaponWeights[i];
            if (randomValue <= currentWeight)
            {
                return weapons[i];
            }
        }
        
        // 폴백
        // 폴백 선택
        return weapons[weapons.Count - 1];
    }

    /// <summary>
    /// 특정 타입의 랜덤 무기를 반환합니다
    /// </summary>
    public WeaponData GetRandomWeaponByType(WeaponType type)
    {
        var weaponsOfType = GetWeaponsByType(type);
        if (weaponsOfType.Count == 0) return null;
        return weaponsOfType[UnityEngine.Random.Range(0, weaponsOfType.Count)];
    }

    /// <summary>
    /// 랜덤 방어구를 반환합니다
    /// </summary>
    public ArmorData GetRandomArmor()
    {
        if (_armors.Count == 0) return null;
        return _armors[UnityEngine.Random.Range(0, _armors.Count)];
    }

    /// <summary>
    /// 특정 타입의 랜덤 방어구를 반환합니다
    /// </summary>
    public ArmorData GetRandomArmorByType(ArmorType type)
    {
        var armorsOfType = GetArmorsByType(type);
        if (armorsOfType.Count == 0) return null;
        return armorsOfType[UnityEngine.Random.Range(0, armorsOfType.Count)];
    }

    /// <summary>
    /// 특정 레어리티의 랜덤 방어구를 반환합니다
    /// </summary>
    public ArmorData GetRandomArmorByRarity(ArmorRarity rarity)
    {
        var armorsOfRarity = GetArmorsByRarity(rarity);
        if (armorsOfRarity.Count == 0) return null;
        return armorsOfRarity[UnityEngine.Random.Range(0, armorsOfRarity.Count)];
    }

    #endregion

    #region 데이터 추가/수정 메서드

    /// <summary>
    /// 무기 데이터를 추가합니다
    /// </summary>
    public void AddWeapon(WeaponData weapon)
    {
        if (weapon != null && !_weapons.Contains(weapon))
        {
            _weapons.Add(weapon);
            OnWeaponsUpdated?.Invoke(_weapons);
        }
    }

    /// <summary>
    /// 방어구 데이터를 추가합니다
    /// </summary>
    public void AddArmor(ArmorData armor)
    {
        if (armor != null && !_armors.Contains(armor))
        {
            _armors.Add(armor);
            OnArmorsUpdated?.Invoke(_armors);
        }
    }

    /// <summary>
    /// 보스 패턴을 추가합니다
    /// </summary>
    public void AddBossPattern(BossAttackPattern pattern)
    {
        if (pattern != null && !_bossPatterns.Contains(pattern))
        {
            _bossPatterns.Add(pattern);
            OnBossPatternsUpdated?.Invoke(_bossPatterns);
        }
    }
    
    // 칩셋 관련 유틸리티 메서드들
    public WeaponChipsetData GetWeaponChipsetById(string chipsetId)
    {
        return _weaponChipsets.Find(c => c.chipsetId == chipsetId);
    }
    
    public ArmorChipsetData GetArmorChipsetById(string chipsetId)
    {
        return _armorChipsets.Find(c => c.chipsetId == chipsetId);
    }
    
    public PlayerChipsetData GetPlayerChipsetById(string chipsetId)
    {
        return _playerChipsets.Find(c => c.chipsetId == chipsetId);
    }
    
    public List<WeaponChipsetData> GetWeaponChipsetsByType(WeaponChipsetType type)
    {
        return _weaponChipsets.Where(c => c.chipsetType == type).ToList();
    }
    
    public List<ArmorChipsetData> GetArmorChipsetsByType(ArmorChipsetType type)
    {
        return _armorChipsets.Where(c => c.chipsetType == type).ToList();
    }
    
    public List<PlayerChipsetData> GetPlayerChipsetsByType(PlayerChipsetType type)
    {
        return _playerChipsets.Where(c => c.chipsetType == type).ToList();
    }
    
    public List<WeaponChipsetData> GetWeaponChipsetsByRarity(ChipsetRarity rarity)
    {
        return _weaponChipsets.Where(c => c.rarity == rarity).ToList();
    }
    
    public List<ArmorChipsetData> GetArmorChipsetsByRarity(ChipsetRarity rarity)
    {
        return _armorChipsets.Where(c => c.rarity == rarity).ToList();
    }
    
    public List<PlayerChipsetData> GetPlayerChipsetsByRarity(ChipsetRarity rarity)
    {
        return _playerChipsets.Where(c => c.rarity == rarity).ToList();
    }
    
    // 모든 칩셋 데이터 반환 메서드들
    public List<WeaponChipsetData> GetAllWeaponChipsets()
    {
        return _weaponChipsets;
    }
    
    public List<ArmorChipsetData> GetAllArmorChipsets()
    {
        return _armorChipsets;
    }
    
    public List<PlayerChipsetData> GetAllPlayerChipsets()
    {
        return _playerChipsets;
    }

    #endregion

    #region 디버그 메서드

    /// <summary>
    /// 현재 로드된 데이터 상태를 출력합니다
    /// </summary>
    [ContextMenu("데이터 상태 출력")]
    public void PrintDataStatus()
    {
        // Debug.Log($"[GameDataRepository] 데이터 상태:");
        // Debug.Log($"  무기: {_weapons.Count}개 (로드됨: {_weaponsLoaded})");
        // Debug.Log($"  방어구: {_armors.Count}개 (로드됨: {_armorsLoaded})");
        // Debug.Log($"  보스 패턴: {_bossPatterns.Count}개 (로드됨: {_bossPatternsLoaded})");
        // Debug.Log($"  전체 로드 완료: {IsAllDataLoaded}");
    }

    #endregion

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (googleSheetsManager != null)
        {
            googleSheetsManager.OnWeaponsLoaded -= OnWeaponsLoadedFromSheets;
            googleSheetsManager.OnArmorsLoaded -= OnArmorsLoadedFromSheets;
            googleSheetsManager.OnError -= OnSheetsError;
        }
        // 싱글톤 인스턴스 정리
        if (_instance == this)
            _instance = null;
    }
} 