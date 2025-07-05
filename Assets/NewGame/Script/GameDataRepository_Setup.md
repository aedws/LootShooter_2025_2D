# Unity 설정 가이드

## 1. GameDataRepository 설정

### 1-1. GameDataRepository GameObject 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. 이름을 **"GameDataRepository"**로 변경
3. **GameDataRepository.cs** 스크립트를 추가
4. **DontDestroyOnLoad** 설정 (Inspector에서 체크)

### 1-2. 설정 옵션
- **Load Data On Start**: 시작 시 자동으로 데이터 로드 (권장: 체크)
- **Use Local Data As Fallback**: Google Sheets 실패 시 로컬 데이터 사용 (권장: 체크)
- **Local Weapon Data**: 로컬 무기 데이터 (폴백용)
- **Local Armor Data**: 로컬 방어구 데이터 (폴백용)

## 2. GoogleSheetsManager 설정

### 2-1. GoogleSheetsManager GameObject 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. 이름을 **"GoogleSheetsManager"**로 변경
3. **GoogleSheetsManager.cs** 스크립트를 추가

### 2-2. GoogleSheetsConfig 설정
1. **Project 창에서 우클릭** → **Create** → **ScriptableObject** → **GoogleSheetsConfig**
2. 이름을 **"GoogleSheetsConfig"**로 설정
3. **API Key**와 **Spreadsheet ID** 입력
4. **GoogleSheetsManager**의 **Config** 필드에 할당

## 3. RandomDropSystem 설정

### 3-1. RandomDropSystem GameObject 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. 이름을 **"RandomDropSystem"**로 변경
3. **RandomDropSystem.cs** 스크립트를 추가

### 3-2. 드롭 프리팹 설정
- **Weapon Drop Prefab**: 무기 드롭 프리팹 할당
- **Armor Drop Prefab**: 방어구 드롭 프리팹 할당

### 3-3. 드롭 설정
- **Enable Random Drops**: 랜덤 드롭 활성화 (권장: 체크)
- **Drop Chance**: 드롭 확률 (기본값: 0.3 = 30%)
- **Drop Radius**: 드롭 반경 (기본값: 2)
- **Weapon Drop Weight**: 무기 드롭 가중치 (기본값: 0.6 = 60%)

## 4. GameDataExample 설정 (테스트용)

### 4-1. GameDataExample GameObject 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. 이름을 **"GameDataExample"**로 변경
3. **GameDataExample.cs** 스크립트를 추가

### 4-2. UI 텍스트 연결
- **Status Text**: 상태 표시용 TextMeshPro
- **Weapons Text**: 무기 개수 표시용 TextMeshPro
- **Armors Text**: 방어구 개수 표시용 TextMeshPro

## 5. Resources 폴더 설정

### 5-1. BossPatterns 폴더 생성
1. **Assets/Resources** 폴더 생성 (없다면)
2. **Resources/BossPatterns** 폴더 생성
3. 보스 패턴 ScriptableObject들을 이 폴더에 저장

## 6. 프리팹 설정

### 6-1. 무기 드롭 프리팹
1. **WeaponPickup** 스크립트가 있는 GameObject 생성
2. **Collider2D** (IsTrigger = true) 추가
3. **Rigidbody2D** 추가 (Gravity Scale = 0)
4. 프리팹으로 저장

### 6-2. 방어구 드롭 프리팹
1. **ArmorPickup** 스크립트가 있는 GameObject 생성
2. **Collider2D** (IsTrigger = true) 추가
3. **Rigidbody2D** 추가 (Gravity Scale = 0)
4. 프리팹으로 저장

## 7. 실행 순서

### 7-1. 씬 설정
1. **GameDataRepository** (가장 먼저 로드)
2. **GoogleSheetsManager** (데이터 로드)
3. **RandomDropSystem** (드롭 시스템)
4. **GameDataExample** (테스트용, 선택사항)

### 7-2. 테스트 방법
1. **Play** 버튼 클릭
2. **Console** 창에서 로그 확인
3. **GameDataExample**의 **Context Menu** 사용하여 테스트

## 8. 주의사항

### 8-1. API 키 보안
- **GoogleSheetsConfig**는 **.gitignore**에 추가
- 환경 변수 사용 권장

### 8-2. 성능 최적화
- **GameDataRepository**는 **DontDestroyOnLoad**로 설정
- 불필요한 GameObject는 제거

### 8-3. 오류 처리
- **Console** 창에서 오류 메시지 확인
- **Network** 연결 상태 확인 (Google Sheets 사용 시) 