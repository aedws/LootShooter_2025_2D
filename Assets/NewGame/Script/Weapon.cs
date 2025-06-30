using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 기본 정보")]
    public string weaponName = "Basic Gun";
    public float fireRate = 0.2f;
    public int damage = 10;
    public GameObject projectilePrefab;

    [Header("발사 설정")]
    public float projectileSpeed = 10f;

    private float fireCooldown = 0f;

    void Start()
    {
        // 초기화 (firePoint는 더 이상 사용하지 않음)
    }

    void OnEnable()
    {
        // Debug.Log($"[Weapon][OnEnable] {weaponName} scale: {transform.localScale}");
    }

    void Update()
    {
        // 쿨다운 감소
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
        
        // Debug.Log($"[Weapon][Update] {weaponName} scale: {transform.localScale}");
    }

    public void TryFire(Vector2 direction, Vector3 weaponPosition)
    {
        // 쿨다운 확인
        if (fireCooldown > 0f)
        {
            return;
        }

        // 필수 컴포넌트 확인
        if (projectilePrefab == null)
        {
            // Debug.LogError("projectilePrefab이 설정되지 않았습니다!");
            return;
        }

        // 발사 실행
        Fire(direction, weaponPosition);
        
        // 쿨다운 설정
        fireCooldown = fireRate;
    }

    public void Fire(Vector2 direction, Vector3 weaponPosition)
    {
        Transform firePoint = transform.Find("FirePoint");
        Vector3 spawnPosition;
        if (firePoint != null)
        {
            // PlayerController에서 이미 FirePoint x좌표를 반전했으므로, 여기서는 추가 flip 불필요
            spawnPosition = firePoint.position;
            // 디버그 로그 활성화
            var parentSprite = GetComponentInParent<SpriteRenderer>();
            Debug.Log($"[Weapon.Fire] weapon={weaponName}, flipX={(parentSprite != null ? parentSprite.flipX.ToString() : "null")}, FirePoint.localPosition={firePoint.localPosition}, FirePoint.position={firePoint.position}");
        }
        else
        {
            // FirePoint가 없으면 무기 위치에서 방향에 따라 오프셋 적용 (예: x=0.6f, y=0.2f)
            float offsetX = 0.6f;
            float offsetY = 0.2f;
            if (direction.x > 0) // 오른쪽
                spawnPosition = weaponPosition + new Vector3(offsetX, offsetY, 0f);
            else // 왼쪽
                spawnPosition = weaponPosition + new Vector3(-offsetX, offsetY, 0f);
            Debug.Log($"[Weapon.Fire] weapon={weaponName}, FirePoint 없음, spawnPosition={spawnPosition}");
        }
        
        // 투사체 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[Weapon.Fire] weapon={weaponName}, Projectile 생성 위치={spawnPosition}");
        
        // 투사체 컴포넌트 찾기 및 초기화
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(direction.normalized, damage, projectileSpeed);
        }
        else
        {
            // Debug.LogError("Projectile 컴포넌트를 찾을 수 없습니다!");
        }
    }

    // 무기 데이터로 초기화
    public void InitializeFromWeaponData(WeaponData weaponData)
    {
        if (weaponData != null)
        {
            weaponName = weaponData.weaponName;
            fireRate = weaponData.fireRate;
            damage = weaponData.damage;
            projectilePrefab = weaponData.projectilePrefab;
        }
    }
} 