using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 네트워크 방어구 픽업 프리팹용 스크립트
/// GoogleSheets에서 로드된 방어구 데이터를 동적으로 설정합니다.
/// </summary>
public class NetworkArmorPickup : MonoBehaviour, IItemPickup, IArmorPickup
{
    [Header("🔧 네트워크 방어구 픽업 설정")]
    [Tooltip("이 픽업이 생성할 방어구 타입")]
    public ArmorType armorType;
    
    [Tooltip("방어구 등급 (1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Primordial)")]
    [Range(1, 5)]
    public int armorTier = 1;
    
    [Tooltip("런타임에 등급을 랜덤하게 설정할지 여부")]
    public bool useRandomTier = true;
    
    [Tooltip("랜덤 등급 사용시 최소 등급")]
    [Range(1, 5)]
    public int minTier = 1;
    
    [Tooltip("랜덤 등급 사용시 최대 등급")]
    [Range(1, 5)]
    public int maxTier = 5;
    
    [Header("🎨 시각적 효과")]
    [Tooltip("픽업 시 파티클 효과")]
    public GameObject pickupEffect;
    
    [Tooltip("픽업 시 사운드")]
    public AudioClip pickupSound;
    
    [Header("🔧 설정")]
    [Tooltip("픽업 범위")]
    public float pickupRange = 1.5f;
    
    [Tooltip("자동 픽업 여부")]
    public bool autoPickup = false;
    
    [Header("디버그")]
    [Tooltip("디버그 모드 활성화")]
    public bool debugMode = false;
    
    // 동적으로 설정될 방어구 데이터
    private ArmorData armorData;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool isPickedUp = false;
    
