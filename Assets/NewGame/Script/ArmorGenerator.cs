using UnityEngine;
using System.Collections.Generic;

public class ArmorGenerator : MonoBehaviour
{
    [Header("🛡️ 방어구 생성기 설정")]
    [Tooltip("기본 방어구 템플릿들")]
    public ArmorData[] armorTemplates;
    
    [Header("📊 레어리티 확률")]
    [Range(0f, 1f)]
    public float commonChance = 0.6f;      // 60%
    
    [Range(0f, 1f)]
    public float rareChance = 0.25f;       // 25%
    
    [Range(0f, 1f)]
    public float epicChance = 0.12f;       // 12%
    
    [Range(0f, 1f)]
    public float legendaryChance = 0.03f;  // 3%
    
    [Header("🎲 옵션 설정")]
    [Tooltip("방어력 변동 범위 (±%)")]
    [Range(0f, 0.5f)]
    public float defenseVariation = 0.2f;  // ±20%
    
    [Tooltip("체력 보너스 변동 범위 (±%)")]
    [Range(0f, 0.5f)]
    public float healthVariation = 0.3f;   // ±30%
    
    [Tooltip("이동속도 보너스 변동 범위 (±%)")]
    [Range(0f, 0.5f)]
    public float speedVariation = 0.25f;   // ±25%
    
    [Header("📝 이름 생성")]
    [Tooltip("접두사 목록")]
    public string[] prefixes = {
        "강화된", "마법의", "신성한", "악마의", "고대의", "미래의",
        "얼음", "불꽃", "번개", "어둠", "빛", "자연의",
        "전사의", "마법사의", "도적의", "성기사의", "사신의", "천사의"
    };
    
    [Tooltip("접미사 목록")]
    public string[] suffixes = {
        "의 보호", "의 힘", "의 지혜", "의 용기", "의 속도", "의 생명",
        "의 견고함", "의 민첩함", "의 지속력", "의 회복", "의 저항", "의 축복"
    };
    
    [Header("🎨 시각적 효과")]
    [Tooltip("생성 시 파티클 효과")]
    public GameObject generationEffect;
    
    [Tooltip("생성 시 사운드")]
    public AudioClip generationSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    // 랜덤 방어구 생성
    public ArmorData GenerateRandomArmor()
    {
        if (armorTemplates == null || armorTemplates.Length == 0)
        {
            Debug.LogError("❌ [ArmorGenerator] 방어구 템플릿이 설정되지 않았습니다!");
            return null;
        }
        
        // 랜덤 템플릿 선택
        ArmorData template = armorTemplates[Random.Range(0, armorTemplates.Length)];
        
        // 새로운 방어구 데이터 생성
        ArmorData newArmor = ScriptableObject.CreateInstance<ArmorData>();
        
        // 기본 정보 복사
        newArmor.armorName = GenerateRandomName(template.armorName);
        newArmor.armorType = template.armorType;
        newArmor.icon = template.icon;
        newArmor.description = template.description;
        
        // 레어리티 결정
        newArmor.rarity = DetermineRarity();
        currentRarity = newArmor.rarity; // 현재 레어리티 설정
        
        // 능력치 랜덤화
        RandomizeStats(newArmor, template);
        
        // 특수 효과 랜덤화
        RandomizeSpecialEffects(newArmor, template);
        
        // 색상 설정
        SetRarityColor(newArmor); // 레어리티에 따른 색상 자동 설정
        
        return newArmor;
    }
    
    // 특정 타입의 랜덤 방어구 생성
    public ArmorData GenerateRandomArmorByType(ArmorType armorType)
    {
        // 해당 타입의 템플릿들 필터링
        List<ArmorData> typeTemplates = new List<ArmorData>();
        foreach (var template in armorTemplates)
        {
            if (template.armorType == armorType)
            {
                typeTemplates.Add(template);
            }
        }
        
        if (typeTemplates.Count == 0)
        {
            Debug.LogWarning($"⚠️ [ArmorGenerator] {armorType} 타입의 템플릿이 없습니다!");
            return GenerateRandomArmor(); // 전체에서 랜덤 생성
        }
        
        // 임시로 해당 타입의 템플릿만 사용
        ArmorData[] originalTemplates = armorTemplates;
        armorTemplates = typeTemplates.ToArray();
        
        ArmorData result = GenerateRandomArmor();
        
        // 원래 템플릿 복원
        armorTemplates = originalTemplates;
        
        return result;
    }
    
