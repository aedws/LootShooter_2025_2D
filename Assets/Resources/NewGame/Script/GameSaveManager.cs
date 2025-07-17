using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 게임 전체 세이브/로드 시스템
/// 플레이어 상태, 인벤토리, 칩셋, 게임 진행도 등을 포괄적으로 관리
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameSaveManager _instance;
    public static GameSaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameSaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSaveManager");
                    _instance = go.AddComponent<GameSaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("세이브 설정")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5분마다 자동 저장
    [SerializeField] private bool showSaveLogs = true;

    [Header("세이브 데이터 버전")]
    [SerializeField] private string saveVersion = "1.0.0";

    // 자동 저장 관련
    private Coroutine autoSaveCoroutine;
    private float lastSaveTime = 0f;

    // 이벤트
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    public event Action<string> OnSaveError;

    // 세이브 데이터 구조
    [System.Serializable]
    public class GameSaveData
    {
        public string version = "1.0.0";
        public long saveTimestamp;
        public string saveDate;
        public float playTime; // 총 플레이 시간

        // 플레이어 기본 정보
        public PlayerSaveData playerData = new PlayerSaveData();
        
        // 인벤토리 정보
        public InventorySaveData inventoryData = new InventorySaveData();
        
        // 칩셋 정보
        public ChipsetSaveData chipsetData = new ChipsetSaveData();
        
        // 게임 진행도
        public ProgressSaveData progressData = new ProgressSaveData();
        
        // 설정 정보
        public SettingsSaveData settingsData = new SettingsSaveData();
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public int currentHealth;
        public int maxHealth;
        public Vector3 position;
        public float experience;
        public int level;
        public int score;
        public float playTime;
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<string> weaponNames = new List<string>();
        public string equippedWeaponName;
        public List<string> armorNames = new List<string>();
        public Dictionary<string, string> equippedArmors = new Dictionary<string, string>();
        public int money;
        public List<string> collectedItems = new List<string>();
    }

    [System.Serializable]
    public class ChipsetSaveData
    {
        public List<string> weaponChipsetIds = new List<string>();
        public List<string> armorChipsetIds = new List<string>();
        public List<string> playerChipsetIds = new List<string>();
        public Dictionary<string, List<string>> equippedChipsets = new Dictionary<string, List<string>>();
    }

    [System.Serializable]
    public class ProgressSaveData
    {
        public int currentWave;
        public int maxWaveReached;
        public int enemiesKilled;
        public int bossesDefeated;
        public List<string> completedAchievements = new List<string>();
        public Dictionary<string, bool> unlockedFeatures = new Dictionary<string, bool>();
    }

    [System.Serializable]
    public class SettingsSaveData
    {
        public float bgmVolume;
        public float sfxVolume;
        public bool fullscreen;
        public int qualityLevel;
        public string language;
        public Dictionary<string, float> customSettings = new Dictionary<string, float>();
    }

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
    }

    private void Start()
    {
        // 자동 저장 시작
        if (autoSaveEnabled)
        {
            StartAutoSave();
        }
    }

    /// <summary>
    /// 자동 저장 시작
    /// </summary>
    public void StartAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
        
        autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        
        if (showSaveLogs)
        {
            Debug.Log($"[GameSaveManager] 자동 저장 시작 - {autoSaveInterval}초 간격");
        }
    }

    /// <summary>
    /// 자동 저장 중지
    /// </summary>
    public void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
            
            if (showSaveLogs)
            {
                Debug.Log("[GameSaveManager] 자동 저장 중지");
            }
        }
    }

    /// <summary>
    /// 자동 저장 코루틴
    /// </summary>
    private System.Collections.IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            
            if (showSaveLogs)
            {
                Debug.Log("[GameSaveManager] 자동 저장 실행");
            }
            
            lastSaveTime = Time.time;
            SaveGame();
        }
    }

    /// <summary>
    /// 게임 저장
    /// </summary>
    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = CollectGameData();
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetSaveFilePath();
            
            // 백업 파일 생성
            CreateBackup();
            
            // 새 파일 저장
            File.WriteAllText(filePath, json);
            
            if (showSaveLogs)
            {
                Debug.Log($"[GameSaveManager] 게임 저장 완료: {filePath}");
            }
            
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            string errorMsg = $"게임 저장 실패: {e.Message}";
            Debug.LogError($"[GameSaveManager] {errorMsg}");
            OnSaveError?.Invoke(errorMsg);
        }
    }

    /// <summary>
    /// 게임 로드
    /// </summary>
    public void LoadGame()
    {
        try
        {
            string filePath = GetSaveFilePath();
            
            if (!File.Exists(filePath))
            {
                string errorMsg = "저장 파일이 없습니다.";
                Debug.LogWarning($"[GameSaveManager] {errorMsg}");
                OnSaveError?.Invoke(errorMsg);
                return;
            }
            
            string json = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            // 버전 호환성 검사
            if (!IsVersionCompatible(saveData.version))
            {
                Debug.LogWarning($"[GameSaveManager] 저장 파일 버전이 다릅니다. 현재: {saveVersion}, 저장: {saveData.version}");
            }
            
            ApplyGameData(saveData);
            
            if (showSaveLogs)
            {
                Debug.Log("[GameSaveManager] 게임 로드 완료");
            }
            
            OnGameLoaded?.Invoke();
        }
        catch (Exception e)
        {
            string errorMsg = $"게임 로드 실패: {e.Message}";
            Debug.LogError($"[GameSaveManager] {errorMsg}");
            OnSaveError?.Invoke(errorMsg);
        }
    }

    /// <summary>
    /// 저장 파일 삭제
    /// </summary>
    public void DeleteSaveFile()
    {
        try
        {
            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                
                // 백업 파일도 삭제
                string backupPath = GetBackupFilePath();
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                
                if (showSaveLogs)
                {
                    Debug.Log("[GameSaveManager] 저장 파일 삭제 완료");
                }
            }
            else
            {
                Debug.Log("[GameSaveManager] 저장 파일이 이미 없습니다.");
            }
        }
        catch (Exception e)
        {
            string errorMsg = $"저장 파일 삭제 실패: {e.Message}";
            Debug.LogError($"[GameSaveManager] {errorMsg}");
            OnSaveError?.Invoke(errorMsg);
        }
    }

    /// <summary>
    /// 저장 파일 존재 여부 확인
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(GetSaveFilePath());
    }

    /// <summary>
    /// 게임 데이터 수집
    /// </summary>
    private GameSaveData CollectGameData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.version = saveVersion;
        saveData.saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.playTime = Time.time;

        // 플레이어 데이터 수집
        CollectPlayerData(saveData.playerData);
        
        // 인벤토리 데이터 수집
        CollectInventoryData(saveData.inventoryData);
        
        // 칩셋 데이터 수집
        CollectChipsetData(saveData.chipsetData);
        
        // 진행도 데이터 수집
        CollectProgressData(saveData.progressData);
        
        // 설정 데이터 수집
        CollectSettingsData(saveData.settingsData);

        return saveData;
    }

    /// <summary>
    /// 플레이어 데이터 수집
    /// </summary>
    private void CollectPlayerData(PlayerSaveData playerData)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null)
            {
                playerData.currentHealth = health.currentHealth;
                playerData.maxHealth = health.maxHealth;
            }
            
            playerData.position = player.transform.position;
            // 추가 플레이어 데이터 수집 가능
        }
    }

    /// <summary>
    /// 인벤토리 데이터 수집
    /// </summary>
    private void CollectInventoryData(InventorySaveData inventoryData)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            var inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // 무기 데이터
                var weapons = inventory.GetWeapons();
                inventoryData.weaponNames = weapons.Select(w => w.weaponName).ToList();
                inventoryData.equippedWeaponName = inventory.GetEquippedWeapon()?.weaponName;

                // 방어구 데이터
                var armors = inventory.GetAllEquippedArmors();
                inventoryData.armorNames = armors.Values.Where(a => a != null).Select(a => a.armorName).ToList();
                inventoryData.equippedArmors = armors.Where(kv => kv.Value != null)
                                                    .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.armorName);
            }
        }
    }

    /// <summary>
    /// 칩셋 데이터 수집
    /// </summary>
    private void CollectChipsetData(ChipsetSaveData chipsetData)
    {
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 칩셋 인벤토리 데이터 수집
            chipsetData.weaponChipsetIds = new List<string>(chipsetManager.playerWeaponChipsetInventory);
            chipsetData.armorChipsetIds = new List<string>(chipsetManager.playerArmorChipsetInventory);
            chipsetData.playerChipsetIds = new List<string>(chipsetManager.playerPlayerChipsetInventory);
            
            // 장착된 칩셋 데이터 수집
            chipsetData.equippedChipsets = new Dictionary<string, List<string>>();
            
            // 플레이어 칩셋 장착 상태
            if (chipsetManager.playerChipsetIds != null)
            {
                chipsetData.equippedChipsets["Player"] = new List<string>(chipsetManager.playerChipsetIds);
            }
            
            // 무기/방어구 칩셋 장착 상태는 각 슬롯에서 수집
            var weaponSlots = chipsetManager.GetWeaponSlots();
            var armorSlots = chipsetManager.GetArmorSlots();
            
            if (weaponSlots != null)
            {
                var equippedWeaponChipsets = new List<string>();
                foreach (var slot in weaponSlots)
                {
                    if (slot != null && slot.IsEquipped())
                    {
                        var chipset = slot.GetEquippedChipset();
                        if (chipset != null)
                        {
                            string chipsetId = GetChipsetId(chipset);
                            if (!string.IsNullOrEmpty(chipsetId))
                                equippedWeaponChipsets.Add(chipsetId);
                        }
                    }
                }
                chipsetData.equippedChipsets["Weapon"] = equippedWeaponChipsets;
            }
            
            if (armorSlots != null)
            {
                var equippedArmorChipsets = new List<string>();
                foreach (var slot in armorSlots)
                {
                    if (slot != null && slot.IsEquipped())
                    {
                        var chipset = slot.GetEquippedChipset();
                        if (chipset != null)
                        {
                            string chipsetId = GetChipsetId(chipset);
                            if (!string.IsNullOrEmpty(chipsetId))
                                equippedArmorChipsets.Add(chipsetId);
                        }
                    }
                }
                chipsetData.equippedChipsets["Armor"] = equippedArmorChipsets;
            }
        }
    }

    /// <summary>
    /// 진행도 데이터 수집
    /// </summary>
    private void CollectProgressData(ProgressSaveData progressData)
    {
        // 게임 진행도 관련 매니저에서 데이터 수집
        // 예: WaveManager, AchievementManager 등
    }

    /// <summary>
    /// 설정 데이터 수집
    /// </summary>
    private void CollectSettingsData(SettingsSaveData settingsData)
    {
        // BGM 볼륨
        var bgmPlayer = BGMPlayer.Instance;
        if (bgmPlayer != null)
        {
            settingsData.bgmVolume = bgmPlayer.GetVolume();
        }
        
        // 기타 설정들
        settingsData.fullscreen = Screen.fullScreen;
        settingsData.qualityLevel = QualitySettings.GetQualityLevel();
    }

    /// <summary>
    /// 게임 데이터 적용
    /// </summary>
    private void ApplyGameData(GameSaveData saveData)
    {
        // 플레이어 데이터 적용
        ApplyPlayerData(saveData.playerData);
        
        // 인벤토리 데이터 적용
        ApplyInventoryData(saveData.inventoryData);
        
        // 칩셋 데이터 적용
        ApplyChipsetData(saveData.chipsetData);
        
        // 진행도 데이터 적용
        ApplyProgressData(saveData.progressData);
        
        // 설정 데이터 적용
        ApplySettingsData(saveData.settingsData);
    }

    /// <summary>
    /// 플레이어 데이터 적용
    /// </summary>
    private void ApplyPlayerData(PlayerSaveData playerData)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null)
            {
                health.SetMaxHealth(playerData.maxHealth);
                // 체력은 SetMaxHealth에서 비율에 맞게 조정됨
            }
            
            player.transform.position = playerData.position;
        }
    }

    /// <summary>
    /// 인벤토리 데이터 적용
    /// </summary>
    private void ApplyInventoryData(InventorySaveData inventoryData)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            var inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // 무기 복원
                inventory.ClearWeapons();
                foreach (var weaponName in inventoryData.weaponNames)
                {
                    var weaponData = GameDataRepository.Instance.GetWeaponByName(weaponName);
                    if (weaponData != null)
                        inventory.AddWeapon(weaponData);
                }
                
                if (!string.IsNullOrEmpty(inventoryData.equippedWeaponName))
                {
                    var weaponData = GameDataRepository.Instance.GetWeaponByName(inventoryData.equippedWeaponName);
                    if (weaponData != null)
                        inventory.SetEquippedWeapon(weaponData);
                }

                // 방어구 복원
                inventory.ClearArmors();
                foreach (var armorName in inventoryData.armorNames)
                {
                    var armorData = GameDataRepository.Instance.GetArmorByName(armorName);
                    if (armorData != null)
                        inventory.AddArmor(armorData);
                }
                
                foreach (var kv in inventoryData.equippedArmors)
                {
                    var armorData = GameDataRepository.Instance.GetArmorByName(kv.Value);
                    if (armorData != null)
                        inventory.SetEquippedArmor(armorData, (ArmorType)System.Enum.Parse(typeof(ArmorType), kv.Key));
                }
            }
        }
    }

    /// <summary>
    /// 칩셋 데이터 적용
    /// </summary>
    private void ApplyChipsetData(ChipsetSaveData chipsetData)
    {
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 칩셋 인벤토리 데이터 적용
            chipsetManager.SetChipsetInventoryData(
                chipsetData.weaponChipsetIds,
                chipsetData.armorChipsetIds,
                chipsetData.playerChipsetIds
            );
            
            // 플레이어 칩셋 장착 상태 적용
            if (chipsetData.equippedChipsets.ContainsKey("Player"))
            {
                chipsetManager.SetPlayerChipsetIds(chipsetData.equippedChipsets["Player"].ToArray());
            }
            
            // 무기/방어구 칩셋 장착 상태 적용
            ApplyWeaponChipsets(chipsetManager, chipsetData);
            ApplyArmorChipsets(chipsetManager, chipsetData);
        }
    }

    /// <summary>
    /// 무기 칩셋 장착 상태 적용
    /// </summary>
    private void ApplyWeaponChipsets(ChipsetManager chipsetManager, ChipsetSaveData chipsetData)
    {
        if (!chipsetData.equippedChipsets.ContainsKey("Weapon")) return;
        
        var weaponSlots = chipsetManager.GetWeaponSlots();
        if (weaponSlots == null) return;
        
        var equippedWeaponChipsets = chipsetData.equippedChipsets["Weapon"];
        
        // 기존 장착 해제
        foreach (var slot in weaponSlots)
        {
            if (slot != null && slot.IsEquipped())
            {
                slot.UnequipChipset();
            }
        }
        
        // 새로운 칩셋 장착
        for (int i = 0; i < equippedWeaponChipsets.Count && i < weaponSlots.Length; i++)
        {
            if (!string.IsNullOrEmpty(equippedWeaponChipsets[i]))
            {
                var chipset = GameDataRepository.Instance.GetWeaponChipsetById(equippedWeaponChipsets[i]);
                if (chipset != null)
                {
                    weaponSlots[i].EquipChipset(chipset);
                }
            }
        }
    }

    /// <summary>
    /// 방어구 칩셋 장착 상태 적용
    /// </summary>
    private void ApplyArmorChipsets(ChipsetManager chipsetManager, ChipsetSaveData chipsetData)
    {
        if (!chipsetData.equippedChipsets.ContainsKey("Armor")) return;
        
        var armorSlots = chipsetManager.GetArmorSlots();
        if (armorSlots == null) return;
        
        var equippedArmorChipsets = chipsetData.equippedChipsets["Armor"];
        
        // 기존 장착 해제
        foreach (var slot in armorSlots)
        {
            if (slot != null && slot.IsEquipped())
            {
                slot.UnequipChipset();
            }
        }
        
        // 새로운 칩셋 장착
        for (int i = 0; i < equippedArmorChipsets.Count && i < armorSlots.Length; i++)
        {
            if (!string.IsNullOrEmpty(equippedArmorChipsets[i]))
            {
                var chipset = GameDataRepository.Instance.GetArmorChipsetById(equippedArmorChipsets[i]);
                if (chipset != null)
                {
                    armorSlots[i].EquipChipset(chipset);
                }
            }
        }
    }

    /// <summary>
    /// 칩셋 ID 가져오기
    /// </summary>
    private string GetChipsetId(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipset)
            return weaponChipset.chipsetId;
        else if (chipset is ArmorChipsetData armorChipset)
            return armorChipset.chipsetId;
        else if (chipset is PlayerChipsetData playerChipset)
            return playerChipset.chipsetId;
        else
            return null;
    }

    /// <summary>
    /// 진행도 데이터 적용
    /// </summary>
    private void ApplyProgressData(ProgressSaveData progressData)
    {
        // 게임 진행도 관련 매니저에서 데이터 적용
    }

    /// <summary>
    /// 설정 데이터 적용
    /// </summary>
    private void ApplySettingsData(SettingsSaveData settingsData)
    {
        // BGM 볼륨 적용
        var bgmPlayer = BGMPlayer.Instance;
        if (bgmPlayer != null)
        {
            bgmPlayer.SetVolume(settingsData.bgmVolume);
        }
        
        // 기타 설정들 적용
        Screen.fullScreen = settingsData.fullscreen;
        QualitySettings.SetQualityLevel(settingsData.qualityLevel);
    }

    /// <summary>
    /// 세이브 파일 경로 반환
    /// </summary>
    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "save.json");
    }

    /// <summary>
    /// 백업 파일 경로 반환
    /// </summary>
    private string GetBackupFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "save_backup.json");
    }

    /// <summary>
    /// 백업 파일 생성
    /// </summary>
    private void CreateBackup()
    {
        string originalPath = GetSaveFilePath();
        if (File.Exists(originalPath))
        {
            string backupPath = GetBackupFilePath();
            File.Copy(originalPath, backupPath, true);
        }
    }

    /// <summary>
    /// 버전 호환성 검사
    /// </summary>
    private bool IsVersionCompatible(string savedVersion)
    {
        // 간단한 버전 비교 (메이저 버전만 체크)
        string currentMajor = saveVersion.Split('.')[0];
        string savedMajor = savedVersion.Split('.')[0];
        return currentMajor == savedMajor;
    }

    private void OnDestroy()
    {
        // 자동 저장 중지
        StopAutoSave();
    }
} 