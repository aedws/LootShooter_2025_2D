using UnityEngine;
using TMPro;

/// <summary>
/// 필드에 떨어져 있는 칩셋 픽업 오브젝트
/// 플레이어가 접근하면 인벤토리에 추가됨
/// </summary>
public class ChipsetPickup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro chipsetNameText;
    [SerializeField] private TextMeshPro chipsetRarityText;
    [SerializeField] private SpriteRenderer chipsetIcon;
    [SerializeField] private GameObject pickupEffect;
    
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    // 칩셋 데이터
    private WeaponChipsetData weaponChipset;
    private ArmorChipsetData armorChipset;
    private PlayerChipsetData playerChipset;
    
    // 애니메이션 관련
    private Vector3 startPosition;
    private float bobTime;
    
    // 이벤트
    public System.Action<object> OnChipsetPickedUp;
    
    private void Start()
    {
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // 랜덤 시작 시간
        
        // 칩셋 이름 텍스트 설정
        if (chipsetNameText != null)
        {
            chipsetNameText.text = GetChipsetName();
        }
        
        // 칩셋 희귀도 텍스트 설정
        if (chipsetRarityText != null)
        {
            chipsetRarityText.text = GetChipsetRarityName();
            chipsetRarityText.color = GetChipsetRarityColor();
        }
        
        // 칩셋 아이콘 색상 설정
        if (chipsetIcon != null)
        {
            chipsetIcon.color = GetChipsetRarityColor();
        }
    }
    
    private void Update()
    {
        // 회전 애니메이션
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // 위아래 움직임 애니메이션
        bobTime += bobSpeed * Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(bobTime) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // 플레이어 접근 감지
        CheckPlayerProximity();
    }
    
    /// <summary>
    /// 칩셋 초기화
    /// </summary>
    public void Initialize(object chipset)
    {
        if (chipset is WeaponChipsetData weaponChipsetData)
        {
            weaponChipset = weaponChipsetData;
            armorChipset = null;
            playerChipset = null;
        }
        else if (chipset is ArmorChipsetData armorChipsetData)
        {
            armorChipset = armorChipsetData;
            weaponChipset = null;
            playerChipset = null;
        }
        else if (chipset is PlayerChipsetData playerChipsetData)
        {
            playerChipset = playerChipsetData;
            weaponChipset = null;
            armorChipset = null;
        }
    }
    
    /// <summary>
    /// 플레이어 접근 감지
    /// </summary>
    private void CheckPlayerProximity()
    {
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= pickupRange)
            {
                PickupChipset();
            }
        }
    }
    
    /// <summary>
    /// 칩셋 픽업
    /// </summary>
    private void PickupChipset()
    {
        var chipsetManager = FindAnyObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 칩셋을 인벤토리에 추가
            chipsetManager.AddChipsetToInventory(GetCurrentChipset());
            
            // 이벤트 발생
            OnChipsetPickedUp?.Invoke(GetCurrentChipset());
            
            // 픽업 효과 생성
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // 오브젝트 제거
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 현재 칩셋 데이터 반환
    /// </summary>
    public object GetCurrentChipset()
    {
        if (weaponChipset != null) return weaponChipset;
        if (armorChipset != null) return armorChipset;
        if (playerChipset != null) return playerChipset;
        return null;
    }
    
    /// <summary>
    /// 칩셋 이름 반환
    /// </summary>
    public string GetChipsetName()
    {
        if (weaponChipset != null) return weaponChipset.chipsetName;
        if (armorChipset != null) return armorChipset.chipsetName;
        if (playerChipset != null) return playerChipset.chipsetName;
        return "Unknown Chipset";
    }
    
    /// <summary>
    /// 칩셋 희귀도 이름 반환
    /// </summary>
    public string GetChipsetRarityName()
    {
        if (weaponChipset != null) return weaponChipset.GetRarityName();
        if (armorChipset != null) return armorChipset.GetRarityName();
        if (playerChipset != null) return playerChipset.GetRarityName();
        return "Unknown";
    }
    
    /// <summary>
    /// 칩셋 희귀도 색상 반환
    /// </summary>
    public Color GetChipsetRarityColor()
    {
        if (weaponChipset != null) return weaponChipset.GetRarityColor();
        if (armorChipset != null) return armorChipset.GetRarityColor();
        if (playerChipset != null) return playerChipset.GetRarityColor();
        return Color.white;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 픽업 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
} 