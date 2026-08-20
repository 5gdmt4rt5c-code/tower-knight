using UnityEngine;

/// <summary>
/// 기본 적 AI
/// 이동, 공격, 플레이어 추적을 담당합니다
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField]
    private float _moveSpeed = 2f;

    [SerializeField]
    private float _patrolDistance = 5f;

    [Header("감지")]
    [SerializeField]
    private float _detectionRange = 10f;

    [SerializeField]
    private LayerMask _playerLayer;

    [Header("공격")]
    [SerializeField]
    private float _attackRange = 1.5f;

    [SerializeField]
    private float _attackCooldown = 1f;

    [SerializeField]
    private int _attackDamage = 10;

    [Header("스탯")]
    [SerializeField]
    private int _maxHealth = 30;

    [SerializeField]
    private int _scoreReward = 10;

    private int _currentHealth;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _patrolStart;
    private Vector2 _patrolEnd;
    private bool _movingRight = true;
    private PlayerController _targetPlayer = null;
    private float _lastAttackTime = 0f;
    private bool _isAlive = true;

    // 애니메이션 해시
    private int _hashIsMoving = Animator.StringToHash("IsMoving");
    private int _hashIsAttacking = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (_rigidbody == null)
            Debug.LogError("Rigidbody2D 컴포넌트가 없습니다!");
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        _patrolStart = transform.position;
        _patrolEnd = _patrolStart + Vector2.right * _patrolDistance;
    }

    private void Update()
    {
        if (!_isAlive)
            return;

        SearchForPlayer();
        UpdateBehavior();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!_isAlive)
            return;

        ApplyMovement();
    }

    /// <summary>
    /// 플레이어 검색
    /// </summary>
    private void SearchForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectionRange, _playerLayer);

        if (hits.Length > 0)
        {
            _targetPlayer = hits[0].GetComponent<PlayerController>();
        }
        else
        {
            _targetPlayer = null;
        }
    }

    /// <summary>
    /// 행동 업데이트 (순찰 또는 추적)
    /// </summary>
    private void UpdateBehavior()
    {
        if (_targetPlayer != null && _targetPlayer.IsAlive)
        {
            // 플레이어를 추적 중
            float distanceToPlayer = Vector2.Distance(transform.position, _targetPlayer.transform.position);

            if (distanceToPlayer <= _attackRange)
            {
                // 공격 범위 내
                Attack();
            }
            else
            {
                // 플레이어 추적
                ChasePlayer();
            }
        }
        else
        {
            // 순찰 중
            Patrol();
        }
    }

    /// <summary>
    /// 순찰
    /// </summary>
    private void Patrol()
    {
        if (transform.position.x >= _patrolEnd.x)
        {
            _movingRight = false;
        }
        else if (transform.position.x <= _patrolStart.x)
        {
            _movingRight = true;
        }
    }

    /// <summary>
    /// 플레이어 추적
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (_targetPlayer.transform.position - transform.position).normalized;
        _movingRight = directionToPlayer.x > 0;
    }

    /// <summary>
    /// 이동 적용
    /// </summary>
    private void ApplyMovement()
    {
        float moveDirection = _movingRight ? 1f : -1f;
        _rigidbody.velocity = new Vector2(moveDirection * _moveSpeed, _rigidbody.velocity.y);
        _spriteRenderer.flipX = !_movingRight;
    }

    /// <summary>
    /// 공격
    /// </summary>
    private void Attack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown)
            return;

        _lastAttackTime = Time.time;

        if (_targetPlayer != null && _targetPlayer.IsAlive)
        {
            _targetPlayer.TakeDamage(_attackDamage);
            Debug.Log($"{gameObject.name}이 플레이어를 공격했습니다! ({_attackDamage} 데미지)");
        }
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// </summary>
    private void UpdateAnimation()
    {
        bool isMoving = Mathf.Abs(_rigidbody.velocity.x) > 0.1f;
        bool isAttacking = Time.time - _lastAttackTime < 0.3f;

        _animator.SetBool(_hashIsMoving, isMoving);
        _animator.SetBool(_hashIsAttacking, isAttacking);
    }

    /// <summary>
    /// 데미지 받음
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!_isAlive)
            return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}이 {damage} 데미지를 받았습니다. (남은 체력: {_currentHealth})");

        if (_currentHealth <= 0)
        {
            Die();
        }
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

        // 점수 추가
        GameManager.Instance.AddScore(_scoreReward);

        // 사망 효과 (나중에 파티클 등 추가)
        gameObject.SetActive(false);

        Debug.Log($"{gameObject.name}이(가) 사망했습니다. +{_scoreReward} 점수");
    }

    // Getter 메서드들
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _isAlive;

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        // 순찰 범위
        Gizmos.color = Color.green;
        Vector2 start = (Vector2)transform.position + Vector2.left * _patrolDistance * 0.5f;
        Vector2 end = (Vector2)transform.position + Vector2.right * _patrolDistance * 0.5f;
        Gizmos.DrawLine(start, end);
    }
}