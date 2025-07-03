using UnityEngine;

public class TestArmorSystem : MonoBehaviour
{
    [Header("🛡️ 테스트 설정")]
    [Tooltip("방어구 생성기")]
    public ArmorGenerator armorGenerator;
    
    [Tooltip("방어구 생성 위치")]
    public Transform spawnPoint;
    
    [Tooltip("플레이어 오브젝트")]
    public GameObject player;
    
    [Header("🎮 테스트 컨트롤")]
    [Tooltip("R키: 랜덤 방어구 생성")]
    public bool enableRandomSpawn = true;
    
    [Tooltip("T키: 특정 타입 방어구 생성")]
    public bool enableSpecificSpawn = true;
    
    [Tooltip("특정 타입 (T키로 생성)")]
    public ArmorType specificType = ArmorType.Chest;
    
    [Tooltip("Y키: 모든 타입 방어구 한번에 생성")]
    public bool enableAllTypesSpawn = true;
    
    [Header("📊 디버그 정보")]
    [Tooltip("현재 인벤토리의 방어구 개수")]
    public int currentArmorCount = 0;
    
    [Tooltip("현재 장착된 방어구 개수")]
    public int equippedArmorCount = 0;
    
    private InventoryManager inventoryManager;
    private PlayerInventory playerInventory;
    
    void Start()
    {
        // 레이어 설정 확인
        CheckAndSetupLayers();
        
        // 자동 연결
        if (armorGenerator == null)
            armorGenerator = FindAnyObjectByType<ArmorGenerator>();
        
        if (spawnPoint == null)
        {
            // 플레이어 근처에 스폰 포인트 생성
            GameObject spawnPointObj = new GameObject("ArmorSpawnPoint");
            spawnPointObj.transform.position = Vector3.zero;
            spawnPoint = spawnPointObj.transform;
        }
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        
        // 매니저들 연결
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        Debug.Log("🛡️ [TestArmorSystem] 방어구 테스트 시스템 초기화 완료!");
        Debug.Log("🎮 컨트롤:");
        Debug.Log("   R키: 랜덤 방어구 생성");
        Debug.Log("   T키: 특정 타입 방어구 생성");
        Debug.Log("   Y키: 모든 타입 방어구 생성");
        Debug.Log("   I키: 인벤토리 열기/닫기");
    }
    
    void CheckAndSetupLayers()
    {
        // Pickup 레이어 확인
        if (LayerMask.NameToLayer("Pickup") == -1)
        {
            Debug.LogWarning("⚠️ [TestArmorSystem] 'Pickup' 레이어가 없습니다!");
            Debug.Log("📋 해결 방법:");
            Debug.Log("   1. Edit → Project Settings → Tags and Layers");
            Debug.Log("   2. Layers 섹션에서 빈 슬롯에 'Pickup' 입력");
            Debug.Log("   3. 또는 LayerSetup 스크립트를 사용하세요");
        }
        else
        {
            Debug.Log("✅ Pickup 레이어가 설정되어 있습니다.");
        }
        
        // Player 레이어 확인
        if (LayerMask.NameToLayer("Player") == -1)
        {
            Debug.LogWarning("⚠️ [TestArmorSystem] 'Player' 레이어가 없습니다!");
        }
        else
        {
            Debug.Log("✅ Player 레이어가 설정되어 있습니다.");
        }
    }
    
    void Update()
    {
        // 테스트 컨트롤
        if (enableRandomSpawn && Input.GetKeyDown(KeyCode.R))
        {
            SpawnRandomArmor();
        }
        
        if (enableSpecificSpawn && Input.GetKeyDown(KeyCode.T))
        {
            SpawnSpecificArmor();
        }
        
        if (enableAllTypesSpawn && Input.GetKeyDown(KeyCode.Y))
        {
            SpawnAllArmorTypes();
        }
        
        // 디버그 정보 업데이트
        UpdateDebugInfo();
    }
    
