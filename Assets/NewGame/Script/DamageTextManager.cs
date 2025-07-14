using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageTextManager : MonoBehaviour
{
    [Header("Damage Text Settings")]
    public GameObject damageTextPrefab;
    public TMP_FontAsset damageFont;
    public int poolSize = 20;
    
    [Header("Text Settings")]
    public float fontSize = 20f;
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.red;
    public float textLifetime = 1.5f;
    public float moveSpeed = 1f;
    public float fadeSpeed = 1f;
    
    private Queue<GameObject> textPool;
    private static DamageTextManager instance;
    
    public static DamageTextManager Instance
    {
        get
        {
            if (instance == null)
            {
            instance = FindAnyObjectByType<DamageTextManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DamageTextManager");
                    instance = go.AddComponent<DamageTextManager>();
                }
            }
            return instance;
        }
    }
    
    // 기존 코드 호환성을 위한 메서드
    public static DamageTextManager GetInstance()
    {
        return Instance;
    }
    
    // 기존 코드 호환성을 위한 정적 메서드
    public static void ShowDamage(Vector3 position, int damage, bool isCritical = false, bool isHeal = false, bool isPlayerDamage = false)
    {
        Instance.ShowDamageText(damage, position, isCritical);
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePool()
    {
        textPool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewTextObject();
        }
    }
    
    private void CreateNewTextObject()
    {
        GameObject textObj;
        
        if (damageTextPrefab != null)
        {
            textObj = Instantiate(damageTextPrefab, transform);
        }
        else
        {
            // 프리팹이 없으면 동적 생성
            textObj = new GameObject("DamageText");
            textObj.transform.SetParent(transform);
            
            // TextMeshPro 컴포넌트 추가
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.fontSize = fontSize;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontStyle = FontStyles.Bold;
            
            // 폰트 적용
            if (damageFont != null)
            {
                textMesh.font = damageFont;
            }
            
            // DamageText 컴포넌트 추가
            textObj.AddComponent<DamageText>();
        }
        
        DamageText damageText = textObj.GetComponent<DamageText>();
        
        if (damageText != null)
        {
            damageText.Initialize(this);
        }
        
        textObj.SetActive(false);
        textPool.Enqueue(textObj);
    }
    
    public void ShowDamageText(int damage, Vector3 position, bool isCritical = false)
    {
        if (textPool.Count == 0)
        {
            CreateNewTextObject();
        }
        
        GameObject textObj = textPool.Dequeue();
        DamageText damageText = textObj.GetComponent<DamageText>();
        
        if (damageText != null)
        {
            damageText.ShowDamage(damage, position, isCritical);
        }
    }
    
    public void ReturnToPool(GameObject textObj)
    {
        textObj.SetActive(false);
        textPool.Enqueue(textObj);
    }
} 