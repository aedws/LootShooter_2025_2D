using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Text bossNameText;
    public Text healthText;
    public Text phaseText;
    public Image healthFillImage;
    
    [Header("Phase Colors")]
    public Color phase1Color = Color.green;
    public Color phase2Color = Color.yellow;
    public Color phase3Color = Color.red;
    
    [Header("Animation")]
    public float updateSpeed = 5f;
    public bool showPhaseTransition = true;
    
    private BossEnemy targetBoss;
    private float currentHealth;
    private BossPhase currentPhase;
    
    void Start()
    {
        // 보스 찾기
        targetBoss = FindFirstObjectByType<BossEnemy>();
        
        if (targetBoss != null)
        {
            // 이벤트 연결
            targetBoss.OnBossDamaged += OnBossDamaged;
            targetBoss.OnPhaseChanged += OnPhaseChanged;
            targetBoss.OnBossDeath += OnBossDeath;
            
            // 초기 설정
            InitializeHealthBar();
        }
        else
        {
            // 보스가 없으면 숨기기
            gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (targetBoss == null) return;
        
        // 체력바 부드러운 업데이트
        UpdateHealthBar();
    }
    
    void InitializeHealthBar()
    {
        if (targetBoss == null) return;
        
        // 보스 이름 설정
        if (bossNameText != null)
        {
            bossNameText.text = targetBoss.bossName;
        }
        
        // 체력바 초기화
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
        
        // 초기 페이즈 설정
        currentPhase = targetBoss.GetCurrentPhase();
        UpdatePhaseDisplay();
        
        // 초기 체력 설정
        currentHealth = targetBoss.GetHealthPercentage();
        UpdateHealthText();
    }
    
    void UpdateHealthBar()
    {
        if (targetBoss == null) return;
        
        float targetHealth = targetBoss.GetHealthPercentage();
        
        // 부드러운 체력바 업데이트
        currentHealth = Mathf.Lerp(currentHealth, targetHealth, updateSpeed * Time.deltaTime);
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        UpdateHealthText();
        UpdateHealthColor();
    }
    
    void UpdateHealthText()
    {
        if (healthText != null)
        {
            int currentHealthInt = Mathf.RoundToInt(currentHealth * 100f);
            healthText.text = $"{currentHealthInt}%";
        }
    }
    
    void UpdateHealthColor()
    {
        if (healthFillImage == null) return;
        
        Color targetColor = phase1Color;
        
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                targetColor = phase1Color;
                break;
            case BossPhase.Phase2:
                targetColor = phase2Color;
                break;
            case BossPhase.Phase3:
                targetColor = phase3Color;
                break;
        }
        
        healthFillImage.color = targetColor;
    }
    
    void UpdatePhaseDisplay()
    {
        if (phaseText == null) return;
        
        string phaseName = "";
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                phaseName = "1단계";
                break;
            case BossPhase.Phase2:
                phaseName = "2단계";
                break;
            case BossPhase.Phase3:
                phaseName = "절망 모드";
                break;
        }
        
        phaseText.text = phaseName;
        
        // 페이즈 전환 애니메이션
        if (showPhaseTransition)
        {
            StartCoroutine(PhaseTransitionAnimation());
        }
    }
    
    System.Collections.IEnumerator PhaseTransitionAnimation()
    {
        if (phaseText == null) yield break;
        
        // 페이즈 텍스트 깜박임 효과
        Color originalColor = phaseText.color;
        float animationTime = 1f;
        float timer = 0f;
        
        while (timer < animationTime)
        {
            float alpha = Mathf.Sin(timer * 10f) * 0.5f + 0.5f;
            phaseText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        
        phaseText.color = originalColor;
    }
    
    void OnBossDamaged(int damage)
    {
        // 데미지 표시 효과 (선택사항)
        // ShowDamageText(damage);
    }
    
    void OnPhaseChanged(BossPhase newPhase)
    {
        currentPhase = newPhase;
        UpdatePhaseDisplay();
    }
    
    void OnBossDeath()
    {
        // 보스 사망 시 UI 숨기기
        gameObject.SetActive(false);
    }
    
    void ShowDamageText(int damage)
    {
        // 데미지 텍스트 표시 (선택사항)
        // GameObject damageText = Instantiate(damageTextPrefab, transform);
        // damageText.GetComponent<Text>().text = damage.ToString();
        // Destroy(damageText, 1f);
    }
    
    void OnDestroy()
    {
        // 이벤트 연결 해제
        if (targetBoss != null)
        {
            targetBoss.OnBossDamaged -= OnBossDamaged;
            targetBoss.OnPhaseChanged -= OnPhaseChanged;
            targetBoss.OnBossDeath -= OnBossDeath;
        }
    }
} 