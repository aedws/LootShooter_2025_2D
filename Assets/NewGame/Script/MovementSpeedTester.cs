using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 🏃‍♂️ 무기별 이동속도 테스트 도구
/// F1~F6 키로 각 무기 타입의 이동속도를 빠르게 테스트할 수 있습니다.
/// </summary>
public class MovementSpeedTester : MonoBehaviour
{
    [Header("📋 이동속도 테스터 사용법")]
    [TextArea(4, 8)]
    public string instructions = "🎮 키보드 테스트 명령어:\n" +
                                "F1: 권총 (HG) 속도로 변경\n" +
                                "F2: 기관단총 (SMG) 속도로 변경\n" +
                                "F3: 돌격소총 (AR) 속도로 변경\n" +
                                "F4: 산탄총 (SG) 속도로 변경\n" +
                                "F5: 기관총 (MG) 속도로 변경\n" +
                                "F6: 저격총 (SR) 속도로 변경\n" +
                                "F7: 기본 속도로 복원\n" +
                                "F8: 모든 무기 타입 속도 정보 출력";

    [Header("🔗 References")]
    [Tooltip("테스트할 플레이어 컨트롤러 (자동으로 찾음)")]
    public PlayerController playerController;
    
    [Header("🎯 Test Settings")]
    [Tooltip("테스트 모드 활성화 (게임 중에만 작동)")]
    public bool enableTesting = true;
    
    [Tooltip("키 입력 로그 표시")]
    public bool showKeyLogs = true;
    
    private Dictionary<WeaponType, KeyCode> testKeys;
    private WeaponType? currentTestType = null;

    void Start()
    {
        // PlayerController 자동 찾기
        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("❌ [MovementSpeedTester] PlayerController를 찾을 수 없습니다!");
            this.enabled = false;
            return;
        }
        
        // 테스트 키 매핑 설정
        testKeys = new Dictionary<WeaponType, KeyCode>
        {
            { WeaponType.HG, KeyCode.F1 },
            { WeaponType.SMG, KeyCode.F2 },
            { WeaponType.AR, KeyCode.F3 },
            { WeaponType.SG, KeyCode.F4 },
            { WeaponType.MG, KeyCode.F5 },
            { WeaponType.SR, KeyCode.F6 }
        };
        
        Debug.Log("🏃‍♂️ [MovementSpeedTester] 초기화 완료! F1~F6으로 무기별 이동속도를 테스트하세요.");
        
