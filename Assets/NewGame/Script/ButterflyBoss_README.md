# 나비 보스 시스템 (Butterfly Boss System)

## 개요
나비 보스는 5페이즈 시스템을 가진 고급 보스 몬스터입니다. 중력의 영향을 받지 않고 자유롭게 날아다니며, 다양한 공격 패턴을 사용하여 플레이어를 공격합니다.

## 주요 특징

### 🦋 비행 시스템
- 중력 영향 없음 (Rigidbody2D.gravityScale = 0)
- 부드러운 비행 이동
- 날개짓 애니메이션
- 화면 전체를 자유롭게 이동

### ⚡ 5페이즈 시스템
1. **Phase 1** (100%~70%): 기본 패턴
2. **Phase 2** (69%~30%): 중급 패턴
3. **Phase 3** (29%~10%): 고급 패턴
4. **Phase 4** (10% 이하): 최종 패턴
5. **Phase 5** (0% 도달 시): 부활 시퀀스

### 🎯 공격 패턴

#### Phase 1 패턴
1. **빔 스윕**: 좌상단→우상단 이동하며 하향 빔 발사
2. **유도 투사체**: 플레이어 방향 투사체 30개 순차 발사
3. **하향 랜덤 투사체**: 하향 방향 랜덤 각도 투사체 발사
4. **랜덤 폭발**: 화면 랜덤 위치 폭발 1개 (사전 경고)

#### Phase 2 패턴
1. **랜덤 각도 투사체**: 중앙에서 360도 투사체 25개
2. **텔레포트 돌진**: 플레이어 X축 기준 텔레포트 후 돌진
3. **유도 투사체**: 플레이어 방향 투사체 30개
4. **하향 랜덤 투사체**: 하향 랜덤 각도 투사체

#### Phase 3 패턴
1. **좌상단→우하단**: 좌상단에서 우하단 방향 투사체 45개
2. **우상단→좌하단**: 우상단에서 좌하단 방향 투사체 45개
3. **좌측 광역 빔**: 좌측에서 우측 광역 빔 (1초)
4. **우측 광역 빔**: 우측에서 좌측 광역 빔 (1초)
5. **원형 호버링**: 플레이어 주변 원형 이동하며 투사체 발사

#### Phase 4 패턴
1. **투사체+폭발**: 랜덤 투사체 25개 + 동시 폭발 2개
2. **플레이어 방향+폭발**: 플레이어 방향 투사체 10개 + 폭발 2개
3. **Y축 텔레포트 돌진**: 플레이어 Y축 기준 텔레포트 돌진
4. **좌측 광역 빔**: 좌측 광역 빔 (2초)
5. **우측 광역 빔**: 우측 광역 빔 (2초)

#### Phase 5 (부활 시퀀스)
1. **붉어짐 효과**: 화면 중앙에서 붉어짐
2. **체력 재생**: HP 15%로 천천히 재생
3. **호버링 이동**: 우측 호버링 후 중앙 상단으로 이동
4. **Phase 4 복귀**: 부활 완료 후 Phase 4로 복귀

## 스크립트 구성

### 1. ButterflyBoss.cs (메인 보스 스크립트)
```csharp
// 주요 설정
public int maxHealth = 1000;
public float moveSpeed = 5f;
public float flightSpeed = 8f;
public float patternCooldown = 2f;

// 페이즈 임계값
public float phase1Threshold = 0.7f;  // 70%
public float phase2Threshold = 0.3f;  // 30%
public float phase3Threshold = 0.1f;  // 10%

// 프리팹 참조
public GameObject projectilePrefab1; // 투사체 프리팹_1
public GameObject projectilePrefab2; // 투사체 프리팹_2
public GameObject beamPrefab;        // 빔 프리팹
public GameObject explosionPrefab;   // 폭발 프리팹
public GameObject warningPrefab;     // 경고 표시 프리팹
```

### 2. ButterflyProjectile.cs (투사체 시스템)
```csharp
// 투사체 설정
public float speed = 15f;
public int damage = 15;
public float lifetime = 5f;
public bool isGuided = false;
public float guidedStrength = 2f;

// 시각 효과
public Color projectileColor = Color.magenta;
public float rotationSpeed = 180f;
public bool hasTrail = true;

// 특수 효과
public bool isExplosive = false;
public float explosionRadius = 2f;
```

### 3. ButterflyBeam.cs (빔 시스템)
```csharp
// 빔 설정
public float beamWidth = 2f;
public float beamLength = 20f;
public int damage = 25;
public float damageInterval = 0.1f;
public float lifetime = 3f;

// 빔 타입
public bool isWideBeam = false;
public bool isLeftBeam = false;
public bool isRightBeam = false;
public float wideBeamWidth = 5f;
```

### 4. ButterflyWarning.cs (경고 시스템)
```csharp
// 경고 설정
public float warningDuration = 1f;
public float blinkSpeed = 5f;
public Color warningColor = Color.red;

// 경고 타입
public bool isExplosionWarning = true;
public bool isBeamWarning = false;
public bool isChargeWarning = false;
```

### 5. ButterflyExplosion.cs (폭발 시스템)
```csharp
// 폭발 설정
public float explosionRadius = 3f;
public int explosionDamage = 30;
public float explosionForce = 10f;
public float explosionDuration = 1f;

// 시각 효과
public Color explosionColor = Color.orange;
public float maxScale = 3f;
```

