using UnityEngine;

/// <summary>
/// 레이드 시스템 매니저
/// 길드 레이드 보스 및 보상을 관리합니다
/// </summary>
public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance { get; private set; }

    [SerializeField]
    private BossController _bossPrefab;

    [SerializeField]
    private Transform _bossSpawnPoint;

    [SerializeField]
    private int _raidDifficulty = 1; // 1-10

    [SerializeField]
    private int _baseReward = 1000;

    [SerializeField]
    private float _raidTimeLimit = 600f; // 10분

    private BossController _currentBoss;
    private float _raidStartTime;
    private float _raidRemainingTime;
    private bool _raidActive = false;
    private bool _raidCleared = false;

    // 레이드 통계
    private int _totalDamageDealt = 0;
    private int _totalDamageReceived = 0;
    private int _enemiesDefeated = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 레이드 시작
    /// </summary>
    public void StartRaid()
    {
        if (_raidActive)
            return;

        _raidActive = true;
        _raidCleared = false;
        _raidStartTime = Time.time;
        _raidRemainingTime = _raidTimeLimit;

        // 보스 스폰
        SpawnBoss();

        Debug.Log($\"레이드 시작! 난이도: {_raidDifficulty}\");
    }

    /// <summary>
    /// 보스 스폰
    /// </summary>
    private void SpawnBoss()
    {
        if (_bossPrefab != null && _bossSpawnPoint != null)
        {
            _currentBoss = Instantiate(_bossPrefab, _bossSpawnPoint.position, Quaternion.identity);

            // 난이도에 따라 보스 스탯 증강
            int healthBonus = Mathf.FloorToInt(_currentBoss.MaxHealth * (_raidDifficulty - 1) * 0.2f);
            int finalHealth = _currentBoss.MaxHealth + healthBonus;

            Debug.Log($\"보스 스폰: {_currentBoss.BossName} (최대 체력: {finalHealth})\");
        }
    }

    /// <summary>
    /// 레이드 업데이트
    /// </summary>
    private void Update()
    {
        if (!_raidActive)
            return;

        _raidRemainingTime -= Time.deltaTime;

        // 보스 처치
        if (_currentBoss != null && !_currentBoss.IsAlive)
        {
            ClearRaid();
        }

        // 시간 초과
        if (_raidRemainingTime <= 0)
        {
            FailRaid();
        }
    }

    /// <summary>
    /// 레이드 클리어
    /// </summary>
    private void ClearRaid()
    {
        if (_raidCleared)
            return;

        _raidCleared = true;
        _raidActive = false;

        // 보상 계산
        int reward = CalculateReward();

        Debug.Log($\"레이드 클리어! 보상: {reward} 골드\");
    }

    /// <summary>
    /// 레이드 실패
    /// </summary>
    private void FailRaid()
    {
        _raidActive = false;

        Debug.Log(\"레이드 실패! 시간 초과\");
    }

    /// <summary>
    /// 보상 계산
    /// </summary>
    private int CalculateReward()
    {
        // 기본 보상 + 난이도 보너스 + 클리어 시간 보너스
        int baseReward = _baseReward;
        int difficultyBonus = Mathf.FloorToInt(baseReward * (_raidDifficulty - 1) * 0.3f);
        float clearTimeBonus = (_raidRemainingTime / _raidTimeLimit) * 0.2f;

        int totalReward = Mathf.FloorToInt((baseReward + difficultyBonus) * (1f + clearTimeBonus));

        return totalReward;
    }

    /// <summary>
    /// 난이도 설정
    /// </summary>
    public void SetDifficulty(int difficulty)
    {
        _raidDifficulty = Mathf.Clamp(difficulty, 1, 10);
        Debug.Log($\"레이드 난이도 설정: {_raidDifficulty}\");
    }

    // Getter 메서드들
    public bool RaidActive => _raidActive;
    public bool RaidCleared => _raidCleared;
    public float RaidRemainingTime => _raidRemainingTime;
    public int RaidDifficulty => _raidDifficulty;
    public BossController CurrentBoss => _currentBoss;
}
