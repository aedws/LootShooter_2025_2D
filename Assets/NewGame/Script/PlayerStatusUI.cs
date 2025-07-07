using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image reloadProgressBar;
    [SerializeField] private GameObject reloadProgressContainer;
    [SerializeField] private Image dashCooldownProgressBar;
    [SerializeField] private GameObject dashCooldownProgressContainer;

    [Header("UI 위치 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(2f, 0f, 0f); // 플레이어 오른쪽 2유닛
    [SerializeField] private bool followPlayer = true;

    [Header("UI 애니메이션")]
    [SerializeField] private float damageShakeIntensity = 0.1f;
    [SerializeField] private float damageShakeDuration = 0.2f;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color normalHealthColor = Color.green;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private Health playerHealth;
    private PlayerInventory playerInventory;
    private Weapon currentWeapon;
    private Canvas canvas;
    private Camera mainCamera;
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float shakeTimer = 0f;

    void Start()
    {
        // 컴포넌트 초기화
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
        originalPosition = transform.localPosition;

        // 플레이어 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            // 플레이어 컴포넌트 가져오기
            playerHealth = player.GetComponent<Health>();
            playerInventory = player.GetComponent<PlayerInventory>();

            // 이벤트 구독
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnDamaged += OnPlayerDamaged;
            }

            if (playerInventory != null)
            {
                playerInventory.OnWeaponChanged += OnWeaponChanged;
            }

            // 초기 UI 업데이트
            UpdateHealthUI(playerHealth.currentHealth, playerHealth.maxHealth);
            UpdateWeaponUI();
        }

        // 재장전 진행 바 초기 비활성화
        if (reloadProgressContainer != null)
        {
            reloadProgressContainer.SetActive(false);
        }
    }

    void Update()
    {
        // 플레이어 따라다니기
        if (followPlayer && player != null)
        {
            UpdatePosition();
        }

        // 흔들림 효과 처리
        if (isShaking)
        {
            UpdateShake();
        }

        // 재장전 진행 바 업데이트
        UpdateReloadProgress();
        // 대시 쿨타임 바 업데이트
        UpdateDashCooldownProgress();
    }

    void UpdatePosition()
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // World Space 모드: 플레이어 위치에 직접 따라다니기
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        }
        else
        {
            // Screen Space 모드: 플레이어 위치를 화면 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(player.position + offset);
            transform.position = screenPos;
        }
    }

    void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            // 체력 비율에 따른 색상 변경
            float healthRatio = (float)currentHealth / maxHealth;
            
            // lowHealthThreshold를 사용하여 색상 변경 로직 개선
            Color healthColor;
            if (healthRatio <= lowHealthThreshold)
            {
                // 체력이 임계값 이하일 때 빨간색으로
                healthColor = lowHealthColor;
            }
            else
            {
                // 체력이 임계값 이상일 때 그라데이션
                float normalizedRatio = (healthRatio - lowHealthThreshold) / (1f - lowHealthThreshold);
                healthColor = Color.Lerp(lowHealthColor, normalHealthColor, normalizedRatio);
            }
            
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = healthColor;
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
            
            // 체력이 낮을 때 텍스트 색상도 변경
            float healthRatio = (float)currentHealth / maxHealth;
            if (healthRatio <= lowHealthThreshold)
            {
                healthText.color = lowHealthColor;
            }
            else
            {
                healthText.color = Color.white;
            }
        }
    }

    void OnPlayerDamaged(int damage)
    {
        // 피격 시 흔들림 효과
        StartShake();
    }

    void OnWeaponChanged(WeaponData newWeapon, WeaponData oldWeapon)
    {
        // 기존 무기 이벤트 구독 해제
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoUI;
            currentWeapon.OnReloadStart -= OnReloadStart;
            currentWeapon.OnReloadComplete -= OnReloadComplete;
        }

        // 새 무기 설정
        if (newWeapon != null)
        {
            currentWeapon = playerInventory.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged += UpdateAmmoUI;
                currentWeapon.OnReloadStart += OnReloadStart;
                currentWeapon.OnReloadComplete += OnReloadComplete;
            }
        }
        else
        {
            currentWeapon = null;
        }

        UpdateWeaponUI();
    }

    public void UpdateWeaponUI()
    {
        if (currentWeapon != null)
        {
            // 무기 이름 표시 (AR이면 모드도 함께)
            if (weaponNameText != null)
            {
                string name = currentWeapon.GetWeaponData().weaponName;
                if (currentWeapon.GetWeaponData().weaponType == WeaponType.AR && player != null)
                {
                    var pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        name += pc.isBurstMode ? " [3점사]" : " [연사]";
                    }
                }
                weaponNameText.text = name;
            }

            // 탄약 표시
            UpdateAmmoUI(currentWeapon.GetCurrentAmmo(), currentWeapon.GetMaxAmmo());
        }
        else
        {
            // 무기가 없을 때
            if (weaponNameText != null)
            {
                weaponNameText.text = "무기 없음";
            }

            if (ammoText != null)
            {
                ammoText.text = "";
            }
        }
    }

    void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null && currentWeapon != null)
        {
            if (currentWeapon.GetWeaponData().infiniteAmmo)
            {
                ammoText.text = "∞";
            }
            else
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
                
                // 탄약 부족 시 색상 변경
                if (currentAmmo <= maxAmmo * 0.2f && currentAmmo > 0)
                {
                    ammoText.color = Color.yellow;
                }
                else if (currentAmmo <= 0)
                {
                    ammoText.color = Color.red;
                }
                else
                {
                    ammoText.color = Color.white;
                }
            }
        }
    }

    void OnReloadStart()
    {
        if (reloadProgressContainer != null)
        {
            reloadProgressContainer.SetActive(true);
        }
    }

    void OnReloadComplete()
    {
        if (reloadProgressContainer != null)
        {
            reloadProgressContainer.SetActive(false);
        }
    }

    void UpdateReloadProgress()
    {
        if (currentWeapon != null && currentWeapon.IsReloading())
        {
            if (reloadProgressBar != null)
            {
                reloadProgressBar.fillAmount = currentWeapon.GetReloadProgress();
            }
        }
    }

    void StartShake()
    {
        if (!isShaking)
        {
            isShaking = true;
            shakeTimer = 0f;
            originalPosition = transform.localPosition;
        }
    }

    void UpdateShake()
    {
        shakeTimer += Time.deltaTime;
        
        if (shakeTimer < damageShakeDuration)
        {
            float intensity = damageShakeIntensity * (1f - shakeTimer / damageShakeDuration);
            Vector3 shake = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0f
            );
            transform.localPosition = originalPosition + shake;
        }
        else
        {
            isShaking = false;
            transform.localPosition = originalPosition;
        }
    }

    public void ShowDashCooldownBar()
    {
        if (dashCooldownProgressContainer != null)
            dashCooldownProgressContainer.SetActive(true);
    }
    public void HideDashCooldownBar()
    {
        if (dashCooldownProgressContainer != null)
            dashCooldownProgressContainer.SetActive(false);
    }
    public void UpdateDashCooldown(float current, float max)
    {
        if (dashCooldownProgressBar != null)
            dashCooldownProgressBar.fillAmount = Mathf.Clamp01(current / max);
    }
    void UpdateDashCooldownProgress()
    {
        // PlayerController에서 값을 받아와야 함. (외부에서 호출 권장)
        // 이 함수는 필요시 확장 가능
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            playerHealth.OnDamaged -= OnPlayerDamaged;
        }

        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoUI;
            currentWeapon.OnReloadStart -= OnReloadStart;
            currentWeapon.OnReloadComplete -= OnReloadComplete;
        }
    }
} 