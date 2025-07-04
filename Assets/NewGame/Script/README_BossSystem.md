# 보스 몬스터 시스템 사용법

## 📋 개요
이 시스템은 Unity 2D 슈팅 게임을 위한 완전한 보스 몬스터 시스템입니다. 페이즈 시스템, 다양한 공격 패턴, 하수인 소환, 특수 능력 등을 포함합니다.

## 🎯 주요 기능

### 1. 페이즈 시스템
- **3단계 페이즈**: Phase1 → Phase2 → Phase3 (절망 모드)
- 체력에 따른 자동 페이즈 전환
- 페이즈별 공격력, 이동속도 증가
- 페이즈 전환 시 화면 흔들림 효과

### 2. 공격 시스템
- **6가지 공격 타입**:
  - `MeleeAttack`: 근접 공격
  - `RangedAttack`: 원거리 공격
  - `ChargeAttack`: 돌진 공격
  - `SummonMinions`: 하수인 소환
  - `AreaAttack`: 범위 공격
  - `SpecialAbility`: 특수 능력

### 3. 특수 능력
- **텔레포트**: 플레이어 근처로 순간이동
- **실드**: 일정 시간 무적 상태
- **하수인 소환**: 최대 3마리까지 소환 가능

### 4. 보상 시스템
- **확정 드롭**: 보스 사망 시 100% 확률로 드롭
- **희귀 드롭**: 낮은 확률로 특별한 아이템 드롭
- **경험치 보상**: 대량의 경험치 제공

## 🛠️ 설정 방법

### 1. 보스 몬스터 생성

#### 1-1. 보스 프리팹 만들기
```
1. 빈 GameObject 생성
2. 다음 컴포넌트 추가:
   - BossEnemy 스크립트
   - Health 컴포넌트
   - Rigidbody2D (Kinematic 권장)
   - Collider2D
   - SpriteRenderer
   - Animator (선택사항)
3. 프리팹으로 저장
```

#### 1-2. BossEnemy 설정
```csharp
[Header("보스 기본 정보")]
bossName = "드래곤 보스"
maxHealth = 500
moveSpeed = 2f
detectionRange = 20f

[Header("페이즈 시스템")]
phase1HealthThreshold = 0.7f  // 70%에서 페이즈 2
phase2HealthThreshold = 0.3f  // 30%에서 페이즈 3

[Header("공격 시스템")]
// 각 페이즈별 공격 목록 설정
phase1Attacks = [근접공격, 원거리공격]
phase2Attacks = [돌진공격, 하수인소환]
phase3Attacks = [범위공격, 특수능력]

[Header("특수 능력")]
canTeleport = true
teleportCooldown = 15f
canShield = true
shieldDuration = 5f
```

### 2. 보스 스폰 시스템 설정

#### 2-1. BossSpawner 설정
```csharp
[Header("보스 설정")]
bossPrefabs = [보스 프리팹들]
spawnConditions = [스폰 조건들]

[Header("스폰 조건")]
// 웨이브 기반 스폰
trigger = WaveCount
triggerValue = 10  // 10웨이브에서 스폰

// 시간 기반 스폰
trigger = TimeElapsed
triggerValue = 300  // 5분 후 스폰

// 수동 스폰
trigger = ManualTrigger
```

#### 2-2. 스폰 조건 설정
```csharp
// 웨이브 10에서 보스 스폰
BossSpawnCondition waveCondition = new BossSpawnCondition
{
    trigger = BossSpawnCondition.SpawnTrigger.WaveCount,
    triggerValue = 10,
    hasSpawned = false
};

// 5분 후 보스 스폰
BossSpawnCondition timeCondition = new BossSpawnCondition
{
    trigger = BossSpawnCondition.SpawnTrigger.TimeElapsed,
    triggerValue = 300,
    hasSpawned = false
};
```

### 3. 보스 헬스바 UI 설정

#### 3-1. UI 프리팹 생성
```
1. Canvas 하위에 UI 패널 생성
2. 다음 요소들 추가:
   - Slider (체력바)
   - Text (보스 이름)
   - Text (체력 수치)
   - Text (페이즈 표시)
   - Image (체력바 배경)
3. BossHealthBar 스크립트 추가
4. 프리팹으로 저장
```

#### 3-2. BossHealthBar 설정
```csharp
[Header("UI References")]
healthSlider = 체력바 슬라이더
bossNameText = 보스 이름 텍스트
healthText = 체력 수치 텍스트
phaseText = 페이즈 텍스트
healthFillImage = 체력바 이미지

[Header("Phase Colors")]
phase1Color = Color.green
phase2Color = Color.yellow
phase3Color = Color.red
```

### 4. 공격 패턴 설정

#### 4-1. BossAttackPattern 생성
```
1. Project 창에서 우클릭
2. Create → LootShooter → Boss Attack Pattern
3. 패턴 설정
```

