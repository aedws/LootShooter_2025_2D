using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("무기 데이터")]
    public WeaponData weaponData;
    
    [Header("현재 상태")]
    private float fireCooldown = 0f;
    private float currentSpread = 0f;
    private bool isFiring = false;
    private float lastFireTime = 0f;
    private float warmupProgress = 0f; // MG용 예열 진행도
    private bool isWarmedUp = false;
    
    [Header("탄약 시스템")]
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimeRemaining = 0f;
    
    [Header("반동 시스템")]
    private float currentRecoilAngle = 0f; // 현재 반동 각도
    private float recoilTimeRemaining = 0f;
    private bool isRecoiling = false;
    
    [Header("발사 상태 추적")]
    private bool wasFireButtonPressed = false;
    private float timeSinceLastShot = 0f;
    
    private Coroutine autoReloadCoroutine;
    
    // 이벤트 및 델리게이트
    public System.Action<int, int> OnAmmoChanged; // 현재탄약, 최대탄약
    public System.Action OnReloadStart;
    public System.Action OnReloadComplete;
    public System.Action<Vector3> OnRecoil; // 반동 이벤트
    public System.Action<Vector3, float> OnExplosion; // 폭발 이벤트

    void Start()
    {
        if (weaponData != null)
        {
            Debug.Log($"[무기등급] {weaponData.weaponName} rarity: {weaponData.rarity}, color: {weaponData.GetRarityColor()}");
            InitializeFromWeaponData();
        }
        // 등급별 색상 적용
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && weaponData != null)
            sr.color = weaponData.GetRarityColor();
    }

    void Update()
    {
        // 쿨다운 업데이트
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }
        
        // 재장전 처리
        UpdateReload();
        
        // 반동 처리
        UpdateRecoil();
        
        // 탄 퍼짐 감소 (발사하지 않을 때)
        if (!isFiring && currentSpread > weaponData.baseSpread)
        {
            currentSpread -= weaponData.spreadDecreaseRate * Time.deltaTime;
            currentSpread = Mathf.Max(currentSpread, weaponData.baseSpread);
        }
        
        // 머신건 예열 시스템
        if (weaponData != null && weaponData.weaponType == WeaponType.MG)
        {
            UpdateMachineGunWarmup();
        }
        
        // 발사 후 시간 추적
        timeSinceLastShot += Time.deltaTime;
        
        // 발사 상태 리셋
        if (timeSinceLastShot > 0.2f)
        {
            isFiring = false;
        }
        
        // 발사 버튼 상태 리셋 (PlayerController에서 버튼을 놓았을 때 감지)
        if (!Input.GetKey(KeyCode.Z))
        {
            wasFireButtonPressed = false;
        }
    }

    // isBurst: 3점사 여부, isFireButtonPressed: 연사/단발 입력
    public bool TryFire(Vector2 direction, Vector3 weaponPosition, bool isFireButtonPressed, bool isBurst)
    {
        if (weaponData == null || isReloading) return false;
        
        // 쿨다운 체크 (더 관대하게)
        if (fireCooldown > 0.01f) return false; // 0.01초 이하면 발사 허용
        
        // 탄약 확인 (무한 탄약이 아닌 경우)
        if (!weaponData.infiniteAmmo && currentAmmo <= 0)
        {
            // 자동 재장전 시도
            TryReload();
            return false;
        }
        
        // 발사 버튼 상태 추적
        bool isNewPress = isFireButtonPressed && !wasFireButtonPressed;
        wasFireButtonPressed = isFireButtonPressed;
        
        // AR: 3점사/연사 분기
        if (weaponData.weaponType == WeaponType.AR)
        {
            if (isBurst)
            {
                if (isNewPress)
                {
                    StartCoroutine(BurstFire(direction, weaponPosition, 3, 0.07f)); // 3발, 0.07초 간격
                }
                return false;
            }
            else
            {
                // 연사(자동사격): 기존 Fire 호출
                return Fire(direction, weaponPosition, isNewPress);
            }
        }
        
        // 저격총: 단발만 가능
        if (weaponData.weaponType == WeaponType.SR && weaponData.singleFireOnly)
        {
            if (!isNewPress) return false;
        }
        
        // 머신건: 예열 시스템
        if (weaponData.weaponType == WeaponType.MG)
        {
            if (!HandleMachineGunFiring(isFireButtonPressed)) return false;
        }
        
        // 발사 실행
        return Fire(direction, weaponPosition, isNewPress);
    }

    // 3점사 코루틴
    private IEnumerator BurstFire(Vector2 direction, Vector3 weaponPosition, int burstCount, float burstInterval)
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (!weaponData.infiniteAmmo && currentAmmo <= 0)
            {
                TryReload();
                yield break;
            }
            Fire(direction, weaponPosition, true);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private bool Fire(Vector2 direction, Vector3 weaponPosition, bool isNewPress)
    {
        Vector3 spawnPosition = GetFirePosition(weaponPosition);
        
        // 탄약 소모 (무한 탄약이 아닌 경우)
        if (!weaponData.infiniteAmmo)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
        }
        
        // 무기 타입별 발사 처리
        switch (weaponData.weaponType)
        {
            case WeaponType.SG:
                FireShotgun(direction, spawnPosition);
                break;
            default:
                FireSingleProjectile(direction, spawnPosition, isNewPress);
                break;
        }
        
        // 발사 후 처리
        UpdateFireState();
        SetCooldown();
        ApplyRecoil();
        
        return true;
    }

    private void FireSingleProjectile(Vector2 direction, Vector3 spawnPosition, bool isNewPress)
    {
        // 탄 퍼짐 계산
        Vector2 finalDirection = ApplySpread(direction, isNewPress);
        
        // 투사체 생성
        GameObject proj = Instantiate(weaponData.projectilePrefab, spawnPosition, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            // 기본 데미지 계산
            int finalDamage = GetCurrentDamage();
            
            // 크리티컬 계산
            bool isCritical = Random.Range(0f, 1f) < weaponData.criticalChance;
            if (isCritical)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * weaponData.criticalMultiplier);
                // Debug.Log($"Critical Hit! Damage: {finalDamage}");
            }
            
            // 투사체 초기화 (관통, 폭발 등 특수 효과 포함)
            projectile.Init(finalDirection.normalized, finalDamage, GetCurrentProjectileSpeed());
            
            // 특수 효과 적용
            ApplySpecialEffects(projectile, isCritical);
        }

        // 탄약이 0이 되었을 때 자동 재장전 예약
        if (!weaponData.infiniteAmmo && currentAmmo == 0 && !isReloading)
        {
            if (autoReloadCoroutine != null)
                StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = StartCoroutine(AutoReloadAfterDelay(0.3f));
        }
    }

    private void FireShotgun(Vector2 direction, Vector3 spawnPosition)
    {
        // 샷건: 여러 발 부채꼴로 발사
        for (int i = 0; i < weaponData.pelletsPerShot; i++)
        {
            // 부채꼴 각도 계산
            float angleOffset = Random.Range(-weaponData.shotgunSpreadAngle / 2f, weaponData.shotgunSpreadAngle / 2f);
            Vector2 spreadDirection = RotateVector(direction, angleOffset);
            
            // 투사체 생성
            GameObject proj = Instantiate(weaponData.projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                // 샷건 펠릿별 개별 크리티컬 계산
                int finalDamage = GetCurrentDamage();
                bool isCritical = Random.Range(0f, 1f) < weaponData.criticalChance;
                if (isCritical)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * weaponData.criticalMultiplier);
                }
                
                projectile.Init(spreadDirection.normalized, finalDamage, GetCurrentProjectileSpeed());
                ApplySpecialEffects(projectile, isCritical);
            }
        }
        // 샷건도 탄약 0이면 자동 재장전 예약
        if (!weaponData.infiniteAmmo && currentAmmo == 0 && !isReloading)
        {
            if (autoReloadCoroutine != null)
                StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = StartCoroutine(AutoReloadAfterDelay(0.3f));
        }
    }

    private void ApplySpecialEffects(Projectile projectile, bool isCritical)
    {
        // 관통 설정
        if (weaponData.pierceCount > 0)
        {
            projectile.SetPiercing(weaponData.pierceCount, weaponData.pierceDamageReduction);
        }
        
        // 폭발 효과 설정
        if (weaponData.hasExplosiveKills)
        {
            projectile.SetExplosive(weaponData.explosionRadius, OnExplosion);
        }
        
        // 예광탄 효과 (시각적 효과는 Projectile에서 처리)
        if (weaponData.hasTracerRounds)
        {
            projectile.SetTracer(true);
        }
        
        // 크리티컬 효과 표시
        if (isCritical)
        {
            projectile.SetCritical(true);
        }
    }

    public bool TryReload()
    {
        if (weaponData == null || isReloading || weaponData.infiniteAmmo) return false;
        if (currentAmmo >= weaponData.maxAmmo) return false; // 이미 가득참
        
        StartReload();
        return true;
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimeRemaining = weaponData.reloadTime;
        OnReloadStart?.Invoke();
        // Debug.Log($"Reloading {weaponData.weaponName}... ({weaponData.reloadTime}s)");
    }

    private void UpdateReload()
    {
        if (!isReloading) return;
        
        reloadTimeRemaining -= Time.deltaTime;
        
        if (reloadTimeRemaining <= 0f)
        {
            CompleteReload();
        }
    }

    private void CompleteReload()
    {
        isReloading = false;
        currentAmmo = weaponData.maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
        OnReloadComplete?.Invoke();
        // Debug.Log($"Reload complete! {currentAmmo}/{weaponData.maxAmmo}");
    }

    private void ApplyRecoil()
    {
        if (weaponData.recoilForce <= 0f) return;
        
        // 반동 각도 계산 (위쪽으로만, 랜덤 요소 추가)
        float recoilAngle = weaponData.recoilForce * Random.Range(0.7f, 1.3f);
        
        // 현재 반동 각도에 누적 (연사 시 점점 위로 올라감)
        currentRecoilAngle += recoilAngle;
        
        // 최대 반동 각도 제한 (너무 위로 올라가지 않도록)
        currentRecoilAngle = Mathf.Min(currentRecoilAngle, 15f);
        
        recoilTimeRemaining = weaponData.recoilDuration;
        isRecoiling = true;
        
        // 반동 이벤트 발생 (각도 정보 전달)
        Vector3 recoilInfo = new Vector3(0, recoilAngle, 0); // Y축에 각도 정보
        OnRecoil?.Invoke(recoilInfo);
    }

    private void UpdateRecoil()
    {
        if (!isRecoiling && currentRecoilAngle <= 0f) return;
        
        if (recoilTimeRemaining > 0f)
        {
            recoilTimeRemaining -= Time.deltaTime;
        }
        else
        {
            // 반동 각도 복구 (원래 각도로 부드럽게 복귀)
            currentRecoilAngle = Mathf.Lerp(currentRecoilAngle, 0f, weaponData.recoilRecoverySpeed * Time.deltaTime);
            
            if (currentRecoilAngle < 0.1f)
            {
                currentRecoilAngle = 0f;
                isRecoiling = false;
            }
        }
    }

    private Vector2 ApplySpread(Vector2 direction, bool isNewPress)
    {
        float spreadAmount = 0f;
        
        switch (weaponData.weaponType)
        {
            case WeaponType.AR:
                // AR: 기본 퍼짐 적음
                spreadAmount = currentSpread * 0.5f;
                break;
                
            case WeaponType.HG:
                // HG: 단발시 정확, 연사시 퍼짐
                if (isNewPress || timeSinceLastShot > 0.3f)
                {
                    spreadAmount = 0f; // 단발 시 정확
                }
                else
                {
                    spreadAmount = currentSpread * 1.5f; // 연사 시 큰 퍼짐
                }
                break;
                
            case WeaponType.SMG:
                // SMG: 중간 정도 퍼짐
                spreadAmount = currentSpread;
                break;
                
            case WeaponType.MG:
                // MG: 예열 상태에 따른 퍼짐
                if (isWarmedUp)
                {
                    spreadAmount = currentSpread * 0.7f; // 예열되면 안정적
                }
                else
                {
                    spreadAmount = currentSpread * 2f; // 예열 안되면 불안정
                }
                break;
                
            case WeaponType.SR:
                // SR: 퍼짐 없음
                spreadAmount = 0f;
                break;
        }
        
        // 랜덤 퍼짐 적용
        float randomAngle = Random.Range(-spreadAmount, spreadAmount);
        return RotateVector(direction, randomAngle);
    }

    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void UpdateMachineGunWarmup()
    {
        if (isFiring)
        {
            // 예열 진행
            warmupProgress += Time.deltaTime / weaponData.warmupTime;
            warmupProgress = Mathf.Clamp01(warmupProgress);
            isWarmedUp = warmupProgress >= 1f;
        }
        else
        {
            // 예열 감소
            warmupProgress -= Time.deltaTime * 2f; // 더 빠르게 감소
            warmupProgress = Mathf.Max(0f, warmupProgress);
            isWarmedUp = false;
        }
    }

    private bool HandleMachineGunFiring(bool isFireButtonPressed)
    {
        if (!isFireButtonPressed)
        {
            return false;
        }
        
        // 단발로 쏠 때 효율 감소
        if (timeSinceLastShot > 0.5f)
        {
            warmupProgress *= 0.5f; // 예열 감소
        }
        
        return true;
    }

    private void UpdateFireState()
    {
        isFiring = true;
        lastFireTime = Time.time;
        timeSinceLastShot = 0f;
        
        // 탄 퍼짐 증가 (SR 제외)
        if (weaponData.weaponType != WeaponType.SR)
        {
            currentSpread += weaponData.spreadIncreaseRate;
            currentSpread = Mathf.Min(currentSpread, weaponData.maxSpread);
        }
    }

    private void SetCooldown()
    {
        float currentFireRate = weaponData.fireRate;
        
        // 머신건 예열 시 연사속도 증가 (maxWarmupFireRate가 0보다 클 때만)
        if (weaponData.weaponType == WeaponType.MG && isWarmedUp && weaponData.maxWarmupFireRate > 0f)
        {
            currentFireRate = Mathf.Lerp(weaponData.fireRate, weaponData.maxWarmupFireRate, warmupProgress);
        }
        
        fireCooldown = currentFireRate;
        
        // 디버그: 쿨다운 설정 확인
        // Debug.Log($"🔫 [COOLDOWN] {weaponData.weaponName}: {currentFireRate}초 쿨다운 설정");
    }

    private int GetCurrentDamage()
    {
        int damage = weaponData.damage;
        
        // HG: 근거리 데미지 보너스 (거리 계산은 실제 구현시 추가 가능)
        if (weaponData.weaponType == WeaponType.HG)
        {
            damage = Mathf.RoundToInt(damage * 1.2f); // 20% 보너스
        }
        
        return damage;
    }

    private float GetCurrentProjectileSpeed()
    {
        return weaponData.projectileSpeed;
    }

    private Vector3 GetFirePosition(Vector3 weaponPosition)
    {
        Transform firePoint = transform.Find("FirePoint");
        if (firePoint != null)
        {
            return firePoint.position;
        }
        else
        {
            // FirePoint가 없으면 기본 오프셋 사용
            return weaponPosition + new Vector3(0.6f, 0.2f, 0f);
        }
    }

    public void InitializeFromWeaponData()
    {
        if (weaponData != null)
        {
            Debug.Log($"[무기등급] {weaponData.weaponName} rarity: {weaponData.rarity}, color: {weaponData.GetRarityColor()}");
            currentSpread = weaponData.baseSpread;
            warmupProgress = 0f;
            isWarmedUp = false;
            currentAmmo = weaponData.currentAmmo;
            OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
            // 등급별 색상 적용
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = weaponData.GetRarityColor();
        }
    }

    // Public Getters
    public WeaponData GetWeaponData() => weaponData;
    public float GetWarmupProgress() => warmupProgress;
    public float GetCurrentSpread() => currentSpread;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => weaponData?.maxAmmo ?? 0;
    public bool IsReloading() => isReloading;
    public float GetReloadProgress() => isReloading ? (1f - (reloadTimeRemaining / weaponData.reloadTime)) : 0f;
    public float GetCurrentRecoilAngle() => currentRecoilAngle; // 현재 반동 각도

    private IEnumerator AutoReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isReloading && !weaponData.infiniteAmmo && currentAmmo == 0)
        {
            TryReload();
        }
    }
}