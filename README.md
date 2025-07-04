# LootShooter - Google Sheets 연동

## API 키 설정 방법

### 1. Google Sheets API 키 생성
1. [Google Cloud Console](https://console.cloud.google.com/)에 접속
2. 새 프로젝트 생성 또는 기존 프로젝트 선택
3. Google Sheets API 활성화
4. 사용자 인증 정보에서 API 키 생성
5. API 키 제한 설정 (Google Sheets API만 허용)

### 2. Unity에서 설정 파일 생성
1. Unity 에디터에서 `Tools > Google Sheets > Create Config` 메뉴 선택
2. 생성된 `Assets/Resources/GoogleSheetsConfig.asset` 파일 선택
3. Inspector에서 다음 정보 입력:
   - **API Key**: Google Cloud Console에서 생성한 API 키
   - **Weapons Spreadsheet ID**: 무기 데이터가 있는 스프레드시트 ID
   - **Armors Spreadsheet ID**: 방어구 데이터가 있는 스프레드시트 ID

### 3. 스프레드시트 설정
1. Google Sheets에서 스프레드시트 생성
2. 스프레드시트를 "링크가 있는 모든 사용자가 볼 수 있음"으로 설정
3. URL에서 스프레드시트 ID 복사 (URL의 `/d/`와 `/edit` 사이의 문자열)

## 보안 주의사항

⚠️ **중요**: API 키가 포함된 설정 파일은 절대 Git에 커밋하지 마세요!

### 안전한 개발 방법:
1. **환경 변수 사용** (권장):
   ```bash
   # Windows
   set GOOGLE_SHEETS_API_KEY=your_api_key_here
   
   # macOS/Linux
   export GOOGLE_SHEETS_API_KEY=your_api_key_here
   ```

2. **로컬 설정 파일**:
   - `Assets/Resources/GoogleSheetsConfig.asset` 파일은 `.gitignore`에 포함됨
   - 각 개발자가 로컬에서만 설정 파일 생성

3. **팀 개발 시**:
   - `GoogleSheetsConfig.asset.example` 파일을 생성하여 형식만 공유
   - 실제 API 키는 개별적으로 설정

## 사용법

```csharp
// GoogleSheetsManager 컴포넌트가 있는 GameObject에서
GoogleSheetsManager sheetsManager = FindObjectOfType<GoogleSheetsManager>();

// 이벤트 구독
sheetsManager.OnWeaponsLoaded += (weapons) => {
    Debug.Log($"무기 {weapons.Count}개 로드됨");
    // 무기 데이터 사용
};

sheetsManager.OnError += (error) => {
    Debug.LogError($"오류: {error}");
};

// 데이터 로드
sheetsManager.LoadWeapons();
sheetsManager.LoadArmors();
```

## 문제 해결

### API 키 오류
- Google Cloud Console에서 API 키가 올바르게 생성되었는지 확인
- Google Sheets API가 활성화되었는지 확인
- API 키 제한 설정 확인

### 스프레드시트 접근 오류
- 스프레드시트가 공개로 설정되었는지 확인
- 스프레드시트 ID가 올바른지 확인
- 스프레드시트에 데이터가 있는지 확인

### 데이터 파싱 오류
- 스프레드시트 형식이 올바른지 확인 (첫 번째 행은 헤더)
- 필요한 컬럼이 모두 있는지 확인
- 데이터 타입이 올바른지 확인 (숫자 필드는 숫자만)

### 스프레드시트 형식

#### 무기 데이터 (Weapons)
| 컬럼 | 설명 | 예시 |
|------|------|------|
| A | 무기 타입 | AR, HG, MG, SG, SMG, SR |
| B | 무기 이름 | M4A1, Desert Eagle |
| C | 데미지 | 25 |
| D | 발사 속도 | 0.1 |
| E | (사용 안함) | - |
| F | 설명 | 강력한 돌격소총입니다. |

#### 방어구 데이터 (Armors)
| 컬럼 | 설명 | 예시 |
|------|------|------|
| A | (사용 안함) | - |
| B | 방어구 이름 | 강화 헬멧 |
| C | 방어력 | 15 |
| D | 방어구 타입 | Helmet, Chest, Legs, Boots, Shoulder, Accessory |
| E | 설명 | 머리를 보호하는 강화 헬멧입니다. | 