using UnityEngine;

public class ArmorTemplates : MonoBehaviour
{
    [Header("🛡️ 기본 방어구 템플릿 생성")]
    [Tooltip("생성할 템플릿들을 저장할 폴더")]
    public string saveFolder = "Assets/NewGame/Prefab/ArmorData";
    
    [ContextMenu("기본 방어구 템플릿 생성")]
    public void CreateBasicArmorTemplates()
    {
        Debug.Log("🛡️ 기본 방어구 템플릿 생성 시작...");
        
        // 머리 방어구
        CreateArmorTemplate("Helmet_Basic", ArmorType.Helmet, "기본 헬멧", 5, 10, 0f, 
            "기본적인 보호를 제공하는 헬멧");
        
        // 상체 방어구
        CreateArmorTemplate("Chest_Basic", ArmorType.Chest, "기본 갑옷", 15, 20, -0.1f, 
            "무거우지만 강력한 보호를 제공하는 갑옷");
        
        // 하체 방어구
        CreateArmorTemplate("Legs_Basic", ArmorType.Legs, "기본 바지", 8, 5, 0.05f, 
            "가벼운 바지로 이동이 편리하다");
        
        // 신발
        CreateArmorTemplate("Boots_Basic", ArmorType.Boots, "기본 신발", 3, 0, 0.1f, 
            "편안한 신발로 이동속도가 증가한다");
        
        // 어깨
        CreateArmorTemplate("Shoulder_Basic", ArmorType.Shoulder, "기본 어깨보호대", 4, 5, 0f, 
            "어깨를 보호하는 장비");
        
        // 악세사리
        CreateArmorTemplate("Accessory_Basic", ArmorType.Accessory, "기본 반지", 2, 0, 0.02f, 
            "마법의 힘이 깃든 반지");
        
        Debug.Log("✅ 기본 방어구 템플릿 생성 완료!");
        Debug.Log("📁 생성된 파일들을 ArmorGenerator의 Armor Templates 배열에 할당하세요.");
    }
    
    void CreateArmorTemplate(string fileName, ArmorType armorType, string armorName, 
        int defense, int maxHealth, float moveSpeedBonus, string description)
    {
        // ScriptableObject 생성
        ArmorData armorData = ScriptableObject.CreateInstance<ArmorData>();
        
        // 기본 정보 설정
        armorData.armorName = armorName;
        armorData.armorType = armorType;
        armorData.rarity = ArmorRarity.Common;
        armorData.description = description;
        
        // 능력치 설정
        armorData.defense = defense;
        armorData.maxHealth = maxHealth;
        armorData.moveSpeedBonus = moveSpeedBonus;
        armorData.jumpForceBonus = 0f;
        armorData.dashCooldownReduction = 0f;
        armorData.damageReduction = 0f;
        
        // 특수 효과 (기본값)
        armorData.hasRegeneration = false;
        armorData.regenerationRate = 0f;
        armorData.hasInvincibilityFrame = false;
        armorData.invincibilityBonus = 0f;
        
        // 색상 설정
        SetRarityColor(armorData);
        
        // 파일로 저장
        string filePath = $"{saveFolder}/{fileName}.asset";
        
        // 폴더가 없으면 생성
        if (!System.IO.Directory.Exists(saveFolder))
        {
            System.IO.Directory.CreateDirectory(saveFolder);
        }
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(armorData, filePath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"🛡️ {fileName} 생성됨: {filePath}");
        #else
        Debug.Log($"🛡️ {fileName} 생성됨 (런타임에서는 파일 저장 불가)");
        #endif
    }
    
