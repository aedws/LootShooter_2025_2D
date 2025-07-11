using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 드래그 가능한 칩셋 아이템 컴포넌트
/// 인벤토리에서 칩셋 슬롯으로 드래그할 수 있는 아이템
/// </summary>
public class ChipsetItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image chipsetIcon;
    [SerializeField] private TextMeshProUGUI chipsetNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image rarityBackground;
    
    [Header("Drag Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    
    // 칩셋 데이터
    [HideInInspector] public WeaponChipsetData weaponChipset;
    [HideInInspector] public ArmorChipsetData armorChipset;
    [HideInInspector] public PlayerChipsetData playerChipset;
    
    // 드래그 관련
    private Canvas parentCanvas;
    private Vector3 originalPosition;
    private Transform originalParent;
    
    // 이벤트
    public System.Action<ChipsetItem> OnDragStarted;
    public System.Action<ChipsetItem> OnDragEnded;
    
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        parentCanvas = GetComponentInParent<Canvas>();
    }
    
    /// <summary>
    /// 무기 칩셋으로 초기화
    /// </summary>
    public void Initialize(WeaponChipsetData chipset)
    {
        weaponChipset = chipset;
        armorChipset = null;
        playerChipset = null;
        
        UpdateUI();
    }
    
    /// <summary>
    /// 방어구 칩셋으로 초기화
    /// </summary>
    public void Initialize(ArmorChipsetData chipset)
    {
        armorChipset = chipset;
        weaponChipset = null;
        playerChipset = null;
        
        UpdateUI();
    }
    
    /// <summary>
    /// 플레이어 칩셋으로 초기화
    /// </summary>
    public void Initialize(PlayerChipsetData chipset)
    {
        playerChipset = chipset;
        weaponChipset = null;
        armorChipset = null;
        
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        var chipset = GetCurrentChipset();
        if (chipset == null) return;
        
        // 칩셋 이름 설정
        if (chipsetNameText != null)
        {
            chipsetNameText.text = GetChipsetName();
        }
        
        // 코스트 설정
        if (costText != null)
        {
            costText.text = GetChipsetCost().ToString();
        }
        
        // 희귀도 설정
        if (rarityText != null)
        {
            rarityText.text = GetChipsetRarityName();
        }
        
        // 희귀도 배경색 설정
        if (rarityBackground != null)
        {
            rarityBackground.color = GetChipsetRarityColor();
        }
        
        // 칩셋 아이콘 설정
        if (chipsetIcon != null)
        {
            chipsetIcon.color = GetChipsetRarityColor();
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
        return "Unknown";
    }
    
    /// <summary>
    /// 칩셋 코스트 반환
    /// </summary>
    public int GetChipsetCost()
    {
        if (weaponChipset != null) return weaponChipset.cost;
        if (armorChipset != null) return armorChipset.cost;
        if (playerChipset != null) return playerChipset.cost;
        return 0;
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
    
    // 드래그 이벤트
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = transform.parent;
        
        // 드래그 중일 때 반투명하게
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
        
        // 최상위로 이동
        transform.SetParent(parentCanvas.transform);
        
        OnDragStarted?.Invoke(this);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform != null)
        {
            rectTransform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // 원래 위치로 복원
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        transform.SetParent(originalParent);
        rectTransform.position = originalPosition;
        
        OnDragEnded?.Invoke(this);
    }
} 