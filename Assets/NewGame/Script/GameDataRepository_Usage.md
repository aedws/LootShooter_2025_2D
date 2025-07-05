# GameDataRepository 사용 가이드

## 개요

`GameDataRepository`는 게임의 모든 데이터(무기, 방어구, 보스 패턴 등)를 중앙에서 관리하는 싱글톤 시스템입니다. Google Sheets에서 데이터를 로드하고, 게임 전체에서 일관된 방식으로 데이터에 접근할 수 있도록 합니다.

## 주요 기능

- **중앙 집중식 데이터 관리**: 모든 게임 데이터를 한 곳에서 관리
- **Google Sheets 연동**: 실시간 데이터 업데이트 지원
- **이벤트 기반 시스템**: 데이터 로드 완료를 감지하여 UI 업데이트
- **폴백 시스템**: Google Sheets 접근 실패 시 로컬 데이터 사용
- **타입별 검색**: 무기 타입, 방어구 레어리티 등으로 필터링
- **랜덤 드롭 시스템**: 가중치 기반 랜덤 아이템 생성

## 기본 사용법

### 1. GameDataRepository 인스턴스 접근

```csharp
// 싱글톤 인스턴스로 접근
GameDataRepository repo = GameDataRepository.Instance;

// 데이터 로드 상태 확인
if (repo.IsAllDataLoaded)
{
    // 데이터 사용 가능
}
```

### 2. 이벤트 구독

```csharp
void Start()
{
    // 모든 데이터 로드 완료 이벤트
    GameDataRepository.Instance.OnAllDataLoaded += OnAllDataLoaded;
    
    // 개별 데이터 업데이트 이벤트
    GameDataRepository.Instance.OnWeaponsUpdated += OnWeaponsUpdated;
    GameDataRepository.Instance.OnArmorsUpdated += OnArmorsUpdated;
    GameDataRepository.Instance.OnBossPatternsUpdated += OnBossPatternsUpdated;
    
    // 오류 이벤트
    GameDataRepository.Instance.OnDataLoadError += OnDataLoadError;
}

void OnAllDataLoaded()
{
    Debug.Log("모든 데이터 로드 완료!");
    // UI 업데이트, 게임 시작 등
}

void OnWeaponsUpdated(List<WeaponData> weapons)
{
    Debug.Log($"무기 데이터 업데이트: {weapons.Count}개");
    // 무기 목록 UI 업데이트
}

void OnDestroy()
{
    // 이벤트 구독 해제 (중요!)
    if (GameDataRepository.Instance != null)
    {
        GameDataRepository.Instance.OnAllDataLoaded -= OnAllDataLoaded;
        GameDataRepository.Instance.OnWeaponsUpdated -= OnWeaponsUpdated;
        GameDataRepository.Instance.OnArmorsUpdated -= OnArmorsUpdated;
        GameDataRepository.Instance.OnBossPatternsUpdated -= OnBossPatternsUpdated;
        GameDataRepository.Instance.OnDataLoadError -= OnDataLoadError;
    }
}
```

## 데이터 조회 메서드

### 무기 데이터 조회

```csharp
var repo = GameDataRepository.Instance;

// 모든 무기 가져오기
List<WeaponData> allWeapons = repo.Weapons;

// 타입별 무기 조회
List<WeaponData> assaultRifles = repo.GetWeaponsByType(WeaponType.AR);
List<WeaponData> handguns = repo.GetWeaponsByType(WeaponType.HG);

// 이름으로 무기 찾기
WeaponData ak47 = repo.GetWeaponByName("AK-47");

// 랜덤 무기
WeaponData randomWeapon = repo.GetRandomWeapon();
WeaponData randomAR = repo.GetRandomWeaponByType(WeaponType.AR);
```

### 방어구 데이터 조회

```csharp
var repo = GameDataRepository.Instance;

// 모든 방어구 가져오기
List<ArmorData> allArmors = repo.Armors;

// 타입별 방어구 조회
List<ArmorData> helmets = repo.GetArmorsByType(ArmorType.Helmet);
List<ArmorData> chestArmors = repo.GetArmorsByType(ArmorType.Chest);

// 레어리티별 방어구 조회
List<ArmorData> legendaryArmors = repo.GetArmorsByRarity(ArmorRarity.Legendary);
List<ArmorData> epicArmors = repo.GetArmorsByRarity(ArmorRarity.Epic);

// 이름으로 방어구 찾기
ArmorData legendaryChest = repo.GetArmorByName("전설의 갑옷");

// 랜덤 방어구
ArmorData randomArmor = repo.GetRandomArmor();
ArmorData randomHelmet = repo.GetRandomArmorByType(ArmorType.Helmet);
ArmorData randomEpic = repo.GetRandomArmorByRarity(ArmorRarity.Epic);
```

### 보스 패턴 조회

```csharp
var repo = GameDataRepository.Instance;

// 모든 보스 패턴
List<BossAttackPattern> allPatterns = repo.BossPatterns;

// 이름으로 보스 패턴 찾기
BossAttackPattern giantZombie = repo.GetBossPatternByName("Giant Zombie");
```

## RandomDropSystem 사용법

### 기본 설정

```csharp
// RandomDropSystem 컴포넌트를 GameObject에 추가
// Inspector에서 설정:
// - Weapon Drop Prefab: 무기 드롭 프리팹
// - Armor Drop Prefab: 방어구 드롭 프리팹
// - Drop Chance: 드롭 확률 (0.3 = 30%)
// - Drop Radius: 드롭 반경
```

