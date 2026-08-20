using UnityEngine;

/// <summary>
/// 성능 최적화 도구
/// 성능 모니터링 및 최적화 기능
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    [SerializeField]
    private bool _enableVSync = true;

    [SerializeField]
    private int _targetFrameRate = 60;

    [SerializeField]
    private float _monitoringInterval = 1f;

    private float _lastMonitoringTime = 0f;
    private float _currentFPS = 0f;
    private int _frameCount = 0;

    private void Start()
    {
        // V-Sync 설정
        QualitySettings.vSyncCount = _enableVSync ? 1 : 0;

        // 프레임 레이트 설정
        Application.targetFrameRate = _targetFrameRate;

        Debug.Log($"성능 최적화 설정: FPS={_targetFrameRate}, V-Sync={_enableVSync}");
    }

    private void Update()
    {
        _frameCount++;
        float elapsed = Time.time - _lastMonitoringTime;

        if (elapsed >= _monitoringInterval)
        {
            _currentFPS = _frameCount / elapsed;
            _frameCount = 0;
            _lastMonitoringTime = Time.time;

            MonitorPerformance();
        }
    }

    /// <summary>
    /// 성능 모니터링
    /// </summary>
    private void MonitorPerformance()
    {
        // FPS가 30 이하면 경고
        if (_currentFPS < 30)
        {
            Debug.LogWarning($"FPS 저하: {_currentFPS:F1} FPS");
            ApplyPerformanceOptimizations();
        }
    }

    /// <summary>
    /// 성능 최적화 적용
    /// </summary>
    private void ApplyPerformanceOptimizations()
    {
        // 텍스처 품질 감소
        QualitySettings.masterTextureLimit = Mathf.Min(QualitySettings.masterTextureLimit + 1, 3);

        // 쉐이더 품질 감소
        QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing / 2, 0);

        Debug.Log("성능 최적화 적용됨");
    }

    /// <summary>
    /// 현재 FPS 반환
    /// </summary>
    public float CurrentFPS => _currentFPS;

    /// <summary>
    /// 메모리 사용량 반환
    /// </summary>
    public long MemoryUsage => System.GC.GetTotalMemory(false);
}