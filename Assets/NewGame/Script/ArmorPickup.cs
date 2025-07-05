using UnityEngine;

public class ArmorPickup : MonoBehaviour, IItemPickup, IArmorPickup
{
    [Header("🛡️ 방어구 픽업 설정")]
    [Tooltip("픽업할 방어구 데이터")]
    public ArmorData armorData;
    
    [Header("🎨 시각적 효과")]
    [Tooltip("픽업 시 파티클 효과")]
    public GameObject pickupEffect;
    
    [Tooltip("픽업 시 사운드")]
    public AudioClip pickupSound;
    
    [Header("🔧 설정")]
    [Tooltip("픽업 범위")]
    public float pickupRange = 1.5f;
    
    [Tooltip("자동 픽업 여부")]
    public bool autoPickup = false; // E키 픽업으로 변경
    
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool isPickedUp = false;
    
    void Start()
    {
        // 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 방어구 아이콘 설정
        if (armorData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = armorData.icon;
            spriteRenderer.color = armorData.GetRarityColor();
        }
        
        // 픽업 레이어 설정 (안전하게 처리)
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (pickupLayer != -1)
        {
            gameObject.layer = pickupLayer;
        }
        else
        {
            // Pickup 레이어가 없으면 기본 레이어 사용
            gameObject.layer = 0; // Default layer
            Debug.LogWarning("⚠️ [ArmorPickup] 'Pickup' 레이어가 없습니다. Default 레이어를 사용합니다.");
        }
        
        // 콜라이더가 없다면 추가
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = pickupRange;
        }
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
    
    // IItemPickup 인터페이스 구현
    public void OnPickup(GameObject player)
    {
        if (isPickedUp || armorData == null) 
        {
            Debug.LogWarning($"⚠️ [ArmorPickup] 픽업 실패 - isPickedUp: {isPickedUp}, armorData: {(armorData == null ? "null" : "있음")}");
            return;
        }
        
        isPickedUp = true;
        Debug.Log($"🛡️ [ArmorPickup] 방어구 픽업 시작: {armorData.armorName}");
        
        // 플레이어 인벤토리에 방어구 추가
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            Debug.Log($"✅ [ArmorPickup] PlayerInventory 찾음");
            
            // InventoryManager를 통해 방어구 추가
            InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
            if (inventoryManager != null)
            {
                Debug.Log($"✅ [ArmorPickup] InventoryManager 찾음, 방어구 추가 시도...");
                inventoryManager.AddArmor(armorData);
                Debug.Log($"🛡️ 방어구 획득: {armorData.armorName} ({armorData.GetRarityName()})");
            }
            else
            {
                Debug.LogError("❌ [ArmorPickup] InventoryManager를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("❌ [ArmorPickup] PlayerInventory를 찾을 수 없습니다!");
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
            Debug.Log($"🛡️ {armorData.armorName} 발견! E키를 눌러 픽업하세요.");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 픽업 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
} 