#### 4-2. 패턴 설정 예시
```csharp
[Header("패턴 기본 정보")]
patternName = "드래곤 브레스 패턴"
targetPhase = BossPhase.Phase2
patternDuration = 15f
patternCooldown = 8f

[Header("공격 시퀀스")]
// 시퀀스 1: 브레스 공격
BossAttackSequence breathSequence = new BossAttackSequence
{
    sequenceName = "브레스 공격",
    attacks = [범위공격],
    sequenceDelay = 0f,
    repeatCount = 3
};

// 시퀀스 2: 하수인 소환
BossAttackSequence summonSequence = new BossAttackSequence
{
    sequenceName = "하수인 소환",
    attacks = [하수인소환],
    sequenceDelay = 2f,
    repeatCount = 1
};
```

## 🎮 사용 예시

### 1. 기본 보스 전투
```csharp
// BossSpawner에서 자동으로 보스 스폰
// 플레이어가 보스와 전투
// 체력에 따라 페이즈 자동 전환
// 보스 사망 시 보상 지급
```

### 2. 수동 보스 스폰
```csharp
BossSpawner spawner = FindObjectOfType<BossSpawner>();
spawner.SpawnBossManually(); // 디버그용 수동 스폰
```

### 3. 보스 상태 확인
```csharp
BossEnemy boss = FindObjectOfType<BossEnemy>();
if (boss != null)
{
    bool isAlive = boss.IsAlive();
    BossPhase phase = boss.GetCurrentPhase();
    float healthPercent = boss.GetHealthPercentage();
}
```

## 🔧 커스터마이징

### 1. 새로운 공격 타입 추가
```csharp
public enum BossAttackType
{
    // 기존 타입들...
    NewAttackType  // 새로운 공격 타입 추가
}

// BossEnemy.cs에서 새로운 공격 처리 추가
case BossAttackType.NewAttackType:
    ExecuteNewAttack(attack);
    break;
```

### 2. 새로운 특수 능력 추가
```csharp
// BossEnemy.cs에 새로운 능력 추가
public bool canNewAbility = false;
public float newAbilityCooldown = 20f;

// 능력 실행 메서드 추가
IEnumerator ActivateNewAbility()
{
    // 새로운 능력 로직
    yield return new WaitForSeconds(5f);
}
```

### 3. 페이즈별 효과 커스터마이징
```csharp
void ApplyPhaseEffects(BossPhase phase)
{
    switch (phase)
    {
        case BossPhase.Phase2:
            // 페이즈 2 효과
            moveSpeed *= 1.2f;
            // 새로운 효과 추가
            break;
        case BossPhase.Phase3:
            // 페이즈 3 효과
            moveSpeed *= 1.5f;
            // 새로운 효과 추가
            break;
    }
}
```

## 🐛 문제 해결

### 1. 보스가 스폰되지 않음
- `BossSpawner`의 `spawnConditions` 확인
- `bossPrefabs` 배열에 보스 프리팹이 할당되었는지 확인
- `BossEnemy` 컴포넌트가 프리팹에 있는지 확인

### 2. 보스가 공격하지 않음
- `phase1Attacks`, `phase2Attacks`, `phase3Attacks` 배열 확인
- `detectionRange` 값 확인
- `Health` 컴포넌트가 있는지 확인

### 3. 보스 헬스바가 표시되지 않음
- `BossHealthBar` 프리팹이 `BossSpawner`에 할당되었는지 확인
- Canvas가 있는지 확인
- UI 요소들이 올바르게 연결되었는지 확인

### 4. 페이즈 전환이 안됨
- `phase1HealthThreshold`, `phase2HealthThreshold` 값 확인
- `Health` 컴포넌트의 `OnDamaged` 이벤트가 연결되었는지 확인

## 📝 추가 기능 제안

### 1. 보스별 고유 능력
- 각 보스마다 고유한 특수 능력 추가
- 보스별 차별화된 전투 패턴

### 2. 보스 전투 환경
- 보스 전투 전용 맵
- 환경 위험 요소 (용암, 함정 등)

### 3. 보스 전투 보상 시스템
- 보스별 고유 아이템 드롭
- 보스 처치 업적 시스템

### 4. 멀티플레이어 보스 전투
- 여러 플레이어가 함께 보스와 전투
- 보스 체력 스케일링

## 🎯 성능 최적화 팁

1. **Object Pooling**: 투사체, 이펙트 등에 Object Pooling 사용
2. **LOD 시스템**: 거리에 따른 보스 렌더링 최적화
3. **이벤트 최적화**: 불필요한 이벤트 구독 해제
4. **메모리 관리**: 보스 사망 시 리소스 정리

이 시스템을 사용하여 멋진 보스 전투를 만들어보세요! 🎮 