    void SpawnRandomArmor()
    {
        if (armorGenerator != null && spawnPoint != null)
        {
            GameObject pickup = armorGenerator.CreateRandomArmorPickup(spawnPoint.position);
            if (pickup != null)
            {
                Debug.Log($"🛡️ 랜덤 방어구 생성: {pickup.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [TestArmorSystem] ArmorGenerator 또는 SpawnPoint가 설정되지 않았습니다!");
        }
    }
    
    void SpawnSpecificArmor()
    {
        if (armorGenerator != null && spawnPoint != null)
        {
            GameObject pickup = armorGenerator.CreateRandomArmorPickup(spawnPoint.position, specificType);
            if (pickup != null)
            {
                Debug.Log($"🛡️ {specificType} 타입 방어구 생성: {pickup.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [TestArmorSystem] ArmorGenerator 또는 SpawnPoint가 설정되지 않았습니다!");
        }
    }
    
    void SpawnAllArmorTypes()
    {
        if (armorGenerator != null && spawnPoint != null)
        {
            ArmorType[] allTypes = { 
                ArmorType.Helmet, ArmorType.Chest, ArmorType.Legs, 
                ArmorType.Boots, ArmorType.Shoulder, ArmorType.Accessory 
            };
            
            Vector3 basePosition = spawnPoint.position;
            
            for (int i = 0; i < allTypes.Length; i++)
            {
                Vector3 spawnPos = basePosition + new Vector3(i * 1.5f, 0, 0);
                GameObject pickup = armorGenerator.CreateRandomArmorPickup(spawnPos, allTypes[i]);
                
                if (pickup != null)
                {
                    Debug.Log($"🛡️ {allTypes[i]} 타입 방어구 생성: {pickup.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [TestArmorSystem] ArmorGenerator 또는 SpawnPoint가 설정되지 않았습니다!");
        }
    }
    
    void UpdateDebugInfo()
    {
        if (inventoryManager != null)
        {
            currentArmorCount = inventoryManager.GetArmorCount();
        }
        
        if (playerInventory != null)
        {
            equippedArmorCount = playerInventory.GetEquippedArmorCount();
        }
    }
    
    // 인스펙터에서 호출할 수 있는 메서드들
    [ContextMenu("랜덤 방어구 생성")]
    public void SpawnRandomArmorFromInspector()
    {
        SpawnRandomArmor();
    }
    
    [ContextMenu("특정 타입 방어구 생성")]
    public void SpawnSpecificArmorFromInspector()
    {
        SpawnSpecificArmor();
    }
    
    [ContextMenu("모든 타입 방어구 생성")]
    public void SpawnAllArmorTypesFromInspector()
    {
        SpawnAllArmorTypes();
    }
    
    [ContextMenu("인벤토리 정보 출력")]
    public void PrintInventoryInfo()
    {
        if (inventoryManager != null)
        {
            Debug.Log($"📦 [인벤토리 정보]");
            Debug.Log($"   총 방어구 개수: {inventoryManager.GetArmorCount()}");
            
            ArmorType[] allTypes = { 
                ArmorType.Helmet, ArmorType.Chest, ArmorType.Legs, 
                ArmorType.Boots, ArmorType.Shoulder, ArmorType.Accessory 
            };
            
            foreach (var type in allTypes)
            {
                int count = inventoryManager.GetArmorCountByType(type);
                Debug.Log($"   {type}: {count}개");
            }
        }
        
        if (playerInventory != null)
        {
            Debug.Log($"🛡️ [장착 정보]");
            Debug.Log($"   장착된 방어구 개수: {playerInventory.GetEquippedArmorCount()}");
            Debug.Log($"   총 방어력: {playerInventory.GetTotalDefense()}");
            Debug.Log($"   체력 보너스: {playerInventory.GetTotalHealthBonus()}");
            Debug.Log($"   이동속도 보너스: {playerInventory.GetTotalSpeedBonus():F2}");
            Debug.Log($"   데미지 감소율: {playerInventory.GetTotalDamageReduction():F2}");
        }
    }
} 