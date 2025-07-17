using UnityEngine;

/// <summary>
/// 방어구 칩셋 데이터 클래스
/// Google Sheets에서 로드되는 방어구 칩셋 정보를 저장
/// </summary>
[System.Serializable]
public class ArmorChipsetData
{
    [Header("기본 정보")]
    public string chipsetId;                    // 칩셋 고유 ID
    public string chipsetName;                  // 칩셋 이름
    public ArmorChipsetType chipsetType;        // 칩셋 타입
    public ChipsetRarity rarity;                // 희귀도
    public int cost;                            // 코스트
    public string description;                  // 설명
    
    [Header("방어구 효과")]
    public float defenseBonus;                  // 방어력 보너스
    public float healthBonus;                   // 체력 보너스
    public float speedBonus;                    // 이동속도 보너스
    public float jumpForceBonus;                // 점프력 보너스
    public float dashCooldownReduction;         // 대시 쿨다운 감소
    public bool hasRegeneration;                // 체력 재생 여부
    public float regenerationRate;              // 재생 속도
    public bool hasInvincibilityFrame;          // 무적 시간 증가 여부
    public float invincibilityBonus;            // 무적 시간 보너스
    
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
            case ArmorChipsetType.Defense:
                return "방어력";
            case ArmorChipsetType.Health:
                return "체력";
            case ArmorChipsetType.Speed:
                return "이동속도";
            case ArmorChipsetType.JumpForce:
                return "점프력";
            case ArmorChipsetType.DashCooldown:
                return "대시";
            case ArmorChipsetType.Regeneration:
                return "재생";
            case ArmorChipsetType.Resistance:
                return "저항력";
            case ArmorChipsetType.Utility:
                return "유틸리티";
            case ArmorChipsetType.Synergy:
                return "시너지";
            case ArmorChipsetType.Hybrid:
                return "하이브리드";
            default:
                return "기타";
        }
    }
} 