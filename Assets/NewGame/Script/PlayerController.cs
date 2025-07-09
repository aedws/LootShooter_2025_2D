using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("이동/점프")]
    public float baseMoveSpeed = 5f; // 기본 이동속도 (무기 없을 때)
    [SerializeField] public float currentMoveSpeed = 5f; // 현재 적용 중인 이동속도 (무기 영향 포함)
    public float jumpForce = 13f; // 기본 점프력 - 밸런스 조정
    public float jumpBoost = 2.0f; // 점프 시 X축 속도 배수 (더 빠르게)
    public float maxJumpTime = 0.15f; // 점프 최대 지속 시간(초) - 미묘한 차이용

    [Header("대시")]
    public float dashForce = 25f; // 더 빠른 대시
    public float dashDuration = 0.25f; // 대시 지속시간
    public float dashCooldown = 1.2f; // 쿨다운
    public float dashInvincibleTime = 0.15f; // 무적시간
    
    [Header("대시 잔상 이펙트")]
    public float afterImageInterval = 0.05f; // 잔상 생성 간격
    public float afterImageDuration = 0.3f; // 잔상 지속 시간
    public Color afterImageColor = new Color(0.5f, 0.8f, 1f, 0.7f); // 잔상 색상 (파란 틴트)

    [Header("점프/중력 커스터마이즈")]
    public float fallMultiplier = 3.5f; // 떨어지는 속도 배수 - 더 빠르게
    public float lowJumpMultiplier = 3.0f; // 짧은 점프 속도 배수 - 더 빠르게
    public float jumpHoldForce = 0.8f; // 점프키 유지 시 추가 힘 - 매우 미미하게
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("상호작용")]
    public float pickupRange = 1.5f;
    public LayerMask pickupLayer = -1;

    [Header("무기 회전")]
    public float upAimAngle = 45f; // 위쪽 조준 각도
    public float downAimAngle = -45f; // 아래쪽 조준 각도
    public float aimRotationSpeed = 5f; // 회전 속도
    
    [Header("무기 반동")]
    public float recoilMultiplier = 1f; // 반동 강도 배수
    public float maxRecoilForce = 5f; // 최대 반동 힘
    public bool enablePlayerRecoil = true; // 플레이어 반동 활성화

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInventory playerInventory;
    private Health health;
    private Vector3 originalWeaponHolderPos;
    private float currentWeaponAngle = 0f; // 현재 무기 각도
    private float targetWeaponAngle = 0f; // 목표 무기 각도
    private bool isGrounded;
    private bool isDashing;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private List<GameObject> afterImages = new List<GameObject>(); // 잔상 오브젝트들
    private bool isJumping;
    private float jumpTimer;
    private float lastJumpX;
    private bool facingRight = true;
    
    // 무기 반동 관련
    private Weapon currentSubscribedWeapon = null; // 현재 이벤트 구독 중인 무기

    // 3점사/연사 모드 토글 변수
    public bool isBurstMode = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInventory = GetComponent<PlayerInventory>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        // PlayerInventory의 WeaponHolder 원본 위치 저장
        if (playerInventory != null && playerInventory.weaponHolder != null)
        {
            originalWeaponHolderPos = playerInventory.weaponHolder.localPosition;
            // Debug.Log($"[PlayerController] WeaponHolder 원본 위치 저장: {originalWeaponHolderPos}");
        }
        else
        {
            // Debug.LogWarning("[PlayerController] PlayerInventory 또는 WeaponHolder가 할당되지 않았습니다!");
        }
        
        // Health 이벤트 연결
        if (health != null)
        {
            health.OnDeath += OnPlayerDeath;
            health.OnDamaged += OnPlayerDamaged;
        }
        
        // 플레이어 태그 확인/설정
        if (!gameObject.CompareTag("Player"))
            gameObject.tag = "Player";
        
        // 이동속도 초기화
        currentMoveSpeed = baseMoveSpeed;
        
        // Inspector 파라미터를 그대로 사용
    }

    void OnDestroy()
    {
        // 잔상 오브젝트들 정리
        foreach (GameObject afterImage in afterImages)
        {
            if (afterImage != null)
                Destroy(afterImage);
        }
        afterImages.Clear();
        
        // 무기 이벤트 구독 해제
        UnsubscribeFromWeaponEvents();
    }

    void Update()
    {
        GroundCheck();
        // Debug.Log($"isGrounded: {isGrounded}, linearVelocity: {rb.linearVelocity}, Position: {transform.position}");

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // 대시 쿨타임 UI 갱신 및 표시/숨김 제어
        var statusUI = FindAnyObjectByType<PlayerStatusUI>();
        if (statusUI != null)
        {
            statusUI.UpdateDashCooldown(dashCooldownTimer, dashCooldown);
            if (dashCooldownTimer > 0f)
                statusUI.ShowDashCooldownBar();
            else
                statusUI.HideDashCooldownBar();
        }

        if (!isDashing)
        {
            Move();
            
            // 점프 시작
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
                isJumping = true;
                jumpTimer = 0f;
            }
            
            // 점프 중이면 무조건 타이머 증가
            if (isJumping)
            {
                jumpTimer += Time.deltaTime;
                
                // 스페이스바를 누르고 있고 아직 시간이 남았고 위로 올라가고 있을 때만 매우 미미한 추가 상승력
                if (Input.GetKey(KeyCode.Space) && jumpTimer < maxJumpTime && rb.linearVelocity.y > 1f)
                {
                    // 중력 상쇄 방식으로 더 자연스럽게
                    float gravityCounterForce = -Physics2D.gravity.y * rb.gravityScale * jumpHoldForce * 0.1f;
                    rb.linearVelocity += Vector2.up * gravityCounterForce * Time.deltaTime;
                }
                
                // 타이머가 끝나면 점프 상태 해제
                if (jumpTimer >= maxJumpTime)
                {
                    isJumping = false;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
                StartCoroutine(Dash());

            // R키로 재장전
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (playerInventory != null)
                {
                    Weapon weapon = playerInventory.GetCurrentWeapon();
                    if (weapon != null)
                    {
                        weapon.TryReload();
                }
            }
        }

        // X키로 3점사/연사 모드 토글
        if (Input.GetKeyDown(KeyCode.X))
        {
            isBurstMode = !isBurstMode;

            // UI 즉시 갱신
            if (statusUI != null)
                statusUI.UpdateWeaponUI();
            }
        }

        // Z키로 발사 (현재 모드에 따라)
        bool isFire = Input.GetKey(KeyCode.Z);
        if (isFire)
        {
            TryFireWeapon(isBurstMode, isFire);
        }

        // 쫄깃한 중력 적용
        if (rb.linearVelocity.y < 0)
        {
            // 떨어질 때 더 빠르게
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            // 점프키를 놓거나 점프 홀드가 끝나거나 속도가 느려지면 빠르게 하강
            bool jumpHoldActive = Input.GetKey(KeyCode.Space) && isJumping && jumpTimer < maxJumpTime && rb.linearVelocity.y > 1f;
            if (!jumpHoldActive)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        // 착지 시 점프 상태 완전 해제
        if (isGrounded && isJumping)
        {
            isJumping = false;
            jumpTimer = 0f;
        }

        // 애니메이션 업데이트
        UpdateAnimation();

        // 무기 조준 입력 처리
        HandleWeaponAiming();

        // 무기 방향 업데이트
        UpdateWeaponDirection();

        // 아이템 픽업 상호작용
        if (Input.GetKeyDown(KeyCode.E))
            TryPickupItem();

        // 무기 반동 이벤트 구독 관리
        UpdateWeaponEventSubscription();
    }

    void Move()
    {
        if (!isDashing)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * currentMoveSpeed, rb.linearVelocity.y);
            
            // 스프라이트 플립
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();
        }
    }

    void Jump()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float jumpX = moveInput * currentMoveSpeed * jumpBoost;
        lastJumpX = jumpX;
        rb.linearVelocity = new Vector2(jumpX, jumpForce);
        // isJumping은 Update()에서 관리
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        // 대시 시작: 무적 적용
        if (health != null) health.SetInvincible(true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), 0).normalized;
        if (dashDirection == Vector2.zero)
            dashDirection = facingRight ? Vector2.right : Vector2.left;

        rb.linearVelocity = dashDirection * dashForce;

        // 잔상 이펙트 시작
        StartCoroutine(CreateAfterImages());

        // 🛡️ 방어구 무적 시간 보너스 합산
        float bonusInvincible = 0f;
        if (playerInventory != null)
        {
            var armors = playerInventory.GetAllEquippedArmors();
            foreach (var armor in armors.Values)
            {
                bonusInvincible += armor.invincibilityBonus;
            }
        }
        float totalInvincibleTime = dashInvincibleTime + bonusInvincible;

        // 무적 시간 (대시 지속시간 내에서만)
        float invincibleTimer = Mathf.Min(totalInvincibleTime, dashDuration);
        while (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
            yield return null;
        }

        // 대시 지속시간의 나머지 부분
        float remainingDashTime = dashDuration - Mathf.Min(totalInvincibleTime, dashDuration);
        if (remainingDashTime > 0)
        {
            yield return new WaitForSeconds(remainingDashTime);
        }

        rb.gravityScale = originalGravity;
        isDashing = false;

        // 대시 종료: 무적 해제
        if (health != null) health.SetInvincible(false);
    }

    System.Collections.IEnumerator CreateAfterImages()
    {
        float timer = 0f;
        while (isDashing && timer < dashDuration)
        {
            CreateAfterImage();
            yield return new WaitForSeconds(afterImageInterval);
            timer += afterImageInterval;
        }
        
        // 대시가 끝난 후에도 잔상을 조금 더 생성 (자연스러운 페이드아웃)
        float extraTimer = 0f;
        float extraDuration = 0.1f; // 추가 잔상 시간
        while (extraTimer < extraDuration)
        {
            CreateAfterImage();
            yield return new WaitForSeconds(afterImageInterval);
            extraTimer += afterImageInterval;
        }
    }

    void CreateAfterImage()
    {
        // 잔상 오브젝트 생성
        GameObject afterImage = new GameObject("AfterImage");
        afterImage.transform.position = transform.position;
        afterImage.transform.rotation = transform.rotation;
        afterImage.transform.localScale = transform.localScale;

        // 플레이어 스프라이트 복사
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        SpriteRenderer afterImageSprite = afterImage.AddComponent<SpriteRenderer>();
        
        afterImageSprite.sprite = playerSprite.sprite;
        afterImageSprite.color = afterImageColor;
        afterImageSprite.flipX = playerSprite.flipX;
        afterImageSprite.sortingLayerName = playerSprite.sortingLayerName;
        afterImageSprite.sortingOrder = playerSprite.sortingOrder - 1;

        // 무기 잔상도 추가 (있다면)
        GameObject currentWeaponObj = playerInventory?.GetCurrentWeaponObject();
        if (currentWeaponObj != null)
        {
            SpriteRenderer weaponSprite = currentWeaponObj.GetComponent<SpriteRenderer>();
            if (weaponSprite != null)
            {
                GameObject weaponAfterImage = new GameObject("WeaponAfterImage");
                weaponAfterImage.transform.SetParent(afterImage.transform);
                weaponAfterImage.transform.localPosition = currentWeaponObj.transform.localPosition;
                weaponAfterImage.transform.localRotation = currentWeaponObj.transform.localRotation;
                weaponAfterImage.transform.localScale = currentWeaponObj.transform.localScale;

                SpriteRenderer weaponAfterImageSprite = weaponAfterImage.AddComponent<SpriteRenderer>();
                weaponAfterImageSprite.sprite = weaponSprite.sprite;
                weaponAfterImageSprite.color = afterImageColor;
                weaponAfterImageSprite.flipX = weaponSprite.flipX;
                weaponAfterImageSprite.sortingLayerName = weaponSprite.sortingLayerName;
                weaponAfterImageSprite.sortingOrder = weaponSprite.sortingOrder - 1;
            }
        }

        // 잔상 리스트에 추가
        afterImages.Add(afterImage);

        // 페이드아웃 시작
        StartCoroutine(FadeOutAfterImage(afterImage));
    }

    System.Collections.IEnumerator FadeOutAfterImage(GameObject afterImage)
    {
        float timer = 0f;
        SpriteRenderer[] spriteRenderers = afterImage.GetComponentsInChildren<SpriteRenderer>();
        Color[] startColors = new Color[spriteRenderers.Length];
        
        // 시작 색상들 저장
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            startColors[i] = spriteRenderers[i].color;
        }

        while (timer < afterImageDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(afterImageColor.a, 0f, timer / afterImageDuration);
            
            // 모든 스프라이트 렌더러의 투명도 업데이트
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color currentColor = startColors[i];
                    spriteRenderers[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                }
            }
            
            yield return null;
        }

        // 잔상 제거
        afterImages.Remove(afterImage);
        if (afterImage != null)
            Destroy(afterImage);
    }

    void GroundCheck()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.01f;
        float width = 0.2f;
        isGrounded =
            Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer) ||
            Physics2D.Raycast(origin + Vector2.left * width, Vector2.down, groundCheckDistance, groundLayer) ||
            Physics2D.Raycast(origin + Vector2.right * width, Vector2.down, groundCheckDistance, groundLayer);
    }

    void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
            spriteRenderer.flipX = !facingRight;
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            
            // 현재 Animator Controller에 있는 파라미터들에 맞게 업데이트
            animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f && isGrounded);
            animator.SetBool("isJumping", isJumping || !isGrounded);
            
            // 체력 시스템과 연동된 죽음 상태
            bool isDead = health != null && !health.IsAlive();
            animator.SetBool("isDead", isDead);
        }
    }
    
    void OnPlayerDamaged(int damage)
    {
        // Debug.Log($"[PlayerController] 플레이어가 {damage} 데미지를 받았습니다!");
        
        // 피격 시 화면 흔들림이나 이펙트 추가 가능
        // CameraShake.Instance?.Shake(0.1f, 0.5f);
    }
    
    void OnPlayerDeath()
    {
        // Debug.Log("[PlayerController] 플레이어가 죽었습니다!");
        
        // 플레이어 조작 비활성화
        this.enabled = false;
        
        // 물리 정지
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        // 게임 오버 처리 (나중에 GameManager에서 처리)
        // GameManager.Instance?.GameOver();
    }

    void TryPickupItem()
    {
        // 범위 내 아이템 검색
        Collider2D[] nearbyItems = Physics2D.OverlapCircleAll(transform.position, pickupRange, pickupLayer);
        
        foreach (Collider2D item in nearbyItems)
        {
            IItemPickup pickup = item.GetComponent<IItemPickup>();
            
            if (pickup != null)
            {
                // Debug.Log($"[TryPickupItem] {item.name} 픽업!");
                pickup.OnPickup(gameObject);
                break; // 하나씩만 픽업
            }
        }
    }

    // TryFireWeapon 오버로드: isBurst, isAuto 구분
    void TryFireWeapon(bool isBurst, bool isAuto)
    {
        if (playerInventory == null)
            return;

        Weapon currentWeapon = playerInventory.GetCurrentWeapon();
        if (currentWeapon == null)
            return;

        // AR 무기일 때만 분기, 그 외는 기존 방식
        if (currentWeapon.GetWeaponData() != null && currentWeapon.GetWeaponData().weaponType == WeaponType.AR)
        {
            if (isBurst)
            {
                // 3점사 (X키)
                currentWeapon.TryFire(GetFireDirection(), GetFirePosition(), true, true);
            }
            else if (isAuto)
            {
                // 연사 (Z키)
                currentWeapon.TryFire(GetFireDirection(), GetFirePosition(), true, false);
            }
        }
        else
        {
            // 기존 방식 (연사)
            currentWeapon.TryFire(GetFireDirection(), GetFirePosition(), isAuto, false);
        }
    }

    void HandleWeaponAiming()
    {
        // 위아래 입력 확인
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // 목표 각도 설정
        if (verticalInput > 0) // 위키
        {
            targetWeaponAngle = upAimAngle;
        }
        else if (verticalInput < 0) // 아래키
        {
            targetWeaponAngle = downAimAngle;
        }
        else // 키를 놓으면 수평으로
        {
            targetWeaponAngle = 0f;
        }
        
        // 현재 각도를 목표 각도로 부드럽게 보간
        currentWeaponAngle = Mathf.Lerp(currentWeaponAngle, targetWeaponAngle, aimRotationSpeed * Time.deltaTime);
    }

    Vector2 GetFireDirection()
    {
        // 기본 방향 (수평)
        Vector2 baseDirection = facingRight ? Vector2.right : Vector2.left;
        
        // 왼쪽을 바라볼 때는 각도를 반전
        float actualAngle = facingRight ? currentWeaponAngle : -currentWeaponAngle;
        
        // 현재 무기의 반동 각도 가져오기
        float recoilAngle = 0f;
        Weapon currentWeapon = playerInventory?.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            recoilAngle = currentWeapon.GetCurrentRecoilAngle();
            // 왼쪽을 바라볼 때는 반동 각도도 반전
            if (!facingRight) recoilAngle = -recoilAngle;
        }
        
        // 조준 각도 + 반동 각도
        float totalAngle = actualAngle + recoilAngle;
        
        // 각도를 라디안으로 변환
        float angleInRadians = totalAngle * Mathf.Deg2Rad;
        
        // 회전된 방향 벡터 계산
        Vector2 rotatedDirection = new Vector2(
            baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
            baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
        );
        
        return rotatedDirection.normalized;
    }

    // SniperAimingSystem에서 사용할 현재 무기 각도 getter
    public float GetCurrentWeaponAngle()
    {
        return currentWeaponAngle;
    }

    // SniperAimingSystem에서 사용할 플레이어 방향 getter
    public bool IsFacingRight()
    {
        return facingRight;
    }

    Vector3 GetFirePosition()
    {
        // 현재 무기의 FirePoint 찾기
        GameObject currentWeaponObj = playerInventory?.GetCurrentWeaponObject();
        if (currentWeaponObj != null)
        {
            Transform firePoint = currentWeaponObj.transform.Find("FirePoint");
            if (firePoint != null)
            {
                return firePoint.position;
            }
        }
        
        // FirePoint가 없으면 WeaponHolder 위치 사용
        if (playerInventory?.weaponHolder != null)
        {
            return playerInventory.weaponHolder.position;
        }
        
        // 그것도 없으면 플레이어 위치 사용
        return transform.position;
    }

    void UpdateWeaponDirection()
    {
        // WeaponHolder 위치 조정
        if (playerInventory?.weaponHolder != null && originalWeaponHolderPos != Vector3.zero)
        {
            Vector3 newPos = originalWeaponHolderPos;
            if (!facingRight)
            {
                newPos.x = -Mathf.Abs(originalWeaponHolderPos.x); // X 좌표 반전 (절댓값 사용)
            }
            else
            {
                newPos.x = Mathf.Abs(originalWeaponHolderPos.x); // X 좌표 정방향 (절댓값 사용)
            }
            
            playerInventory.weaponHolder.localPosition = newPos;
            // Debug.Log($"[UpdateWeaponDirection] WeaponHolder 위치 업데이트: {newPos}, facingRight: {facingRight}");
        }

        // WeaponHolder 회전 적용 (무기 조준각도 + 반동 각도)
        if (playerInventory?.weaponHolder != null)
        {
            float baseAngle = facingRight ? currentWeaponAngle : -currentWeaponAngle;
            
            // 현재 무기의 반동 각도 가져오기
            float recoilAngle = 0f;
            Weapon currentWeapon = playerInventory.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                recoilAngle = currentWeapon.GetCurrentRecoilAngle();
                // 왼쪽을 바라볼 때는 반동 각도도 반전
                if (!facingRight) recoilAngle = -recoilAngle;
            }
            
            float finalAngle = baseAngle + recoilAngle;
            playerInventory.weaponHolder.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
        }

        // 현재 무기 스프라이트 플립
        GameObject currentWeaponObj = playerInventory?.GetCurrentWeaponObject();
        if (currentWeaponObj != null)
        {
            SpriteRenderer weaponSprite = currentWeaponObj.GetComponent<SpriteRenderer>();
            if (weaponSprite != null)
            {
                weaponSprite.flipX = !facingRight;
            }

            // FirePoint 위치 조정
            Transform firePoint = currentWeaponObj.transform.Find("FirePoint");
            if (firePoint != null)
            {
                Vector3 firePointPos = firePoint.localPosition;
                // 원본 FirePoint 위치에서 X 좌표만 조정
                if (!facingRight && firePointPos.x > 0)
                {
                    firePoint.localPosition = new Vector3(-Mathf.Abs(firePointPos.x), firePointPos.y, firePointPos.z);
                }
                else if (facingRight && firePointPos.x < 0)
                {
                    firePoint.localPosition = new Vector3(Mathf.Abs(firePointPos.x), firePointPos.y, firePointPos.z);
                }
                // Debug.Log($"[UpdateWeaponDirection] FirePoint 위치 업데이트: {firePoint.localPosition}");
            }
        }
    }

    void UpdateWeaponEventSubscription()
    {
        // 현재 장착된 무기 가져오기
        Weapon currentWeapon = playerInventory?.GetCurrentWeapon();
        
        // 무기가 바뀌었거나 새로 장착되었으면 이벤트 구독 업데이트
        if (currentWeapon != currentSubscribedWeapon)
        {
            // 이전 무기 이벤트 구독 해제
            UnsubscribeFromWeaponEvents();
            
            // 새 무기 이벤트 구독
            if (currentWeapon != null)
            {
                currentWeapon.OnRecoil += OnWeaponRecoil;
                currentSubscribedWeapon = currentWeapon;
                // Debug.Log($"[PlayerController] 무기 반동 이벤트 구독: {currentWeapon.GetWeaponData()?.weaponName}");
            }
        }
    }
    
    void UnsubscribeFromWeaponEvents()
    {
        if (currentSubscribedWeapon != null)
        {
            currentSubscribedWeapon.OnRecoil -= OnWeaponRecoil;
            currentSubscribedWeapon = null;
            // Debug.Log("[PlayerController] 무기 반동 이벤트 구독 해제");
        }
    }
    
    void OnWeaponRecoil(Vector3 recoilInfo)
    {
        if (!enablePlayerRecoil || rb == null) return;
        
        // recoilInfo.y에 반동 각도 정보가 들어있음
        float recoilAngle = recoilInfo.y;
        
        // 발사 방향의 반대로 플레이어를 약간 밀어냄 (각도 기반)
        Vector2 fireDirection = GetFireDirection();
        Vector2 recoilDirection = -fireDirection; // 발사 방향의 반대
        
        // 반동 강도 계산 (각도를 Force로 변환)
        float recoilForce = recoilAngle * 0.1f * recoilMultiplier; // 각도를 적당한 Force로 변환
        recoilForce = Mathf.Min(recoilForce, maxRecoilForce); // 최대 반동 제한
        
        // 수평 반동만 적용 (점프에 영향 주지 않음)
        Vector2 horizontalRecoil = new Vector2(recoilDirection.x, 0) * recoilForce;
        
        // 대시 중이 아닐 때만 반동 적용
        if (!isDashing)
        {
            rb.AddForce(horizontalRecoil, ForceMode2D.Impulse);
            // Debug.Log($"🔥 [RECOIL] 플레이어 반동 적용: {horizontalRecoil}, 반동각도: {recoilAngle}도");
        }
    }

    // 🏃‍♂️ 무기에 따른 이동속도 업데이트 시스템
    public void UpdateMovementSpeed(WeaponData weaponData)
    {
        if (weaponData != null)
        {
            float previousSpeed = currentMoveSpeed;
            currentMoveSpeed = baseMoveSpeed * weaponData.movementSpeedMultiplier;
            
            // Debug.Log($"🏃‍♂️ [PlayerController] 이동속도 업데이트: {weaponData.weaponName} 장착");
            // Debug.Log($"   기본속도: {baseMoveSpeed} → 현재속도: {currentMoveSpeed:F2} (배수: {weaponData.movementSpeedMultiplier:F2})");
            
            // 무기 타입별 메시지 표시
            string speedEffect = GetSpeedEffectMessage(weaponData.movementSpeedMultiplier);
            // Debug.Log($"   {GetWeaponTypeKorean(weaponData.weaponType)} 무기 효과: {speedEffect}");
        }
        else
        {
            // 무기가 없을 때는 기본 속도로 복원
            currentMoveSpeed = baseMoveSpeed;
            // Debug.Log($"🏃‍♂️ [PlayerController] 무기 해제로 인한 이동속도 복원: {currentMoveSpeed}");
        }
    }
    
    // 무기 타입별 한국어 이름 반환
    string GetWeaponTypeKorean(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HG: return "권총";
            case WeaponType.SMG: return "기관단총";
            case WeaponType.AR: return "돌격소총";
            case WeaponType.SG: return "산탄총";
            case WeaponType.MG: return "기관총";
            case WeaponType.SR: return "저격총";
            default: return "알 수 없는 무기";
        }
    }
    
    // 속도 효과 메시지 반환
    string GetSpeedEffectMessage(float multiplier)
    {
        if (multiplier >= 1.1f) return "🟢 매우 빠름";
        else if (multiplier >= 1.0f) return "🟢 빠름";
        else if (multiplier >= 0.9f) return "🟡 약간 빠름";
        else if (multiplier >= 0.8f) return "🟡 보통";
        else if (multiplier >= 0.7f) return "🟠 약간 느림";
        else if (multiplier >= 0.6f) return "🟠 느림";
        else return "🔴 매우 느림";
    }
    
    // 현재 이동속도 반환 (외부에서 확인용)
    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }
    
    // 기본 이동속도 반환 (외부에서 확인용)
    public float GetBaseMoveSpeed()
    {
        return baseMoveSpeed;
    }

    void OnDrawGizmosSelected()
    {
        // 픽업 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        
        // 그라운드 체크 시각화
        Gizmos.color = Color.red;
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.01f;
        float width = 0.2f;
        Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(origin + Vector2.left * width, origin + Vector2.left * width + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(origin + Vector2.right * width, origin + Vector2.right * width + Vector2.down * groundCheckDistance);
        
        // WeaponHolder 위치 시각화
        if (playerInventory?.weaponHolder != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerInventory.weaponHolder.position, 0.1f);
            
            // 원본 위치와 현재 위치 비교
            if (originalWeaponHolderPos != Vector3.zero)
            {
                Vector3 originalWorldPos = transform.TransformPoint(originalWeaponHolderPos);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(originalWorldPos, 0.05f);
                Gizmos.DrawLine(playerInventory.weaponHolder.position, originalWorldPos);
            }
        }
    }
} 