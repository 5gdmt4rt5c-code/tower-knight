using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적 생성 및 관리
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private int _enemiesToSpawn = 5;

    [SerializeField]
    private float _spawnInterval = 2f;

    [SerializeField]
    private Vector2 _spawnAreaMin = new(-8f, 3f);

    [SerializeField]
    private Vector2 _spawnAreaMax = new(8f, 4f);

    private List<EnemyController> _activeEnemies = new();
    private float _lastSpawnTime = 0f;
    private int _enemiesSpawned = 0;
    private bool _allEnemiesSpawned = false;

    private void Update()
    {
        if (_enemyPrefab == null)
        {
            Debug.LogError("적 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 정기적으로 적 생성
        if (!_allEnemiesSpawned && Time.time - _lastSpawnTime >= _spawnInterval)
        {
            SpawnEnemy();
            _lastSpawnTime = Time.time;
        }

        // 죽은 적 제거
        _activeEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);

        // 모든 적이 생성되고 죽으면 스테이지 클리어
        if (_allEnemiesSpawned && _activeEnemies.Count == 0)
        {
            ClearStage();
        }
    }

    /// <summary>
    /// 적 생성
    /// </summary>
    private void SpawnEnemy()
    {
        if (_enemiesSpawned >= _enemiesToSpawn)
        {
            _allEnemiesSpawned = true;
            Debug.Log($"모든 적 생성 완료! (총 {_enemiesToSpawn}마리)");
            return;
        }

        // 랜덤 위치에서 생성
        Vector2 spawnPosition = new(
            Random.Range(_spawnAreaMin.x, _spawnAreaMax.x),
            Random.Range(_spawnAreaMin.y, _spawnAreaMax.y)
        );

        GameObject enemyInstance = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, transform);
        EnemyController enemy = enemyInstance.GetComponent<EnemyController>();

        if (enemy != null)
        {
            _activeEnemies.Add(enemy);
            _enemiesSpawned++;
            Debug.Log($"적 생성: {_enemiesSpawned}/{_enemiesToSpawn}");
        }
    }

    /// <summary>
    /// 스테이지 클리어
    /// </summary>
    private void ClearStage()
    {
        Debug.Log("스테이지 클리어!");
        GameManager.Instance.ClearStage();
    }

    /// <summary>
    /// 스포너 리셋
    /// </summary>
    public void Reset()
    {
        // 모든 적 제거
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        _activeEnemies.Clear();

        _enemiesSpawned = 0;
        _allEnemiesSpawned = false;
        _lastSpawnTime = Time.time;
    }

    public int ActiveEnemyCount => _activeEnemies.Count;
    public int TotalEnemiesSpawned => _enemiesSpawned;
}