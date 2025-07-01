using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 🏃‍♂️ 무기 타입별 이동속도 영향 설정 헬퍼
/// WeaponData 생성 시 무기 타입에 맞는 권장 이동속도 배수를 자동으로 설정해줍니다.
/// </summary>
public static class WeaponMovementHelper
{
    [System.Serializable]
    public struct WeaponSpeedProfile
    {
        [Tooltip("권장 이동속도 배수")]
        public float recommendedMultiplier;
        
        [Tooltip("최소 이동속도 배수")]
        public float minMultiplier;
        
        [Tooltip("최대 이동속도 배수")]
        public float maxMultiplier;
        
        [Tooltip("속도 효과 설명")]
        public string description;
        
        public WeaponSpeedProfile(float recommended, float min, float max, string desc)
        {
            recommendedMultiplier = recommended;
            minMultiplier = min;
            maxMultiplier = max;
            description = desc;
        }
    }
    
    /// <summary>
    /// 무기 타입별 권장 이동속도 프로필
    /// </summary>
    public static readonly System.Collections.Generic.Dictionary<WeaponType, WeaponSpeedProfile> SpeedProfiles = 
        new System.Collections.Generic.Dictionary<WeaponType, WeaponSpeedProfile>
        {
            {
                WeaponType.HG, 
                new WeaponSpeedProfile(1.1f, 1.0f, 1.2f, "권총 - 가볍고 빠름")
            },
            {
                WeaponType.SMG, 
                new WeaponSpeedProfile(1.0f, 0.9f, 1.1f, "기관단총 - 기동성 중시")
            },
            {
                WeaponType.AR, 
                new WeaponSpeedProfile(0.85f, 0.8f, 0.9f, "돌격소총 - 균형잡힌 성능")
            },
            {
                WeaponType.SG, 
                new WeaponSpeedProfile(0.75f, 0.7f, 0.8f, "산탄총 - 무겁지만 강력함")
            },
            {
                WeaponType.MG, 
                new WeaponSpeedProfile(0.65f, 0.6f, 0.7f, "기관총 - 매우 무겁고 느림")
            },
            {
                WeaponType.SR, 
                new WeaponSpeedProfile(0.55f, 0.5f, 0.6f, "저격총 - 정밀하지만 둔중함")
            }
        };
    
    /// <summary>
    /// 무기 타입에 따라 권장 이동속도 배수를 반환합니다.
    /// </summary>
    /// <param name="weaponType">무기 타입</param>
    /// <returns>권장 이동속도 배수</returns>
    public static float GetRecommendedSpeedMultiplier(WeaponType weaponType)
    {
        if (SpeedProfiles.TryGetValue(weaponType, out WeaponSpeedProfile profile))
        {
            return profile.recommendedMultiplier;
        }
        
        Debug.LogWarning($"⚠️ [WeaponMovementHelper] 알 수 없는 무기 타입: {weaponType}, 기본값 1.0 반환");
        return 1.0f;
    }
    
    /// <summary>
    /// 무기 타입에 따라 속도 프로필을 반환합니다.
    /// </summary>
    /// <param name="weaponType">무기 타입</param>
    /// <returns>속도 프로필</returns>
    public static WeaponSpeedProfile GetSpeedProfile(WeaponType weaponType)
    {
        if (SpeedProfiles.TryGetValue(weaponType, out WeaponSpeedProfile profile))
        {
            return profile;
        }
        
        return new WeaponSpeedProfile(1.0f, 0.8f, 1.2f, "기본값");
    }
    
    /// <summary>
    /// WeaponData의 이동속도 배수를 무기 타입에 맞는 권장값으로 설정합니다.
    /// </summary>
    /// <param name="weaponData">설정할 무기 데이터</param>
    /// <param name="useRecommended">true면 권장값 사용, false면 현재값 유지</param>
    public static void ApplyRecommendedSpeed(WeaponData weaponData, bool useRecommended = true)
    {
        if (weaponData == null)
        {
            Debug.LogError("❌ [WeaponMovementHelper] WeaponData가 null입니다!");
            return;
        }
        
        if (useRecommended)
        {
            float recommendedSpeed = GetRecommendedSpeedMultiplier(weaponData.weaponType);
            weaponData.movementSpeedMultiplier = recommendedSpeed;
            
            Debug.Log($"🎯 [WeaponMovementHelper] {weaponData.weaponName}의 이동속도를 {weaponData.weaponType} 타입 권장값 {recommendedSpeed:F2}로 설정했습니다.");
        }
    }
    
