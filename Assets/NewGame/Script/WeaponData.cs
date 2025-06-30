using UnityEngine;

[CreateAssetMenu(menuName = "LootShooter/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public float fireRate;
    public int damage;
    public GameObject projectilePrefab;
    public GameObject weaponPrefab;
} 