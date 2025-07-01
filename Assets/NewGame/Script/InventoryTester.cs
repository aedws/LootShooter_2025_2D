using UnityEngine;
using System.Collections.Generic;

public class InventoryTester : MonoBehaviour
{
    [Header("📋 테스터 사용법")]
    [TextArea(5, 8)]
    public string testerInstructions = "🎮 테스트 키 조작법:\n• F1: 랜덤 무기 추가\n• F2: 랜덤 무기 제거\n• F3: 모든 무기 제거\n• F4: 인벤토리 상태 콘솔 출력\n• F5: 무기 생성 도움말\n• I: 인벤토리 열기/닫기\n\n⚙️ 설정:\n1. sampleWeapons에 WeaponData 에셋 추가\n2. 게임 실행하면 자동으로 샘플 무기 추가";

    [Header("⚙️ Test Settings")]
    [Tooltip("게임 시작 시 샘플 무기들을 자동으로 추가할지 여부")]
    public bool addSampleWeaponsOnStart = true;
    
    [Tooltip("시작 시 추가할 무기 개수")]
    [Range(1, 20)]
    public int numberOfWeaponsToAdd = 10;
    
    [Header("🔫 Sample Weapon Assets")]
    [Tooltip("테스트용 WeaponData 에셋들을 여기에 추가하세요 (Create -> LootShooter -> WeaponData)")]
    public List<WeaponData> sampleWeapons = new List<WeaponData>();
    
    [Header("🔗 References (자동 연결)")]
    [Tooltip("플레이어 인벤토리 (자동으로 찾아서 연결됨)")]
    public PlayerInventory playerInventory;
    
    [Tooltip("인벤토리 매니저 (자동으로 찾아서 연결됨)")]
    public InventoryManager inventoryManager;
    
    void Start()
    {
        // 자동 연결
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        if (addSampleWeaponsOnStart)
        {
            AddSampleWeapons();
        }
    }
    
    void Update()
    {
        // 테스트 키들
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddRandomWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            RemoveRandomWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ClearAllWeapons();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            PrintInventoryStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreateTestWeapons();
        }
    }
    
    void AddSampleWeapons()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[InventoryTester] PlayerInventory를 찾을 수 없습니다!");
            return;
        }
        
        // 샘플 무기들이 있으면 추가
        if (sampleWeapons.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(numberOfWeaponsToAdd, sampleWeapons.Count); i++)
            {
                if (sampleWeapons[i] != null)
                {
                    playerInventory.AddWeapon(sampleWeapons[i]);
                    Debug.Log($"[InventoryTester] 샘플 무기 추가: {sampleWeapons[i].weaponName}");
                }
            }
        }
        else
        {
            Debug.Log("[InventoryTester] 샘플 무기가 없어서 테스트 무기를 생성합니다. F5를 누르세요.");
        }
    }
    
    void AddRandomWeapon()
    {
        if (sampleWeapons.Count == 0)
        {
            Debug.LogWarning("[InventoryTester] 추가할 샘플 무기가 없습니다!");
            return;
        }
        
        WeaponData randomWeapon = sampleWeapons[Random.Range(0, sampleWeapons.Count)];
        if (playerInventory != null && randomWeapon != null)
        {
            playerInventory.AddWeapon(randomWeapon);
            Debug.Log($"[InventoryTester] 랜덤 무기 추가: {randomWeapon.weaponName}");
        }
    }
    
    void RemoveRandomWeapon()
    {
        if (playerInventory == null) return;
        
        var weapons = playerInventory.GetWeapons();
        if (weapons.Count > 0)
        {
            WeaponData randomWeapon = weapons[Random.Range(0, weapons.Count)];
            playerInventory.RemoveWeapon(randomWeapon);
            Debug.Log($"[InventoryTester] 랜덤 무기 제거: {randomWeapon.weaponName}");
        }
        else
        {
            Debug.Log("[InventoryTester] 제거할 무기가 없습니다!");
        }
    }
    
    void ClearAllWeapons()
    {
        if (playerInventory == null) return;
        
        var weapons = new List<WeaponData>(playerInventory.GetWeapons());
        foreach (var weapon in weapons)
        {
            playerInventory.RemoveWeapon(weapon);
        }
        
        Debug.Log($"[InventoryTester] 모든 무기 제거 완료! ({weapons.Count}개)");
    }
    
    void PrintInventoryStatus()
    {
        if (playerInventory == null)
        {
            Debug.Log("[InventoryTester] PlayerInventory가 없습니다!");
            return;
        }
        
        var weapons = playerInventory.GetWeapons();
        Debug.Log($"=== 인벤토리 상태 ===");
        Debug.Log($"총 무기 개수: {weapons.Count}");
        Debug.Log($"인벤토리 가득참: {playerInventory.IsInventoryFull()}");
        Debug.Log($"장착된 무기: {(playerInventory.equippedWeapon != null ? playerInventory.equippedWeapon.weaponName : "None")}");
        
        foreach (var weapon in weapons)
        {
            Debug.Log($"- {weapon.weaponName} ({weapon.weaponType}) - Damage: {weapon.damage}");
        }
    }
    
    void CreateTestWeapons()
    {
        Debug.Log("[InventoryTester] 프로그래밍 방식으로 테스트 무기를 생성하는 것은 ScriptableObject의 특성상 런타임에서 제한적입니다.");
        Debug.Log("[InventoryTester] 에디터에서 WeaponData 애셋을 만들고 sampleWeapons 리스트에 추가해주세요.");
        Debug.Log("[InventoryTester] Create -> LootShooter -> WeaponData로 무기 데이터를 만들 수 있습니다.");
    }
    
    void OnGUI()
    {
        // 간단한 GUI 도우미
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 인벤토리 테스터 ===");
        GUILayout.Label("F1: 랜덤 무기 추가");
        GUILayout.Label("F2: 랜덤 무기 제거");
        GUILayout.Label("F3: 모든 무기 제거");
        GUILayout.Label("F4: 인벤토리 상태 출력");
        GUILayout.Label("F5: 테스트 무기 생성 도움말");
        GUILayout.Label("I: 인벤토리 열기/닫기");
        
        if (playerInventory != null)
        {
            GUILayout.Label($"무기 개수: {playerInventory.GetWeaponCount()}");
            GUILayout.Label($"장착 무기: {(playerInventory.equippedWeapon != null ? playerInventory.equippedWeapon.weaponName : "None")}");
        }
        
        GUILayout.EndArea();
    }
} 