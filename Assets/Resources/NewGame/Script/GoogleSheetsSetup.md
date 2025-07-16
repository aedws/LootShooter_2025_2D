# 구글 스프레드시트 연동 설정 가이드

## 1. 구글 클라우드 콘솔 설정

### 1.1 프로젝트 생성
1. [Google Cloud Console](https://console.cloud.google.com/)에 접속
2. 새 프로젝트 생성 또는 기존 프로젝트 선택
3. 프로젝트 이름: `LootShooter-Data` (예시)

### 1.2 Google Sheets API 활성화
1. 왼쪽 메뉴에서 "API 및 서비스" > "라이브러리" 선택
2. "Google Sheets API" 검색 후 활성화

### 1.3 API 키 생성
1. "API 및 서비스" > "사용자 인증 정보" 선택
2. "사용자 인증 정보 만들기" > "API 키" 클릭
3. 생성된 API 키 복사 (나중에 Unity에서 사용)

## 2. 구글 스프레드시트 설정

### 2.1 스프레드시트 생성
1. [Google Sheets](https://sheets.google.com/)에서 새 스프레드시트 생성
2. 스프레드시트 이름: `LootShooter_GameData` (예시)

### 2.2 스프레드시트 ID 확인
- URL에서 스프레드시트 ID 복사
- 예: `https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit`
- ID: `1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms`

### 2.3 공유 설정
1. 우상단 "공유" 버튼 클릭
2. "링크가 있는 모든 사용자" 선택
3. 권한: "뷰어" 설정

## 3. 스프레드시트 구조

### 3.1 무기 데이터 시트 ("Weapons")
첫 번째 행은 헤더로 사용됩니다:

| weaponName | weaponType | flavorText | fireRate | damage | projectileSpeed | maxAmmo | currentAmmo | reloadTime | infiniteAmmo | baseSpread | maxSpread | spreadIncreaseRate | spreadDecreaseRate | pelletsPerShot | shotgunSpreadAngle | warmupTime | maxWarmupFireRate | singleFireOnly | aimingRange | movementSpeedMultiplier | recoilForce | recoilDuration | recoilRecoverySpeed | criticalChance | criticalMultiplier | pierceCount | pierceDamageReduction | hasTracerRounds | hasMuzzleFlash | hasExplosiveKills | explosionRadius |
|------------|------------|------------|----------|--------|-----------------|---------|-------------|------------|--------------|------------|-----------|-------------------|-------------------|----------------|-------------------|------------|------------------|----------------|-------------|------------------------|-------------|----------------|-------------------|----------------|-------------------|-------------|---------------------|-----------------|-----------------|-------------------|-----------------|
| AK-47 | AR | 강력한 돌격소총 | 0.1 | 25 | 15 | 30 | 30 | 2.5 | false | 2 | 8 | 1.5 | 2 | 1 | 0 | 0 | 0 | false | 0 | 0.9 | 1.2 | 0.1 | 5 | 0.05 | 2 | 0 | 0.1 | false | true | false | 0 |
| Desert Eagle | HG | 강력한 권총 | 0.3 | 45 | 12 | 7 | 7 | 1.8 | false | 1 | 3 | 0.5 | 1.5 | 1 | 0 | 0 | 0 | true | 0 | 1.0 | 1.5 | 0.15 | 3 | 0.1 | 2.5 | 0 | 0.2 | false | true | false | 0 |

### 3.2 방어구 데이터 시트 ("Armors")
첫 번째 행은 헤더로 사용됩니다:

| armorName | armorType | rarity | description | defense | maxHealth | damageReduction | moveSpeedBonus | jumpForceBonus | dashCooldownReduction | hasRegeneration | regenerationRate | hasInvincibilityFrame | invincibilityBonus |
|-----------|-----------|--------|-------------|---------|-----------|-----------------|----------------|----------------|---------------------|-----------------|------------------|---------------------|-------------------|
| 철갑옷 | chest | common | 기본 철갑옷 | 15 | 0 | 0.1 | 0 | 0 | 0 | false | 0 | false | 0 |
| 마법사 로브 | chest | rare | 마법 보호 로브 | 10 | 20 | 0.05 | 0.1 | 0 | 0.2 | true | 1 | false | 0 |
| 전설의 갑옷 | chest | legendary | 전설급 갑옷 | 30 | 50 | 0.2 | 0.05 | 0.1 | 0.5 | true | 2 | true | 0.5 |

## 4. Unity 설정

### 4.1 GoogleSheetsManager 컴포넌트 추가
1. 빈 GameObject 생성
2. `GoogleSheetsManager` 스크립트 추가
3. Inspector에서 설정:
   - **Spreadsheet ID**: 스프레드시트 ID 입력
   - **API Key**: 생성한 API 키 입력
   - **Weapons Sheet Name**: "Weapons" (기본값)
   - **Armors Sheet Name**: "Armors" (기본값)
   - **Load On Start**: true (게임 시작시 자동 로드)
   - **Cache Time**: 300 (5분 캐시)

### 4.2 이벤트 구독
다른 스크립트에서 데이터 로드 완료를 감지하려면:

```csharp
void Start()
{
    GoogleSheetsManager.OnWeaponsLoaded += OnWeaponsLoaded;
    GoogleSheetsManager.OnArmorsLoaded += OnArmorsLoaded;
    GoogleSheetsManager.OnError += OnError;
}

void OnWeaponsLoaded(List<WeaponData> weapons)
{
    Debug.Log($"무기 {weapons.Count}개 로드 완료");
    // 무기 데이터 처리
}

void OnArmorsLoaded(List<ArmorData> armors)
{
    Debug.Log($"방어구 {armors.Count}개 로드 완료");
    // 방어구 데이터 처리
}

void OnError(string error)
{
    Debug.LogError($"구글 시트 오류: {error}");
}
```

## 5. 테스트

### 5.1 데이터 로드 테스트
1. Unity 에디터에서 Play 모드 실행
2. Console 창에서 로그 확인:
   - 성공: `[GoogleSheetsManager] 무기 데이터 로드 완료: X개 행`
   - 실패: `[GoogleSheetsManager] 무기 데이터 로드 실패: [오류 메시지]`

### 5.2 일반적인 오류 해결
- **403 Forbidden**: API 키가 잘못되었거나 API가 비활성화됨
- **404 Not Found**: 스프레드시트 ID가 잘못되었거나 공유 설정 문제
- **400 Bad Request**: 시트 이름이 잘못되었거나 데이터 형식 문제

## 6. 보안 고려사항

### 6.1 API 키 보안
- API 키를 소스 코드에 직접 하드코딩하지 마세요
- Unity의 PlayerPrefs나 환경 변수 사용 고려
- 프로덕션에서는 서버를 통한 프록시 방식 권장

### 6.2 스프레드시트 접근 제한
- 필요한 경우 특정 이메일 주소만 접근 허용
- 읽기 전용 권한으로 설정

## 7. 성능 최적화

### 7.1 캐싱 전략
- `cacheTime` 설정으로 불필요한 API 호출 방지
- 게임 시작시 한 번만 로드하고 메모리에 저장

### 7.2 데이터 구조 최적화
- 필요한 컬럼만 포함
- 불필요한 공백이나 특수문자 제거
- 데이터 타입 일관성 유지 