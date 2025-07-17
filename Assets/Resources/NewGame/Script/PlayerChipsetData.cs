using UnityEngine;

/// <summary>
/// 플레이어 칩셋 데이터 클래스
/// Google Sheets에서 로드되는 플레이어 칩셋 정보를 저장
/// </summary>
[System.Serializable]
public class PlayerChipsetData
{
    [Header("기본 정보")]
    public string chipsetId;                    // 칩셋 고유 ID
    public string chipsetName;                  // 칩셋 이름
    public PlayerChipsetType chipsetType;       // 칩셋 타입
    public ChipsetRarity rarity;                // 희귀도
    public int cost;                            // 코스트
    public string description;                  // 설명
    
    [Header("플레이어 기본 스탯 효과")]
    public float moveSpeedBonus;                // 이동속도 보너스
    public float jumpForceBonus;                // 점프력 보너스
    public float dashForceBonus;                // 대시 힘 보너스
    public float dashCooldownReduction;         // 대시 쿨다운 감소
    public float maxHealthBonus;                // 최대 체력 보너스
    public float damageReduction;               // 데미지 감소율
    public float pickupRangeBonus;              // 아이템 픽업 범위 보너스
    
    [Header("무기 스탯 효과 (모든 무기에 적용)")]
    public float weaponDamageBonus;             // 무기 데미지 보너스
    public float weaponFireRateBonus;           // 무기 발사속도 보너스
    public float weaponAccuracyBonus;           // 무기 정확도 보너스
    public float weaponRecoilReduction;         // 무기 반동 감소
    public float weaponReloadSpeedBonus;        // 무기 재장전속도 보너스
    public int weaponAmmoCapacityBonus;         // 무기 탄약량 보너스
    public float weaponCriticalChanceBonus;     // 무기 크리티컬 확률 보너스
    public float weaponCriticalMultiplierBonus; // 무기 크리티컬 배율 보너스
    
    [Header("칩셋 효과")]
    public float effectValue;                   // 칩셋 효과 수치
    
    [Header("특수 효과")]
    public bool hasSpecialEffect;               // 특수효과 여부
    public string specialEffectType;            // 특수효과 타입
    public float specialEffectValue;            // 특수효과 수치
    
    /// <summary>
    /// 칩셋의 희귀도에 따른 색상을 반환
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ChipsetRarity.Primordial:
                return new Color(0f, 1f, 1f); // 청록색
            case ChipsetRarity.Common:
                return Color.white;
            case ChipsetRarity.Rare:
                return Color.blue;
            case ChipsetRarity.Epic:
                return Color.magenta;
            case ChipsetRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 칩셋의 희귀도에 따른 이름을 반환
    /// </summary>
    public string GetRarityName()
    {
        switch (rarity)
        {
            case ChipsetRarity.Primordial:
                return "태초";
            case ChipsetRarity.Common:
                return "일반";
            case ChipsetRarity.Rare:
                return "희귀";
            case ChipsetRarity.Epic:
                return "영웅";
            case ChipsetRarity.Legendary:
                return "전설";
            default:
                return "일반";
        }
    }
    
    /// <summary>
    /// 칩셋의 타입에 따른 이름을 반환
    /// </summary>
    public string GetTypeName()
    {
        switch (chipsetType)
        {
            case PlayerChipsetType.WeaponMastery:
                return "무기 숙련도";
            case PlayerChipsetType.CombatExpert:
                return "전투 전문가";
            case PlayerChipsetType.Survivor:
                return "생존자";
            case PlayerChipsetType.Speedster:
                return "스피드스터";
            case PlayerChipsetType.Tactical:
                return "전술가";
            case PlayerChipsetType.Synergy:
                return "시너지";
            case PlayerChipsetType.Hybrid:
                return "하이브리드";
            case PlayerChipsetType.Ultimate:
                return "궁극";
            default:
                return "기타";
        }
    }
} 