using UnityEngine;

/// <summary>
/// 파티클 이펙트 관리
/// 히트 이펙트, 스킬 이펙트 등을 처리합니다
/// </summary>
public class EffectSpawner : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _hitEffectPrefab;

    [SerializeField]
    private ParticleSystem _skillEffectPrefab;

    [SerializeField]
    private ParticleSystem _levelUpEffectPrefab;

    [SerializeField]
    private float _effectDuration = 1f;

    /// <summary>
    /// 히트 이펙트 생성
    /// </summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (_hitEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(_hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect.gameObject, _effectDuration);
        }
    }

    /// <summary>
    /// 스킬 이펙트 생성
    /// </summary>
    public void SpawnSkillEffect(Vector3 position)
    {
        if (_skillEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(_skillEffectPrefab, position, Quaternion.identity);
            Destroy(effect.gameObject, _effectDuration * 1.5f);
        }
    }

    /// <summary>
    /// 레벨업 이펙트 생성
    /// </summary>
    public void SpawnLevelUpEffect(Vector3 position)
    {
        if (_levelUpEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(_levelUpEffectPrefab, position, Quaternion.identity);
            Destroy(effect.gameObject, _effectDuration * 2f);
        }
    }
}