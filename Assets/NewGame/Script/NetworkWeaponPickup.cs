using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 네트워크 무기 픽업 프리팹용 스크립트
/// GoogleSheets에서 로드된 무기 데이터를 동적으로 설정합니다.
/// </summary>
public class NetworkWeaponPickup : MonoBehaviour, IItemPickup
{
    [Header("🔧 네트워크 무기 픽업 설정")]
    [Tooltip("이 픽업이 생성할 무기 타입")]
    public WeaponType weaponType;
    
    [Tooltip("무기 등급 (1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Primordial)")]
    [Range(1, 5)]
    public int weaponTier = 1;
    
    [Tooltip("런타임에 등급을 랜덤하게 설정할지 여부")]
    public bool useRandomTier = true;
    
    [Tooltip("랜덤 등급 사용시 최소 등급")]
    [Range(1, 5)]
    public int minTier = 1;
    
    [Tooltip("랜덤 등급 사용시 최대 등급")]
    [Range(1, 5)]
    public int maxTier = 5;
    
    [Header("디버그")]
    [Tooltip("디버그 모드 활성화")]
    public bool debugMode = false;
    
    // 동적으로 설정될 무기 데이터
    private WeaponData weaponData;
    
    void Start()
    {
        // 런타임에 랜덤 등급 설정
        if (useRandomTier)
        {
            weaponTier = UnityEngine.Random.Range(minTier, maxTier + 1);
            // if (debugMode)
            //     Debug.Log($"[NetworkWeaponPickup] 랜덤 등급 설정: {weaponTier} (범위: {minTier}-{maxTier})");
        }
        
        // 🆕 Rigidbody2D 설정 (아이템이 바닥에 떨어지도록)
        SetupRigidbody();
        
        // GoogleSheets에서 로드된 무기 데이터를 찾아서 설정
        SetupWeaponData();
    }
    
    /// <summary>
    /// Rigidbody2D를 설정하여 아이템이 바닥에 떨어지도록 합니다.
    /// </summary>
    void SetupRigidbody()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 물리 설정
        rb.gravityScale = 1f; // 중력 적용
        rb.linearDamping = 0.5f; // 공기 저항 (떨어질 때 속도 제한)
        rb.angularDamping = 0.5f; // 회전 저항
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 방지
        
        // 🆕 바닥에 착 붙도록 설정
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // 초기 속도 설정 (약간의 랜덤성 추가)
        float randomX = UnityEngine.Random.Range(-2f, 2f);
        float randomY = UnityEngine.Random.Range(1f, 3f);
        rb.linearVelocity = new Vector2(randomX, randomY);
        
