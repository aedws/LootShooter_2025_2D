using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("이동/점프")]
    public float moveSpeed = 5f;
    public float jumpForce = 13f; // 기본 점프력 - 밸런스 조정
    public float jumpBoost = 2.0f; // 점프 시 X축 속도 배수 (더 빠르게)
    public float maxJumpTime = 0.15f; // 점프 최대 지속 시간(초) - 미묘한 차이용

    [Header("대시")]
    public float dashForce = 20f; // 더 빠른 대시
    public float dashDuration = 0.15f; // 짧고 강력한 대시
    public float dashCooldown = 1.2f; // 조금 더 긴 쿨다운
    public float dashInvincibleTime = 0.1f; // 짧은 무적시간
    
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

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInventory playerInventory;
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInventory = GetComponent<PlayerInventory>();
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
            Debug.LogWarning("[PlayerController] PlayerInventory 또는 WeaponHolder가 할당되지 않았습니다!");
        }
        
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
    }

    void Update()
    {
        GroundCheck();
        // Debug.Log($"isGrounded: {isGrounded}, linearVelocity: {rb.linearVelocity}, Position: {transform.position}");

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

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

        // 무기 발사 (연속 발사 지원)
        if (Input.GetKey(KeyCode.Z)) // Z키를 누르고 있는 동안
            TryFireWeapon();
    }

    void Move()
    {
        if (!isDashing)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            
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
        float jumpX = moveInput * moveSpeed * jumpBoost;
        lastJumpX = jumpX;
        rb.linearVelocity = new Vector2(jumpX, jumpForce);
        // isJumping은 Update()에서 관리
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), 0).normalized;
        if (dashDirection == Vector2.zero)
            dashDirection = facingRight ? Vector2.right : Vector2.left;

        rb.linearVelocity = dashDirection * dashForce;

        // 잔상 이펙트 시작
        StartCoroutine(CreateAfterImages());

        // 무적 시간
        float invincibleTimer = dashInvincibleTime;
        while (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
            yield return null;
        }

        // 대시 지속 시간
        yield return new WaitForSeconds(dashDuration - dashInvincibleTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
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
            animator.SetBool("isDead", false); // 나중에 체력 시스템 연동 시 사용
        }
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
                Debug.Log($"[TryPickupItem] {item.name} 픽업!");
                pickup.OnPickup(gameObject);
                break; // 하나씩만 픽업
            }
        }
    }

    void TryFireWeapon()
    {
        if (playerInventory == null)
            return;

        Weapon currentWeapon = playerInventory.GetCurrentWeapon();
        if (currentWeapon == null)
        {
            Debug.Log("[TryFireWeapon] 장착된 무기가 없습니다.");
            return;
        }

        // 플레이어가 바라보는 방향과 무기 각도를 고려한 발사 방향 계산
        Vector2 fireDirection = GetFireDirection();
        Vector3 firePosition = GetFirePosition();

        // Debug.Log($"[TryFireWeapon] 무기 발사: {currentWeapon.weaponName}, 방향: {fireDirection}");
        
        // 무기 발사
        currentWeapon.TryFire(fireDirection, firePosition);
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
        
        // 각도를 라디안으로 변환
        float angleInRadians = actualAngle * Mathf.Deg2Rad;
        
        // 회전된 방향 벡터 계산
        Vector2 rotatedDirection = new Vector2(
            baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
            baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
        );
        
        return rotatedDirection.normalized;
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

        // WeaponHolder 회전 적용 (총기 끝에서 회전하도록)
        if (playerInventory?.weaponHolder != null)
        {
            float weaponRotationAngle = facingRight ? currentWeaponAngle : -currentWeaponAngle;
            playerInventory.weaponHolder.transform.rotation = Quaternion.AngleAxis(weaponRotationAngle, Vector3.forward);
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