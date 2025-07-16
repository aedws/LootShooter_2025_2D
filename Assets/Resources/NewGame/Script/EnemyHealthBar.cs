using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Canvas canvas;
    
    [Header("Health Bar Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    
    [Header("Animation")]
    [SerializeField] private float updateSpeed = 5f;
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private bool showHealthBar = true;
    
    [Header("Visibility")]
    [SerializeField] private bool hideWhenFullHealth = false;
    [SerializeField] private float hideDelay = 2f;
    
    private Health enemyHealth;
    private Transform enemyTransform;
    private Camera mainCamera;
    private float currentHealth;
    private float targetHealth;
    private float hideTimer = 0f;
    private bool isVisible = true;
    
    void Start()
    {
        // 컴포넌트 초기화
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // 몬스터 찾기 (이 스크립트가 몬스터의 자식으로 있다고 가정)
        enemyTransform = transform.parent;
        if (enemyTransform == null)
        {
            Debug.LogError("[EnemyHealthBar] 부모 몬스터를 찾을 수 없습니다!");
            return;
        }
        
        // Health 컴포넌트 찾기
        enemyHealth = enemyTransform.GetComponent<Health>();
        if (enemyHealth == null)
        {
            Debug.LogError($"[EnemyHealthBar] {enemyTransform.name}에 Health 컴포넌트가 없습니다!");
            return;
        }
        
        // 이벤트 구독
        enemyHealth.OnHealthChanged += OnHealthChanged;
        enemyHealth.OnDamaged += OnDamaged;
        enemyHealth.OnDeath += OnDeath;
        
        // 초기 설정
        InitializeHealthBar();
        
        // Canvas 설정
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
        }
    }
    
    void Update()
    {
        // 카메라 향하기
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0); // 텍스트가 뒤집히지 않도록
        }
        
        // 체력바 부드러운 업데이트
        UpdateHealthBar();
        
        // 숨김 타이머 처리
        UpdateVisibility();
    }
    
    void InitializeHealthBar()
    {
        if (enemyHealth == null) return;
        
        // 체력바 초기화
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
        
        // 초기 체력 설정
        currentHealth = enemyHealth.GetHealthPercentage();
        targetHealth = currentHealth;
        
        // 초기 색상 설정
        UpdateHealthColor();
        
        // 초기 텍스트 설정
        UpdateHealthText();
        
        // 초기 가시성 설정
        UpdateVisibility();
    }
    
    void UpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        // 부드러운 체력바 업데이트
        currentHealth = Mathf.Lerp(currentHealth, targetHealth, updateSpeed * Time.deltaTime);
        healthSlider.value = currentHealth;
        
        // 색상 업데이트
        UpdateHealthColor();
        
        // 텍스트 업데이트
        UpdateHealthText();
    }
    
    void UpdateHealthColor()
    {
        if (healthFillImage == null) return;
        
        Color targetColor;
        
        if (currentHealth <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else if (currentHealth <= mediumHealthThreshold)
        {
            targetColor = mediumHealthColor;
        }
        else
        {
            targetColor = fullHealthColor;
        }
        
        healthFillImage.color = Color.Lerp(healthFillImage.color, targetColor, Time.deltaTime * 3f);
    }
    
    void UpdateHealthText()
    {
        if (!showHealthText || healthText == null || enemyHealth == null) return;
        
        int currentHealthInt = Mathf.RoundToInt(enemyHealth.currentHealth);
        int maxHealthInt = enemyHealth.maxHealth;
        healthText.text = $"{currentHealthInt}/{maxHealthInt}";
    }
    
    void UpdateVisibility()
    {
        if (!hideWhenFullHealth) return;
        
        bool shouldShow = true;
        
        // 체력이 가득 찬 상태에서 일정 시간 후 숨기기
        if (currentHealth >= 0.99f)
        {
            hideTimer += Time.deltaTime;
            if (hideTimer >= hideDelay)
            {
                shouldShow = false;
            }
        }
        else
        {
            hideTimer = 0f;
            shouldShow = true;
        }
        
        // 가시성 변경
        if (shouldShow != isVisible)
        {
            isVisible = shouldShow;
            SetUIVisibility(isVisible);
        }
    }
    
    void SetUIVisibility(bool visible)
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(visible && showHealthBar);
        
        if (healthText != null)
            healthText.gameObject.SetActive(visible && showHealthText);
        
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(visible && showHealthBar);
    }
    
    void OnHealthChanged(int current, int max)
    {
        if (enemyHealth == null) return;
        
        targetHealth = enemyHealth.GetHealthPercentage();
        
        // 데미지를 받으면 즉시 표시
        if (hideWhenFullHealth)
        {
            hideTimer = 0f;
            if (!isVisible)
            {
                isVisible = true;
                SetUIVisibility(true);
            }
        }
    }
    
    void OnDamaged(int damage)
    {
        // 데미지를 받으면 체력바를 즉시 표시
        if (hideWhenFullHealth)
        {
            hideTimer = 0f;
            if (!isVisible)
            {
                isVisible = true;
                SetUIVisibility(true);
            }
        }
    }
    
    void OnDeath()
    {
        // 몬스터가 죽으면 체력바 숨기기
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= OnHealthChanged;
            enemyHealth.OnDamaged -= OnDamaged;
            enemyHealth.OnDeath -= OnDeath;
        }
    }
    
    // 외부에서 호출할 수 있는 메서드들
    public void SetHealthBarVisible(bool visible)
    {
        showHealthBar = visible;
        SetUIVisibility(visible && isVisible);
    }
    
    public void SetHealthTextVisible(bool visible)
    {
        showHealthText = visible;
        if (healthText != null)
            healthText.gameObject.SetActive(visible && isVisible);
    }
} 