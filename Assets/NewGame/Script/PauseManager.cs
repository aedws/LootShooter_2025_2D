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

    void SaveGame()
    {
        // 새로운 GameSaveManager 사용
        GameSaveManager.Instance.SaveGame();
    }

    void LoadGame()
    {
        // 새로운 GameSaveManager 사용
        GameSaveManager.Instance.LoadGame();
    }

    void ResetSave()
    {
        // 새로운 GameSaveManager 사용
        GameSaveManager.Instance.DeleteSaveFile();
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

        // 무기
        var inventory = player.GetComponent<PlayerInventory>();
        string weaponInfo = "무기: 없음";
        if (inventory != null)
        {
            var weapons = inventory.GetWeapons();
            var equippedWeapon = inventory.GetEquippedWeapon();
            weaponInfo = $"무기: {weapons.Count}개 보유";
            if (equippedWeapon != null)
                weaponInfo += $" (장착: {equippedWeapon.weaponName})";
        }

        // 방어구
        string armorInfo = "방어구: 없음";
        if (inventory != null)
        {
            var armors = inventory.GetAllEquippedArmors();
            int equippedCount = armors.Values.Count(a => a != null);
            armorInfo = $"방어구: {equippedCount}개 장착";
        }

        // 칩셋 (ChipsetManager에서 가져오기)
        string chipsetInfo = "칩셋: 정보 없음";
        var chipsetManager = FindFirstObjectByType<ChipsetManager>();
        if (chipsetManager != null)
        {
            // 칩셋 정보 표시 (실제 구현은 ChipsetManager 구조에 따라 조정 필요)
            chipsetInfo = "칩셋: 정보 로드 중";
        }

        // 구글 시트 데이터 상태
        string dataStatus = "데이터: 로드 중";
        if (GameDataRepository.Instance != null && GameDataRepository.Instance.IsAllDataLoaded)
        {
            dataStatus = "데이터: 로드 완료";
        }

        return $"=== 플레이어 스탯 ===\n" +
               $"체력: {hp}/{maxHp}\n" +
               $"{weaponInfo}\n" +
               $"{armorInfo}\n" +
               $"{chipsetInfo}\n" +
               $"{dataStatus}\n" +
               $"==================";
    }
} 