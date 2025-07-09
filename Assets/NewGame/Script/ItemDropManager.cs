using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;
    public float dropChance = 0.1f; // 0.0 ~ 1.0
    public int minQuantity = 1;
    public int maxQuantity = 1;
    public bool isRare = false; // 레어 아이템 여부
}

[System.Serializable]
public class EnemyDropTable
{
    public string enemyType = "Basic Enemy";
    public DropItem[] commonItems; // 일반 아이템들
    public DropItem[] rareItems; // 레어 아이템들
    public float rareItemChance = 0.05f; // 레어 아이템 드랍 확률
    public int maxItemsPerDrop = 3; // 한 번에 드랍할 수 있는 최대 아이템 수
}

public class ItemDropManager : MonoBehaviour
{
    [Header("드랍 테이블 설정")]
    public EnemyDropTable[] dropTables;
    
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
    
    /// <summary>
    /// 몬스터가 죽었을 때 아이템을 드랍합니다.
    /// </summary>
    public void DropItemsFromEnemy(string enemyType, Vector3 position)
    {
        if (debugMode)
            Debug.Log($"[ItemDropManager] {enemyType}에서 아이템 드랍 시도: {position}");
        
        // 해당 몬스터 타입의 드랍 테이블 찾기
        EnemyDropTable dropTable = FindDropTable(enemyType);
        if (dropTable == null)
        {
            if (debugMode)
                Debug.LogWarning($"[ItemDropManager] {enemyType}의 드랍 테이블을 찾을 수 없습니다.");
            return;
        }
        
        List<GameObject> itemsToDrop = new List<GameObject>();
        
        // 레어 아이템 드랍 시도
        if (Random.value <= dropTable.rareItemChance && dropTable.rareItems.Length > 0)
        {
            DropItem rareItem = GetRandomDropItem(dropTable.rareItems);
            if (rareItem != null)
            {
                int quantity = Random.Range(rareItem.minQuantity, rareItem.maxQuantity + 1);
                for (int i = 0; i < quantity; i++)
                {
                    itemsToDrop.Add(rareItem.itemPrefab);
                }
                
                if (debugMode)
                    Debug.Log($"[ItemDropManager] 레어 아이템 드랍: {rareItem.itemPrefab.name} x{quantity}");
            }
        }
        
        // 일반 아이템 드랍 시도
        foreach (DropItem item in dropTable.commonItems)
        {
            if (Random.value <= item.dropChance)
            {
                int quantity = Random.Range(item.minQuantity, item.maxQuantity + 1);
                for (int i = 0; i < quantity; i++)
                {
                    itemsToDrop.Add(item.itemPrefab);
                }
                
                if (debugMode)
                    Debug.Log($"[ItemDropManager] 일반 아이템 드랍: {item.itemPrefab.name} x{quantity}");
            }
        }
        
        // 최대 드랍 개수 제한
        if (itemsToDrop.Count > dropTable.maxItemsPerDrop)
        {
            itemsToDrop.RemoveRange(dropTable.maxItemsPerDrop, itemsToDrop.Count - dropTable.maxItemsPerDrop);
        }
        
        // 아이템들을 실제로 생성
        foreach (GameObject itemPrefab in itemsToDrop)
        {
            DropItemAtPosition(itemPrefab, position);
        }
        
        if (debugMode)
            Debug.Log($"[ItemDropManager] 총 {itemsToDrop.Count}개의 아이템을 드랍했습니다.");
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
        
        // 네트워크 픽업 시스템 사용 시 추가 설정
        if (useNetworkPickup)
        {
            SetupNetworkPickup(droppedItem);
        }
        
        if (debugMode)
            Debug.Log($"[ItemDropManager] 아이템 드랍: {itemPrefab.name} at {dropPosition}");
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
    /// 드랍 테이블에서 해당 몬스터 타입을 찾습니다.
    /// </summary>
    private EnemyDropTable FindDropTable(string enemyType)
    {
        foreach (EnemyDropTable table in dropTables)
        {
            if (table.enemyType == enemyType)
                return table;
        }
        return null;
    }
    
    /// <summary>
    /// 드랍 아이템 배열에서 랜덤하게 하나를 선택합니다.
    /// </summary>
    private DropItem GetRandomDropItem(DropItem[] items)
    {
        if (items.Length == 0) return null;
        return items[Random.Range(0, items.Length)];
    }
    
    /// <summary>
    /// 기본 드랍 테이블을 생성합니다 (에디터에서 사용).
    /// </summary>
    [ContextMenu("기본 드랍 테이블 생성")]
    public void CreateDefaultDropTable()
    {
        if (dropTables == null || dropTables.Length == 0)
        {
            dropTables = new EnemyDropTable[2];
            
            // 일반 몬스터 드랍 테이블
            dropTables[0] = new EnemyDropTable();
            dropTables[0].enemyType = "Basic Enemy";
            dropTables[0].commonItems = new DropItem[0];
            dropTables[0].rareItems = new DropItem[0];
            dropTables[0].rareItemChance = 0.05f;
            dropTables[0].maxItemsPerDrop = 3;
            
            // 보스 몬스터 드랍 테이블
            dropTables[1] = new EnemyDropTable();
            dropTables[1].enemyType = "보스 몬스터";
            dropTables[1].commonItems = new DropItem[0];
            dropTables[1].rareItems = new DropItem[0];
            dropTables[1].rareItemChance = 0.3f; // 보스는 레어 아이템 확률이 높음
            dropTables[1].maxItemsPerDrop = 5; // 보스는 더 많은 아이템 드랍
            
            Debug.Log("[ItemDropManager] 기본 드랍 테이블이 생성되었습니다.");
        }
    }
    
    /// <summary>
    /// 보스 전용 드랍 테이블을 생성합니다.
    /// </summary>
    [ContextMenu("보스 드랍 테이블 생성")]
    public void CreateBossDropTable()
    {
        // 기존 테이블에 보스 테이블 추가
        System.Array.Resize(ref dropTables, dropTables.Length + 1);
        
        int newIndex = dropTables.Length - 1;
        dropTables[newIndex] = new EnemyDropTable();
        dropTables[newIndex].enemyType = "보스 몬스터";
        dropTables[newIndex].commonItems = new DropItem[0];
        dropTables[newIndex].rareItems = new DropItem[0];
        dropTables[newIndex].rareItemChance = 0.3f;
        dropTables[newIndex].maxItemsPerDrop = 5;
        
        Debug.Log("[ItemDropManager] 보스 드랍 테이블이 생성되었습니다.");
    }
    
    /// <summary>
    /// 예시 드랍 테이블을 생성합니다 (테스트용).
    /// </summary>
    [ContextMenu("예시 드랍 테이블 생성")]
    public void CreateExampleDropTable()
    {
        if (dropTables == null || dropTables.Length == 0)
        {
            CreateDefaultDropTable();
        }
        
        // 일반 몬스터 테이블에 예시 아이템 추가
        if (dropTables.Length > 0)
        {
            // 일반 아이템 예시 (무기)
            dropTables[0].commonItems = new DropItem[3];
            
            // HG_1 (권총)
            dropTables[0].commonItems[0] = new DropItem();
            dropTables[0].commonItems[0].dropChance = 0.3f;
            dropTables[0].commonItems[0].minQuantity = 1;
            dropTables[0].commonItems[0].maxQuantity = 1;
            
            // AR_1 (돌격소총)
            dropTables[0].commonItems[1] = new DropItem();
            dropTables[0].commonItems[1].dropChance = 0.2f;
            dropTables[0].commonItems[1].minQuantity = 1;
            dropTables[0].commonItems[1].maxQuantity = 1;
            
            // SMG_1 (기관단총)
            dropTables[0].commonItems[2] = new DropItem();
            dropTables[0].commonItems[2].dropChance = 0.15f;
            dropTables[0].commonItems[2].minQuantity = 1;
            dropTables[0].commonItems[2].maxQuantity = 1;
            
            // 레어 아이템 예시 (스나이퍼)
            dropTables[0].rareItems = new DropItem[1];
            dropTables[0].rareItems[0] = new DropItem();
            dropTables[0].rareItems[0].dropChance = 0.1f;
            dropTables[0].rareItems[0].minQuantity = 1;
            dropTables[0].rareItems[0].maxQuantity = 1;
        }
        
        // 보스 테이블에 예시 아이템 추가
        if (dropTables.Length > 1)
        {
            // 보스 일반 아이템 (고급 무기)
            dropTables[1].commonItems = new DropItem[2];
            
            // MG_1 (기관총)
            dropTables[1].commonItems[0] = new DropItem();
            dropTables[1].commonItems[0].dropChance = 0.4f;
            dropTables[1].commonItems[0].minQuantity = 1;
            dropTables[1].commonItems[0].maxQuantity = 1;
            
            // SR_1 (스나이퍼)
            dropTables[1].commonItems[1] = new DropItem();
            dropTables[1].commonItems[1].dropChance = 0.3f;
            dropTables[1].commonItems[1].minQuantity = 1;
            dropTables[1].commonItems[1].maxQuantity = 1;
            
            // 보스 레어 아이템 (최고급 무기)
            dropTables[1].rareItems = new DropItem[1];
            dropTables[1].rareItems[0] = new DropItem();
            dropTables[1].rareItems[0].dropChance = 0.2f;
            dropTables[1].rareItems[0].minQuantity = 1;
            dropTables[1].rareItems[0].maxQuantity = 1;
        }
        
        Debug.Log("[ItemDropManager] 예시 드랍 테이블이 생성되었습니다. 프리팹을 직접 할당해주세요!");
        Debug.Log("[ItemDropManager] 일반 몬스터: HG_1, AR_1, SMG_1 (레어: SR_1)");
        Debug.Log("[ItemDropManager] 보스 몬스터: MG_1, SR_1 (레어: 최고급 무기)");
    }
} 