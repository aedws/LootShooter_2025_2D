using UnityEngine;

public class WeaponPickup : MonoBehaviour, IItemPickup
{
    public WeaponData weaponData;

    public void OnPickup(GameObject picker)
    {
        Debug.Log("[WeaponPickup] OnPickup 호출됨");
        var inventory = picker.GetComponent<PlayerInventory>();
        Debug.Log($"[WeaponPickup] PlayerInventory {(inventory != null ? "존재" : "없음")}");
        Debug.Log($"[WeaponPickup] weaponData: {(weaponData != null ? weaponData.weaponName : "null")}");
        if (inventory != null && weaponData != null)
        {
            inventory.AddWeapon(weaponData);
        }
        Destroy(gameObject);
    }
} 