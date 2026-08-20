using UnityEngine;

/// <summary>
/// 플레이어의 공격 능력 관리
/// 기본 공격, 스킬, 데미지 판정을 담당합니다
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("기본 공격")]
    [SerializeField]
    private float _attackCooldown = 0.5f;

    [SerializeField]
    private float _attackRange = 1.5f;

    [SerializeField]
    private int _attackDamage = 15;

    [SerializeField]
    private float _attackKnockback = 5f;

    [Header("스킬")]
    [SerializeField]
    private float _skill1Cooldown = 2f;

    [SerializeField]
    private int _skill1Damage = 25;

    [SerializeField]
    private float _skill1Range = 3f;

    [SerializeField]
    private int _skill1ManaCost = 20;

    [Header("이펙트")]
    [SerializeField]
    private GameObject _hitEffectPrefab;

    [SerializeField]
    private GameObject _skillEffectPrefab;

    private PlayerController _playerController;
    private PlayerStats _playerStats;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private float _lastAttackTime = 0f;
    private float _lastSkill1Time = 0f;
    private bool _isAttacking = false;

    // 애니메이션 해시
    private int _hashAttack = Animator.StringToHash("Attack");
    private int _hashSkill1 = Animator.StringToHash("Skill1");

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerStats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_playerController == null)
            Debug.LogError("PlayerController 컴포넌트가 없습니다!");
        if (_playerStats == null)
            Debug.LogError("PlayerStats 컴포넌트가 없습니다!");
    }

    private void Update()
    {
        if (!_playerController.IsAlive)
            return;

        HandleAttackInput();
    }

    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    private void HandleAttackInput()
    {
        // 기본 공격
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            BasicAttack();
        }

        // 스킬 1
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Skill1();
        }
    }

    /// <summary>
    /// 기본 공격
    /// </summary>
    private void BasicAttack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown || _isAttacking)
            return;

        _lastAttackTime = Time.time;
        _isAttacking = true;

        _animator.SetTrigger(_hashAttack);

        // 공격 범위 내 적 탐지
        Collider2D[] hitEnemies = DetectEnemies(_attackRange);

        if (hitEnemies.Length > 0)
        {
            foreach (var enemyCollider in hitEnemies)
            {
                EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
                if (enemy != null && enemy.IsAlive)
                {
                    // 데미지 적용
                    int actualDamage = CalculateDamage(_attackDamage, enemy);
                    enemy.TakeDamage(actualDamage);

                    // 넉백 적용
                    ApplyKnockback(enemy, _attackKnockback);

                    // 히트 이펙트
                    CreateHitEffect(enemy.transform.position);

                    Debug.Log($"기본 공격 적중! {enemy.gameObject.name}에게 {actualDamage} 데미지");
                }
            }
        }
        else
        {
            Debug.Log("기본 공격: 적 없음");
        }

        // 공격 애니메이션 완료 후 상태 초기화 (0.3초)
        Invoke(nameof(ResetAttack), 0.3f);
    }

    /// <summary>
    /// 스킬 1 - 광역 공격
    /// </summary>
    private void Skill1()
    {
        if (Time.time - _lastSkill1Time < _skill1Cooldown || _isAttacking)
            return;

        // 마나 체크
        if (_playerStats.CurrentMana < _skill1ManaCost)
        {
            Debug.Log("마나가 부족합니다!");
            return;
        }

        _lastSkill1Time = Time.time;
        _isAttacking = true;

        // 마나 소비
        _playerStats.UseMana(_skill1ManaCost);

        _animator.SetTrigger(_hashSkill1);

        // 광역 공격
        Collider2D[] hitEnemies = DetectEnemies(_skill1Range);

        if (hitEnemies.Length > 0)
        {
            foreach (var enemyCollider in hitEnemies)
            {
                EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
                if (enemy != null && enemy.IsAlive)
                {
                    int actualDamage = CalculateDamage(_skill1Damage, enemy);
                    enemy.TakeDamage(actualDamage);

                    // 더 강한 넉백
                    ApplyKnockback(enemy, _attackKnockback * 1.5f);

                    // 스킬 이펙트
                    CreateSkillEffect(enemy.transform.position);

                    Debug.Log($"스킬 1 적중! {enemy.gameObject.name}에게 {actualDamage} 데미지");
                }
            }
        }
        else
        {
            Debug.Log("스킬 1: 적 없음");
        }

        // 스킬 애니메이션 완료 후 상태 초기화 (0.5초)
        Invoke(nameof(ResetAttack), 0.5f);
    }

    /// <summary>
    /// 범위 내 적 탐지
    /// </summary>
    private Collider2D[] DetectEnemies(float range)
    {
        // 플레이어가 바라보는 방향에 따라 범위 조정
        Vector2 checkDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 checkPosition = (Vector2)transform.position + checkDirection * range * 0.5f;

        return Physics2D.OverlapCircleAll(checkPosition, range, LayerMask.GetMask("Enemy"));
    }

    /// <summary>
    /// 데미지 계산 (플레이어 공격력 - 적 방어력)
    /// </summary>
    private int CalculateDamage(int baseDamage, EnemyController enemy)
    {
        int playerAttack = _playerStats.Attack;
        int enemyDefense = 5; // 기본 적 방어력

        // 실제 데미지 = (기본 데미지 + 플레이어 공격력) - 적 방어력
        int damage = Mathf.Max(1, (baseDamage + playerAttack) - enemyDefense);

        // 치명타 확률 (20%)
        if (Random.value < 0.2f)
        {
            damage = Mathf.FloorToInt(damage * 1.5f);
            Debug.Log("치명타!");
        }

        return damage;
    }

    /// <summary>
    /// 넉백 적용
    /// </summary>
    private void ApplyKnockback(EnemyController enemy, float force)
    {
        Rigidbody2D enemyRigidbody = enemy.GetComponent<Rigidbody2D>();
        if (enemyRigidbody != null)
        {
            Vector2 knockbackDirection = (_spriteRenderer.flipX ? Vector2.left : Vector2.right);
            enemyRigidbody.velocity = knockbackDirection * force;
        }
    }

    /// <summary>
    /// 히트 이펙트 생성
    /// </summary>
    private void CreateHitEffect(Vector3 position)
    {
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 스킬 이펙트 생성
    /// </summary>
    private void CreateSkillEffect(Vector3 position)
    {
        if (_skillEffectPrefab != null)
        {
            Instantiate(_skillEffectPrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 공격 상태 초기화
    /// </summary>
    private void ResetAttack()
    {
        _isAttacking = false;
    }

    // Getter 메서드들
    public float LastAttackTime => _lastAttackTime;
    public float AttackCooldown => _attackCooldown;
    public float LastSkill1Time => _lastSkill1Time;
    public float Skill1Cooldown => _skill1Cooldown;

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        // 기본 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        // 스킬 1 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _skill1Range);
    }
}