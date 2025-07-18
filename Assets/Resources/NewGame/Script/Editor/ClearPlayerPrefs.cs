using UnityEditor;
using UnityEngine;

public class ClearPlayerPrefs
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs가 모두 삭제되었습니다.");
    }
}
