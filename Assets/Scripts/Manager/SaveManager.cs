using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 저장 시스템 매니저
/// PlayerPrefs를 사용한 게임 데이터 저장/로드
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField]
    private string _playerNameKey = "PlayerName";

    [SerializeField]
    private string _levelKey = "Level";

    [SerializeField]
    private string _experienceKey = "Experience";

    [SerializeField]
    private string _currentFloorKey = "CurrentFloor";

    [SerializeField]
    private string _scoreKey = "Score";

    [SerializeField]
    private string _totalPlayTimeKey = "TotalPlayTime";

    private float _sessionStartTime;

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
        _sessionStartTime = Time.time;
    }

    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    public void SavePlayerData(PlayerStats playerStats, string playerName)
    {
        PlayerPrefs.SetString(_playerNameKey, playerName);
        PlayerPrefs.SetInt(_levelKey, playerStats.CurrentLevel);
        PlayerPrefs.SetInt(_experienceKey, playerStats.CurrentExperience);
        PlayerPrefs.SetInt(_scoreKey, GameManager.Instance.Score);
        PlayerPrefs.SetInt(_currentFloorKey, GameManager.Instance.CurrentFloor);

        // 플레이 시간 저장
        float totalPlayTime = PlayerPrefs.GetFloat(_totalPlayTimeKey, 0f) + (Time.time - _sessionStartTime);
        PlayerPrefs.SetFloat(_totalPlayTimeKey, totalPlayTime);

        PlayerPrefs.Save();
        Debug.Log("플레이어 데이터 저장 완료");
    }

    /// <summary>
    /// 플레이어 데이터 로드
    /// </summary>
    public bool LoadPlayerData(out string playerName, out int level, out int experience, out int floor)
    {
        if (!PlayerPrefs.HasKey(_playerNameKey))
        {
            playerName = "";
            level = 1;
            experience = 0;
            floor = 1;
            return false;
        }

        playerName = PlayerPrefs.GetString(_playerNameKey);
        level = PlayerPrefs.GetInt(_levelKey, 1);
        experience = PlayerPrefs.GetInt(_experienceKey, 0);
        floor = PlayerPrefs.GetInt(_currentFloorKey, 1);

        Debug.Log($"플레이어 데이터 로드: {playerName} (Lv.{level})");
        return true;
    }

    /// <summary>
    /// 게임 통계 저장
    /// </summary>
    public void SaveGameStats(int score, int floor)
    {
        PlayerPrefs.SetInt(_scoreKey, score);
        PlayerPrefs.SetInt(_currentFloorKey, floor);
        PlayerPrefs.Save();

        Debug.Log($"게임 통계 저장: 점수={score}, 층={floor}");
    }

    /// <summary>
    /// 게임 통계 로드
    /// </summary>
    public (int score, int floor) LoadGameStats()
    {
        int score = PlayerPrefs.GetInt(_scoreKey, 0);
        int floor = PlayerPrefs.GetInt(_currentFloorKey, 1);

        return (score, floor);
    }

    /// <summary>
    /// 전체 플레이 시간 조회
    /// </summary>
    public float GetTotalPlayTime()
    {
        return PlayerPrefs.GetFloat(_totalPlayTimeKey, 0f);
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        _sessionStartTime = Time.time;
        Debug.Log("모든 데이터 초기화 완료");
    }

    /// <summary>
    /// 특정 데이터 초기화
    /// </summary>
    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteKey(_playerNameKey);
        PlayerPrefs.DeleteKey(_levelKey);
        PlayerPrefs.DeleteKey(_experienceKey);
        PlayerPrefs.DeleteKey(_currentFloorKey);
        PlayerPrefs.DeleteKey(_scoreKey);
        PlayerPrefs.Save();

        Debug.Log("플레이어 데이터 초기화 완료");
    }
}