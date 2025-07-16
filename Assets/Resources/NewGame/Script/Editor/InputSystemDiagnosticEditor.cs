#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputSystemDiagnostic))]
public class InputSystemDiagnosticEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🔧 Input System 진단 도구", EditorStyles.boldLabel);
        
        var diagnostic = (InputSystemDiagnostic)target;

        EditorGUILayout.BeginVertical("box");

        // 상태 확인 버튼
        if (GUILayout.Button("📊 Input System 상태 확인 (F9)", GUILayout.Height(30)))
        {
            diagnostic.CheckInputSystemStatus();
        }

        // Input Actions 활성화 버튼
        if (GUILayout.Button("⚡ Input Actions 활성화 (F10)", GUILayout.Height(30)))
        {
            diagnostic.CheckAndEnableInputActions();
        }

        // EventSystem 진단 버튼
        if (GUILayout.Button("🔍 EventSystem 진단 (F11)", GUILayout.Height(30)))
        {
            diagnostic.DiagnoseEventSystem();
        }

        // 🆕 자동 수정 버튼 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🛠️ 자동 수정", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Input System 설정 자동 수정", GUILayout.Height(35)))
        {
            diagnostic.FixInputSystemSettings();
        }

        // 마우스 이벤트 테스트 버튼
        if (GUILayout.Button("🖱️ 마우스 이벤트 테스트 (F12)", GUILayout.Height(30)))
        {
            diagnostic.TestMouseEvents();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "💡 팁:\n" +
            "• F9: 상태 확인\n" +
            "• F10: Actions 활성화\n" +
            "• F11: EventSystem 진단\n" +
            "• F12: 마우스 이벤트 테스트\n\n" +
            "⚠️ Input Module 문제 발생 시 '자동 수정' 버튼을 클릭하세요!",
            MessageType.Info
        );
    }
}
#endif 