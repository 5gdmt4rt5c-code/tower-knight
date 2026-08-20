using UnityEngine;

/// <summary>
/// 보스 캐릭터 AI
/// 기본 적과 다른 고급 패턴을 가집니다
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("보스 정보")]
    [SerializeField]
    private string _bossName = \"Boss\";

    [SerializeField]
    private int _maxHealth = 500;

    [SerializeField]
    private int _scoreReward = 500;

    [Header("이동")]
    [SerializeField]
    private float _moveSpeed = 1.5f;

    [SerializeField]
    private float _patrolDistance = 8f;

    [Header(\"감지\")]
    [SerializeField]
    private float _detectionRange = 15f;

    [SerializeField]
    private LayerMask _playerLayer;

    [Header(\"공격 1 - 기본 공격\")]
    [SerializeField]
    private float _basicAttackRange = 2f;

    [SerializeField]
    private float _basicAttackCooldown = 2f;

    [SerializeField]
    private int _basicAttackDamage = 25;

    [Header(\"공격 2 - 광역 공격\")]
    [SerializeField]
    private float _areaAttackRange = 5f;

    [SerializeField]
    private float _areaAttackCooldown = 5f;

    [SerializeField]
    private int _areaAttackDamage = 40;

    [Header(\"공격 3 - 광선 공격\")]
    [SerializeField]
    private float _beamAttackRange = 10f;

    [SerializeField]
    private float _beamAttackCooldown = 8f;

    [SerializeField]
    private int _beamAttackDamage = 50;

    [Header(\"특수 능력\")]
    [SerializeField]
    private float _healCooldown = 10f;

    [SerializeField]
    private int _healAmount = 100;

    private int _currentHealth;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _patrolStart;
    private Vector2 _patrolEnd;
    private bool _movingRight = true;
    private PlayerController _targetPlayer = null;
    private bool _isAlive = true;

    private float _lastBasicAttackTime = 0f;
    private float _lastAreaAttackTime = 0f;
    private float _lastBeamAttackTime = 0f;
    private float _lastHealTime = 0f;

    // 애니메이션 해시
    private int _hashAttack = Animator.StringToHash(\"Attack\");
    private int _hashSpecial = Animator.StringToHash(\"Special\");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (_rigidbody == null)
            Debug.LogError(\"Rigidbody2D 컴포넌트가 없습니다!\");
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        _patrolStart = transform.position;
        _patrolEnd = _patrolStart + Vector2.right * _patrolDistance;

        Debug.Log($\"{_bossName} 등장! 체력: {_currentHealth}\");
    }

    private void Update()
    {
        if (!_isAlive)
            return;

        SearchForPlayer();
        UpdateBehavior();
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
    /// 행동 업데이트
    /// </summary>
    private void UpdateBehavior()
    {
        if (_targetPlayer != null && _targetPlayer.IsAlive)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _targetPlayer.transform.position);

            // 방향 설정
            Vector2 directionToPlayer = (_targetPlayer.transform.position - transform.position).normalized;
            _movingRight = directionToPlayer.x > 0;

            // 공격 선택
            if (distanceToPlayer <= _basicAttackRange)
            {
                BasicAttack();
            }
            else if (distanceToPlayer <= _beamAttackRange && Time.time - _lastBeamAttackTime >= _beamAttackCooldown)
            {
                BeamAttack();
            }
            else if (Time.time - _lastAreaAttackTime >= _areaAttackCooldown)
            {
                AreaAttack();
            }
            else
            {
                // 플레이어 추적
                _rigidbody.velocity = new Vector2(directionToPlayer.x * _moveSpeed, _rigidbody.velocity.y);
            }

            // 체력이 50% 이하면 힐 시도
            if (_currentHealth <= _maxHealth * 0.5f && Time.time - _lastHealTime >= _healCooldown)
            {
                Heal();
            }
        }
        else
        {
            // 순찰
            Patrol();
        }
    }

    /// <summary>
    /// 순찰
    /// </summary>
    private void Patrol()
    {
        if (transform.position.x >= _patrolEnd.x)
            _movingRight = false;
        else if (transform.position.x <= _patrolStart.x)
            _movingRight = true;
    }

    /// <summary>
    /// 이동 적용
    /// </summary>
    private void ApplyMovement()
    {
        float moveDirection = _movingRight ? 1f : -1f;
        _rigidbody.velocity = new Vector2(moveDirection * _moveSpeed * 0.5f, _rigidbody.velocity.y);
        _spriteRenderer.flipX = !_movingRight;
    }

    /// <summary>
    /// 기본 공격
    /// </summary>
    private void BasicAttack()
    {
        if (Time.time - _lastBasicAttackTime < _basicAttackCooldown)
            return;

        _lastBasicAttackTime = Time.time;
        _animator.SetTrigger(_hashAttack);

        if (_targetPlayer != null && _targetPlayer.IsAlive)
        {
            _targetPlayer.TakeDamage(_basicAttackDamage);
            Debug.Log($\"{_bossName}의 기본 공격! {_basicAttackDamage} 데미지\");
        }
    }

    /// <summary>
    /// 광역 공격 (범위 내 모든 플레이어)
    /// </summary>
    private void AreaAttack()
    {
        if (Time.time - _lastAreaAttackTime < _areaAttackCooldown)
            return;

        _lastAreaAttackTime = Time.time;
        _animator.SetTrigger(_hashSpecial);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, _areaAttackRange, _playerLayer);

        foreach (var playerCollider in hitPlayers)
        {
            PlayerController player = playerCollider.GetComponent<PlayerController>();
            if (player != null && player.IsAlive)
            {
                player.TakeDamage(_areaAttackDamage);
            }
        }

        Debug.Log($\"{_bossName}의 광역 공격! 범위: {_areaAttackRange}\");
    }

    /// <summary>
    /// 광선 공격 (직선)
    /// </summary>
    private void BeamAttack()
    {
        if (Time.time - _lastBeamAttackTime < _beamAttackCooldown)
            return;

        _lastBeamAttackTime = Time.time;
        _animator.SetTrigger(_hashSpecial);

        Vector2 beamDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + beamDirection * _beamAttackRange * 0.5f,
            1f,
            _playerLayer
        );

        foreach (var playerCollider in hitPlayers)
        {
            PlayerController player = playerCollider.GetComponent<PlayerController>();
            if (player != null && player.IsAlive)
            {
                player.TakeDamage(_beamAttackDamage);
            }
        }

        Debug.Log($\"{_bossName}의 광선 공격! 방향: {beamDirection}\");
    }

    /// <summary>
    /// 회복
    /// </summary>
    private void Heal()
    {
        _lastHealTime = Time.time;
        _currentHealth = Mathf.Min(_currentHealth + _healAmount, _maxHealth);

        Debug.Log($\"{_bossName}이 회복했습니다. (현재: {_currentHealth}/{_maxHealth})\");
    }

    /// <summary>
    /// 데미지 받음
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!_isAlive)
            return;

        _currentHealth -= damage;
        Debug.Log($\"{_bossName}이 {damage} 데미지를 받았습니다. (남은 체력: {_currentHealth})\");

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

        gameObject.SetActive(false);

        Debug.Log($\"{_bossName}을(를) 처치했습니다! +{_scoreReward} 점수\");
    }

    // Getter 메서드들
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _isAlive;
    public string BossName => _bossName;

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // 기본 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _basicAttackRange);

        // 광역 공격 범위
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _areaAttackRange);

        // 광선 공격 범위
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _beamAttackRange);
    }
}