### 6. ButterflyBossUI.cs (UI 시스템)
```csharp
// UI 참조
public Slider healthBar;
public TextMeshProUGUI healthText;
public TextMeshProUGUI phaseText;
public TextMeshProUGUI bossNameText;
public Image phaseIcon;

// 페이즈 색상
public Color phase1Color = Color.blue;
public Color phase2Color = Color.yellow;
public Color phase3Color = Color.orange;
public Color phase4Color = Color.red;
public Color phase5Color = Color.purple;
```

## 설정 방법

### 1. 보스 오브젝트 생성
1. 빈 GameObject 생성
2. ButterflyBoss 스크립트 추가
3. 필요한 컴포넌트 추가:
   - Rigidbody2D (gravityScale = 0)
   - Collider2D
   - SpriteRenderer
   - Animator (선택사항)
   - Health 컴포넌트

### 2. 프리팹 설정

#### 투사체 프리팹_1
```
- SpriteRenderer
- CircleCollider2D (isTrigger = true)
- ButterflyProjectile 스크립트
- TrailRenderer (선택사항)
```

#### 투사체 프리팹_2
```
- SpriteRenderer
- CircleCollider2D (isTrigger = true)
- ButterflyProjectile 스크립트
- 다른 색상/효과 설정
```

#### 빔 프리팹
```
- SpriteRenderer
- BoxCollider2D (isTrigger = true)
- ButterflyBeam 스크립트
```

#### 폭발 프리팹
```
- SpriteRenderer
- CircleCollider2D (isTrigger = true)
- ButterflyExplosion 스크립트
- AudioSource (선택사항)
```

#### 경고 프리팹
```
- SpriteRenderer
- ButterflyWarning 스크립트
```

### 3. UI 설정
1. Canvas 생성
2. UI 요소들 추가:
   - Slider (체력바)
   - TextMeshPro (체력 텍스트)
   - TextMeshPro (페이즈 텍스트)
   - TextMeshPro (보스 이름)
   - Image (페이즈 아이콘)
3. ButterflyBossUI 스크립트 추가
4. UI 참조 연결

### 4. Layer 설정
```
- Player: 플레이어 레이어
- Enemy: 보스 레이어
- Projectile: 투사체/빔/폭발 레이어
- UI: 경고 표시 레이어
```

## 사용 예시

### 보스 스폰
```csharp
// 보스 스폰
GameObject bossPrefab = Resources.Load<GameObject>("ButterflyBoss");
GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

// UI 연결
ButterflyBossUI bossUI = FindObjectOfType<ButterflyBossUI>();
if (bossUI != null)
{
    bossUI.SetBoss(boss.GetComponent<ButterflyBoss>());
}
```

### 이벤트 구독
```csharp
ButterflyBoss boss = FindObjectOfType<ButterflyBoss>();
if (boss != null)
{
    boss.OnPhaseChanged += (phase) => Debug.Log($"페이즈 변경: {phase}");
    boss.OnBossDeath += () => Debug.Log("보스 사망!");
    boss.OnBossRevival += () => Debug.Log("보스 부활!");
}
```

## 패턴 공통 사항

### 발동 기믹
- 모든 패턴은 랜덤으로 발동
- 한 번에 하나씩만 실행
- 반복 가능

### 휴식 시간
- Phase 1, 2: 패턴 완료 후 중앙에서 1초 휴식
- Phase 3, 4: 패턴 완료 후 중앙에서 0.3초 휴식

### 빔 데미지
- 빔은 해당 범위 내에 존재할 때만 데미지
- 지속 데미지 시스템

### Phase 5
- 1회만 진행
- 순서대로 실행 (붉어짐 → 재생 → 호버링 → Phase 4 복귀)

## 디버깅

### Gizmos 표시
- 화면 범위 (노란색)
- 플레이어까지의 선 (빨간색)
- 빔 범위 (빨간색 박스)
- 폭발 범위 (빨간색 원)
- 경고 범위 (빨간색)

### 로그 메시지
```
[ButterflyBoss] 페이즈 전환: Phase1 → Phase2
[ButterflyBoss] Phase 5 시작 - 부활 시퀀스
[ButterflyBossUI] 보스를 찾을 수 없습니다!
```

## 성능 최적화

### 오브젝트 풀링
- 투사체, 빔, 폭발 효과는 오브젝트 풀링 권장
- 대량의 투사체 생성 시 성능 향상

### Layer 설정
- 적절한 Layer 설정으로 불필요한 충돌 검사 방지
- UI 요소는 별도 Layer 사용

### 애니메이션 최적화
- 날개짓 애니메이션은 간단한 스프라이트 애니메이션 권장
- 복잡한 파티클 효과는 제한적으로 사용

## 확장 가능성

### 새로운 패턴 추가
1. ButterflyPattern enum에 새 패턴 추가
2. ExecutePattern 메서드에 새 케이스 추가
3. 패턴 구현 메서드 작성

### 새로운 페이즈 추가
1. ButterflyPhase enum에 새 페이즈 추가
2. 페이즈 전환 로직 수정
3. UI 시스템 업데이트

### 커스텀 효과
- 각 스크립트의 시각 효과 부분 수정
- 새로운 파티클 시스템 연동
- 커스텀 사운드 효과 추가

## 주의사항

1. **프리팹 참조**: 모든 프리팹이 올바르게 할당되어야 함
2. **Layer 설정**: 적절한 Layer 설정으로 충돌 검사 최적화
3. **성능**: 대량의 투사체 생성 시 성능 모니터링
4. **밸런싱**: 데미지, 체력, 패턴 타이밍 조정 필요
5. **테스트**: 모든 페이즈와 패턴을 충분히 테스트

## 버전 정보
- Unity 2022.3 LTS 이상 지원
- 2D URP 렌더 파이프라인 권장
- TextMeshPro 패키지 필요 