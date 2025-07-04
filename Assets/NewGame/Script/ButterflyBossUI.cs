using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButterflyBossUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI bossNameText;
    public Image phaseIcon;
    public GameObject phaseTransitionEffect;
    
    [Header("페이즈 아이콘")]
    public Sprite phase1Icon;
    public Sprite phase2Icon;
    public Sprite phase3Icon;
    public Sprite phase4Icon;
    public Sprite phase5Icon;
    
    [Header("시각 효과")]
    public Color phase1Color = Color.blue;
    public Color phase2Color = Color.yellow;
    public Color phase3Color = new Color(1f, 0.5f, 0f, 1f); // orange
    public Color phase4Color = Color.red;
    public Color phase5Color = new Color(0.5f, 0f, 0.5f, 1f); // purple
    
    [Header("애니메이션")]
    public float healthBarAnimationSpeed = 5f;
    public float phaseTransitionDuration = 1f;
    public bool useShakeEffect = true;
    public float shakeIntensity = 10f;
    
    private ButterflyBoss boss;
    private Canvas canvas;
    private RectTransform canvasRect;
    private Vector3 originalHealthBarPosition;
    private bool isTransitioning = false;
    
    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        
        if (healthBar != null)
        {
            originalHealthBarPosition = healthBar.transform.localPosition;
        }
    }
    
    void Start()
    {
        // 보스 찾기
        FindBoss();
        
        // 초기 UI 설정
        if (boss != null)
        {
            SetupUI();
            SubscribeToBossEvents();
        }
        else
        {
            Debug.LogWarning("[ButterflyBossUI] 보스를 찾을 수 없습니다!");
        }
    }
    
    void FindBoss()
    {
        // 씬에서 ButterflyBoss 찾기
        ButterflyBoss[] bosses = FindObjectsByType<ButterflyBoss>(FindObjectsSortMode.None);
        if (bosses.Length > 0)
        {
            boss = bosses[0];
        }
    }
    
    void SetupUI()
    {
        if (boss == null) return;
        
        // 보스 이름 설정
        if (bossNameText != null)
        {
            bossNameText.text = boss.bossName;
        }
        
        // 초기 체력바 설정
        UpdateHealthBar();
        
        // 초기 페이즈 설정
        UpdatePhaseUI();
    }
    
    void SubscribeToBossEvents()
    {
        if (boss == null) return;
        
        // 이벤트 구독
        boss.OnPhaseChanged += OnPhaseChanged;
        boss.OnBossDamaged += OnBossDamaged;
        boss.OnBossDeath += OnBossDeath;
        boss.OnBossRevival += OnBossRevival;
    }
    
    void Update()
    {
        if (boss == null) return;
        
        // 실시간 체력 업데이트
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthBar == null || boss == null) return;
        
        float targetHealth = boss.GetHealthPercentage();
        float currentHealth = healthBar.value;
        
        // 부드러운 애니메이션
        float newHealth = Mathf.Lerp(currentHealth, targetHealth, healthBarAnimationSpeed * Time.deltaTime);
        healthBar.value = newHealth;
        
        // 체력 텍스트 업데이트
        if (healthText != null)
        {
            int currentHealthInt = Mathf.RoundToInt(boss.GetHealthPercentage() * 100f);
            healthText.text = $"{currentHealthInt}%";
        }
        
        // 체력바 색상 변경
        UpdateHealthBarColor();
    }
    
    void UpdateHealthBarColor()
    {
        if (healthBar == null || boss == null) return;
        
        Image fillImage = healthBar.fillRect.GetComponent<Image>();
        if (fillImage == null) return;
        
        float healthPercentage = boss.GetHealthPercentage();
        Color targetColor;
        
        if (healthPercentage > 70f)
        {
            targetColor = phase1Color;
        }
        else if (healthPercentage > 30f)
        {
            targetColor = phase2Color;
        }
        else if (healthPercentage > 10f)
        {
            targetColor = phase3Color;
        }
        else
        {
            targetColor = phase4Color;
        }
        
        fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * 2f);
    }
    
    void UpdatePhaseUI()
    {
        if (boss == null) return;
        
        ButterflyPhase currentPhase = boss.GetCurrentPhase();
        
        // 페이즈 텍스트 업데이트
        if (phaseText != null)
        {
            string phaseString = GetPhaseString(currentPhase);
            phaseText.text = $"Phase {phaseString}";
        }
        
        // 페이즈 아이콘 업데이트
        if (phaseIcon != null)
        {
            Sprite targetIcon = GetPhaseIcon(currentPhase);
            if (targetIcon != null)
            {
                phaseIcon.sprite = targetIcon;
            }
        }
        
        // 페이즈 색상 업데이트
        UpdatePhaseColor(currentPhase);
    }
    
    string GetPhaseString(ButterflyPhase phase)
    {
        switch (phase)
        {
            case ButterflyPhase.Phase1: return "1";
            case ButterflyPhase.Phase2: return "2";
            case ButterflyPhase.Phase3: return "3";
            case ButterflyPhase.Phase4: return "4";
            case ButterflyPhase.Phase5: return "5";
            default: return "?";
        }
    }
    
    Sprite GetPhaseIcon(ButterflyPhase phase)
    {
        switch (phase)
        {
            case ButterflyPhase.Phase1: return phase1Icon;
            case ButterflyPhase.Phase2: return phase2Icon;
            case ButterflyPhase.Phase3: return phase3Icon;
            case ButterflyPhase.Phase4: return phase4Icon;
            case ButterflyPhase.Phase5: return phase5Icon;
            default: return null;
        }
    }
    
    void UpdatePhaseColor(ButterflyPhase phase)
    {
        if (phaseText == null) return;
        
        Color targetColor;
        switch (phase)
        {
            case ButterflyPhase.Phase1:
                targetColor = phase1Color;
                break;
            case ButterflyPhase.Phase2:
                targetColor = phase2Color;
                break;
            case ButterflyPhase.Phase3:
                targetColor = phase3Color;
                break;
            case ButterflyPhase.Phase4:
                targetColor = phase4Color;
                break;
            case ButterflyPhase.Phase5:
                targetColor = phase5Color;
                break;
            default:
                targetColor = Color.white;
                break;
        }
        
        phaseText.color = Color.Lerp(phaseText.color, targetColor, Time.deltaTime * 3f);
    }
    
    // 이벤트 핸들러들
    void OnPhaseChanged(ButterflyPhase newPhase)
    {
        if (isTransitioning) return;
        
        StartCoroutine(PhaseTransitionEffect(newPhase));
    }
    
    void OnBossDamaged(int damage)
    {
        if (useShakeEffect && healthBar != null)
        {
            StartCoroutine(HealthBarShake());
        }
    }
    
    void OnBossDeath()
    {
        // 보스 사망 UI 효과
        StartCoroutine(BossDeathEffect());
    }
    
    void OnBossRevival()
    {
        // 보스 부활 UI 효과
        StartCoroutine(BossRevivalEffect());
    }
    
    // 코루틴들
    IEnumerator PhaseTransitionEffect(ButterflyPhase newPhase)
    {
        isTransitioning = true;
        
        // 페이즈 전환 이펙트
        if (phaseTransitionEffect != null)
        {
            phaseTransitionEffect.SetActive(true);
        }
        
        // 페이즈 텍스트 깜박임
        if (phaseText != null)
        {
            Color originalColor = phaseText.color;
            float timer = 0f;
            
            while (timer < phaseTransitionDuration)
            {
                float alpha = Mathf.Sin(timer * 20f) * 0.5f + 0.5f;
                phaseText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                timer += Time.deltaTime;
                yield return null;
            }
            
            phaseText.color = originalColor;
        }
        
        // UI 업데이트
        UpdatePhaseUI();
        
        // 이펙트 비활성화
        if (phaseTransitionEffect != null)
        {
            phaseTransitionEffect.SetActive(false);
        }
        
        isTransitioning = false;
    }
    
    IEnumerator HealthBarShake()
    {
        if (healthBar == null) yield break;
        
        Vector3 originalPos = healthBar.transform.localPosition;
        float shakeTime = 0.3f;
        float timer = 0f;
        
        while (timer < shakeTime)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            healthBar.transform.localPosition = originalPos + shakeOffset;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        healthBar.transform.localPosition = originalPos;
    }
    
    IEnumerator BossDeathEffect()
    {
        // 보스 사망 시 UI 효과
        if (healthBar != null)
        {
            float timer = 0f;
            float duration = 2f;
            
            while (timer < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / duration);
                CanvasGroup canvasGroup = healthBar.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = healthBar.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = alpha;
                
                timer += Time.deltaTime;
                yield return null;
            }
        }
        
        // UI 숨기기
        gameObject.SetActive(false);
    }
    
    IEnumerator BossRevivalEffect()
    {
        // 보스 부활 시 UI 효과
        if (healthBar != null)
        {
            // 체력바 깜박임
            float timer = 0f;
            float duration = 1f;
            
            while (timer < duration)
            {
                float alpha = Mathf.Sin(timer * 10f) * 0.5f + 0.5f;
                CanvasGroup canvasGroup = healthBar.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = healthBar.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = alpha;
                
                timer += Time.deltaTime;
                yield return null;
            }
            
            // 완전히 보이게
            CanvasGroup finalCanvasGroup = healthBar.GetComponent<CanvasGroup>();
            if (finalCanvasGroup != null)
            {
                finalCanvasGroup.alpha = 1f;
            }
        }
    }
    
    // 공개 메서드들
    public void SetBoss(ButterflyBoss newBoss)
    {
        // 기존 이벤트 구독 해제
        if (boss != null)
        {
            boss.OnPhaseChanged -= OnPhaseChanged;
            boss.OnBossDamaged -= OnBossDamaged;
            boss.OnBossDeath -= OnBossDeath;
            boss.OnBossRevival -= OnBossRevival;
        }
        
        boss = newBoss;
        
        if (boss != null)
        {
            SetupUI();
            SubscribeToBossEvents();
        }
    }
    
    public void ShowUI()
    {
        gameObject.SetActive(true);
    }
    
    public void HideUI()
    {
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (boss != null)
        {
            boss.OnPhaseChanged -= OnPhaseChanged;
            boss.OnBossDamaged -= OnBossDamaged;
            boss.OnBossDeath -= OnBossDeath;
            boss.OnBossRevival -= OnBossRevival;
        }
    }
} 