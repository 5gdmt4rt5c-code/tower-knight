using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 게임플레이 UI 업데이트
/// 마나, 경험치, 쿨다운 등을 표시합니다
/// </summary>
public class GameplayUIAdvanced : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _manaText;

    [SerializeField]
    private TextMeshProUGUI _levelText;

    [SerializeField]
    private TextMeshProUGUI _experienceText;

    [SerializeField]
    private TextMeshProUGUI _attackSpeedText;

    [SerializeField]
    private Image _manaBar;

    [SerializeField]
    private Image _experienceBar;

    [SerializeField]
    private PlayerStats _playerStats;

    [SerializeField]
    private PlayerAttack _playerAttack;

    private void Update()
    {
        if (_playerStats == null)
            return;

        UpdateStats();
        UpdateCooldowns();
    }

    /// <summary>
    /// 스탯 UI 업데이트
    /// </summary>
    private void UpdateStats()
    {
        // 마나
        if (_manaText != null)
        {
            _manaText.text = $"Mana: {_playerStats.CurrentMana}/{_playerStats.MaxMana}";
        }

        if (_manaBar != null)
        {
            float manaPercent = (float)_playerStats.CurrentMana / _playerStats.MaxMana;
            _manaBar.fillAmount = manaPercent;
        }

        // 레벨
        if (_levelText != null)
        {
            _levelText.text = $"Level: {_playerStats.CurrentLevel}";
        }

        // 경험치
        if (_experienceText != null)
        {
            _experienceText.text = $"EXP: {_playerStats.CurrentExperience}/{_playerStats.ExperiencePerLevel}";
        }

        if (_experienceBar != null)
        {
            float expPercent = (float)_playerStats.CurrentExperience / _playerStats.ExperiencePerLevel;
            _experienceBar.fillAmount = Mathf.Min(expPercent, 1f);
        }
    }

    /// <summary>
    /// 쿨다운 UI 업데이트
    /// </summary>
    private void UpdateCooldowns()
    {
        if (_playerAttack == null)
            return;

        // 기본 공격 쿨다운
        float basicAttackCooldownRemaining = Mathf.Max(0, _playerAttack.AttackCooldown - (Time.time - _playerAttack.LastAttackTime));

        // 스킬 1 쿨다운
        float skill1CooldownRemaining = Mathf.Max(0, _playerAttack.Skill1Cooldown - (Time.time - _playerAttack.LastSkill1Time));

        if (_attackSpeedText != null)
        {
            _attackSpeedText.text = $"Basic CD: {basicAttackCooldownRemaining:F1}s | Skill1 CD: {skill1CooldownRemaining:F1}s";
        }
    }
}