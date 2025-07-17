using UnityEngine;

/// <summary>
/// 무기 칩셋 데이터 클래스
/// Google Sheets에서 로드되는 무기 칩셋 정보를 저장
/// </summary>
[System.Serializable]
public class WeaponChipsetData
{
    [Header("기본 정보")]
    public string chipsetId;                    // 칩셋 고유 ID
    public string chipsetName;                  // 칩셋 이름
    public WeaponChipsetType chipsetType;       // 칩셋 타입
    public ChipsetRarity rarity;                // 희귀도
    public int cost;                            // 코스트
    public string description;                  // 설명
    
    [Header("무기 효과")]
    public float damageBonus;                   // 데미지 보너스
    public float fireRateBonus;                 // 발사속도 보너스
    public float accuracyBonus;                 // 정확도 보너스
    public float recoilReduction;               // 반동 감소
    public float reloadSpeedBonus;              // 재장전속도 보너스
    public int ammoCapacityBonus;               // 탄약량 보너스
    public float criticalChanceBonus;           // 크리티컬 확률 보너스
    public float criticalMultiplierBonus;       // 크리티컬 배율 보너스
    
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
            case WeaponChipsetType.Damage:
                return "데미지";
            case WeaponChipsetType.FireRate:
                return "발사속도";
            case WeaponChipsetType.Accuracy:
                return "정확도";
            case WeaponChipsetType.Stability:
                return "안정성";
            case WeaponChipsetType.Capacity:
                return "용량";
            case WeaponChipsetType.Reload:
                return "재장전";
            case WeaponChipsetType.Critical:
                return "크리티컬";
            case WeaponChipsetType.Utility:
                return "유틸리티";
            case WeaponChipsetType.Synergy:
                return "시너지";
            case WeaponChipsetType.Hybrid:
                return "하이브리드";
            default:
                return "기타";
        }
    }
} 