# Tower Knight 아키텍처

## 개요

Tower Knight는 모듈화되고 확장 가능한 아키텍처로 설계되었습니다. 각 시스템은 독립적으로 작동하며 느슨한 결합(Loose Coupling)을 유지합니다.

## 핵심 아키텍처 패턴

### 1. Singleton 패턴

게임의 핵심 매니저들은 Singleton으로 구현되어 어디서나 접근 가능합니다:

```csharp
// 전역 접근
GameManager.Instance.AddScore(100);
SaveManager.Instance.SavePlayerData(playerStats, "PlayerName");
NetworkGameManager.Instance.CreateRoom("RoomName");
```

**Singleton 사용 대상:**
- GameManager
- SaveManager
- RaidManager
- NetworkGameManager

### 2. 컴포넌트 기반 설계

플레이어와 적은 여러 컴포넌트로 구성됩니다:

```
PlayerGameObject
├── PlayerController (이동, 점프)
├── PlayerAttack (공격, 스킬)
├── PlayerStats (스탯, 경험치)
├── PlayerNetworkSync (네트워크 동기화)
├── Rigidbody2D (물리)
├── Animator (애니메이션)
└── SpriteRenderer (렌더링)
```

각 컴포넌트는 단일 책임 원칙(Single Responsibility Principle)을 따릅니다.

### 3. 이벤트 기반 통신

컴포넌트 간 통신은 주로 메서드 호출이나 Photon RPC를 통해 진행됩니다:

```csharp
// 데미지 처리
enemy.TakeDamage(damage);

// 네트워크 동기화
playerNetworkSync.SyncDamage(damage);
```

## 시스템 구조

### Phase 1: 핵심 게임루프

```
GameManager
├── GameplayUI
├── PlayerController (x4)
├── EnemyController (x N)
└── EnemySpawner
```

**책임:**
- 게임 상태 관리 (진행, 일시정지, 종료)
- 플레이어 등록/제거
- 점수 및 타이머 관리

### Phase 2: 전투 시스템

```
PlayerAttack
├── PlayerStats (스탯 계산)
├── EnemyController (데미지 적용)
├── DamageDisplay (UI 표시)
├── HitDetection (히트 판정)
└── EffectSpawner (파티클)
```

**책임:**
- 공격 메커니즘
- 데미지 계산
- 이펙트 생성

### Phase 3: 고급 기능

#### 3-1. 보스 시스템

```
BossController
├── BasicAttack
├── AreaAttack
├── BeamAttack
└── Heal
```

**특징:**
- 패턴 기반 AI
- 다중 공격 타입
- 자가 치유 능력

#### 3-2. 레이드 시스템

```
RaidManager
├── BossController
├── 보상 계산
├── 난이도 조정
└── 통계 기록
```

#### 3-3. 네트워크 시스템

```
NetworkGameManager (Photon PUN 2)
├── PlayerNetworkSync
├── 방 관리
├── 플레이어 동기화
└── RPC 호출
```

#### 3-4. 저장/로드 시스템

```
SaveManager
├── PlayerPrefs 저장
├── 데이터 로드
└── 통계 기록
```

#### 3-5. 성능 최적화

```
PerformanceOptimizer
├── FPS 모니터링
├── 자동 최적화
└── 메모리 관리
```

## 데이터 흐름

### 게임 시작

```
1. GameManager 초기화
   ├── 현재 층 설정 (CurrentFloor = 1)
   ├── 점수 초기화 (Score = 0)
   └── 타이머 시작 (RemainingTime = 180s)

2. SaveManager 초기화
   └── 저장된 데이터 로드 (PlayerPrefs)

3. EnemySpawner 시작
   └── 주기적으로 적 생성

4. PlayerController 입력 처리
   └── 플레이어 조작

5. PlayerAttack 공격 처리
   ├── 적 탐지
   ├── 데미지 계산
   └── 이펙트 생성
```

### 멀티플레이 데이터 흐름

```
로컬 플레이어 입력
    ↓
PlayerController 처리
    ↓
PlayerNetworkSync.SyncLocalPlayerState()
    ↓
Photon RPC 호출
    ↓
다른 플레이어들에게 동기화
    ↓
UpdateRemotePlayer() 수신
    ↓
위치/상태 업데이트
```

## 성능 고려사항

### 메모리 최적화

1. **Object Pooling**
   - 총알, 이펙트 재사용
   - GC 할당 최소화

2. **적 초기화**
   - 레이캐스트 캐싱
   - 충돌 체크 최소화

3. **네트워크 최적화**
   - RPC 호출 빈도 제한
   - 데이터 압축 (선택적)

### CPU 최적화

1. **업데이트 빈도**
   - 매 프레임 필수 계산만 수행
   - 비용이 높은 계산은 캐싱

2. **렌더링 최적화**
   - 스프라이트 배칭
   - 동적 배칭 활성화

3. **물리 시뮬레이션**
   - 단순 2D 물리 사용
   - 고정 타임스텝

## 확장 가능성

### 새로운 기능 추가 방법

#### 1. 새로운 적 타입

```csharp
public class BurnerEnemy : EnemyController
{
    // 불을 내뿜는 적
    public override void Attack() { ... }
}
```

#### 2. 새로운 플레이어 클래스

```csharp
public class MagePlayer : PlayerController
{
    // 마법사 클래스
    public void CastSpell() { ... }
}
```

#### 3. 새로운 파워업

```csharp
public class PowerUp : MonoBehaviour
{
    public void OnPickup(PlayerController player) { ... }
}
```

## 의존성 주입

컴포넌트들은 필요한 참조를 Inspector에서 설정하거나 `GetComponent<>()`로 획득합니다:

```csharp
private void Awake()
{
    _rigidbody = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
    _spriteRenderer = GetComponent<SpriteRenderer>();
}
```

## 테스트 전략

### 단위 테스트

```csharp
[Test]
public void TestDamageCalculation()
{
    int damage = player.CalculateDamage(10, enemy);
    Assert.AreEqual(damage, 5); // (10 + 10) - 15 = 5
}
```

### 통합 테스트

```csharp
[Test]
public void TestPlayerAttackFlow()
{
    player.BasicAttack();
    Assert.AreEqual(enemy.CurrentHealth, enemy.MaxHealth - damage);
}
```

## 문제 해결

### 일반적인 이슈

1. **플레이어가 움직이지 않음**
   - Rigidbody2D 체크
   - Ground 레이어 설정 확인
   - 입력 키 확인

2. **적이 보이지 않음**
   - Prefab 설정 확인
   - Spawn Point 위치 확인
   - Canvas Z-order 확인

3. **네트워크 연결 실패**
   - Photon App ID 설정 확인
   - 인터넷 연결 확인
   - 방화벽 설정 확인

---

**Last Updated**: 2026-08-20
