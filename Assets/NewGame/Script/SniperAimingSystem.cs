using UnityEngine;
using UnityEngine.Rendering;

public class SniperAimingSystem : MonoBehaviour
{
    [Header("조준선 설정")]
    public LayerMask obstacleLayer = -1; // 장애물 레이어
    public Color aimingLineColor = Color.red;
    public float lineWidth = 0.1f; // 더 두껍게
    public float aimingRange = 25f; // 조준 범위
    
    private LineRenderer lineRenderer;
    private PlayerController playerController;
    private PlayerInventory playerInventory;
    private bool isAiming = false;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerInventory = GetComponent<PlayerInventory>();
        CreateAimingLine();
    }
    
    void Update()
    {
        // 현재 무기가 저격총인지 확인
        bool hasSniperRifle = HasSniperRifle();
        
        // 저격총을 들고 있으면 항상 조준선 표시
        if (hasSniperRifle)
        {
            if (!isAiming)
            {
                StartAiming();
            }
            UpdateAimingLine();
        }
        else
        {
            if (isAiming)
            {
                StopAiming();
            }
        }
    }
    
    bool HasSniperRifle()
    {
        if (playerInventory == null) return false;
        
        Weapon currentWeapon = playerInventory.GetCurrentWeapon();
        return currentWeapon != null && 
               currentWeapon.GetWeaponData() != null && 
               currentWeapon.GetWeaponData().weaponType == WeaponType.SR;
    }
    
    void CreateAimingLine()
    {
        // 새로운 GameObject에 LineRenderer 생성
        GameObject aimingLineObj = new GameObject("SniperAimingLine");
        aimingLineObj.transform.SetParent(transform);
        
        lineRenderer = aimingLineObj.AddComponent<LineRenderer>();
        
        // LineRenderer 강화된 설정 (0도에서도 확실히 보이도록)
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = aimingLineColor;
        lineRenderer.endColor = aimingLineColor;
        lineRenderer.startWidth = lineWidth * 2f; // 더 두껍게
        lineRenderer.endWidth = lineWidth * 2f; // 더 두껍게
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // 2D 렌더링 최적화
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 1000; // 최상위 렌더링
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        
        // 추가 설정 (더 확실하게 보이도록)
        lineRenderer.alignment = LineAlignment.View; // 카메라를 향해 정렬
        lineRenderer.textureMode = LineTextureMode.Tile;
        
        // 초기에는 비활성화
        aimingLineObj.SetActive(false);
        
        // Debug.Log("🎯 [SNIPER] 조준선 생성 완료 - 강화된 설정");
    }
    
    void StartAiming()
    {
        isAiming = true;
        if (lineRenderer != null && lineRenderer.gameObject != null)
        {
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.enabled = true;
            // Debug.Log("🎯 [SNIPER] 조준 시작");
        }
    }
    
    void StopAiming()
    {
        isAiming = false;
        if (lineRenderer != null && lineRenderer.gameObject != null)
        {
            lineRenderer.gameObject.SetActive(false);
            // Debug.Log("🎯 [SNIPER] 조준 종료");
        }
    }
    
    void UpdateAimingLine()
    {
        if (lineRenderer == null || playerController == null) return;
        
        // PlayerController와 완전히 동일한 방법으로 발사 위치와 방향 계산
        Vector3 firePosition = GetPlayerFirePosition();
        Vector2 fireDirection = GetPlayerFireDirection();
        
        // Z축 위치를 명확히 설정 (2D에서 잘 보이도록)
        Vector3 startPoint = firePosition;
        startPoint.z = -0.5f; // 카메라에 더 가깝게
        
        // 조준선 끝점 계산 (충분한 길이 보장)
        Vector3 endPoint = startPoint + (Vector3)fireDirection * aimingRange;
        endPoint.z = -0.5f; // 동일한 Z축
        
        // 0도일 때도 확실히 보이도록 최소 거리 보장
        float actualDistance = Vector3.Distance(startPoint, endPoint);
        if (actualDistance < 5f) // 너무 짧으면 강제로 늘림
        {
            endPoint = startPoint + (Vector3)fireDirection * 5f;
            endPoint.z = -0.5f;
        }
        
        // 장애물 체크 (Ground, Wall 등)
        RaycastHit2D hit = Physics2D.Raycast(startPoint, fireDirection, aimingRange, GetObstacleLayerMask());
        
        if (hit.collider != null)
        {
            endPoint = hit.point;
            endPoint.z = -0.5f; // Z축 맞춤
            // 장애물에 닿으면 노란색
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        }
        else
        {
            // 장애물이 없으면 빨간색
            lineRenderer.startColor = aimingLineColor;
            lineRenderer.endColor = aimingLineColor;
        }
        
        // 조준선 위치 설정
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        
        // LineRenderer 활성화 확인
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
        }
        
        // 디버그 정보 (필요시 주석 해제)
        // float currentAngle = playerController.GetCurrentWeaponAngle();
        // bool facingRight = playerController.IsFacingRight();
        // Debug.Log($"🎯 [SNIPER] 각도: {currentAngle}도, 방향: {fireDirection}, 거리: {Vector3.Distance(startPoint, endPoint)}");
        // Debug.Log($"🎯 [SNIPER] 시작점: {startPoint}, 끝점: {endPoint}");
    }
    
    Vector3 GetPlayerFirePosition()
    {
        // PlayerController.GetFirePosition()과 동일한 로직
        if (playerInventory == null) return transform.position;
        
        GameObject currentWeaponObj = playerInventory.GetCurrentWeaponObject();
        if (currentWeaponObj != null)
        {
            Transform firePoint = currentWeaponObj.transform.Find("FirePoint");
            if (firePoint != null)
            {
                return firePoint.position;
            }
        }
        
        // FirePoint가 없으면 WeaponHolder 위치 사용
        if (playerInventory.weaponHolder != null)
        {
            return playerInventory.weaponHolder.position;
        }
        
        // 마지막 대안 - 플레이어 위치
        return transform.position;
    }
    
    Vector2 GetPlayerFireDirection()
    {
        // PlayerController.GetFireDirection()과 완전히 동일한 로직
        if (playerController == null) return Vector2.right;
        
        // PlayerController의 실제 방향과 각도 사용
        bool facingRight = playerController.IsFacingRight();
        
        // 기본 방향 (수평)
        Vector2 baseDirection = facingRight ? Vector2.right : Vector2.left;
        
        // PlayerController의 실제 currentWeaponAngle 값 사용 (보간된 값)
        float currentWeaponAngle = playerController.GetCurrentWeaponAngle();
        
        // 왼쪽을 바라볼 때는 각도를 반전
        float actualAngle = facingRight ? currentWeaponAngle : -currentWeaponAngle;
        
        // 각도를 라디안으로 변환
        float angleInRadians = actualAngle * Mathf.Deg2Rad;
        
        // 회전된 방향 벡터 계산 (PlayerController와 완전 동일)
        Vector2 rotatedDirection = new Vector2(
            baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
            baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
        );
        
        return rotatedDirection.normalized;
    }
    
    int GetObstacleLayerMask()
    {
        // Player, Projectile, Weapon 레이어 제외하고 모든 것을 장애물로 간주
        int playerLayer = LayerMask.NameToLayer("Player");
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int weaponLayer = LayerMask.NameToLayer("Weapon");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        
        // 제외할 레이어들
        int ignoreLayers = (1 << playerLayer) | (1 << projectileLayer) | (1 << weaponLayer) | (1 << enemyLayer);
        
        // Default Layer (0)와 명명된 Ground 레이어만 체크
        int groundLayers = (1 << 0); // Default Layer
        int namedGroundLayer = LayerMask.NameToLayer("Ground");
        if (namedGroundLayer != -1)
        {
            groundLayers |= (1 << namedGroundLayer);
        }
        
        return groundLayers;
    }
    
    void OnDestroy()
    {
        if (lineRenderer != null && lineRenderer.gameObject != null)
        {
            Destroy(lineRenderer.gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (isAiming && playerController != null)
        {
            // 발사 위치와 방향 시각화
            Vector3 firePos = GetPlayerFirePosition();
            Vector2 fireDir = GetPlayerFireDirection();
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePos, 0.2f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePos, firePos + (Vector3)fireDir * aimingRange);
        }
    }
} 