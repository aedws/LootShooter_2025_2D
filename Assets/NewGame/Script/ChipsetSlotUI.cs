using UnityEngine;
using TMPro;

public class ChipsetSlotUI : MonoBehaviour
{
    public enum ItemType { None, Weapon, Armor, Player }

    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public ChipsetSlot[] chipsetSlots;
    public GameObject saveButton;
    public GameObject clearButton;

    [Header("현재 올려진 아이템 정보")]
    public ItemType currentType = ItemType.None;
    public WeaponData currentWeapon;
    public ArmorData currentArmor;
    public string[] playerEquippedChipsets = new string[6]; // 플레이어 칩셋 슬롯 (6칸 예시)

    public WeaponSlot linkedWeaponSlot; // 인스펙터에서 연결

    // 저장(완성) 버튼 클릭 시 호출될 이벤트
    public System.Action<WeaponData> OnWeaponSave;
    public System.Action<ArmorData> OnArmorSave;
    public System.Action<string[]> OnPlayerSave;

    void Awake()
    {
        if (linkedWeaponSlot != null)
        {
            linkedWeaponSlot.OnWeaponChanged += OnLinkedWeaponChanged;
        }
    }
    private void OnLinkedWeaponChanged(WeaponData weapon)
    {
        SetItem(weapon);
    }

    // 무기 올리기
    public void SetItem(WeaponData weapon)
    {
        currentType = ItemType.Weapon;
        currentWeapon = weapon;
        currentArmor = null;
        playerEquippedChipsets = null;
        UpdateUI();
    }
    // 방어구 올리기
    public void SetItem(ArmorData armor)
    {
        currentType = ItemType.Armor;
        currentWeapon = null;
        currentArmor = armor;
        playerEquippedChipsets = null;
        UpdateUI();
    }
    // 플레이어 칩셋 배열 올리기
    public void SetPlayerChipsets(string[] chipsets)
    {
        currentType = ItemType.Player;
        currentWeapon = null;
        currentArmor = null;
        playerEquippedChipsets = chipsets != null ? chipsets : new string[6];
        UpdateUI();
    }
    // 초기화
    public void ClearItem()
    {
        currentType = ItemType.None;
        currentWeapon = null;
        currentArmor = null;
        playerEquippedChipsets = null;
        UpdateUI();
    }
    // UI 갱신
    private void UpdateUI()
    {
        if (itemNameText != null)
        {
            if (currentType == ItemType.Weapon && currentWeapon != null)
                itemNameText.text = currentWeapon.weaponName;
            else if (currentType == ItemType.Armor && currentArmor != null)
                itemNameText.text = currentArmor.armorName;
            else if (currentType == ItemType.Player && playerEquippedChipsets != null)
                itemNameText.text = "Player";
            else
                itemNameText.text = "-";
        }
        SyncChipsetSlots();
    }
    // 칩셋 슬롯 UI 동기화
    private void SyncChipsetSlots()
    {
        if (chipsetSlots == null) return;
        string[] equippedChipsets = null;
        if (currentType == ItemType.Weapon && currentWeapon != null)
            equippedChipsets = currentWeapon.GetEquippedChipsetIds();
        else if (currentType == ItemType.Armor && currentArmor != null)
            equippedChipsets = currentArmor.GetEquippedChipsetIds();
        else if (currentType == ItemType.Player && playerEquippedChipsets != null)
            equippedChipsets = playerEquippedChipsets;
        else
            equippedChipsets = new string[chipsetSlots.Length];
        for (int i = 0; i < chipsetSlots.Length; i++)
        {
            chipsetSlots[i].parentSlotUI = this;
            chipsetSlots[i].SetSlotIndex(i);
            if (equippedChipsets != null && i < equippedChipsets.Length && !string.IsNullOrEmpty(equippedChipsets[i]))
            {
                object chipset = null;
                if (currentType == ItemType.Weapon)
                    chipset = GameDataRepository.Instance.GetWeaponChipsetById(equippedChipsets[i]);
                else if (currentType == ItemType.Armor)
                    chipset = GameDataRepository.Instance.GetArmorChipsetById(equippedChipsets[i]);
                else if (currentType == ItemType.Player)
                    chipset = GameDataRepository.Instance.GetPlayerChipsetById(equippedChipsets[i]);
                if (chipset != null)
                {
                    if (currentType == ItemType.Weapon)
                        chipsetSlots[i].EquipWeaponChipset((WeaponChipsetData)chipset);
                    else if (currentType == ItemType.Armor)
                        chipsetSlots[i].EquipArmorChipset((ArmorChipsetData)chipset);
                    else if (currentType == ItemType.Player)
                        chipsetSlots[i].EquipPlayerChipset((PlayerChipsetData)chipset);
                }
                else
                {
                    chipsetSlots[i].UnequipChipset();
                }
            }
            else
            {
                chipsetSlots[i].UnequipChipset();
            }
        }
    }
    // 저장(완성) 버튼 클릭 시 호출
    public void OnSaveButtonClicked()
    {
        if (currentType == ItemType.Weapon && currentWeapon != null)
            OnWeaponSave?.Invoke(currentWeapon);
        else if (currentType == ItemType.Armor && currentArmor != null)
            OnArmorSave?.Invoke(currentArmor);
        else if (currentType == ItemType.Player && playerEquippedChipsets != null)
            OnPlayerSave?.Invoke(playerEquippedChipsets);
    }
} 