    void Start()
    {
        // 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 런타임에 랜덤 등급 설정
        if (useRandomTier)
        {
            armorTier = UnityEngine.Random.Range(minTier, maxTier + 1);
            if (debugMode)
                Debug.Log($"[NetworkArmorPickup] 랜덤 등급 설정: {armorTier} (범위: {minTier}-{maxTier})");
        }
        
        // GoogleSheets에서 로드된 방어구 데이터를 찾아서 설정
        SetupArmorData();
        
        // 픽업 레이어 설정
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (pickupLayer != -1)
        {
            gameObject.layer = pickupLayer;
        }
        else
        {
            gameObject.layer = 0; // Default layer
        }
        
        // 콜라이더가 없다면 추가
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = pickupRange;
        }
    }
    
    /// <summary>
    /// GoogleSheets에서 로드된 방어구 데이터를 찾아서 설정합니다.
    /// </summary>
    void SetupArmorData()
    {
        // GameDataRepository에서 방어구 데이터 가져오기
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null)
        {
            Debug.LogError("[NetworkArmorPickup] GameDataRepository를 찾을 수 없습니다!");
            return;
        }
        
        // 방어구 데이터가 로드될 때까지 대기
        if (!gameDataRepo.IsArmorsLoaded)
        {
            Debug.LogWarning("[NetworkArmorPickup] 방어구 데이터가 아직 로드되지 않았습니다. 이벤트를 구독합니다.");
            gameDataRepo.OnArmorsUpdated += OnArmorsLoaded;
            return;
        }
        
        // 방어구 데이터 찾기
        FindAndSetArmorData();
    }
    
    /// <summary>
    /// 방어구 데이터가 로드되었을 때 호출됩니다.
    /// </summary>
    void OnArmorsLoaded(List<ArmorData> armors)
    {
        if (debugMode)
            Debug.Log($"[NetworkArmorPickup] 방어구 데이터 로드됨: {armors.Count}개");
        
        FindAndSetArmorData();
        
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnArmorsUpdated -= OnArmorsLoaded;
        }
    }
    
    /// <summary>
    /// 방어구 타입과 등급에 맞는 방어구 데이터를 찾아서 설정합니다.
    /// </summary>
    void FindAndSetArmorData()
    {
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null || !gameDataRepo.IsArmorsLoaded)
        {
            Debug.LogError("[NetworkArmorPickup] GameDataRepository가 없거나 방어구 데이터가 로드되지 않았습니다!");
            return;
        }
        
        Debug.Log($"[NetworkArmorPickup] {armorType} 타입, {armorTier} 등급 방어구 찾기 시작");
        
        // 해당 타입의 모든 방어구 찾기
        var armorsOfType = gameDataRepo.GetArmorsByType(armorType);
        
        Debug.Log($"[NetworkArmorPickup] {armorType} 타입 방어구들 (총 {armorsOfType.Count}개):");
        foreach (var armor in armorsOfType)
        {
            Debug.Log($"[NetworkArmorPickup]   - {armor.armorName} (등급: {armor.rarity}, 아이콘: {(armor.icon != null ? "있음" : "없음")})");
        }
        
        if (armorsOfType.Count == 0)
        {
            Debug.LogError($"[NetworkArmorPickup] {armorType} 타입의 방어구가 없습니다!");
            return;
        }
        
        // 등급에 따른 방어구 선택
        ArmorData selectedArmor = null;
        
        // 등급별로 방어구 분류
        var armorsByRarity = new Dictionary<ArmorRarity, List<ArmorData>>();
        foreach (var armor in armorsOfType)
        {
            if (!armorsByRarity.ContainsKey(armor.rarity))
            {
                armorsByRarity[armor.rarity] = new List<ArmorData>();
            }
            armorsByRarity[armor.rarity].Add(armor);
        }
        
        // armorTier에 따른 등급 선택
        ArmorRarity targetRarity = ArmorRarity.Common; // 기본값
        
        switch (armorTier)
        {
            case 1: targetRarity = ArmorRarity.Common; break;
            case 2: targetRarity = ArmorRarity.Rare; break;
            case 3: targetRarity = ArmorRarity.Epic; break;
            case 4: targetRarity = ArmorRarity.Legendary; break;
            case 5: targetRarity = ArmorRarity.Primordial; break;
            default: targetRarity = ArmorRarity.Common; break;
        }
        
        if (debugMode)
            Debug.Log($"[NetworkArmorPickup] 목표 등급: {targetRarity} (Tier: {armorTier})");
        
        // 목표 등급의 방어구가 있으면 선택, 없으면 다른 등급에서 선택
        if (armorsByRarity.ContainsKey(targetRarity) && armorsByRarity[targetRarity].Count > 0)
        {
            // 목표 등급에서 랜덤 선택
            selectedArmor = armorsByRarity[targetRarity][UnityEngine.Random.Range(0, armorsByRarity[targetRarity].Count)];
            if (debugMode)
                Debug.Log($"[NetworkArmorPickup] 목표 등급에서 선택: {selectedArmor.armorName}");
        }
        else
        {
            // 목표 등급이 없으면 가중치 기반으로 선택
            var candidates = new List<ArmorData>();
            foreach (var rarity in armorsByRarity.Keys)
            {
                if (armorsByRarity[rarity].Count > 0)
                {
                    // 해당 등급에서 랜덤 선택
                    var randomArmor = armorsByRarity[rarity][UnityEngine.Random.Range(0, armorsByRarity[rarity].Count)];
                    candidates.Add(randomArmor);
                }
            }
            
            if (candidates.Count > 0)
            {
                // 가중치 기반 최종 선택
                selectedArmor = SelectArmorByWeight(candidates);
                if (debugMode)
                    Debug.Log($"[NetworkArmorPickup] 가중치 기반 선택: {selectedArmor.armorName}");
            }
            else
            {
                // 폴백: 첫 번째 방어구 선택
                selectedArmor = armorsOfType[0];
                if (debugMode)
                    Debug.Log($"[NetworkArmorPickup] 폴백 선택: {selectedArmor.armorName}");
            }
        }
        
        armorData = selectedArmor;
        
        // 방어구 아이콘 설정
        if (armorData != null && spriteRenderer != null)
        {
            if (armorData.icon != null)
                spriteRenderer.sprite = armorData.icon;
            spriteRenderer.color = armorData.GetRarityColor();
        }
        
        Debug.Log($"[NetworkArmorPickup] 최종 선택된 방어구: {armorData.armorName} (등급: {armorData.rarity}, 아이콘: {(armorData.icon != null ? "있음" : "없음")})");
    }
    
    /// <summary>
    /// 가중치 기반으로 방어구를 선택합니다
    /// </summary>
    private ArmorData SelectArmorByWeight(List<ArmorData> armors)
    {
        if (armors.Count == 0) return null;
        if (armors.Count == 1) return armors[0];
        
        // 등급별 가중치 설정
        var weights = new Dictionary<ArmorRarity, float>
        {
            { ArmorRarity.Common, 1.0f },
            { ArmorRarity.Rare, 1.5f },
            { ArmorRarity.Epic, 2.0f },
            { ArmorRarity.Legendary, 3.0f },
            { ArmorRarity.Primordial, 4.0f }
        };
        
        // 각 방어구의 가중치 계산
        var armorWeights = new List<float>();
        foreach (var armor in armors)
        {
            float weight = weights.ContainsKey(armor.rarity) ? weights[armor.rarity] : 1.0f;
            armorWeights.Add(weight);
            
            if (debugMode)
                Debug.Log($"[NetworkArmorPickup] 방어구 '{armor.armorName}' 가중치: {weight}");
        }
        
        // 가중치 기반 랜덤 선택
        float totalWeight = 0f;
        foreach (float weight in armorWeights)
        {
            totalWeight += weight;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        for (int i = 0; i < armors.Count; i++)
        {
            currentWeight += armorWeights[i];
            if (randomValue <= currentWeight)
            {
                return armors[i];
            }
        }
        
        // 폴백
        return armors[armors.Count - 1];
    }
    
    void Update()
    {
        // 자동 픽업이 활성화되어 있다면 플레이어 감지
        if (autoPickup && !isPickedUp)
        {
            CheckForPlayerPickup();
        }
        // E키 픽업이 활성화되어 있다면 E키 입력 감지
        else if (!autoPickup && !isPickedUp)
        {
            CheckForEKeyPickup();
        }
    }
    
    void CheckForEKeyPickup()
    {
        // 플레이어가 범위 안에 있는지 확인
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, pickupRange, LayerMask.GetMask("Player"));
        
        if (playerCollider != null)
        {
            // E키 입력 감지
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnPickup(playerCollider.gameObject);
            }
        }
    }
    
    void CheckForPlayerPickup()
    {
        // 플레이어 감지
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, pickupRange, LayerMask.GetMask("Player"));
        
        if (playerCollider != null)
        {
            // 자동 픽업
            OnPickup(playerCollider.gameObject);
        }
    }
    
    public void OnPickup(GameObject player)
    {
        if (isPickedUp || armorData == null)
        {
            Debug.LogError("[NetworkArmorPickup] armorData가 null입니다! 방어구 데이터 설정을 확인하세요.");
            return;
        }
        
        isPickedUp = true;
        
        if (debugMode)
            Debug.Log($"[NetworkArmorPickup] 방어구 픽업: {armorData.armorName} (등급: {armorData.rarity}, Tier: {armorTier})");
        
        // 플레이어 인벤토리에 방어구 추가
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            // InventoryManager를 통해 방어구 추가
            InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.AddArmor(armorData);
                Debug.Log($"🛡️ 방어구 획득: {armorData.armorName} ({armorData.GetRarityName()})");
            }
            else
            {
                Debug.LogError("❌ [NetworkArmorPickup] InventoryManager를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("❌ [NetworkArmorPickup] PlayerInventory를 찾을 수 없습니다!");
        }
        
        // 시각적 효과
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        // 사운드 재생
        if (pickupSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
        }
        
        // 아이템 제거 (사운드 재생 후)
        Destroy(gameObject, 0.1f);
    }
    
    // 픽업 안내 (플레이어가 범위에 들어왔을 때)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;
        
        if (other.CompareTag("Player"))
        {
            // E키 픽업 안내
            if (armorData != null)
            {
                Debug.Log($"🛡️ {armorData.armorName} 발견! E키를 눌러 픽업하세요.");
            }
        }
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnArmorsUpdated -= OnArmorsLoaded;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 픽업 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
} 