using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class GoogleSheetsManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GoogleSheetsConfig config;
    
    // 이벤트
    public event Action<List<WeaponData>> OnWeaponsLoaded;
    public event Action<List<ArmorData>> OnArmorsLoaded;
    public event Action<string> OnError;
    
    private void Start()
    {
        // 설정이 없으면 자동으로 로드
        if (config == null)
        {
            config = GoogleSheetsConfig.Instance;
        }
        
        if (config == null)
        {
            Debug.LogError("GoogleSheetsConfig를 찾을 수 없습니다!");
            return;
        }
    }
    
    public void LoadWeapons()
    {
        if (string.IsNullOrEmpty(config.WeaponsSpreadsheetId))
        {
            OnError?.Invoke("무기 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchWeaponsData());
    }
    
    public void LoadArmors()
    {
        if (string.IsNullOrEmpty(config.ArmorsSpreadsheetId))
        {
            OnError?.Invoke("방어구 스프레드시트 ID가 설정되지 않았습니다.");
            return;
        }
        
        StartCoroutine(FetchArmorsData());
    }
    
    private IEnumerator FetchWeaponsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.WeaponsSpreadsheetId}/values/Sheet1!A:Z?key={apiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseWeaponsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"무기 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private IEnumerator FetchArmorsData()
    {
        string apiKey = config.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다.");
            yield break;
        }
        
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{config.ArmorsSpreadsheetId}/values/Sheet1!A:Z?key={apiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseArmorsData(request.downloadHandler.text);
            }
            else
            {
                OnError?.Invoke($"방어구 데이터 로드 실패: {request.error}");
            }
        }
    }
    
    private void ParseWeaponsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 2)
            {
                OnError?.Invoke("무기 데이터가 비어있습니다.");
                return;
            }
            
            List<WeaponData> weapons = new List<WeaponData>();
            
            // 첫 번째 행은 헤더이므로 건너뛰기
            for (int i = 1; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 6) // 필요한 컬럼 수 확인
                {
                    WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
                    weapon.weaponName = row[1];
                    weapon.damage = int.Parse(row[2]);
                    weapon.fireRate = float.Parse(row[3]);
                    weapon.flavorText = row[5];
                    
                    // 무기 타입 파싱 (기본값: AR)
                    if (System.Enum.TryParse<WeaponType>(row[0], out WeaponType weaponType))
                    {
                        weapon.weaponType = weaponType;
                    }
                    else
                    {
                        weapon.weaponType = WeaponType.AR; // 기본값
                    }
                    
                    weapons.Add(weapon);
                }
            }
            
            Debug.Log($"무기 데이터 로드 완료: {weapons.Count}개");
            OnWeaponsLoaded?.Invoke(weapons);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"무기 데이터 파싱 오류: {e.Message}");
        }
    }
    
    private void ParseArmorsData(string jsonData)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<GoogleSheetsResponse>(jsonData);
            if (response?.values == null || response.values.Count < 2)
            {
                OnError?.Invoke("방어구 데이터가 비어있습니다.");
                return;
            }
            
            List<ArmorData> armors = new List<ArmorData>();
            
            // 첫 번째 행은 헤더이므로 건너뛰기
            for (int i = 1; i < response.values.Count; i++)
            {
                var row = response.values[i];
                if (row.Count >= 5) // 필요한 컬럼 수 확인
                {
                    ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
                    armor.armorName = row[1];
                    armor.defense = int.Parse(row[2]);
                    armor.description = row[4];
                    
                    // 방어구 타입 파싱 (기본값: Chest)
                    if (System.Enum.TryParse<ArmorType>(row[3], out ArmorType armorType))
                    {
                        armor.armorType = armorType;
                    }
                    else
                    {
                        armor.armorType = ArmorType.Chest; // 기본값
                    }
                    
                    armors.Add(armor);
                }
            }
            
            Debug.Log($"방어구 데이터 로드 완료: {armors.Count}개");
            OnArmorsLoaded?.Invoke(armors);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"방어구 데이터 파싱 오류: {e.Message}");
        }
    }
}

[System.Serializable]
public class GoogleSheetsResponse
{
    public string range;
    public string majorDimension;
    public List<List<string>> values;
} 