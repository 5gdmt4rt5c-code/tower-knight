using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어 네트워크 동기화
/// 위치, 애니메이션, 공격 정보를 네트워크로 전송합니다
/// </summary>
public class PlayerNetworkSync : MonoBehaviourPun
{
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private Animator _animator;

    // 동기화할 정보
    private Vector3 _networkPosition;
    private Vector3 _networkVelocity;
    private bool _networkIsGrounded;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerAttack = GetComponent<PlayerAttack>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 자신의 플레이어만 입력 처리
        if (!photonView.IsMine)
            return;

        // 로컬 플레이어 입력 처리
        HandleLocalInput();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            // 다른 플레이어의 위치 동기화
            UpdateRemotePlayerPosition();
            return;
        }

        // 로컬 플레이어 상태 동기화
        SyncLocalPlayerState();
    }

    /// <summary>
    /// 로컬 입력 처리
    /// </summary>
    private void HandleLocalInput()
    {
        // 플레이어 컨트롤러가 입력 처리
        // 별도의 입력 처리 로직이 필요하면 여기에 추가
    }

    /// <summary>
    /// 로컬 플레이어 상태 동기화
    /// </summary>
    private void SyncLocalPlayerState()
    {
        // 위치, 속도, 접지 상태 등을 네트워크로 전송
        photonView.RPC("UpdateRemotePlayer", RpcTarget.Others,
            transform.position,
            _playerController.IsGrounded,
            _playerController.CurrentHealth
        );
    }

    /// <summary>
    /// 원격 플레이어 상태 업데이트
    /// </summary>
    [PunRPC]
    private void UpdateRemotePlayer(Vector3 position, bool isGrounded, int health)
    {
        _networkPosition = position;
        _networkIsGrounded = isGrounded;

        // 위치 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 10f);
    }

    /// <summary>
    /// 원격 플레이어 위치 업데이트
    /// </summary>
    private void UpdateRemotePlayerPosition()
    {
        // 네트워크에서 받은 위치로 이동
        transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 5f);
    }

    /// <summary>
    /// 공격 동기화 (원격 플레이어에게 표시)
    /// </summary>
    public void SyncAttack(int damage, Vector3 attackPosition)
    {
        photonView.RPC("PlayAttackAnimation", RpcTarget.Others, attackPosition);
    }

    /// <summary>
    /// 공격 애니메이션 재생
    /// </summary>
    [PunRPC]
    private void PlayAttackAnimation(Vector3 attackPosition)
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 피해 동기화
    /// </summary>
    public void SyncDamage(int damage)
    {
        photonView.RPC("ReceiveDamage", RpcTarget.All, damage);
    }

    /// <summary>
    /// 데미지 수신
    /// </summary>
    [PunRPC]
    private void ReceiveDamage(int damage)
    {
        if (_playerController != null && _playerController.IsAlive)
        {
            _playerController.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 사망 동기화
    /// </summary>
    public void SyncDeath()
    {
        photonView.RPC("PlayerDied", RpcTarget.All);
    }

    /// <summary>
    /// 플레이어 사망
    /// </summary>
    [PunRPC]
    private void PlayerDied()
    {
        if (_playerController != null)
        {
            _playerController.Die();
        }
    }
}
