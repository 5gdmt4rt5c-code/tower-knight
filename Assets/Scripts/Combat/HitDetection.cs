using UnityEngine;

/// <summary>
/// 히트 판정 및 데미지 시스템
/// 공격 범위, 데미지 계산을 담당합니다
/// </summary>
public class HitDetection : MonoBehaviour
{
    [SerializeField]
    private float _hitCooldown = 0.5f; // 같은 대상에 연속 히트 방지

    private float _lastHitTime = 0f;
    private Collider2D _hitCollider;

    private void Awake()
    {
        _hitCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 충돌 감지 (OnTriggerEnter2D로 호출)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - _lastHitTime < _hitCooldown)
            return;

        // 적과 충돌했는지 확인
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsAlive)
            {
                _lastHitTime = Time.time;
                OnHitEnemy(enemy);
            }
        }
    }

    /// <summary>
    /// 적 히트 처리
    /// </summary>
    private void OnHitEnemy(EnemyController enemy)
    {
        Debug.Log($"히트 판정 발동: {enemy.gameObject.name}");

        // 여기서 추가 이펙트나 로직 처리 가능
        // 예: 사운드, 진동, 이펙트 등
    }
}