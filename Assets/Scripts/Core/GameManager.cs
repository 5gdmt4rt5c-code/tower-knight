using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 전체를 관리하는 핵심 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private int _currentFloor = 1;

    [SerializeField]
    private int _maxFloors = 100;

    private int _score = 0;
    private float _stageTimer = 180f; // 3분
    private float _remainingTime;
    private bool _isGameOver = false;
    private bool _isPaused = false;

    private List<PlayerController> _activePlayers = new();

    public int CurrentFloor => _currentFloor;
    public int MaxFloors => _maxFloors;
    public int Score => _score;
    public float RemainingTime => _remainingTime;
    public bool IsGameOver => _isGameOver;
    public bool IsPaused => _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _remainingTime = _stageTimer;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (_isGameOver || _isPaused)
            return;

        // 시간 감소
        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0)
        {
            EndStage(false);
        }

        // 폰 뒤로 가기 또는 ESC 키로 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 플레이어 등록
    /// </summary>
    public void RegisterPlayer(PlayerController player)
    {
        if (!_activePlayers.Contains(player))
        {
            _activePlayers.Add(player);
            Debug.Log($"플레이어 등록됨: {_activePlayers.Count}명");
        }
    }

    /// <summary>
    /// 플레이어 제거 (쓰러짐)
    /// </summary>
    public void UnregisterPlayer(PlayerController player)
    {
        _activePlayers.Remove(player);
        Debug.Log($"플레이어 제거됨: {_activePlayers.Count}명 남음");

        // 모든 플레이어가 쓰러지면 게임 오버
        if (_activePlayers.Count == 0)
        {
            EndStage(false);
        }
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int points)
    {
        _score += points;
    }

    /// <summary>
    /// 스테이지 클리어 (모든 적 처치)
    /// </summary>
    public void ClearStage()
    {
        if (_currentFloor < _maxFloors)
        {
            _currentFloor++;
            _remainingTime = _stageTimer;
            Debug.Log($"층 클리어! 현재 층: {_currentFloor}");
        }
        else
        {
            EndStage(true); // 최종 보스 클리어
        }
    }

    /// <summary>
    /// 스테이지 종료
    /// </summary>
    public void EndStage(bool success)
    {
        _isGameOver = true;
        Time.timeScale = 0f;

        if (success)
        {
            Debug.Log($"게임 승리! 최종 점수: {_score}");
        }
        else
        {
            Debug.Log($"게임 오버! 점수: {_score}");
        }
    }

    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        Debug.Log(_isPaused ? "일시정지" : "재개");
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        _currentFloor = 1;
        _score = 0;
        _remainingTime = _stageTimer;
        _isGameOver = false;
        _isPaused = false;
        _activePlayers.Clear();
        Time.timeScale = 1f;

        // 씬 다시 로드 (실제 구현시)
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}