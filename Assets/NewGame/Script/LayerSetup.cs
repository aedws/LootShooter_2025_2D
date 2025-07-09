using UnityEngine;

public class LayerSetup : MonoBehaviour
{
    [Header("🔧 레이어 설정")]
    [Tooltip("필요한 레이어들을 자동으로 설정합니다")]
    public bool setupLayersOnStart = true;
    
    void Start()
    {
        if (setupLayersOnStart)
        {
            SetupRequiredLayers();
        }
    }
    
    [ContextMenu("필요한 레이어 설정")]
    public void SetupRequiredLayers()
    {
        Debug.Log("🔧 필요한 레이어들을 설정합니다...");
        
        // PickupLayer 레이어 확인 및 설정
        if (LayerMask.NameToLayer("PickupLayer") == -1)
        {
            Debug.LogWarning("⚠️ 'PickupLayer' 레이어가 없습니다. Unity의 Layer 설정에서 추가해주세요.");
            Debug.Log("📋 Layer 설정 방법:");
            Debug.Log("   1. Edit → Project Settings → Tags and Layers");
            Debug.Log("   2. Layers 섹션에서 빈 슬롯에 'PickupLayer' 입력");
            Debug.Log("   3. Player 레이어도 확인 (기본값: 8)");
        }
        else
        {
            Debug.Log("✅ PickupLayer 레이어가 설정되어 있습니다.");
        }
        
        // Player 레이어 확인
        if (LayerMask.NameToLayer("Player") == -1)
        {
            Debug.LogWarning("⚠️ 'Player' 레이어가 없습니다. Unity의 Layer 설정에서 추가해주세요.");
        }
        else
        {
            Debug.Log("✅ Player 레이어가 설정되어 있습니다.");
        }
        
        // 기본 레이어들 확인
        string[] requiredLayers = { "Default", "Player", "PickupLayer" };
        
        foreach (string layerName in requiredLayers)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                Debug.Log($"✅ {layerName} 레이어: {layerIndex}");
            }
            else
            {
                Debug.LogWarning($"⚠️ {layerName} 레이어가 없습니다!");
            }
        }
    }
    
    [ContextMenu("레이어 정보 출력")]
    public void PrintLayerInfo()
    {
        Debug.Log("📋 현재 레이어 정보:");
        
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                Debug.Log($"   Layer {i}: {layerName}");
            }
        }
    }
    
    [ContextMenu("PickupLayer 레이어 테스트")]
    public void TestPickupLayer()
    {
        int pickupLayer = LayerMask.NameToLayer("PickupLayer");
        if (pickupLayer != -1)
        {
            Debug.Log($"✅ PickupLayer 레이어 테스트 성공: {pickupLayer}");
            
            // 테스트 오브젝트 생성
            GameObject testObj = new GameObject("PickupLayerTest");
            testObj.layer = pickupLayer;
            Debug.Log($"✅ 테스트 오브젝트 생성됨: {testObj.name} (Layer: {testObj.layer})");
            
            // 3초 후 삭제
            Destroy(testObj, 3f);
        }
        else
        {
            Debug.LogError("❌ PickupLayer 레이어가 없습니다!");
        }
    }
} 