### 드롭 생성

```csharp
RandomDropSystem dropSystem = GetComponent<RandomDropSystem>();

// 랜덤 드롭 생성
dropSystem.CreateRandomDrop(transform.position);

// 특정 타입 무기 드롭
dropSystem.CreateWeaponDropByType(transform.position, WeaponType.AR);

// 특정 타입 방어구 드롭
dropSystem.CreateArmorDropByType(transform.position, ArmorType.Chest);

// 보스 드롭 (고급 아이템 확률 증가)
dropSystem.CreateBossDrop(transform.position);
```

## 실제 사용 예제

### 1. 적 처치 시 드롭

```csharp
public class Enemy : MonoBehaviour
{
    [SerializeField] private RandomDropSystem dropSystem;
    
    public void OnDeath()
    {
        // 적이 죽었을 때 랜덤 드롭 생성
        if (dropSystem != null)
        {
            dropSystem.CreateRandomDrop(transform.position);
        }
    }
}
```

### 2. 보스 처치 시 드롭

```csharp
public class BossEnemy : MonoBehaviour
{
    [SerializeField] private RandomDropSystem dropSystem;
    
    public void OnDeath()
    {
        // 보스가 죽었을 때 보스 드롭 생성
        if (dropSystem != null)
        {
            dropSystem.CreateBossDrop(transform.position);
        }
    }
}
```

### 3. 상자/보물상자에서 드롭

```csharp
public class TreasureChest : MonoBehaviour
{
    [SerializeField] private RandomDropSystem dropSystem;
    
    public void OpenChest()
    {
        // 상자 열기 시 특정 타입 아이템 드롭
        dropSystem.CreateWeaponDropByType(transform.position, WeaponType.SR);
        dropSystem.CreateArmorDropByType(transform.position, ArmorType.Chest);
    }
}
```

### 4. UI에서 무기 목록 표시

```csharp
public class WeaponListUI : MonoBehaviour
{
    [SerializeField] private Transform weaponListParent;
    [SerializeField] private GameObject weaponItemPrefab;
    
    void Start()
    {
        // 데이터 로드 완료 대기
        if (GameDataRepository.Instance.IsAllDataLoaded)
        {
            UpdateWeaponList();
        }
        else
        {
            GameDataRepository.Instance.OnWeaponsUpdated += OnWeaponsUpdated;
        }
    }
    
    void OnWeaponsUpdated(List<WeaponData> weapons)
    {
        UpdateWeaponList();
    }
    
    void UpdateWeaponList()
    {
        // 기존 아이템 제거
        foreach (Transform child in weaponListParent)
        {
            Destroy(child.gameObject);
        }
        
        // 새로운 무기 목록 생성
        var weapons = GameDataRepository.Instance.Weapons;
        foreach (var weapon in weapons)
        {
            GameObject item = Instantiate(weaponItemPrefab, weaponListParent);
            WeaponItemUI itemUI = item.GetComponent<WeaponItemUI>();
            if (itemUI != null)
            {
                itemUI.SetWeaponData(weapon);
            }
        }
    }
}
```

## 설정 및 초기화

### 1. GameDataRepository 설정

```csharp
// GameDataRepository GameObject 생성
// Inspector에서 설정:
// - Google Sheets Manager: GoogleSheetsManager 참조
// - Load Data On Start: 시작 시 자동 로드 여부
// - Use Local Data As Fallback: 폴백 사용 여부
// - Local Weapon Data: 로컬 무기 데이터 (폴백용)
// - Local Armor Data: 로컬 방어구 데이터 (폴백용)
```

### 2. Google Sheets 연동

```csharp
// GoogleSheetsManager 설정
// - API Key: Google Sheets API 키
// - Spreadsheet ID: 스프레드시트 ID
// - Weapons Sheet Name: 무기 시트 이름
// - Armors Sheet Name: 방어구 시트 이름
```

## 디버그 및 테스트

### 1. 데이터 상태 확인

```csharp
// GameDataRepository에서 우클릭 > "데이터 상태 출력"
// Console에서 로드된 데이터 개수 확인
```

### 2. 랜덤 드롭 테스트

```csharp
// RandomDropSystem에서 우클릭 > "테스트 드롭 생성"
// 또는 "테스트 보스 드롭"
```

### 3. 데이터 검색 테스트

```csharp
// GameDataExample 스크립트 사용
// - "무기 검색 테스트"
// - "방어구 검색 테스트"
// - "보스 패턴 검색 테스트"
```

## 주의사항

1. **이벤트 구독 해제**: OnDestroy에서 반드시 이벤트 구독을 해제하세요.
2. **데이터 로드 대기**: 데이터가 완전히 로드되기 전에 접근하지 마세요.
3. **null 체크**: 데이터가 없을 수 있으므로 항상 null 체크를 하세요.
4. **메모리 관리**: ScriptableObject 인스턴스는 자동으로 관리되지만, 대량 생성 시 주의하세요.

## 확장 가능성

- **새로운 데이터 타입 추가**: ItemData, SkillData 등
- **캐싱 시스템**: 자주 사용되는 데이터 캐싱
- **데이터 버전 관리**: 데이터 업데이트 감지
- **오프라인 모드**: 네트워크 없이도 작동
- **데이터 백업**: 로컬 저장소에 데이터 백업 