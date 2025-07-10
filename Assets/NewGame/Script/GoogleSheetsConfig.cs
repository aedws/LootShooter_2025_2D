using UnityEngine;

[CreateAssetMenu(fileName = "GoogleSheetsConfig", menuName = "Google Sheets/Config")]
public class GoogleSheetsConfig : ScriptableObject
{
    [Header("Google Sheets API 설정")]
    [SerializeField] private string apiKey = "";
    [SerializeField] private string weaponsSpreadsheetId = "";
    [SerializeField] private string armorsSpreadsheetId = "";
    [SerializeField] private string dropTableSpreadsheetId = "";
    
    [Header("시트 이름 (탭 이름)")]
    [SerializeField] public string WeaponsSheetName = "Weapons";
    [SerializeField] public string ArmorsSheetName = "Armors";
    [SerializeField] public string MonsterInfoSheetName = "MonsterInfo";
    [SerializeField] public string ItemTypeDropRatesSheetName = "ItemTypeDropRates";
    [SerializeField] public string MonsterRarityDropRatesSheetName = "MonsterRarityDropRates";
    
    // 싱글톤 인스턴스
    private static GoogleSheetsConfig _instance;
    public static GoogleSheetsConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GoogleSheetsConfig>("GoogleSheetsConfig");
                if (_instance == null)
                {
                    Debug.LogError("GoogleSheetsConfig를 찾을 수 없습니다. Resources 폴더에 GoogleSheetsConfig.asset 파일을 생성해주세요.");
                }
            }
            return _instance;
        }
    }
    
    public string ApiKey => apiKey;
    public string WeaponsSpreadsheetId => weaponsSpreadsheetId;
    public string ArmorsSpreadsheetId => armorsSpreadsheetId;
    public string DropTableSpreadsheetId => dropTableSpreadsheetId;
    
    // 환경 변수에서 API 키를 가져오는 메서드 (개발 환경용)
    public string GetApiKey()
    {
        // 환경 변수에서 먼저 확인
        string envApiKey = System.Environment.GetEnvironmentVariable("GOOGLE_SHEETS_API_KEY");
        if (!string.IsNullOrEmpty(envApiKey))
        {
            return envApiKey;
        }
        
        // 설정 파일에서 가져오기
        return apiKey;
    }
} 