        if (enableTesting)
        {
            LogAllSpeedInfo();
        }
    }

    void Update()
    {
        if (!enableTesting) return;
        
        // 무기 타입별 속도 테스트
        foreach (var kvp in testKeys)
        {
            if (Input.GetKeyDown(kvp.Value))
            {
                TestWeaponTypeSpeed(kvp.Key);
            }
        }
        
        // 기본 속도 복원 (F7)
        if (Input.GetKeyDown(KeyCode.F7))
        {
            RestoreDefaultSpeed();
        }
        
        // 모든 속도 정보 출력 (F8)
        if (Input.GetKeyDown(KeyCode.F8))
        {
            LogAllSpeedInfo();
        }
    }
    
    void TestWeaponTypeSpeed(WeaponType weaponType)
    {
        if (playerController == null) return;
        
        float recommendedSpeed = WeaponMovementHelper.GetRecommendedSpeedMultiplier(weaponType);
        
        // 가상의 WeaponData 생성하여 테스트
        WeaponData testWeaponData = ScriptableObject.CreateInstance<WeaponData>();
        testWeaponData.weaponType = weaponType;
        testWeaponData.weaponName = $"Test {weaponType}";
        testWeaponData.movementSpeedMultiplier = recommendedSpeed;
        
        // 플레이어 이동속도 업데이트
        playerController.UpdateMovementSpeed(testWeaponData);
        
        currentTestType = weaponType;
        
        if (showKeyLogs)
        {
            string korean = WeaponMovementHelper.GetWeaponTypeKorean(weaponType);
            string emoji = WeaponMovementHelper.GetWeaponTypeEmoji(weaponType);
            string effect = WeaponMovementHelper.GetSpeedEffectMessage(recommendedSpeed);
            
            Debug.Log($"🎮 [MovementSpeedTester] {emoji} {korean} 무기 속도로 변경: " +
                     $"{recommendedSpeed:F2} ({effect})");
        }
        
        // 임시 WeaponData 정리
        if (Application.isPlaying)
            Destroy(testWeaponData);
    }
    
    void RestoreDefaultSpeed()
    {
        if (playerController == null) return;
        
        // null을 전달하여 기본 속도로 복원
        playerController.UpdateMovementSpeed(null);
        currentTestType = null;
        
        if (showKeyLogs)
        {
            Debug.Log($"🏃‍♂️ [MovementSpeedTester] 기본 이동속도로 복원: {playerController.GetBaseMoveSpeed()}");
        }
    }
    
    void LogAllSpeedInfo()
    {
        Debug.Log("🏃‍♂️ [MovementSpeedTester] 현재 플레이어 이동속도 정보:");
        Debug.Log($"   기본 속도: {playerController.GetBaseMoveSpeed()}");
        Debug.Log($"   현재 속도: {playerController.GetCurrentMoveSpeed():F2}");
        
        if (currentTestType.HasValue)
        {
            string korean = WeaponMovementHelper.GetWeaponTypeKorean(currentTestType.Value);
            Debug.Log($"   테스트 중인 무기: {korean} ({currentTestType.Value})");
        }
        else
        {
            Debug.Log($"   테스트 중인 무기: 없음 (기본 속도)");
        }
        
        Debug.Log("");
        WeaponMovementHelper.LogAllSpeedProfiles();
    }
    
    // UI에서 호출할 수 있는 메서드들
    [ContextMenu("권총 속도 테스트")]
    public void TestHandgunSpeed() => TestWeaponTypeSpeed(WeaponType.HG);
    
    [ContextMenu("기관단총 속도 테스트")]
    public void TestSMGSpeed() => TestWeaponTypeSpeed(WeaponType.SMG);
    
    [ContextMenu("돌격소총 속도 테스트")]
    public void TestAssaultRifleSpeed() => TestWeaponTypeSpeed(WeaponType.AR);
    
    [ContextMenu("산탄총 속도 테스트")]
    public void TestShotgunSpeed() => TestWeaponTypeSpeed(WeaponType.SG);
    
    [ContextMenu("기관총 속도 테스트")]
    public void TestMachineGunSpeed() => TestWeaponTypeSpeed(WeaponType.MG);
    
    [ContextMenu("저격총 속도 테스트")]
    public void TestSniperRifleSpeed() => TestWeaponTypeSpeed(WeaponType.SR);
    
    [ContextMenu("기본 속도 복원")]
    public void RestoreDefaultSpeedContext() => RestoreDefaultSpeed();
    
    [ContextMenu("모든 속도 정보 출력")]
    public void LogAllSpeedInfoContext() => LogAllSpeedInfo();
    
    void OnGUI()
    {
        if (!enableTesting) return;
        
        // 화면 좌상단에 테스트 정보 표시
        GUI.color = Color.white;
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 12;
        
        string displayText = "🏃‍♂️ 이동속도 테스터\n";
        displayText += $"기본속도: {playerController.GetBaseMoveSpeed()}\n";
        displayText += $"현재속도: {playerController.GetCurrentMoveSpeed():F2}\n";
        
        if (currentTestType.HasValue)
        {
            string korean = WeaponMovementHelper.GetWeaponTypeKorean(currentTestType.Value);
            displayText += $"테스트중: {korean}\n";
        }
        else
        {
            displayText += "테스트중: 기본상태\n";
        }
        
        displayText += "\nF1-F6: 무기타입별 테스트\nF7: 기본속도 복원\nF8: 정보출력";
        
        GUI.Box(new Rect(10, 10, 200, 140), displayText, style);
    }
    
    void OnValidate()
    {
        // Inspector에서 설정 변경 시 자동으로 PlayerController 찾기
        if (playerController == null && Application.isPlaying)
            playerController = FindAnyObjectByType<PlayerController>();
    }
} 