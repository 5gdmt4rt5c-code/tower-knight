# Tower Knight - 네트워크 스펙

## Photon PUN 2 통합

### 설정

1. **Photon PUN 2 패키지 설치**
   ```
   Window → TextMesh Pro → Import TMP Essential Resources
   Window → Photon PUN 2 → Highlight Server Settings
   ```

2. **Photon App ID 설정**
   ```
   Window → Photon PUN 2 → Highlight Server Settings
   AppID 입력 (photon.io에서 가입)
   ```

3. **플레이어 프리팹 설정**
   - NetworkGameManager에서 플레이어 프리팹 지정

## 네트워크 아키텍처

### 클라이언트-서버 모델

```
                    Photon Cloud
                         ↑
                         │
        ┌────────────────┼────────────────┐
        ↓                ↓                ↓
    Player 1        Player 2         Player 3
   (Master)       (User)           (User)
```

### 마스터 플레이어

- 게임 시작/종료
- 보스 스폰
- 게임 상태 관리

### 일반 플레이어

- 입력 처리
- 로컬 상태 업데이트
- 다른 플레이어에게 상태 전송

## 동기화 시스템

### 1. 위치 동기화

```csharp
public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        // 자신의 위치 전송
        stream.SendNext(transform.position);
        stream.SendNext(_rigidbody.velocity);
    }
    else
    {
        // 다른 플레이어의 위치 수신
        Vector3 position = (Vector3)stream.ReceiveNext();
        Vector2 velocity = (Vector2)stream.ReceiveNext();
        
        // 부드러운 이동
        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 5f);
    }
}
```

### 2. 공격 동기화

```csharp
// 공격 실행
public void BasicAttack()
{
    // 로컬 처리
    DealDamage();
    
    // 다른 플레이어에게 알림
    photonView.RPC("PlayAttackAnimation", RpcTarget.Others, position);
}

[PunRPC]
public void PlayAttackAnimation(Vector3 position)
{
    // 원격 플레이어의 공격 애니메이션
}
```

### 3. 상태 동기화

```csharp
[PunRPC]
public void SyncHealth(int health)
{
    _currentHealth = health;
    UpdateHealthBar();
}

[PunRPC]
public void SyncLevel(int level)
{
    _level = level;
    UpdateLevelDisplay();
}
```

## 네트워크 프로토콜

### 메시지 타입

| 메시지 | 빈도 | 크기 | 우선순위 |
|--------|------|------|--------|
| 위치 업데이트 | 매 프레임 | 24 bytes | 높음 |
| 공격 | 드문 | 16 bytes | 높음 |
| 데미지 | 드문 | 12 bytes | 높음 |
| 채팅 | 매우 드문 | 가변 | 낮음 |

### 대역폭 최적화

1. **업데이트 레이트**
   ```
   위치: 10 Hz (100ms)
   상태: 2 Hz (500ms)
   채팅: 실시간
   ```

2. **데이터 압축**
   ```csharp
   // 부동소수점 정밀도 감소
   Vector3 compressedPos = new Vector3(
       Mathf.Round(position.x, 2),
       Mathf.Round(position.y, 2),
       Mathf.Round(position.z, 2)
   );
   ```

## 멀티플레이 게임 흐름

### 1. 방 생성

```csharp
NetworkGameManager.Instance.CreateRoom("RoomName");
```

### 2. 방 진입

```csharp
NetworkGameManager.Instance.JoinRoom("RoomName");
// 또는
NetworkGameManager.Instance.JoinRandomRoom();
```

### 3. 게임 시작

```csharp
if (PhotonNetwork.IsMasterClient)
{
    NetworkGameManager.Instance.StartGame();
}
```

### 4. 게임 진행

```
로컬 입력 처리
    ↓
상태 업데이트
    ↓
RPC/OnPhotonSerializeView로 동기화
    ↓
모든 플레이어에서 동일한 상태 유지
```

### 5. 게임 종료

```csharp
NetworkGameManager.Instance.LeaveRoom();
```

## 문제 해결

### 위치 동기화 지연

**원인:** 네트워크 지연

**해결:**
```csharp
// 보간(Interpolation) 사용
targetPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * speed);
```

### RPC 호출 실패

**원인:** 타겟이 연결되지 않음

**해결:**
```csharp
// 타겟 확인
if (PhotonNetwork.IsConnected)
{
    photonView.RPC("Method", RpcTarget.Others);
}
```

### 방 진입 실패

**원인:** 만석 또는 네트워크 오류

**해결:**
```csharp
public override void OnJoinRoomFailed(short returnCode, string message)
{
    Debug.LogError($"방 진입 실패: {message}");
    NetworkGameManager.Instance.JoinRandomRoom();
}
```

## 보안 고려사항

### 1. 공격 검증

```csharp
[PunRPC]
public void TakeDamage(int damage)
{
    // 서버에서 데미지 검증
    if (ValidateDamage(damage))
    {
        _currentHealth -= damage;
    }
}
```

### 2. 스팸 방지

```csharp
private float _lastAttackTime = 0f;

public void Attack()
{
    if (Time.time - _lastAttackTime < 0.5f)
        return; // 쿨다운 적용
    
    _lastAttackTime = Time.time;
}
```

### 3. 핑 확인

```csharp
int ping = PhotonNetwork.GetPing();
if (ping > 200)
{
    Debug.LogWarning("네트워크 지연 높음");
}
```

## 테스트

### 로컬 테스트

```
1. 게임 2개 실행 (에디터 + 빌드)
2. 같은 방 진입
3. 플레이어 이동/공격 확인
```

### 온라인 테스트

```
1. 실제 Photon 계정으로 테스트
2. 다양한 네트워크 조건 시뮬레이션
3. 레이턴시 테스트
```

---

**Last Updated**: 2026-08-20
