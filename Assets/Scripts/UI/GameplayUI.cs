using UnityEngine;
using TMPro;

/// <summary>
/// 게임플레이 화면 UI
/// 점수, 타이머, 플레이어 정보 등을 표시합니다
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _floorText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private TextMeshProUGUI _timerText;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private TextMeshProUGUI _enemyCountText;

    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private GameObject _pausePanel;

    [SerializeField]
    private PlayerController _player;

    [SerializeField]
    private EnemySpawner _enemySpawner;

    private void Start()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            ShowGameOverPanel();
            return;
        }

        if (GameManager.Instance.IsPaused)
        {
            ShowPausePanel();
        }
        else
        {
            HidePausePanel();
        }

        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        // 층 정보
        if (_floorText != null)
        {
            int floor = GameManager.Instance.CurrentFloor;
            _floorText.text = $"Floor {floor}/100";

            // 중간 보스 또는 최종 보스 표시
            if (floor == 50)
                _floorText.text += " - BOSS!";
            else if (floor == 100)
                _floorText.text += " - FINAL BOSS!";
        }

        // 점수
        if (_scoreText != null)
            _scoreText.text = $"Score: {GameManager.Instance.Score}";

        // 타이머
        if (_timerText != null)
        {
            float remainingTime = GameManager.Instance.RemainingTime;
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";

            // 시간이 부족하면 빨간색으로 표시
            if (remainingTime < 30f)
                _timerText.color = Color.red;
            else
                _timerText.color = Color.white;
        }

        // 플레이어 체력
        if (_healthText != null && _player != null)
        {
            _healthText.text = $"HP: {_player.CurrentHealth}/{_player.MaxHealth}";
        }

        // 남은 적 수
        if (_enemyCountText != null && _enemySpawner != null)
        {
            int remaining = _enemySpawner.TotalEnemiesSpawned - Mathf.Max(0, _enemySpawner.TotalEnemiesSpawned - _enemySpawner.ActiveEnemyCount);
            _enemyCountText.text = $"Enemies: {_enemySpawner.ActiveEnemyCount}";
        }
    }

    /// <summary>
    /// 게임 오버 패널 표시
    /// </summary>
    private void ShowGameOverPanel()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// 일시정지 패널 표시
    /// </summary>
    private void ShowPausePanel()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(true);
    }

    /// <summary>
    /// 일시정지 패널 숨김
    /// </summary>
    private void HidePausePanel()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }

    /// <summary>
    /// 게임 재시작 버튼
    /// </summary>
    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    /// <summary>
    /// 일시정지 재개 버튼
    /// </summary>
    public void OnResumeButtonClicked()
    {
        GameManager.Instance.TogglePause();
    }
}
