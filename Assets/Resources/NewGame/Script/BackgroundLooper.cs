using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    public Transform cameraTransform;
    public float backgroundWidth = 20f; // 한 장의 월드 단위 길이
    public Transform[] backgrounds; // 2장 이상의 배경 오브젝트

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        foreach (var bg in backgrounds)
        {
            float diff = cameraTransform.position.x - bg.position.x;
            if (Mathf.Abs(diff) > backgroundWidth)
            {
                float direction = Mathf.Sign(diff);
                bg.position += new Vector3(backgroundWidth * backgrounds.Length * direction, 0, 0);
            }
        }
    }
} 