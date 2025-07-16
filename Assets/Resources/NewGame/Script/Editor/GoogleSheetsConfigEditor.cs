using UnityEngine;
using UnityEditor;

public class GoogleSheetsConfigEditor : EditorWindow
{
    [MenuItem("Tools/Google Sheets/Create Config")]
    public static void CreateConfig()
    {
        // Resources 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // 설정 파일 생성
        GoogleSheetsConfig config = ScriptableObject.CreateInstance<GoogleSheetsConfig>();
        AssetDatabase.CreateAsset(config, "Assets/Resources/GoogleSheetsConfig.asset");
        AssetDatabase.SaveAssets();
        
        // 생성된 파일을 선택
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
        
        Debug.Log("GoogleSheetsConfig.asset 파일이 생성되었습니다. 이 파일에 API 키와 스프레드시트 ID를 입력해주세요.");
    }
} 