    [ContextMenu("희귀 방어구 템플릿 생성")]
    public void CreateRareArmorTemplates()
    {
        Debug.Log("🛡️ 희귀 방어구 템플릿 생성 시작...");
        
        // 희귀 머리 방어구
        CreateRareArmorTemplate("Helmet_Rare", ArmorType.Helmet, "강화된 헬멧", 8, 15, 0.05f, 
            "강화된 재료로 만든 헬멧");
        
        // 희귀 상체 방어구
        CreateRareArmorTemplate("Chest_Rare", ArmorType.Chest, "강화된 갑옷", 25, 35, -0.05f, 
            "강화된 갑옷으로 더 강력한 보호를 제공한다");
        
        // 희귀 하체 방어구
        CreateRareArmorTemplate("Legs_Rare", ArmorType.Legs, "강화된 바지", 12, 10, 0.1f, 
            "강화된 바지로 더 빠른 이동이 가능하다");
        
        // 희귀 신발
        CreateRareArmorTemplate("Boots_Rare", ArmorType.Boots, "강화된 신발", 5, 5, 0.15f, 
            "강화된 신발로 더 빠른 이동이 가능하다");
        
        // 희귀 어깨
        CreateRareArmorTemplate("Shoulder_Rare", ArmorType.Shoulder, "강화된 어깨보호대", 7, 10, 0.05f, 
            "강화된 어깨보호대로 더 강력한 보호를 제공한다");
        
        // 희귀 악세사리
        CreateRareArmorTemplate("Accessory_Rare", ArmorType.Accessory, "강화된 반지", 4, 5, 0.05f, 
            "강화된 마법 반지로 더 강력한 힘을 제공한다");
        
        Debug.Log("✅ 희귀 방어구 템플릿 생성 완료!");
    }
    
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
    
    void CreateRareArmorTemplate(string fileName, ArmorType armorType, string armorName, 
        int defense, int maxHealth, float moveSpeedBonus, string description)
    {
        // ScriptableObject 생성
        ArmorData armorData = ScriptableObject.CreateInstance<ArmorData>();
        
        // 기본 정보 설정
        armorData.armorName = armorName;
        armorData.armorType = armorType;
        armorData.rarity = ArmorRarity.Rare;
        armorData.description = description;
        
        // 능력치 설정
        armorData.defense = defense;
        armorData.maxHealth = maxHealth;
        armorData.moveSpeedBonus = moveSpeedBonus;
        armorData.jumpForceBonus = 0.1f;
        armorData.dashCooldownReduction = 0.1f;
        armorData.damageReduction = 0.05f;
        
        // 특수 효과 (희귀 등급)
        armorData.hasRegeneration = Random.Range(0f, 1f) < 0.3f; // 30% 확률
        armorData.regenerationRate = armorData.hasRegeneration ? Random.Range(0.5f, 1f) : 0f;
        armorData.hasInvincibilityFrame = Random.Range(0f, 1f) < 0.2f; // 20% 확률
        armorData.invincibilityBonus = armorData.hasInvincibilityFrame ? Random.Range(0.1f, 0.3f) : 0f;
        
        // 색상 설정
        SetRarityColor(armorData);
        
        // 파일로 저장
        string filePath = $"{saveFolder}/{fileName}.asset";
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(armorData, filePath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"🛡️ {fileName} 생성됨: {filePath}");
        #else
        Debug.Log($"🛡️ {fileName} 생성됨 (런타임에서는 파일 저장 불가)");
        #endif
    }
    
    [ContextMenu("모든 템플릿 삭제")]
    public void DeleteAllTemplates()
    {
        Debug.LogWarning("⚠️ 모든 방어구 템플릿을 삭제하시겠습니까?");
        
        #if UNITY_EDITOR
        if (UnityEditor.EditorUtility.DisplayDialog("방어구 템플릿 삭제", 
            "모든 방어구 템플릿을 삭제하시겠습니까?", "삭제", "취소"))
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ArmorData", new[] { saveFolder });
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                UnityEditor.AssetDatabase.DeleteAsset(path);
                Debug.Log($"🗑️ 삭제됨: {path}");
            }
            
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("✅ 모든 방어구 템플릿 삭제 완료!");
        }
        #else
        Debug.Log("런타임에서는 파일 삭제가 불가능합니다.");
        #endif
    }
} 