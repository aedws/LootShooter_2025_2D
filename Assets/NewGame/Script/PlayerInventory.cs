using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();
    public WeaponData equippedWeapon; // 현재 장착된 무기
    public InventoryUI inventoryUI;
    public PlayerController playerController; // 플레이어 컨트롤러 참조
    public Transform weaponHolder; // Inspector에서 연결
    private GameObject currentWeaponObj;

    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    public void AddWeapon(WeaponData weapon)
    {
        // Debug.Log($"[AddWeapon] 무기 추가: {(weapon != null ? weapon.weaponName : "null")}, 현재 개수: {weapons.Count}, inventoryUI null? {inventoryUI == null}");
        
        if (!weapons.Contains(weapon))
        {
            weapons.Add(weapon);
            if (inventoryUI != null)
                inventoryUI.RefreshInventory(weapons);
        }
    }

    public void RemoveWeapon(WeaponData weapon)
    {
        // Debug.Log($"[RemoveWeapon] 무기 제거: {(weapon != null ? weapon.weaponName : "null")}, 현재 개수: {weapons.Count}, inventoryUI null? {inventoryUI == null}");
        
        if (weapons.Contains(weapon))
        {
            weapons.Remove(weapon);
            if (inventoryUI != null)
                inventoryUI.RefreshInventory(weapons);
        }
    }

    public void SetEquippedWeapon(WeaponData weaponData)
    {
        equippedWeapon = weaponData;

        // 기존 무기 오브젝트 파괴
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);

        // 새 무기 생성 및 장착
        if (weaponData != null && weaponData.weaponPrefab != null)
        {
            Vector3 prefabScale = weaponData.weaponPrefab.transform.localScale;
            currentWeaponObj = Instantiate(weaponData.weaponPrefab, weaponHolder);
            currentWeaponObj.transform.localPosition = Vector3.zero;
            currentWeaponObj.transform.localRotation = Quaternion.identity;
            currentWeaponObj.transform.localScale = prefabScale; // 프리팹 크기 유지
        }
    }

    public WeaponData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        // 기존 무기 해제
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);

        // 새 무기 생성 (월드 오브젝트에만)
        currentWeaponObj = Instantiate(weaponData.weaponPrefab, weaponHolder);
        // 필요시 위치/회전만 초기화 (scale은 건드리지 않음)
        currentWeaponObj.transform.localPosition = Vector3.zero;
        currentWeaponObj.transform.localRotation = Quaternion.identity;
    }

    public void UnequipWeapon()
    {
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);
    }

    // 현재 장착 무기 오브젝트 반환
    public GameObject GetCurrentWeaponObject()
    {
        return currentWeaponObj;
    }

    // 현재 장착 무기 Weapon 컴포넌트 반환
    public Weapon GetCurrentWeapon()
    {
        return currentWeaponObj != null ? currentWeaponObj.GetComponent<Weapon>() : null;
    }


} 