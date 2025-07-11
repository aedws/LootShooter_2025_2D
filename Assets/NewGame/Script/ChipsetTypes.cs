using System;

/// <summary>
/// 칩셋 시스템에 필요한 모든 enum 정의
/// </summary>

[Serializable]
public enum ChipsetRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 영웅
    Legendary   // 전설
}

[Serializable]
public enum WeaponChipsetType
{
    Damage,         // 데미지 증가
    FireRate,       // 발사속도 증가
    Accuracy,       // 정확도 증가
    Stability,      // 반동 감소
    Capacity,       // 탄약량 증가
    Reload,         // 재장전속도 증가
    Critical,       // 크리티컬 관련
    Utility,        // 유틸리티형
    Synergy,        // 시너지형
    Hybrid          // 하이브리드형
}

[Serializable]
public enum ArmorChipsetType
{
    Defense,        // 방어력 증가
    Health,         // 체력 보너스
    Speed,          // 이동속도 보너스
    JumpForce,      // 점프력 보너스
    DashCooldown,   // 대시 쿨다운 감소
    Regeneration,   // 체력 재생
    Resistance,     // 저항력
    Utility,        // 유틸리티형
    Synergy,        // 시너지형
    Hybrid          // 하이브리드형
}

[Serializable]
public enum PlayerChipsetType
{
    WeaponMastery,  // 무기 숙련도 (모든 무기 스탯 증가)
    CombatExpert,   // 전투 전문가 (전투 관련 스탯 증가)
    Survivor,       // 생존자 (체력, 방어 관련 스탯 증가)
    Speedster,      // 스피드스터 (이동, 회피 관련 스탯 증가)
    Tactical,       // 전술가 (특수 능력 관련)
    Synergy,        // 시너지형
    Hybrid,         // 하이브리드형
    Ultimate        // 궁극형 (모든 스탯 증가)
} 