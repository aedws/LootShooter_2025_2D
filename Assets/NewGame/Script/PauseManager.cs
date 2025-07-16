using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic; // Added for List and Dictionary
using System.Linq; // Added for Select and Where

public class PauseManager : MonoBehaviour
{
    [Header("패널 및 UI 연결")]
    public GameObject pausePanel;
    public TextMeshProUGUI statText;
    public Button saveButton;
    public Button exitButton;
    public Button loadButton; // 불러오기 버튼
    public Button resetButton; // 초기화 버튼

    [Header("Press Esc 안내 UI")]
    public TextMeshProUGUI pressEscText; // Inspector에서 연결
    public float pressEscDuration = 2.5f; // 안내 표시 시간(초)

    private bool isPaused = false;
    private bool escHintActive = true;

    [System.Serializable]
    public class SaveData
    {
        public int hp;
        public int maxHp;
        // 무기
        public List<string> weaponNames = new List<string>();
        public string equippedWeaponName;
        // 방어구
        public List<string> armorNames = new List<string>();
        public Dictionary<string, string> equippedArmors = new Dictionary<string, string>(); // ArmorType, ArmorName
        // 칩셋 (Chipset 구조에 맞게 확장)
        public List<string> chipsetIds = new List<string>();
        public List<string> equippedChipsetIds = new List<string>();
    }

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);

        // Press Esc 안내 UI 표시
        if (pressEscText != null)
        {
            pressEscText.gameObject.SetActive(true);
            Invoke(nameof(HidePressEscHint), pressEscDuration);
        }
    }

    void HidePressEscHint()
    {
        if (pressEscText != null)
            pressEscText.gameObject.SetActive(false);
        escHintActive = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 안내 UI가 떠 있는 동안 ESC를 누르면 안내를 바로 숨기고 패널 오픈
            if (escHintActive)
            {
                HidePressEscHint();
                PauseGame();
            }
            else
            {
                if (!isPaused)
                    PauseGame();
                else
                    ResumeGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        // 스탯 정보 갱신
        if (statText != null)
            statText.text = GetPlayerStatString();
    }

    void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "save.json");
    }

    void SaveGame()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var health = player.GetComponent<Health>();
        var inventory = player.GetComponent<PlayerInventory>();

        SaveData data = new SaveData();
        data.hp = health != null ? health.currentHealth : 0;
        data.maxHp = health != null ? health.maxHealth : 0;

        if (inventory != null)
        {
            // 무기 전체 저장
            var weapons = inventory.GetWeapons();
            data.weaponNames = weapons.Select(w => w.weaponName).ToList();
            data.equippedWeaponName = inventory.equippedWeapon != null ? inventory.equippedWeapon.weaponName : null;

            // 방어구 전체 저장
            var armors = inventory.GetAllEquippedArmors();
            data.armorNames = armors.Values.Where(a => a != null).Select(a => a.armorName).ToList();
            data.equippedArmors = armors.Where(kv => kv.Value != null)
                                        .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.armorName);

            // 칩셋 전체 저장 (Chipset 구조에 맞게 확장)
            // 예시: inventory.chipsets, inventory.equippedChipsets 등
            // data.chipsetIds = inventory.chipsets.Select(c => c.chipsetId).ToList();
            // data.equippedChipsetIds = inventory.equippedChipsets.Select(c => c.chipsetId).ToList();
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("게임 저장 완료! " + GetSavePath());
    }

    void LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var health = player.GetComponent<Health>();
        var inventory = player.GetComponent<PlayerInventory>();

        if (health != null)
        {
            health.maxHealth = data.maxHp;
            health.currentHealth = data.hp;
        }

        if (inventory != null)
        {
            // 무기 복원
            inventory.ClearWeapons();
            foreach (var weaponName in data.weaponNames)
            {
                var weaponData = GameDataRepository.Instance.GetWeaponByName(weaponName);
                if (weaponData != null)
                    inventory.AddWeapon(weaponData);
            }
            if (!string.IsNullOrEmpty(data.equippedWeaponName))
            {
                var weaponData = GameDataRepository.Instance.GetWeaponByName(data.equippedWeaponName);
                if (weaponData != null)
                    inventory.SetEquippedWeapon(weaponData);
            }

            // 방어구 복원
            inventory.ClearArmors();
            foreach (var armorName in data.armorNames)
            {
                var armorData = GameDataRepository.Instance.GetArmorByName(armorName);
                if (armorData != null)
                    inventory.AddArmor(armorData);
            }
            if (data.equippedArmors != null)
            {
                foreach (var kv in data.equippedArmors)
                {
                    var armorData = GameDataRepository.Instance.GetArmorByName(kv.Value);
                    if (armorData != null)
                        inventory.SetEquippedArmor(armorData, (ArmorType)System.Enum.Parse(typeof(ArmorType), kv.Key));
                }
            }

            // 칩셋 복원 (Chipset 구조에 맞게 확장)
            // inventory.ClearChipsets();
            // foreach (var chipsetId in data.chipsetIds)
            // {
            //     var chipsetData = GameDataRepository.Instance.GetWeaponChipsetById(chipsetId);
            //     if (chipsetData != null)
            //         inventory.AddChipset(chipsetData);
            // }
            // foreach (var equippedId in data.equippedChipsetIds)
            // {
            //     var chipsetData = GameDataRepository.Instance.GetWeaponChipsetById(equippedId);
            //     if (chipsetData != null)
            //         inventory.EquipChipset(chipsetData);
            // }
        }

        Debug.Log("게임 불러오기 완료!");
    }

    void ResetSave()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("저장 파일이 삭제되었습니다.");
        }
        else
        {
            Debug.Log("저장 파일이 이미 없습니다.");
        }
    }

    void OnSaveClicked()
    {
        SaveGame();
    }

    void OnLoadClicked()
    {
        LoadGame();
    }

    void OnResetClicked()
    {
        ResetSave();
    }

    void OnExitClicked()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    string GetPlayerStatString()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return "플레이어 정보를 찾을 수 없습니다.";

        // 체력
        var health = player.GetComponent<Health>();
        int hp = health != null ? health.currentHealth : 0;
        int maxHp = health != null ? health.maxHealth : 0;

        // 무기 정보
        var inventory = player.GetComponent<PlayerInventory>();
        string weaponName = "-";
        int weaponDamage = 0;
        string weaponType = "-";
        float weaponCrit = 0f, weaponCritMul = 0f, weaponMoveSpeed = 1f;
        if (inventory != null && inventory.equippedWeapon != null)
        {
            var w = inventory.equippedWeapon;
            weaponName = w.weaponName;
            weaponDamage = w.damage;
            weaponType = w.weaponType.ToString();
            weaponCrit = w.criticalChance;
            weaponCritMul = w.criticalMultiplier;
            weaponMoveSpeed = w.movementSpeedMultiplier;
        }

        // 방어구 정보 (여러 부위 합산)
        float totalDefense = 0, totalMoveSpeed = 0, totalJump = 0, totalDash = 0, totalDmgReduce = 0, totalRegen = 0, totalInvincible = 0;
        if (inventory != null && inventory.equippedArmors != null)
        {
            foreach (var kv in inventory.equippedArmors)
            {
                var armor = kv.Value;
                if (armor == null) continue;
                totalDefense += armor.defense;
                totalMoveSpeed += armor.moveSpeedBonus;
                totalJump += armor.jumpForceBonus;
                totalDash += armor.dashCooldownReduction;
                totalDmgReduce += armor.damageReduction;
                if (armor.hasRegeneration) totalRegen += armor.regenerationRate;
                if (armor.hasInvincibilityFrame) totalInvincible += armor.invincibilityBonus;
            }
        }

        // 칩셋 등 추가 스탯도 여기에 합산 가능 (Chipset 구조 파악 후 추가)

        return
            $"HP: {hp} / {maxHp}\n" +
            $"무기: {weaponName} ({weaponType})\n" +
            $"무기 공격력: {weaponDamage}\n" +
            $"무기 크리티컬: {weaponCrit * 100f}% x{weaponCritMul}\n" +
            $"무기 이동속도 배수: {weaponMoveSpeed}\n" +
            $"방어구 총 방어력: {totalDefense}\n" +
            $"방어구 이동속도 보너스: {totalMoveSpeed}\n" +
            $"방어구 점프력 보너스: {totalJump}\n" +
            $"방어구 대시 쿨다운 감소: {totalDash}\n" +
            $"방어구 데미지 감소율: {totalDmgReduce * 100f}%\n" +
            $"방어구 체력 재생: {totalRegen}/초\n" +
            $"방어구 무적 시간 보너스: {totalInvincible}초\n";
    }
} 