using UnityEngine;

public enum WeaponType
{
    AR,  // Assault Rifle
    HG,  // Handgun
    MG,  // Machine Gun
    SG,  // Shotgun
    SMG, // Submachine Gun
    SR   // Sniper Rifle
}

public enum WeaponRarity
{
    Primordial, // 태초(청록)
    Common,     // 흰색
    Rare,       // 파랑
    Epic,       // 보라
    Legendary   // 주황
}

[CreateAssetMenu(menuName = "LootShooter/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보 - Basic Information")]
    [Tooltip("UI와 게임에서 표시될 무기 이름")]
    public string weaponName;
    
    [Tooltip("인벤토리에서 보여질 무기 아이콘 이미지")]
    public Sprite icon;
    
    [Tooltip("무기 종류 (AR=돌격소총, HG=권총, MG=기관총, SG=산탄총, SMG=기관단총, SR=저격총)")]
    public WeaponType weaponType;
    
    [Tooltip("무기 플레이버 텍스트 (설명, 배경 스토리 등)")]
    [TextArea(2, 4)]
    public string flavorText = "이 무기에 대한 설명이 여기에 표시됩니다.";
    
    [Header("기본 스탯 - Basic Stats")]
    [Tooltip("발사 간격 (초 단위, 낮을수록 빠름)")]
    public float fireRate;
    
    [Tooltip("한 발당 기본 데미지")]
    public int damage;
    
    [Tooltip("발사할 총알 프리팹")]
    public GameObject projectilePrefab;
    
    [Tooltip("플레이어가 들고 있을 무기 모델 프리팹")]
    public GameObject weaponPrefab;
    
    [Tooltip("총알이 날아가는 속도")]
    public float projectileSpeed = 10f;
    
    [Header("탄약 시스템 - Ammunition System")]
    [Tooltip("최대 탄창 용량 (한번에 장전할 수 있는 최대 탄약 수)")]
    public int maxAmmo = 30;
    
    [Tooltip("현재 탄창에 들어있는 탄약 수 (게임 시작시 초기값)")]
    public int currentAmmo = 30;
    
    [Tooltip("재장전에 걸리는 시간 (초 단위)")]
    public float reloadTime = 2f;
    
    [Tooltip("무한 탄약 여부 (true시 탄약 소모 없음)")]
    public bool infiniteAmmo = false;
    
    [Header("탄 퍼짐 설정 - Bullet Spread (AR/HG/SMG 주로 사용)")]
    [Tooltip("기본 탄퍼짐 각도 (0 = 완전 정확, 높을수록 부정확)")]
    public float baseSpread = 0f;
    
    [Tooltip("연사시 도달 가능한 최대 탄퍼짐 각도")]
    public float maxSpread = 5f;
    
    [Tooltip("연사할 때마다 퍼짐이 증가하는 속도")]
    public float spreadIncreaseRate = 1f;
    
    [Tooltip("발사를 멈췄을 때 퍼짐이 감소하는 속도")]
    public float spreadDecreaseRate = 2f;
    
    [Header("샷건 설정 - Shotgun Settings (SG 타입 전용)")]
    [Tooltip("샷건 한 번 발사시 나가는 탄환 개수")]
    public int pelletsPerShot = 6;
    
    [Tooltip("샷건 탄환들이 퍼지는 부채꼴 각도 (도 단위)")]
    public float shotgunSpreadAngle = 30f;
    
    [Header("머신건 설정 - Machine Gun Settings (MG 타입 전용)")]
    [Tooltip("연사 시작 후 최대 연사속도에 도달하는데 걸리는 예열 시간")]
    public float warmupTime = 1f;
    
    [Tooltip("예열 완료시 최대 연사속도 (초 단위, 낮을수록 빠름)")]
    public float maxWarmupFireRate = 0.05f;
    
    [Header("저격총 설정 - Sniper Rifle Settings (SR 타입 전용)")]
    [Tooltip("단발만 가능 여부 (true시 연사 불가)")]
    public bool singleFireOnly = true;
    
    [Tooltip("조준 모드에서 조준선이 표시되는 거리")]
    public float aimingRange = 15f;
    
    [Header("쌍권총 설정 - Dual Pistol Settings (HG 타입 전용)")]
    [Tooltip("쌍권총 사용 여부 (true시 양손에 권총 장착)")]
    public bool isDualPistol = false;

    [Tooltip("쌍권총 발사 간격 (초 단위, 낮을수록 빠름)")]
    public float dualPistolFireInterval = 0.1f;

    [Tooltip("쌍권총 좌우 발사 위치 오프셋")]
    public float dualPistolOffset = 0.3f;

    [Tooltip("쌍권총 좌우 탄 퍼짐 차이 (도 단위)")]
    public float dualPistolSpreadDifference = 2f;
    
    [Header("이동속도 영향 - Movement Speed Effect")]
    [Tooltip("무기가 플레이어 이동속도에 미치는 영향 (1.0 = 기본속도, 0.8 = 20% 감소, 1.2 = 20% 증가)")]
    [Range(0.3f, 1.5f)]
    public float movementSpeedMultiplier = 1.0f;
    
    [Header("SMG 대시 후 이동속도 증가 - SMG Dash Speed Boost")]
    [Tooltip("SMG 대시 후 이동속도 증가량 (SMG 타입 전용)")]
    public float smgDashSpeedBonus = 2f;
    
    [Tooltip("SMG 대시 후 이동속도 증가 지속시간 (초 단위)")]
    public float smgDashSpeedDuration = 3f;
    
    [Header("반동 설정 - Recoil Settings")]
    [Tooltip("발사시 카메라/무기에 가해지는 반동 강도 (0 = 반동 없음)")]
    public float recoilForce = 1f;
    
    [Tooltip("반동이 가해지는 방향 (X=좌우, Y=상하)")]
    public Vector2 recoilDirection = Vector2.up;
    
    [Tooltip("반동이 지속되는 시간 (초 단위)")]
    public float recoilDuration = 0.1f;
    
    [Tooltip("반동에서 원래 위치로 돌아오는 속도")]
    public float recoilRecoverySpeed = 5f;
    
    [Header("특수 효과 - Special Effects")]
    [Tooltip("크리티컬 히트 발생 확률 (0.0 = 0%, 1.0 = 100%)")]
    [Range(0f, 1f)]
    public float criticalChance = 0f;
    
    [Tooltip("크리티컬 히트시 데미지 배수 (2.0 = 2배 데미지)")]
    public float criticalMultiplier = 2f;
    
    [Tooltip("적을 관통할 수 있는 최대 개수 (0 = 관통 없음)")]
    public int pierceCount = 0;
    
    [Tooltip("관통시마다 데미지 감소율 (0.1 = 10%씩 감소, 0 = 감소 없음)")]
    [Range(0f, 1f)]
    public float pierceDamageReduction = 0.1f;
    
    [Tooltip("발사체에 예광탄 효과 적용 여부")]
    public bool hasTracerRounds = false;
    
    [Tooltip("발사시 화염 효과 생성 여부")]
    public bool hasMuzzleFlash = true;
    
    [Tooltip("적 처치시 폭발 효과 생성 여부")]
    public bool hasExplosiveKills = false;
    
    [Tooltip("폭발 반경 (hasExplosiveKills가 true일 때만 적용)")]
    public float explosionRadius = 2f;
    
    [Tooltip("폭발 시각 효과 프리팹 (선택사항)")]
    public GameObject explosionEffectPrefab;
    
    [Tooltip("폭발 색상 (프리팹이 없을 때 사용)")]
    public Color explosionColor = new Color(1f, 0.5f, 0f, 1f); // 주황색

    [Header("등급(레어리티)")]
    public WeaponRarity rarity = WeaponRarity.Common;

    [Header("칩셋 시스템 - Chipset System")]
    [Tooltip("무기에 장착 가능한 최대 칩셋 코스트")]
    public int maxChipsetCost = 25;
    
    [Tooltip("현재 장착된 칩셋 ID 목록 (쉼표로 구분)")]
    public string equippedChipsets = "";

    void OnValidate()
    {
        // This method is kept empty as the existing code handles validation
    }

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case WeaponRarity.Primordial: return new Color(0f, 1f, 1f); // 청록색
            case WeaponRarity.Common: return Color.white;
            case WeaponRarity.Rare: return Color.blue;
            case WeaponRarity.Epic: return new Color(0.5f, 0f, 1f); // 보라색
            case WeaponRarity.Legendary: return new Color(1f, 0.5f, 0f); // 주황색
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// 장착된 칩셋 ID 목록을 배열로 반환
    /// </summary>
    public string[] GetEquippedChipsetIds()
    {
        if (string.IsNullOrEmpty(equippedChipsets))
            return new string[0];
        
        return equippedChipsets.Split(',');
    }
    
    /// <summary>
    /// 칩셋 ID 목록을 문자열로 설정
    /// </summary>
    public void SetEquippedChipsetIds(string[] chipsetIds)
    {
        equippedChipsets = string.Join(",", chipsetIds);
    }
} 