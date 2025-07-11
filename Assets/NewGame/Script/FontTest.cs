using UnityEngine;
using TMPro;

public class FontTest : MonoBehaviour
{
    [Header("폰트 테스트")]
    public TMPro.TMP_FontAsset testFont;
    public TextMeshPro testTextMesh;
    
    void Start()
    {
        // DamageTextManager의 폰트 확인
        if (DamageTextManager.Instance != null)
        {
            Debug.Log($"DamageTextManager 폰트: {(DamageTextManager.Instance.damageFont != null ? DamageTextManager.Instance.damageFont.name : "없음")}");
        }
        
        // 테스트 텍스트에 폰트 적용
        if (testTextMesh != null && testFont != null)
        {
            testTextMesh.font = testFont;
            testTextMesh.text = "폰트 테스트";
            testTextMesh.fontSize = 10f;
            testTextMesh.ForceMeshUpdate();
            Debug.Log($"테스트 폰트 적용: {testFont.name}");
        }
    }
    
    void Update()
    {
        // F키로 폰트 테스트
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (testFont != null)
            {
                // 씬의 모든 TextMeshPro에 폰트 적용
                TextMeshPro[] allTextMeshes = FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);
                foreach (var tm in allTextMeshes)
                {
                    tm.font = testFont;
                    tm.ForceMeshUpdate();
                }
                Debug.Log($"모든 TextMeshPro에 폰트 적용: {testFont.name}");
            }
        }
    }
} 