    // 레어리티 결정
    ArmorRarity DetermineRarity()
    {
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;
        
        cumulative += commonChance;
        if (random <= cumulative) return ArmorRarity.Common;
        
        cumulative += rareChance;
        if (random <= cumulative) return ArmorRarity.Rare;
        
        cumulative += epicChance;
        if (random <= cumulative) return ArmorRarity.Epic;
        
        return ArmorRarity.Legendary;
    }
    
    // 능력치 랜덤화
    void RandomizeStats(ArmorData newArmor, ArmorData template)
    {
        // 방어력 랜덤화
        float defenseMultiplier = 1f + Random.Range(-defenseVariation, defenseVariation);
        newArmor.defense = Mathf.RoundToInt(template.defense * defenseMultiplier);
        
        // 체력 보너스 랜덤화
        float healthMultiplier = 1f + Random.Range(-healthVariation, healthVariation);
        newArmor.maxHealth = Mathf.RoundToInt(template.maxHealth * healthMultiplier);
        
        // 이동속도 보너스 랜덤화
        float speedMultiplier = 1f + Random.Range(-speedVariation, speedVariation);
        newArmor.moveSpeedBonus = template.moveSpeedBonus * speedMultiplier;
        
        // 점프력 보너스 (고정값 또는 랜덤화)
        newArmor.jumpForceBonus = template.jumpForceBonus;
        
        // 대시 쿨다운 감소 (고정값 또는 랜덤화)
        newArmor.dashCooldownReduction = template.dashCooldownReduction;
        
        // 데미지 감소율 (고정값 또는 랜덤화)
        newArmor.damageReduction = template.damageReduction;
    }
    
    // 특수 효과 랜덤화
    void RandomizeSpecialEffects(ArmorData newArmor, ArmorData template)
    {
        // 레어리티에 따른 특수 효과 확률 증가
        float specialEffectChance = GetSpecialEffectChance(newArmor.rarity);
        
        // 체력 재생 효과
        if (Random.Range(0f, 1f) < specialEffectChance)
        {
            newArmor.hasRegeneration = true;
            newArmor.regenerationRate = Random.Range(0.5f, 2f);
        }
        else
        {
            newArmor.hasRegeneration = template.hasRegeneration;
            newArmor.regenerationRate = template.regenerationRate;
        }
        
        // 무적 시간 증가 효과
        if (Random.Range(0f, 1f) < specialEffectChance)
        {
            newArmor.hasInvincibilityFrame = true;
            newArmor.invincibilityBonus = Random.Range(0.1f, 0.5f);
        }
        else
        {
            newArmor.hasInvincibilityFrame = template.hasInvincibilityFrame;
            newArmor.invincibilityBonus = template.invincibilityBonus;
        }
    }
    
    // 레어리티별 특수 효과 확률
    float GetSpecialEffectChance(ArmorRarity rarity)
    {
        switch (rarity)
        {
            case ArmorRarity.Common: return 0.1f;   // 10%
            case ArmorRarity.Rare: return 0.3f;     // 30%
            case ArmorRarity.Epic: return 0.6f;     // 60%
            case ArmorRarity.Legendary: return 1f;  // 100%
            default: return 0.1f;
        }
    }
    
    // 랜덤 이름 생성
    string GenerateRandomName(string baseName)
    {
        string prefix = "";
        string suffix = "";
        
        // 레어리티에 따른 접두사/접미사 확률
        float prefixChance = GetPrefixChance();
        float suffixChance = GetSuffixChance();
        
        if (Random.Range(0f, 1f) < prefixChance && prefixes.Length > 0)
        {
            prefix = prefixes[Random.Range(0, prefixes.Length)] + " ";
        }
        
        if (Random.Range(0f, 1f) < suffixChance && suffixes.Length > 0)
        {
            suffix = " " + suffixes[Random.Range(0, suffixes.Length)];
        }
        
        return prefix + baseName + suffix;
    }
    
