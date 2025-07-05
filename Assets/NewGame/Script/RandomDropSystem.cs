using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameDataRepository를 사용해서 랜덤 아이템을 드롭하는 시스템
/// </summary>
public class RandomDropSystem : MonoBehaviour
{
    [Header("드롭 설정")]
    [SerializeField] private bool enableRandomDrops = true;
    [SerializeField] private float dropChance = 0.3f; // 30% 확률
    [SerializeField] private float dropRadius = 2f;
    
    [Header("드롭 프리팹")]
    [SerializeField] private GameObject weaponDropPrefab;
    [SerializeField] private GameObject armorDropPrefab;
    
    [System.Serializable]
    public struct WeaponDropPrefabEntry
    {
        public WeaponType weaponType;
        public GameObject prefab;
    }
    [Header("드롭 프리팹 (타입별)")]
    [SerializeField] public WeaponDropPrefabEntry[] weaponDropPrefabs;

    [System.Serializable]
    public struct ArmorDropPrefabEntry
    {
        public ArmorType armorType;
        public GameObject prefab;
    }
    [SerializeField] public ArmorDropPrefabEntry[] armorDropPrefabs;
    
    [Header("드롭 가중치")]
    [SerializeField] private float weaponDropWeight = 0.6f; // 60% 무기, 40% 방어구
    
    [Header("레어리티 가중치")]
    [SerializeField] private float commonWeight = 0.5f;
    [SerializeField] private float rareWeight = 0.3f;
    [SerializeField] private float epicWeight = 0.15f;
    [SerializeField] private float legendaryWeight = 0.05f;
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;
    
    private void Start()
    {
        // GameDataRepository 데이터 로드 완료 대기
        if (GameDataRepository.Instance.IsAllDataLoaded)
        {
            OnDataLoaded();
        }
        else
        {
            GameDataRepository.Instance.OnAllDataLoaded += OnDataLoaded;
        }
    }
    
    private void OnDataLoaded()
    {
        if (debugMode)
        {
            Debug.Log("[RandomDropSystem] 데이터 로드 완료, 드롭 시스템 준비됨");
        }
    }
    
    /// <summary>
    /// 지정된 위치에서 랜덤 드롭을 생성합니다
    /// </summary>
    public void CreateRandomDrop(Vector3 position)
    {
        if (!enableRandomDrops || !GameDataRepository.Instance.IsAllDataLoaded)
        {
            return;
        }
        
        // 드롭 확률 체크
        if (Random.Range(0f, 1f) > dropChance)
        {
            return;
        }
        
        // 아이템 타입 결정 (무기 vs 방어구)
        float typeRoll = Random.Range(0f, 1f);
        if (typeRoll < weaponDropWeight)
        {
            CreateWeaponDrop(position);
        }
        else
        {
            CreateArmorDrop(position);
        }
    }
    
    /// <summary>
    /// 랜덤 무기를 드롭합니다
    /// </summary>
    private void CreateWeaponDrop(Vector3 position)
    {
        var repo = GameDataRepository.Instance;
        WeaponData randomWeapon = repo.GetRandomWeapon();
        if (randomWeapon == null) return;

        // 타입별 프리팹 찾기
        GameObject prefab = weaponDropPrefabs != null ? System.Array.Find(weaponDropPrefabs, e => e.weaponType == randomWeapon.weaponType).prefab : null;
        if (prefab == null) prefab = weaponDropPrefab; // fallback

        Vector3 dropPosition = position + Vector3.up * 1.5f;
        GameObject dropObject = Instantiate(prefab, dropPosition, Quaternion.identity);
        WeaponPickup weaponPickup = dropObject.GetComponent<WeaponPickup>();
        if (weaponPickup != null)
        {
            weaponPickup.weaponData = randomWeapon;
            // 아이콘 즉시 적용
            SpriteRenderer sr = dropObject.GetComponent<SpriteRenderer>();
            if (sr != null && randomWeapon.icon != null)
                sr.sprite = randomWeapon.icon;
        }
    }
    
