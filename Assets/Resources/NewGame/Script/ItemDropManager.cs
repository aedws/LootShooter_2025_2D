using UnityEngine;
using System.Collections.Generic;

public class ItemDropManager : MonoBehaviour
{
    [Header("드랍 설정")]
    public float dropRadius = 1.5f; // 드랍 반경
    public bool useNetworkPickup = true; // 네트워크 픽업 시스템 사용 여부
    
    [Header("디버그")]
    public bool debugMode = false;
    
    private static ItemDropManager instance;
    public static ItemDropManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ItemDropManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ItemDropManager");
                    instance = go.AddComponent<ItemDropManager>();
                }
            }
            return instance;
        }
    }
    
    // 드랍 테이블 데이터
    private DropTableData dropTableData;
    private bool isDropTableLoaded = false;
    
    // 무기/방어구 프리팹 캐시
    private List<WeaponData> weaponDataList = new List<WeaponData>();
    private List<ArmorData> armorDataList = new List<ArmorData>();
    
    // 칩셋 데이터 로드 상태
    private bool isChipsetDataLoaded = false;
    
    // 선택된 칩셋 데이터를 저장할 변수
    private object selectedChipsetData = null;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 구글 시트 매니저 이벤트 구독
        GoogleSheetsManager sheetsManager = FindFirstObjectByType<GoogleSheetsManager>();
        if (sheetsManager != null)
        {
            sheetsManager.OnDropTableLoaded += OnDropTableLoaded;
            sheetsManager.OnWeaponsLoaded += OnWeaponsLoaded;
            sheetsManager.OnArmorsLoaded += OnArmorsLoaded;
            sheetsManager.OnWeaponChipsetsLoaded += OnWeaponChipsetsLoaded;
            sheetsManager.OnArmorChipsetsLoaded += OnArmorChipsetsLoaded;
            sheetsManager.OnPlayerChipsetsLoaded += OnPlayerChipsetsLoaded;
            sheetsManager.OnError += OnGoogleSheetsError;
        }
        
        // 드랍 테이블 및 칩셋 데이터 로드
        LoadDropTable();
        LoadChipsetData();
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        GoogleSheetsManager sheetsManager = FindFirstObjectByType<GoogleSheetsManager>();
        if (sheetsManager != null)
        {
            sheetsManager.OnDropTableLoaded -= OnDropTableLoaded;
            sheetsManager.OnWeaponsLoaded -= OnWeaponsLoaded;
            sheetsManager.OnArmorsLoaded -= OnArmorsLoaded;
            sheetsManager.OnWeaponChipsetsLoaded -= OnWeaponChipsetsLoaded;
            sheetsManager.OnArmorChipsetsLoaded -= OnArmorChipsetsLoaded;
            sheetsManager.OnPlayerChipsetsLoaded -= OnPlayerChipsetsLoaded;
            sheetsManager.OnError -= OnGoogleSheetsError;
        }
    }
    
    private void LoadDropTable()
    {
        GoogleSheetsManager sheetsManager = FindFirstObjectByType<GoogleSheetsManager>();
        if (sheetsManager != null)
        {
            sheetsManager.LoadDropTable();
            sheetsManager.LoadWeapons();
            sheetsManager.LoadArmors();
        }
        else
        {
            Debug.LogError("[ItemDropManager] GoogleSheetsManager를 찾을 수 없습니다!");
        }
    }
    
    private void OnDropTableLoaded(DropTableData data)
    {
        dropTableData = data;
        isDropTableLoaded = true;
        
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 드랍 테이블 로드 완료: {data.MonsterInfos.Count}개 몬스터, {data.ItemTypeDropRates.Count}개 아이템 타입, {data.MonsterRarityDropRates.Count}개 등급 드랍률");
        }
    }
    
    private void OnWeaponsLoaded(List<WeaponData> weapons)
    {
        weaponDataList = weapons;
        
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 무기 데이터 로드 완료: {weapons.Count}개");
        }
    }
    
    private void OnArmorsLoaded(List<ArmorData> armors)
    {
        armorDataList = armors;
        
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 방어구 데이터 로드 완료: {armors.Count}개");
        }
    }
    
    private void OnGoogleSheetsError(string error)
    {
        Debug.LogError($"[ItemDropManager] 구글 시트 오류: {error}");
    }
    
    private void LoadChipsetData()
    {
        GoogleSheetsManager sheetsManager = FindFirstObjectByType<GoogleSheetsManager>();
        if (sheetsManager != null)
        {
            sheetsManager.LoadWeaponChipsets();
            sheetsManager.LoadArmorChipsets();
            sheetsManager.LoadPlayerChipsets();
        }
        else
        {
            Debug.LogError("[ItemDropManager] GoogleSheetsManager를 찾을 수 없습니다!");
        }
    }
    
    private void OnWeaponChipsetsLoaded(List<WeaponChipsetData> chipsets)
    {
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 무기 칩셋 데이터 로드 완료: {chipsets.Count}개");
        }
        CheckChipsetDataLoaded();
    }
    
    private void OnArmorChipsetsLoaded(List<ArmorChipsetData> chipsets)
    {
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 방어구 칩셋 데이터 로드 완료: {chipsets.Count}개");
        }
        CheckChipsetDataLoaded();
    }
    
    private void OnPlayerChipsetsLoaded(List<PlayerChipsetData> chipsets)
    {
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 플레이어 칩셋 데이터 로드 완료: {chipsets.Count}개");
        }
        CheckChipsetDataLoaded();
    }
    
    private void CheckChipsetDataLoaded()
    {
        // GameDataRepository를 통해 칩셋 데이터가 모두 로드되었는지 확인
        if (GameDataRepository.Instance != null && 
            GameDataRepository.Instance.GetAllWeaponChipsets().Count > 0 &&
            GameDataRepository.Instance.GetAllArmorChipsets().Count > 0 &&
            GameDataRepository.Instance.GetAllPlayerChipsets().Count > 0)
        {
            isChipsetDataLoaded = true;
            if (debugMode)
            {
                Debug.Log("[ItemDropManager] 모든 칩셋 데이터 로드 완료");
            }
        }
    }
    
    /// <summary>
    /// 몬스터가 죽었을 때 아이템을 드랍합니다.
    /// </summary>
    public void DropItemsFromEnemy(string enemyType, Vector3 position)
    {
        if (debugMode)
            Debug.Log($"[ItemDropManager] {enemyType}에서 아이템 드랍 시도: {position}");
        
        // 구글 시트 데이터가 로드되지 않았으면 드랍하지 않음
        if (!isDropTableLoaded)
        {
            if (debugMode)
                Debug.LogWarning("[ItemDropManager] 드랍 테이블이 아직 로드되지 않았습니다.");
            return;
        }
        
        // 해당 몬스터의 정보 찾기
        MonsterInfo monsterInfo = dropTableData.GetMonsterInfo(enemyType);
        if (monsterInfo == null)
        {
            if (debugMode)
                Debug.LogWarning($"[ItemDropManager] {enemyType}의 몬스터 정보를 찾을 수 없습니다.");
            return;
        }
        
        // 드랍 확률 체크
        if (Random.value > monsterInfo.DropChance)
        {
            if (debugMode)
                Debug.Log($"[ItemDropManager] {enemyType} 드랍 실패 (확률: {monsterInfo.DropChance})");
            return;
        }
        
        // 드랍할 아이템 수 결정
        int dropCount = Random.Range(monsterInfo.MinDropCount, monsterInfo.MaxDropCount + 1);
        
        if (debugMode)
            Debug.Log($"[ItemDropManager] {enemyType} 드랍 성공: {dropCount}개 아이템");
        
        // 아이템 타입별 드랍 확률 가져오기
        ItemTypeDropRate itemTypeRate = dropTableData.GetItemTypeDropRate(enemyType);
        MonsterRarityDropRate rarityRate = dropTableData.GetMonsterRarityDropRate(enemyType);
        
        if (itemTypeRate == null || rarityRate == null)
        {
            if (debugMode)
                Debug.LogWarning($"[ItemDropManager] {enemyType}의 드랍 확률 정보를 찾을 수 없습니다.");
            return;
        }
        
        // 아이템 드랍
        for (int i = 0; i < dropCount; i++)
        {
            DropRandomItem(position, itemTypeRate, rarityRate);
        }
    }
    
    /// <summary>
    /// 랜덤 아이템을 드랍합니다.
    /// </summary>
    private void DropRandomItem(Vector3 position, ItemTypeDropRate itemTypeRate, MonsterRarityDropRate rarityRate)
    {
        // 아이템 타입 결정
        float typeRoll = Random.value;
        string itemType;
        
        float weaponThreshold = itemTypeRate.WeaponDropRate;
        float armorThreshold = weaponThreshold + itemTypeRate.ArmorDropRate;
        float accessoryThreshold = armorThreshold + itemTypeRate.AccessoryDropRate;
        float moduleThreshold = accessoryThreshold + itemTypeRate.ModuleDropRate;
        
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 타입 결정 - Roll: {typeRoll:F3}, Weapon: {weaponThreshold:F3}, Armor: {armorThreshold:F3}, Accessory: {accessoryThreshold:F3}, Module: {moduleThreshold:F3}");
        }
        
        if (typeRoll < weaponThreshold)
        {
            itemType = "Weapon";
        }
        else if (typeRoll < armorThreshold)
        {
            itemType = "Armor";
        }
        else if (typeRoll < accessoryThreshold)
        {
            itemType = "Accessory";
        }
        else if (typeRoll < moduleThreshold)
        {
            itemType = "Module";
        }
        else
        {
            // 모든 확률을 초과한 경우 기본값으로 무기 드랍
            itemType = "Weapon";
        }
        
        if (debugMode)
        {
            Debug.Log($"[ItemDropManager] 선택된 아이템 타입: {itemType}");
        }
        
        // 등급 결정
        float rarityRoll = Random.value;
        string rarity;
        
        if (rarityRoll < rarityRate.CommonRate)
        {
            rarity = "Common";
        }
        else if (rarityRoll < rarityRate.CommonRate + rarityRate.RareRate)
        {
            rarity = "Rare";
        }
        else if (rarityRoll < rarityRate.CommonRate + rarityRate.RareRate + rarityRate.EpicRate)
        {
            rarity = "Epic";
        }
        else if (rarityRoll < rarityRate.CommonRate + rarityRate.RareRate + rarityRate.EpicRate + rarityRate.LegendaryRate)
        {
            rarity = "Legendary";
        }
        else
        {
            rarity = "Primordial";
        }
        
        // 해당 타입과 등급의 아이템 찾기
        GameObject itemPrefab = GetRandomItemPrefab(itemType, rarity);
        if (itemPrefab != null)
        {
            DropItemAtPosition(itemPrefab, position);
            
            if (debugMode)
                Debug.Log($"[ItemDropManager] 아이템 드랍: {itemType} {rarity} - {itemPrefab.name}");
        }
        else
        {
            if (debugMode)
                Debug.LogWarning($"[ItemDropManager] {itemType} {rarity} 등급의 아이템을 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 해당 타입과 등급의 랜덤 아이템 프리팹을 반환합니다.
    /// </summary>
    private GameObject GetRandomItemPrefab(string itemType, string rarity)
    {
        List<GameObject> candidates = new List<GameObject>();
        
        if (itemType == "Weapon")
        {
            // 무기 프리팹에서 해당 등급 찾기
            foreach (WeaponData weaponData in weaponDataList)
            {
                if (weaponData.rarity.ToString() == rarity)
                {
                    // Network 폴더의 무기 픽업 프리팹 사용
                    #if UNITY_EDITOR
                    string prefabPath = $"Assets/Resources/NewGame/Prefab/Network/WeaponPickup_{weaponData.weaponType}.prefab";
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                    GameObject prefab = Resources.Load<GameObject>($"NewGame/Prefab/Network/WeaponPickup_{weaponData.weaponType}");
#endif
                    
                    if (prefab != null)
                    {
                        candidates.Add(prefab);
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 무기 프리팹 로드 성공: {prefabPath}");
                    }
                    else
                    {
                        if (debugMode)
                            Debug.LogWarning($"[ItemDropManager] 무기 프리팹 로드 실패: {prefabPath}");
                    }
                }
            }
        }
        else if (itemType == "Armor")
        {
            // 방어구 프리팹에서 해당 등급 찾기
            foreach (ArmorData armorData in armorDataList)
            {
                if (armorData.rarity.ToString() == rarity)
                {
                    // Network 폴더의 방어구 픽업 프리팹 사용
                    #if UNITY_EDITOR
                    string prefabPath = $"Assets/Resources/NewGame/Prefab/Network/ArmorPickup_{armorData.armorType}.prefab";
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                    GameObject prefab = Resources.Load<GameObject>($"NewGame/Prefab/Network/ArmorPickup_{armorData.armorType}");
#endif
                    
                    if (prefab != null)
                    {
                        candidates.Add(prefab);
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 방어구 프리팹 로드 성공: {prefabPath}");
                    }
                    else
                    {
                        if (debugMode)
                            Debug.LogWarning($"[ItemDropManager] 방어구 프리팹 로드 실패: {prefabPath}");
                    }
                }
            }
        }
        else if (itemType == "Accessory")
        {
            // 장신구는 별도 처리 (현재는 간단히 방어구로 처리)
            foreach (ArmorData armorData in armorDataList)
            {
                if (armorData.rarity.ToString() == rarity && 
                    (armorData.armorType == ArmorType.Accessory || armorData.armorType == ArmorType.Shoulder))
                {
                    // Network 폴더의 장신구 픽업 프리팹 사용
                    #if UNITY_EDITOR
                    string prefabPath = $"Assets/Resources/NewGame/Prefab/Network/ArmorPickup_{armorData.armorType}.prefab";
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                    GameObject prefab = Resources.Load<GameObject>($"NewGame/Prefab/Network/ArmorPickup_{armorData.armorType}");
#endif
                    
                    if (prefab != null)
                    {
                        candidates.Add(prefab);
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 장신구 프리팹 로드 성공: {prefabPath}");
                    }
                }
            }
        }
        else if (itemType == "Module")
        {
            if (debugMode)
                Debug.Log($"[ItemDropManager] 모듈 드랍 시도 - 칩셋 데이터 로드 상태: {isChipsetDataLoaded}");
            
            // 칩셋 데이터가 로드되지 않았으면 드랍하지 않음
            if (!isChipsetDataLoaded)
            {
                if (debugMode)
                    Debug.LogWarning("[ItemDropManager] 칩셋 데이터가 아직 로드되지 않았습니다.");
                return null;
            }
            
            // 모듈은 칩셋 데이터에서 처리
            // 무기 칩셋, 방어구 칩셋, 플레이어 칩셋 중 랜덤 선택
            int chipsetType = Random.Range(0, 3); // 0: 무기, 1: 방어구, 2: 플레이어
            
            if (debugMode)
                Debug.Log($"[ItemDropManager] 칩셋 타입 선택: {chipsetType} (0:무기, 1:방어구, 2:플레이어)");
            
            switch (chipsetType)
            {
                case 0: // 무기 칩셋
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 무기 칩셋 검색 시작 - 등급: {rarity}");
                    
                    var weaponChipsets = GameDataRepository.Instance.GetWeaponChipsetsByRarity(ParseChipsetRarity(rarity));
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 무기 칩셋 검색 결과: {weaponChipsets.Count}개");
                    
                    if (weaponChipsets.Count > 0)
                    {
                        var randomWeaponChipset = weaponChipsets[Random.Range(0, weaponChipsets.Count)];
                        selectedChipsetData = randomWeaponChipset;
                        string prefabPath = $"Assets/Resources/NewGame/Prefab/ChipsetPickup.prefab";
                        
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 무기 칩셋 프리팹 경로: {prefabPath}");
                        
                        #if UNITY_EDITOR
                        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                        GameObject prefab = Resources.Load<GameObject>("NewGame/Prefab/ChipsetPickup");
#endif
                        
                        if (prefab != null)
                        {
                            candidates.Add(prefab);
                            if (debugMode)
                                Debug.Log($"[ItemDropManager] 무기 칩셋 프리팹 로드 성공: {randomWeaponChipset.chipsetName} (등급: {rarity})");
                        }
                        else
                        {
                            if (debugMode)
                                Debug.LogWarning($"[ItemDropManager] 무기 칩셋 프리팹 로드 실패: {prefabPath}");
                        }
                    }
                    else
                    {
                        if (debugMode)
                            Debug.LogWarning($"[ItemDropManager] {rarity} 등급의 무기 칩셋이 없습니다.");
                    }
                    break;
                    
                case 1: // 방어구 칩셋
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 방어구 칩셋 검색 시작 - 등급: {rarity}");
                    
                    var armorChipsets = GameDataRepository.Instance.GetArmorChipsetsByRarity(ParseChipsetRarity(rarity));
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 방어구 칩셋 검색 결과: {armorChipsets.Count}개");
                    
                    if (armorChipsets.Count > 0)
                    {
                        var randomArmorChipset = armorChipsets[Random.Range(0, armorChipsets.Count)];
                        selectedChipsetData = randomArmorChipset;
                        string prefabPath = $"Assets/Resources/NewGame/Prefab/ChipsetPickup.prefab";
                        
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 방어구 칩셋 프리팹 경로: {prefabPath}");
                        
                        #if UNITY_EDITOR
                        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                        GameObject prefab = Resources.Load<GameObject>("NewGame/Prefab/ChipsetPickup");
#endif
                        
                        if (prefab != null)
                        {
                            candidates.Add(prefab);
                            if (debugMode)
                                Debug.Log($"[ItemDropManager] 방어구 칩셋 프리팹 로드 성공: {randomArmorChipset.chipsetName} (등급: {rarity})");
                        }
                        else
                        {
                            if (debugMode)
                                Debug.LogWarning($"[ItemDropManager] 방어구 칩셋 프리팹 로드 실패: {prefabPath}");
                        }
                    }
                    else
                    {
                        if (debugMode)
                            Debug.LogWarning($"[ItemDropManager] {rarity} 등급의 방어구 칩셋이 없습니다.");
                    }
                    break;
                    
                case 2: // 플레이어 칩셋
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 플레이어 칩셋 검색 시작 - 등급: {rarity}");
                    
                    var playerChipsets = GameDataRepository.Instance.GetPlayerChipsetsByRarity(ParseChipsetRarity(rarity));
                    if (debugMode)
                        Debug.Log($"[ItemDropManager] 플레이어 칩셋 검색 결과: {playerChipsets.Count}개");
                    
                    if (playerChipsets.Count > 0)
                    {
                        var randomPlayerChipset = playerChipsets[Random.Range(0, playerChipsets.Count)];
                        selectedChipsetData = randomPlayerChipset;
                        string prefabPath = $"Assets/Resources/NewGame/Prefab/ChipsetPickup.prefab";
                        
                        if (debugMode)
                            Debug.Log($"[ItemDropManager] 플레이어 칩셋 프리팹 경로: {prefabPath}");
                        
                        #if UNITY_EDITOR
                        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
                        GameObject prefab = Resources.Load<GameObject>("NewGame/Prefab/ChipsetPickup");
#endif
                        
                        if (prefab != null)
                        {
                            candidates.Add(prefab);
                            if (debugMode)
                                Debug.Log($"[ItemDropManager] 플레이어 칩셋 프리팹 로드 성공: {randomPlayerChipset.chipsetName} (등급: {rarity})");
                        }
                        else
                        {
                            if (debugMode)
                                Debug.LogWarning($"[ItemDropManager] 플레이어 칩셋 프리팹 로드 실패: {prefabPath}");
                        }
                    }
                    else
                    {
                        if (debugMode)
                            Debug.LogWarning($"[ItemDropManager] {rarity} 등급의 플레이어 칩셋이 없습니다.");
                    }
                    break;
            }
            
            if (debugMode)
            {
                Debug.Log($"[ItemDropManager] 모듈 드랍 후보 수: {candidates.Count}개");
            }
            
            if (candidates.Count == 0 && debugMode)
            {
                Debug.LogWarning($"[ItemDropManager] {rarity} 등급의 칩셋을 찾을 수 없습니다.");
            }
        }
        
        // 랜덤 선택
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        if (debugMode)
            Debug.LogWarning($"[ItemDropManager] {itemType} {rarity} 등급의 아이템을 찾을 수 없습니다. (후보: {candidates.Count}개)");
        
        return null;
    }
    
    /// <summary>
    /// 지정된 위치에 아이템을 드랍합니다.
    /// </summary>
    private void DropItemAtPosition(GameObject itemPrefab, Vector3 position)
    {
        // 랜덤한 위치 계산
        Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
        Vector3 dropPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);
        
        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        
        // 칩셋 프리팹인 경우 칩셋 데이터 초기화
        var chipsetPickup = droppedItem.GetComponent<ChipsetPickup>();
        if (chipsetPickup != null && selectedChipsetData != null)
        {
            chipsetPickup.Initialize(selectedChipsetData);
            if (debugMode)
                Debug.Log($"[ItemDropManager] 칩셋 데이터 초기화 완료: {GetChipsetName(selectedChipsetData)}");
            
            // 초기화 후 선택된 칩셋 데이터 초기화
            selectedChipsetData = null;
        }
        
        // 네트워크 픽업 시스템 사용 시 추가 설정
        if (useNetworkPickup)
        {
            SetupNetworkPickup(droppedItem);
        }
        
        if (debugMode)
            Debug.Log($"[ItemDropManager] 아이템 드랍: {itemPrefab.name} at {dropPosition}");
    }
    
    /// <summary>
    /// 칩셋 이름을 반환하는 헬퍼 메서드
    /// </summary>
    private string GetChipsetName(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipset)
            return weaponChipset.chipsetName;
        else if (chipset is ArmorChipsetData armorChipset)
            return armorChipset.chipsetName;
        else if (chipset is PlayerChipsetData playerChipset)
            return playerChipset.chipsetName;
        else
            return "알 수 없는 칩셋";
    }
    
    /// <summary>
    /// 네트워크 픽업 시스템을 설정합니다.
    /// </summary>
    private void SetupNetworkPickup(GameObject item)
    {
        // 이미 NetworkWeaponPickup이나 NetworkArmorPickup이 있으면 건너뛰기
        if (item.GetComponent<NetworkWeaponPickup>() != null || item.GetComponent<NetworkArmorPickup>() != null)
        {
            return;
        }
        
        // 칩셋 프리팹인지 확인 (이름으로 판단)
        if (item.name.Contains("ChipsetPickup"))
        {
            if (debugMode)
                Debug.Log("[ItemDropManager] 칩셋 프리팹이므로 네트워크 픽업 설정을 건너뜁니다.");
            return;
        }
        
        // 무기 프리팹인지 확인 (WeaponData 컴포넌트가 있는지)
        WeaponData weaponData = item.GetComponent<WeaponData>();
        if (weaponData != null)
        {
            // 무기 픽업으로 변환
            NetworkWeaponPickup weaponPickup = item.AddComponent<NetworkWeaponPickup>();
            
            // 무기 타입 설정 (WeaponData에서 가져오기)
            weaponPickup.weaponType = weaponData.weaponType;
            
            // 등급 설정 (WeaponData의 rarity에서 가져오기)
            switch (weaponData.rarity)
            {
                case WeaponRarity.Common: weaponPickup.weaponTier = 1; break;
                case WeaponRarity.Rare: weaponPickup.weaponTier = 2; break;
                case WeaponRarity.Epic: weaponPickup.weaponTier = 3; break;
                case WeaponRarity.Legendary: weaponPickup.weaponTier = 4; break;
                case WeaponRarity.Primordial: weaponPickup.weaponTier = 5; break;
                default: weaponPickup.weaponTier = 1; break;
            }
            
            // 랜덤 등급 비활성화 (이미 설정된 등급 사용)
            weaponPickup.useRandomTier = false;
            
            if (debugMode)
                Debug.Log($"[ItemDropManager] 무기 픽업 설정: {weaponData.weaponName} (등급: {weaponData.rarity})");
        }
        
        // 방어구 프리팹인지 확인 (ArmorData 컴포넌트가 있는지)
        ArmorData armorData = item.GetComponent<ArmorData>();
        if (armorData != null)
        {
            // 방어구 픽업으로 변환
            NetworkArmorPickup armorPickup = item.AddComponent<NetworkArmorPickup>();
            
            // 방어구 타입 설정 (ArmorData에서 가져오기)
            armorPickup.armorType = armorData.armorType;
            
            // 등급 설정 (ArmorData의 rarity에서 가져오기)
            switch (armorData.rarity)
            {
                case ArmorRarity.Common: armorPickup.armorTier = 1; break;
                case ArmorRarity.Rare: armorPickup.armorTier = 2; break;
                case ArmorRarity.Epic: armorPickup.armorTier = 3; break;
                case ArmorRarity.Legendary: armorPickup.armorTier = 4; break;
                case ArmorRarity.Primordial: armorPickup.armorTier = 5; break;
                default: armorPickup.armorTier = 1; break;
            }
            
            // 랜덤 등급 비활성화 (이미 설정된 등급 사용)
            armorPickup.useRandomTier = false;
            
            if (debugMode)
                Debug.Log($"[ItemDropManager] 방어구 픽업 설정: {armorData.armorName} (등급: {armorData.rarity})");
        }
    }
    
    /// <summary>
    /// 드랍 테이블이 로드되었는지 확인합니다.
    /// </summary>
    public bool IsDropTableLoaded()
    {
        return isDropTableLoaded;
    }
    
    /// <summary>
    /// 문자열 등급을 ChipsetRarity enum으로 변환합니다.
    /// </summary>
    private ChipsetRarity ParseChipsetRarity(string rarity)
    {
        switch (rarity.ToLower())
        {
            case "common": return ChipsetRarity.Common;
            case "rare": return ChipsetRarity.Rare;
            case "epic": return ChipsetRarity.Epic;
            case "legendary": return ChipsetRarity.Legendary;
            case "primordial": return ChipsetRarity.Legendary; // Primordial은 Legendary로 처리
            default: return ChipsetRarity.Common;
        }
    }
    
    /// <summary>
    /// 드랍 테이블을 다시 로드합니다.
    /// </summary>
    public void ReloadDropTable()
    {
        isDropTableLoaded = false;
        LoadDropTable();
    }
    
    /// <summary>
    /// 몬스터 정보를 가져옵니다.
    /// </summary>
    public MonsterInfo GetMonsterInfo(string monsterID)
    {
        if (dropTableData != null)
        {
            return dropTableData.GetMonsterInfo(monsterID);
        }
        return null;
    }
    
    /// <summary>
    /// 모든 몬스터 정보를 가져옵니다.
    /// </summary>
    public List<MonsterInfo> GetAllMonsterInfos()
    {
        if (dropTableData != null)
        {
            return dropTableData.MonsterInfos;
        }
        return new List<MonsterInfo>();
    }
} 