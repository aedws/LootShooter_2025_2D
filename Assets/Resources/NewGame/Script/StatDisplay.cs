using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StatDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("📊 스탯 표시")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("🎨 시각적 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float animationSpeed = 5f;
    
    // Private variables
    private string statKey;
    private string statName;
    private string description;
    private Color statColor;
    private float currentValue = 0f;
    private float targetValue = 0f;
    private bool isHovered = false;
    
    // Public properties
    public string StatKey => statKey;
    public string StatName => statName;
    
    void Awake()
    {
        InitializeComponents();
    }
    
    void Update()
    {
        // 값 애니메이션
        if (Mathf.Abs(currentValue - targetValue) > 0.01f)
        {
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * animationSpeed);
            UpdateValueDisplay();
        }
        
        // 호버 효과
        if (isHovered && backgroundImage != null)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, highlightColor, Time.deltaTime * animationSpeed);
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, normalColor, Time.deltaTime * animationSpeed);
        }
    }
    
    void InitializeComponents()
    {
        // 자동으로 UI 컴포넌트 찾기
        if (nameText == null)
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        
        if (valueText == null)
            valueText = transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (tooltipPanel == null)
            tooltipPanel = transform.Find("TooltipPanel")?.gameObject;
        
        if (tooltipText == null)
            tooltipText = transform.Find("TooltipPanel/TooltipText")?.GetComponent<TextMeshProUGUI>();
        
        // 툴팁 초기 숨김
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    public void Initialize(string key, string name, Color color, string desc)
    {
        statKey = key;
        statName = name;
        statColor = color;
        description = desc;
        
        // UI 설정
        if (nameText != null)
        {
            nameText.text = statName;
            nameText.color = statColor;
        }
        
        if (tooltipText != null)
        {
            tooltipText.text = description;
        }
        
        // 초기 값 설정
        currentValue = 0f;
        targetValue = 0f;
        UpdateValueDisplay();
    }
    
    public void UpdateValue(float newValue)
    {
        targetValue = newValue;
    }
    
    void UpdateValueDisplay()
    {
        if (valueText == null) return;
        
        // 스탯 타입에 따른 표시 형식
        string displayValue = FormatValue(currentValue);
        valueText.text = displayValue;
        valueText.color = statColor;
    }
    
    string FormatValue(float value)
    {
        switch (statKey)
        {
            case "MoveSpeed":
            case "JumpForce":
                return $"{value:F1}";
            
            case "DashCooldown":
            case "ReloadTime":
            case "InvincibilityTime":
                return $"{value:F1}s";
            
            case "DamageReduction":
                return $"{value:F1}%";
            
            case "HealthRegen":
                return $"{value:F1}/s";
            
            case "CurrentWeapon":
                return value > 0 ? "장착됨" : "없음";
            
            case "WeaponChipsets":
            case "ArmorChipsets":
            case "PlayerChipsets":
                return $"{value:F0}개";
            
            default:
                return $"{value:F0}";
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        HideTooltip();
    }
    
    void ShowTooltip()
    {
        if (tooltipPanel != null && !string.IsNullOrEmpty(description))
        {
            tooltipPanel.SetActive(true);
        }
    }
    
    void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    // Public methods for external access
    public void SetColor(Color color)
    {
        statColor = color;
        if (nameText != null)
            nameText.color = color;
        UpdateValueDisplay();
    }
    
    public void SetDescription(string desc)
    {
        description = desc;
        if (tooltipText != null)
            tooltipText.text = desc;
    }
    
    public float GetCurrentValue() => currentValue;
    public float GetTargetValue() => targetValue;
} 