    /// <summary>
    /// 랜덤 방어구를 드롭합니다
    /// </summary>
    private void CreateArmorDrop(Vector3 position)
    {
        var repo = GameDataRepository.Instance;
        ArmorRarity rarity = DetermineRarity();
        ArmorData randomArmor = repo.GetRandomArmorByRarity(rarity);
        if (randomArmor == null) randomArmor = repo.GetRandomArmor();
        if (randomArmor == null) return;

        GameObject prefab = armorDropPrefabs != null ? System.Array.Find(armorDropPrefabs, e => e.armorType == randomArmor.armorType).prefab : null;
        if (prefab == null) prefab = armorDropPrefab; // fallback

        Vector3 dropPosition = position + Vector3.up * 1.5f;
        GameObject dropObject = Instantiate(prefab, dropPosition, Quaternion.identity);
        ArmorPickup armorPickup = dropObject.GetComponent<ArmorPickup>();
        if (armorPickup != null)
        {
            armorPickup.armorData = randomArmor;
            // 아이콘 즉시 적용
            SpriteRenderer sr = dropObject.GetComponent<SpriteRenderer>();
            if (sr != null && randomArmor.icon != null)
                sr.sprite = randomArmor.icon;
        }
    }
    
    /// <summary>
    /// 레어리티를 가중치에 따라 결정합니다
    /// </summary>
    private ArmorRarity DetermineRarity()
    {
        float roll = Random.Range(0f, 1f);
        float cumulativeWeight = 0f;
        
        cumulativeWeight += commonWeight;
        if (roll <= cumulativeWeight) return ArmorRarity.Common;
        
        cumulativeWeight += rareWeight;
        if (roll <= cumulativeWeight) return ArmorRarity.Rare;
        
        cumulativeWeight += epicWeight;
        if (roll <= cumulativeWeight) return ArmorRarity.Epic;
        
        cumulativeWeight += legendaryWeight;
        if (roll <= cumulativeWeight) return ArmorRarity.Legendary;
        
        // 기본값 (가중치 합이 1이 아닌 경우를 대비)
        return ArmorRarity.Common;
    }
    
    /// <summary>
    /// 특정 타입의 무기를 드롭합니다
    /// </summary>
    public void CreateWeaponDropByType(Vector3 position, WeaponType weaponType)
    {
        if (!GameDataRepository.Instance.IsAllDataLoaded) return;
        
        var repo = GameDataRepository.Instance;
        WeaponData weapon = repo.GetRandomWeaponByType(weaponType);
        
        if (weapon == null)
        {
            Debug.LogWarning($"[RandomDropSystem] {weaponType} 타입의 무기가 없습니다.");
            return;
        }
        
        // 랜덤 위치 계산
        Vector3 dropPosition = position + Random.insideUnitSphere * dropRadius;
        dropPosition.z = 0;
        
        // 무기 드롭 생성
        if (weaponDropPrefab != null)
        {
            GameObject dropObject = Instantiate(weaponDropPrefab, dropPosition, Quaternion.identity);
            WeaponPickup weaponPickup = dropObject.GetComponent<WeaponPickup>();
            
            if (weaponPickup != null)
            {
                weaponPickup.weaponData = weapon;
                
                if (debugMode)
                {
                    Debug.Log($"[RandomDropSystem] {weaponType} 타입 무기 드롭: {weapon.weaponName}");
                }
            }
        }
    }
    
