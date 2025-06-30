using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image iconImage; // 슬롯에 표시할 아이콘
    public WeaponData weaponData;   // 슬롯에 들어있는 무기 데이터

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetWeapon(WeaponData newWeaponData)
    {
        Debug.Log($"[SetWeapon] 슬롯에 무기 할당: {(newWeaponData != null ? newWeaponData.weaponName : "null")}");
        weaponData = newWeaponData;
        if (iconImage != null && weaponData != null)
            iconImage.sprite = weaponData.icon;
        else if (iconImage != null)
            iconImage.sprite = null;
    }

    public void ClearSlot()
    {
        Debug.Log($"[ClearSlot] 슬롯 비움");
        weaponData = null;
        if (iconImage != null)
            iconImage.sprite = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (weaponData == null) return;
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = originalPosition;
    }
} 