    /// <summary>
    /// 모든 무기 타입별 속도 정보를 로그로 출력합니다.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogAllSpeedProfiles()
    {
        Debug.Log("🏃‍♂️ [WeaponMovementHelper] 무기 타입별 이동속도 프로필:");
        
        foreach (var kvp in SpeedProfiles)
        {
            WeaponType weaponType = kvp.Key;
            WeaponSpeedProfile profile = kvp.Value;
            
            string korean = GetWeaponTypeKorean(weaponType);
            string emoji = GetWeaponTypeEmoji(weaponType);
            
            Debug.Log($"  {emoji} {korean} ({weaponType}): {profile.recommendedMultiplier:F2} " +
                     $"(범위: {profile.minMultiplier:F2}~{profile.maxMultiplier:F2}) - {profile.description}");
        }
    }
    
    /// <summary>
    /// 무기 타입에 맞는 한국어 이름을 반환합니다.
    /// </summary>
    public static string GetWeaponTypeKorean(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HG: return "권총";
            case WeaponType.SMG: return "기관단총";
            case WeaponType.AR: return "돌격소총";
            case WeaponType.SG: return "산탄총";
            case WeaponType.MG: return "기관총";
            case WeaponType.SR: return "저격총";
            default: return "알 수 없는 무기";
        }
    }
    
    /// <summary>
    /// 무기 타입에 맞는 이모지를 반환합니다.
    /// </summary>
    public static string GetWeaponTypeEmoji(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HG: return "🔫";
            case WeaponType.SMG: return "🔫";
            case WeaponType.AR: return "🔫";
            case WeaponType.SG: return "💥";
            case WeaponType.MG: return "🔥";
            case WeaponType.SR: return "🎯";
            default: return "❓";
        }
    }
    
    /// <summary>
    /// 이동속도 배수에 따른 효과 메시지를 반환합니다.
    /// </summary>
    public static string GetSpeedEffectMessage(float multiplier)
    {
        if (multiplier >= 1.1f) return "🟢 매우 빠름";
        else if (multiplier >= 1.0f) return "🟢 빠름";
        else if (multiplier >= 0.9f) return "🟡 약간 빠름";
        else if (multiplier >= 0.8f) return "🟡 보통";
        else if (multiplier >= 0.7f) return "🟠 약간 느림";
        else if (multiplier >= 0.6f) return "🟠 느림";
        else return "🔴 매우 느림";
    }
}

#if UNITY_EDITOR
/// <summary>
/// 에디터용 WeaponData 확장 기능
/// </summary>
[UnityEditor.CustomEditor(typeof(WeaponData))]
public class WeaponDataMovementEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        WeaponData weaponData = (WeaponData)target;
        
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("🏃‍♂️ 이동속도 도우미", UnityEditor.EditorStyles.boldLabel);
        
        // 현재 무기 타입의 권장값 표시
        var profile = WeaponMovementHelper.GetSpeedProfile(weaponData.weaponType);
        string korean = WeaponMovementHelper.GetWeaponTypeKorean(weaponData.weaponType);
        string emoji = WeaponMovementHelper.GetWeaponTypeEmoji(weaponData.weaponType);
        string effect = WeaponMovementHelper.GetSpeedEffectMessage(weaponData.movementSpeedMultiplier);
        
        UnityEditor.EditorGUILayout.HelpBox(
            $"{emoji} {korean} 권장 이동속도: {profile.recommendedMultiplier:F2}\n" +
            $"허용 범위: {profile.minMultiplier:F2} ~ {profile.maxMultiplier:F2}\n" +
            $"현재 설정값: {weaponData.movementSpeedMultiplier:F2} ({effect})\n" +
            $"설명: {profile.description}", 
            UnityEditor.MessageType.Info
        );
        
        UnityEditor.EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button($"권장값 적용 ({profile.recommendedMultiplier:F2})"))
        {
            UnityEditor.Undo.RecordObject(weaponData, "Apply Recommended Movement Speed");
            weaponData.movementSpeedMultiplier = profile.recommendedMultiplier;
            UnityEditor.EditorUtility.SetDirty(weaponData);
        }
        
        if (GUILayout.Button("모든 프로필 보기"))
        {
            WeaponMovementHelper.LogAllSpeedProfiles();
        }
        
        UnityEditor.EditorGUILayout.EndHorizontal();
        
        // 범위 벗어남 경고
        if (weaponData.movementSpeedMultiplier < profile.minMultiplier || 
            weaponData.movementSpeedMultiplier > profile.maxMultiplier)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                $"⚠️ 현재 설정값이 {korean} 무기의 권장 범위를 벗어났습니다!\n" +
                $"권장 범위: {profile.minMultiplier:F2} ~ {profile.maxMultiplier:F2}", 
                UnityEditor.MessageType.Warning
            );
        }
    }
}
#endif 