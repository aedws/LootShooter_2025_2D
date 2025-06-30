#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class ForceCompositeLink : MonoBehaviour
{
    void OnEnable()
    {
        var tilemapCollider = GetComponent<TilemapCollider2D>();
        var composite = GetComponent<CompositeCollider2D>();
        if (tilemapCollider != null && composite != null)
        {
            SerializedObject so = new SerializedObject(tilemapCollider);
            var prop = so.FindProperty("m_UsedByComposite");
            if (prop != null)
            {
                prop.boolValue = true;
                so.ApplyModifiedProperties();
                // Debug.Log("TilemapCollider2D의 Used By Composite를 강제로 true로 설정했습니다.");
            }
        }
    }
}
#endif 