        // 🆕 콜라이더 설정 (플레이어와 충돌하지 않도록)
        SetupCollider();
    }
    
    /// <summary>
    /// 콜라이더를 설정하여 플레이어와 충돌하지 않도록 합니다.
    /// </summary>
    void SetupCollider()
    {
        // 기존 콜라이더 제거
        Collider2D[] existingColliders = GetComponents<Collider2D>();
        foreach (var collider in existingColliders)
        {
            if (collider.isTrigger == false) // 트리거가 아닌 물리 콜라이더만 제거
            {
                DestroyImmediate(collider);
            }
        }
        
        // 바닥과만 충돌하는 콜라이더 추가
        BoxCollider2D groundCollider = gameObject.AddComponent<BoxCollider2D>();
        groundCollider.size = new Vector2(0.8f, 0.8f); // 아이템 크기에 맞게 조정
        groundCollider.isTrigger = false; // 물리 충돌 활성화
        
        // 🆕 픽업용 트리거 콜라이더 추가 (플레이어와 상호작용용)
        CircleCollider2D pickupCollider = gameObject.AddComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true; // 트리거로 설정
        pickupCollider.radius = 1.5f; // 픽업 범위
        
        // 🆕 플레이어와의 충돌을 무시하도록 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("PickupLayer");
        
        // 🆕 플레이어 레이어와의 충돌 무시 (더 확실한 방법)
        int pickupLayer = LayerMask.NameToLayer("PickupLayer");
        int playerLayer = LayerMask.NameToLayer("Player");
        
        if (pickupLayer != -1 && playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(pickupLayer, playerLayer, true);
            Debug.Log($"[NetworkWeaponPickup] 레이어 충돌 무시 설정: PickupLayer({pickupLayer}) ↔ Player({playerLayer})");
        }
        else
        {
            Debug.LogWarning("[NetworkWeaponPickup] 레이어를 찾을 수 없습니다! PickupLayer 또는 Player 레이어가 없습니다.");
        }
        
        // 🆕 추가 보안: 플레이어 태그를 가진 오브젝트와의 충돌 무시
        StartCoroutine(IgnorePlayerCollisions());
    }
    
    /// <summary>
    /// 플레이어와의 충돌을 무시하는 코루틴
    /// </summary>
    System.Collections.IEnumerator IgnorePlayerCollisions()
    {
        yield return new WaitForSeconds(0.1f); // 약간의 지연
        
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D[] itemColliders = GetComponents<Collider2D>();
            
            if (playerCollider != null)
            {
                foreach (var itemCollider in itemColliders)
                {
                    if (!itemCollider.isTrigger) // 물리 콜라이더만
                    {
                        Physics2D.IgnoreCollision(itemCollider, playerCollider, true);
                        Debug.Log($"[NetworkWeaponPickup] 플레이어와의 충돌 무시: {itemCollider.name}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 🆕 플레이어와 충돌 시 위치 조정 (최후의 수단)
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어와 충돌했는지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[NetworkWeaponPickup] 플레이어와 충돌 감지! 위치 조정 중...");
            
            // 플레이어의 위치에서 약간 떨어진 위치로 이동
            Vector2 playerPos = collision.transform.position;
            Vector2 direction = (Vector2)transform.position - playerPos;
            
            if (direction.magnitude < 2f) // 너무 가까우면
            {
                // 플레이어에서 2f 거리만큼 떨어진 위치로 이동
                Vector2 newPos = playerPos + direction.normalized * 2f;
                transform.position = newPos;
                
                Debug.Log($"[NetworkWeaponPickup] 위치 조정 완료: {transform.position}");
            }
        }
    }
    
    /// <summary>
    /// GoogleSheets에서 로드된 무기 데이터를 찾아서 설정합니다.
    /// </summary>
    void SetupWeaponData()
    {
        // GameDataRepository에서 무기 데이터 가져오기
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null)
        {
            Debug.LogError("[NetworkWeaponPickup] GameDataRepository를 찾을 수 없습니다!");
            return;
        }
        
        // 무기 데이터가 로드될 때까지 대기
        if (!gameDataRepo.IsWeaponsLoaded)
        {
            Debug.LogWarning("[NetworkWeaponPickup] 무기 데이터가 아직 로드되지 않았습니다. 이벤트를 구독합니다.");
            gameDataRepo.OnWeaponsUpdated += OnWeaponsLoaded;
            return;
        }
        
        // 무기 데이터 찾기
        FindAndSetWeaponData();
    }
    
    /// <summary>
    /// 무기 데이터가 로드되었을 때 호출됩니다.
    /// </summary>
    void OnWeaponsLoaded(List<WeaponData> weapons)
    {
        // if (debugMode)
        //     Debug.Log($"[NetworkWeaponPickup] 무기 데이터 로드됨: {weapons.Count}개");
        
        FindAndSetWeaponData();
        
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnWeaponsUpdated -= OnWeaponsLoaded;
        }
    }
    
    /// <summary>
    /// 무기 타입과 등급에 맞는 무기 데이터를 찾아서 설정합니다.
    /// </summary>
    void FindAndSetWeaponData()
    {
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo == null || !gameDataRepo.IsWeaponsLoaded)
        {
            Debug.LogError("[NetworkWeaponPickup] GameDataRepository가 없거나 무기 데이터가 로드되지 않았습니다!");
            return;
        }
        
        // if (debugMode)
        //     Debug.Log($"[NetworkWeaponPickup] {weaponType} 타입, {weaponTier} 등급 무기 찾기 시작");
        
        // 해당 타입의 모든 무기 찾기
        var weaponsOfType = gameDataRepo.GetWeaponsByType(weaponType);
        
        if (debugMode)
        {
            // Debug.Log($"[NetworkWeaponPickup] {weaponType} 타입 무기들:");
            // foreach (var weapon in weaponsOfType)
            // {
            //     Debug.Log($"[NetworkWeaponPickup]   - {weapon.weaponName} (등급: {weapon.rarity})");
            // }
        }
        
        if (weaponsOfType.Count == 0)
        {
            Debug.LogError($"[NetworkWeaponPickup] {weaponType} 타입의 무기가 없습니다!");
            return;
        }
        
        // 등급에 따른 무기 선택
        WeaponData selectedWeapon = null;
        
        // 등급별로 무기 분류
        var weaponsByRarity = new Dictionary<WeaponRarity, List<WeaponData>>();
        foreach (var weapon in weaponsOfType)
        {
            if (!weaponsByRarity.ContainsKey(weapon.rarity))
            {
                weaponsByRarity[weapon.rarity] = new List<WeaponData>();
            }
            weaponsByRarity[weapon.rarity].Add(weapon);
        }
        
        // 등급별 가중치 설정 (1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Primordial)
        var rarityWeights = new Dictionary<WeaponRarity, float>
        {
            { WeaponRarity.Common, 1.0f },
            { WeaponRarity.Rare, 1.5f },
            { WeaponRarity.Epic, 2.0f },
            { WeaponRarity.Legendary, 3.0f },
            { WeaponRarity.Primordial, 4.0f }
        };
        
        // weaponTier에 따른 등급 선택
        WeaponRarity targetRarity = WeaponRarity.Common; // 기본값
        
        switch (weaponTier)
        {
            case 1: targetRarity = WeaponRarity.Common; break;
            case 2: targetRarity = WeaponRarity.Rare; break;
            case 3: targetRarity = WeaponRarity.Epic; break;
            case 4: targetRarity = WeaponRarity.Legendary; break;
            case 5: targetRarity = WeaponRarity.Primordial; break;
            default: targetRarity = WeaponRarity.Common; break;
        }
        
        // if (debugMode)
        //     Debug.Log($"[NetworkWeaponPickup] 목표 등급: {targetRarity} (Tier: {weaponTier})");
        
        // 목표 등급의 무기가 있으면 선택, 없으면 다른 등급에서 선택
        if (weaponsByRarity.ContainsKey(targetRarity) && weaponsByRarity[targetRarity].Count > 0)
        {
            // 목표 등급에서 랜덤 선택
            selectedWeapon = weaponsByRarity[targetRarity][UnityEngine.Random.Range(0, weaponsByRarity[targetRarity].Count)];
            // if (debugMode)
            //     Debug.Log($"[NetworkWeaponPickup] 목표 등급에서 선택: {selectedWeapon.weaponName}");
        }
        else
        {
            // 목표 등급이 없으면 가중치 기반으로 선택
            var candidates = new List<WeaponData>();
            foreach (var rarity in weaponsByRarity.Keys)
            {
                if (weaponsByRarity[rarity].Count > 0)
                {
                    // 해당 등급에서 랜덤 선택
                    var randomWeapon = weaponsByRarity[rarity][UnityEngine.Random.Range(0, weaponsByRarity[rarity].Count)];
                    candidates.Add(randomWeapon);
                }
            }
            
            if (candidates.Count > 0)
            {
                // 가중치 기반 최종 선택
                selectedWeapon = SelectWeaponByWeight(candidates);
                // if (debugMode)
                //     Debug.Log($"[NetworkWeaponPickup] 가중치 기반 선택: {selectedWeapon.weaponName}");
            }
            else
            {
                // 폴백: 첫 번째 무기 선택
                selectedWeapon = weaponsOfType[0];
                // if (debugMode)
                //     Debug.Log($"[NetworkWeaponPickup] 폴백 선택: {selectedWeapon.weaponName}");
            }
        }
        
        weaponData = selectedWeapon;
        
        // 🎨 무기 아이콘 및 색상 설정
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (weaponData != null && spriteRenderer != null)
        {
            if (weaponData.icon != null)
                spriteRenderer.sprite = weaponData.icon;
            spriteRenderer.color = weaponData.GetRarityColor();
            
            // 무기 시각적 설정 완료
        }
        
        // 최종 선택된 무기 설정 완료
    }
    
    /// <summary>
    /// 가중치 기반으로 무기를 선택합니다
    /// </summary>
    private WeaponData SelectWeaponByWeight(List<WeaponData> weapons)
    {
        if (weapons.Count == 0) return null;
        if (weapons.Count == 1) return weapons[0];
        
        // 등급별 가중치 설정
        var weights = new Dictionary<WeaponRarity, float>
        {
            { WeaponRarity.Common, 1.0f },
            { WeaponRarity.Rare, 1.5f },
            { WeaponRarity.Epic, 2.0f },
            { WeaponRarity.Legendary, 3.0f },
            { WeaponRarity.Primordial, 4.0f }
        };
        
        // 각 무기의 가중치 계산
        var weaponWeights = new List<float>();
        foreach (var weapon in weapons)
        {
            float weight = weights.ContainsKey(weapon.rarity) ? weights[weapon.rarity] : 1.0f;
            weaponWeights.Add(weight);
            
            // if (debugMode)
            //     Debug.Log($"[NetworkWeaponPickup] 무기 '{weapon.weaponName}' 가중치: {weight}");
        }
        
        // 가중치 기반 랜덤 선택
        float totalWeight = 0f;
        foreach (float weight in weaponWeights)
        {
            totalWeight += weight;
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        for (int i = 0; i < weapons.Count; i++)
        {
            currentWeight += weaponWeights[i];
            if (randomValue <= currentWeight)
            {
                return weapons[i];
            }
        }
        
        // 폴백
        return weapons[weapons.Count - 1];
    }
    
    public void OnPickup(GameObject player)
    {
        if (weaponData == null)
        {
            Debug.LogError("[NetworkWeaponPickup] weaponData가 null입니다! 무기 데이터 설정을 확인하세요.");
            return;
        }
        
        // if (debugMode)
        //     Debug.Log($"[NetworkWeaponPickup] 무기 픽업: {weaponData.weaponName} (등급: {weaponData.rarity}, Tier: {weaponTier})");
        
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            // 🆕 무기 장착 전 물리 컴포넌트들 제거
            RemovePhysicsComponents();
            
            inventory.AddWeapon(weaponData);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("[NetworkWeaponPickup] PlayerInventory 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 무기 장착 시 물리 컴포넌트들을 제거합니다.
    /// </summary>
    private void RemovePhysicsComponents()
    {
        // Rigidbody2D 제거
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            DestroyImmediate(rb);
        }
        
        // 물리 콜라이더들 제거 (트리거는 유지)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            if (!collider.isTrigger) // 물리 콜라이더만 제거
            {
                DestroyImmediate(collider);
            }
        }
        
        // 레이어를 Default로 변경
        gameObject.layer = 0; // Default layer
        
        if (debugMode)
            Debug.Log("[NetworkWeaponPickup] 물리 컴포넌트들이 제거되었습니다.");
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        var gameDataRepo = GameDataRepository.Instance;
        if (gameDataRepo != null)
        {
            gameDataRepo.OnWeaponsUpdated -= OnWeaponsLoaded;
        }
    }
}