    /// <summary>
    /// 특정 타입의 방어구를 드롭합니다
    /// </summary>
    public void CreateArmorDropByType(Vector3 position, ArmorType armorType)
    {
        if (!GameDataRepository.Instance.IsAllDataLoaded) return;
        
        var repo = GameDataRepository.Instance;
        ArmorData armor = repo.GetRandomArmorByType(armorType);
        
        if (armor == null)
        {
            Debug.LogWarning($"[RandomDropSystem] {armorType} 타입의 방어구가 없습니다.");
            return;
        }
        
        // 랜덤 위치 계산
        Vector3 dropPosition = position + Random.insideUnitSphere * dropRadius;
        dropPosition.z = 0;
        
        // 방어구 드롭 생성
        if (armorDropPrefab != null)
        {
            GameObject dropObject = Instantiate(armorDropPrefab, dropPosition, Quaternion.identity);
            ArmorPickup armorPickup = dropObject.GetComponent<ArmorPickup>();
            
            if (armorPickup != null)
            {
                armorPickup.armorData = armor;
                
                if (debugMode)
                {
                    Debug.Log($"[RandomDropSystem] {armorType} 타입 방어구 드롭: {armor.armorName} ({armor.rarity})");
                }
            }
        }
    }
    
    /// <summary>
    /// 보스 드롭 (고급 아이템 확률 증가)
    /// </summary>
    public void CreateBossDrop(Vector3 position)
    {
        if (!GameDataRepository.Instance.IsAllDataLoaded) return;
        
        // 보스 드롭은 여러 개 아이템을 드롭
        int dropCount = Random.Range(2, 5);
        
        for (int i = 0; i < dropCount; i++)
        {
            // 보스 드롭은 레어 이상 확률 증가
            float rarityRoll = Random.Range(0f, 1f);
            if (rarityRoll < 0.7f) // 70% 확률로 레어 이상
            {
                // 레어 이상 방어구 드롭
                CreateRareArmorDrop(position);
            }
            else
            {
                // 일반 랜덤 드롭
                CreateRandomDrop(position);
            }
        }
    }
    
    /// <summary>
    /// 레어 이상 방어구를 드롭합니다
    /// </summary>
    private void CreateRareArmorDrop(Vector3 position)
    {
        var repo = GameDataRepository.Instance;
        
        // 레어, 에픽, 레전더리 중에서 선택
        float roll = Random.Range(0f, 1f);
        ArmorRarity rarity;
        
        if (roll < 0.6f) rarity = ArmorRarity.Rare;
        else if (roll < 0.9f) rarity = ArmorRarity.Epic;
        else rarity = ArmorRarity.Legendary;
        
        ArmorData armor = repo.GetRandomArmorByRarity(rarity);
        
        if (armor == null)
        {
            // 해당 레어리티에 없으면 전체에서 랜덤
            armor = repo.GetRandomArmor();
        }
        
        if (armor == null) return;
        
        // 랜덤 위치 계산
        Vector3 dropPosition = position + Random.insideUnitSphere * dropRadius;
        dropPosition.z = 0;
        
        // 방어구 드롭 생성
        if (armorDropPrefab != null)
        {
            GameObject dropObject = Instantiate(armorDropPrefab, dropPosition, Quaternion.identity);
            ArmorPickup armorPickup = dropObject.GetComponent<ArmorPickup>();
            
            if (armorPickup != null)
            {
                armorPickup.armorData = armor;
                
                if (debugMode)
                {
                    Debug.Log($"[RandomDropSystem] 보스 드롭 - {armor.rarity} 방어구: {armor.armorName}");
                }
            }
        }
    }
    
    #region 디버그 메서드
    
    [ContextMenu("테스트 드롭 생성")]
    public void TestDrop()
    {
        CreateRandomDrop(transform.position);
    }
    
    [ContextMenu("테스트 보스 드롭")]
    public void TestBossDrop()
    {
        CreateBossDrop(transform.position);
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameDataRepository.Instance != null)
        {
            GameDataRepository.Instance.OnAllDataLoaded -= OnDataLoaded;
        }
    }

    void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.F10))
        {
            CreateWeaponDrop(player.transform.position);
            Debug.Log("[RandomDropSystem] F10: 무기 드롭 생성");
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            CreateArmorDrop(player.transform.position);
            Debug.Log("[RandomDropSystem] F11: 방어구 드롭 생성");
        }
    }
}

/// <summary>
/// 방어구 픽업 인터페이스 (WeaponPickup과 유사)
/// </summary>
public interface IArmorPickup
{
    void OnPickup(GameObject player);
} 