    // 레어리티별 접두사 확률
    float GetPrefixChance()
    {
        // 레어리티가 높을수록 접두사 확률 증가
        return 0.3f + (int)currentRarity * 0.2f;
    }
    
    // 레어리티별 접미사 확률
    float GetSuffixChance()
    {
        // 레어리티가 높을수록 접미사 확률 증가
        return 0.2f + (int)currentRarity * 0.15f;
    }
    
    // 현재 생성 중인 레어리티 (임시 변수)
    private ArmorRarity currentRarity;
    
    // 레어리티에 따른 색상 설정
    void SetRarityColor(ArmorData armor)
    {
        switch (armor.rarity)
        {
            case ArmorRarity.Common:
                armor.rarityColor = Color.white;
                break;
            case ArmorRarity.Rare:
                armor.rarityColor = Color.blue;
                break;
            case ArmorRarity.Epic:
                armor.rarityColor = new Color(0.5f, 0f, 1f); // 보라색
                break;
            case ArmorRarity.Legendary:
                armor.rarityColor = new Color(1f, 0.5f, 0f); // 주황색
                break;
        }
    }
    
    // 특정 위치에 방어구 생성
    public ArmorData GenerateArmorAtPosition(Vector3 position, ArmorType? specificType = null)
    {
        ArmorData armor;
        
        if (specificType.HasValue)
        {
            armor = GenerateRandomArmorByType(specificType.Value);
        }
        else
        {
            armor = GenerateRandomArmor();
        }
        
        if (armor != null)
        {
            // 시각적 효과
            if (generationEffect != null)
            {
                Instantiate(generationEffect, position, Quaternion.identity);
            }
            
            // 사운드 재생
            if (generationSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(generationSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(generationSound, position);
                }
            }
            
            Debug.Log($"🛡️ 방어구 생성: {armor.armorName} ({armor.GetRarityName()}) at {position}");
        }
        
        return armor;
    }
    
    // 방어구 픽업 아이템 생성
    public GameObject CreateArmorPickup(ArmorData armor, Vector3 position)
    {
        if (armor == null) return null;
        
        // ArmorPickup 프리팹 생성 (필요시 프리팹 참조 추가)
        GameObject pickupObj = new GameObject($"ArmorPickup_{armor.armorName}");
        
        // 🆕 바닥에 붙어서 나오도록 Y 위치 조정
        Vector3 groundPosition = new Vector3(position.x, position.y + 0.1f, position.z);
        pickupObj.transform.position = groundPosition;
        
        // ArmorPickup 컴포넌트 추가
        ArmorPickup pickup = pickupObj.AddComponent<ArmorPickup>();
        pickup.armorData = armor;
        
        // SpriteRenderer 추가
        SpriteRenderer spriteRenderer = pickupObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = armor.icon;
        spriteRenderer.color = armor.GetRarityColor();
        spriteRenderer.sortingOrder = 10; // 다른 오브젝트 위에 표시
        
        // 🆕 방어구 픽업 스케일을 0.25로 설정
        pickupObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        
        // 콜라이더 추가
        CircleCollider2D collider = pickupObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // 레이어 설정 (안전하게 처리)
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (pickupLayer != -1)
        {
            pickupObj.layer = pickupLayer;
        }
        else
        {
            // Pickup 레이어가 없으면 기본 레이어 사용
            pickupObj.layer = 0; // Default layer
            Debug.LogWarning("⚠️ [ArmorGenerator] 'Pickup' 레이어가 없습니다. Default 레이어를 사용합니다.");
        }
        
        return pickupObj;
    }
    
    // 랜덤 방어구 픽업 생성
    public GameObject CreateRandomArmorPickup(Vector3 position, ArmorType? specificType = null)
    {
        ArmorData armor = GenerateArmorAtPosition(position, specificType);
        return CreateArmorPickup(armor, position);
    }
} 