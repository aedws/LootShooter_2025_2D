using UnityEngine;

[System.Serializable]
public enum ArmorType
{
    Helmet,     // 머리
    Chest,      // 상체
    Legs,       // 하체
    Boots,      // 신발
    Shoulder,   // 어깨 (악세사리)
    Accessory   // 기타 악세사리
}

[System.Serializable]
public enum ArmorRarity
{
    Primordial, // 태초(청록)
    Common,     // 흰색
    Rare,       // 파랑
    Epic,       // 보라
    Legendary   // 주황
}

[CreateAssetMenu(fileName = "New Armor", menuName = "LootShooter/Armor Data")]
public class ArmorData : ScriptableObject
{
    [Header("기본 정보")]
    public string armorName = "기본 방어구";
    public ArmorType armorType = ArmorType.Chest;
    public ArmorRarity rarity = ArmorRarity.Common;
    public Sprite icon;
    
    [Header("방어 능력")]
    public int defense = 10;
    public int maxHealth = 0;
    public float damageReduction = 0f; // 데미지 감소율 (0.1 = 10% 감소)
    
    [Header("추가 능력")]
    public float moveSpeedBonus = 0f;  // 이동속도 보너스
    public float jumpForceBonus = 0f;  // 점프력 보너스
    public float dashCooldownReduction = 0f; // 대시 쿨다운 감소
    
    [Header("특수 효과")]
    public bool hasRegeneration = false; // 체력 재생
    public float regenerationRate = 1f; // 초당 재생량
    public bool hasInvincibilityFrame = false; // 무적 시간 증가
    public float invincibilityBonus = 0f; // 무적 시간 보너스
    
    [Header("설명")]
    [TextArea(3, 5)]
    public string description = "기본 방어구입니다.";
    
    [Header("시각적 효과")]
    public Color rarityColor = Color.white;
    public GameObject visualEffect; // 장착 시 시각 효과
    
    void OnValidate()
    {
        // 레어리티에 따른 색상 자동 설정
        switch (rarity)
        {
            case ArmorRarity.Primordial:
                rarityColor = new Color(0f, 1f, 1f); // 청록색
                break;
            case ArmorRarity.Common:
                rarityColor = Color.white;
                break;
            case ArmorRarity.Rare:
                rarityColor = Color.blue;
                break;
            case ArmorRarity.Epic:
                rarityColor = new Color(0.5f, 0f, 1f); // 보라색
                break;
            case ArmorRarity.Legendary:
                rarityColor = new Color(1f, 0.5f, 0f); // 주황색
                break;
        }
    }
    
    // 레어리티별 색상 반환
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ArmorRarity.Primordial: return new Color(0f, 1f, 1f); // 청록색
            case ArmorRarity.Common: return Color.white;
            case ArmorRarity.Rare: return Color.blue;
            case ArmorRarity.Epic: return new Color(0.5f, 0f, 1f); // 보라색
            case ArmorRarity.Legendary: return new Color(1f, 0.5f, 0f); // 주황색
            default: return Color.white;
        }
    }
    
    // 레어리티별 이름 반환
    public string GetRarityName()
    {
        switch (rarity)
        {
            case ArmorRarity.Primordial: return "태초";
            case ArmorRarity.Common: return "일반";
            case ArmorRarity.Rare: return "희귀";
            case ArmorRarity.Epic: return "영웅";
            case ArmorRarity.Legendary: return "전설";
            default: return "알 수 없음";
        }
    }
    
    // 타입별 이름 반환
    public string GetTypeName()
    {
        switch (armorType)
        {
            case ArmorType.Helmet: return "머리";
            case ArmorType.Chest: return "상체";
            case ArmorType.Legs: return "하체";
            case ArmorType.Boots: return "신발";
            case ArmorType.Shoulder: return "어깨";
            case ArmorType.Accessory: return "악세사리";
            default: return "알 수 없음";
        }
    }
} 