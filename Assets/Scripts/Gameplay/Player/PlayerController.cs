using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 캐릭터 제어
/// 이동, 점프, 기본 애니메이션을 담당합니다
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField]
    private float _moveSpeed = 5f;

    [SerializeField]
    private float _jumpForce = 10f;

    [SerializeField]
    private float _groundDrag = 5f;

    [SerializeField]
    private float _airDrag = 2f;

    [Header("접지 감지")]
    [SerializeField]
    private float _groundCheckDistance = 0.1f;

    [SerializeField]
    private LayerMask _groundLayer;

    [Header("스탯")]
    [SerializeField]
    private int _maxHealth = 100;

    private int _currentHealth;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private bool _isGrounded = false;
    private bool _isAlive = true;
    private Vector2 _moveInput;
    private float _horizontalVelocity;

    // 애니메이션 해시
    private int _hashIsMoving = Animator.StringToHash("IsMoving");
    private int _hashIsJumping = Animator.StringToHash("IsJumping");
    private int _hashVelocityY = Animator.StringToHash("VelocityY");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_rigidbody == null)
            Debug.LogError("Rigidbody2D 컴포넌트가 없습니다!");
        if (_animator == null)
            Debug.LogError("Animator 컴포넌트가 없습니다!");
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        GameManager.Instance.RegisterPlayer(this);
    }

    private void Update()
    {
        if (!_isAlive)
            return;

        HandleInput();
        UpdateGroundCheck();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!_isAlive)
            return;

        ApplyMovement();
        ApplyDrag();
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // 좌우 이��� 입력
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontalInput = 1f;

        _moveInput.x = horizontalInput;

        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            Jump();
        }
    }

    /// <summary>
    /// 이동 적용
    /// </summary>
    private void ApplyMovement()
    {
        _horizontalVelocity = _moveInput.x * _moveSpeed;
        _rigidbody.velocity = new Vector2(_horizontalVelocity, _rigidbody.velocity.y);

        // 스프라이트 방향 변경
        if (_moveInput.x != 0)
        {
            _spriteRenderer.flipX = _moveInput.x < 0;
        }
    }

    /// <summary>
    /// 점프
    /// </summary>
    private void Jump()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f);
        _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _isGrounded = false;
    }

    /// <summary>
    /// 공기 저항 및 지면 마찰 적용
    /// </summary>
    private void ApplyDrag()
    {
        float currentDrag = _isGrounded ? _groundDrag : _airDrag;
        _rigidbody.drag = currentDrag;
    }

    /// <summary>
    /// 접지 상태 확인
    /// </summary>
    private void UpdateGroundCheck()
    {
        Vector2 raycastOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, _groundCheckDistance, _groundLayer);

        _isGrounded = hit.collider != null;

        // 디버그 라인 (에디터에서만 표시)
        Debug.DrawRay(raycastOrigin, Vector2.down * _groundCheckDistance, _isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    private void UpdateAnimation()
    {
        bool isMoving = Mathf.Abs(_moveInput.x) > 0.1f && _isGrounded;
        _animator.SetBool(_hashIsMoving, isMoving);
        _animator.SetBool(_hashIsJumping, !_isGrounded);
        _animator.SetFloat(_hashVelocityY, _rigidbody.velocity.y);
    }

    /// <summary>
    /// 데미지 받음
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!_isAlive)
            return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}가 {damage} 데미지를 받았습니다. (남은 체력: {_currentHealth})");

        // 플레이어 깜빡임 효과
        StartCoroutine(DamageFlash());

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        Debug.Log($"{gameObject.name}이 {amount} 회복했습니다. (현재 체력: {_currentHealth})");
    }

    /// <summary>
    /// 사망
    /// </summary>
    public void Die()
    {
        if (!_isAlive)
            return;

        _isAlive = false;
        _rigidbody.velocity = Vector2.zero;
        _animator.SetBool(_hashIsMoving, false);

        // 사망 애니메이션 (나중에 구현)
        gameObject.SetActive(false);

        Debug.Log($"{gameObject.name}이(가) 사망했습니다.");
        GameManager.Instance.UnregisterPlayer(this);
    }

    /// <summary>
    /// 데미지 플래시 효과
    /// </summary>
    private IEnumerator DamageFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Getter 메서드들
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _isAlive;
    public bool IsGrounded => _isGrounded;
}
