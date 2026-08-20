using UnityEngine;

/// <summary>
/// 플레이어 스탯 관리
/// 공격력, 방어력, 마나, 경험치 등을 담당합니다
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField]
    private int _baseAttack = 10;

    [SerializeField]
    private int _baseDefense = 5;

    [SerializeField]
    private int _baseMaxMana = 100;

    [Header("레벨")]
    [SerializeField]
    private int _currentLevel = 1;

    [SerializeField]
    private int _experiencePerLevel = 100;

    private int _attack;
    private int _defense;
    private int _currentMana;
    private int _maxMana;
    private int _currentExperience = 0;

    private void Awake()
    {
        _attack = _baseAttack;
        _defense = _baseDefense;
        _maxMana = _baseMaxMana;
        _currentMana = _maxMana;
    }

    /// <summary>
    /// 마나 소비
    /// </summary>
    public bool UseMana(int amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 마나 회복
    /// </summary>
    public void RestoreMana(int amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
    }

    /// <summary>
    /// 자동 마나 회복 (시간에 따라)
    /// </summary>
    public void UpdateManaRegen(float deltaTime)
    {
        float manaRegenPerSecond = _maxMana * 0.05f; // 초당 최대 마나의 5%
        RestoreMana(Mathf.FloorToInt(manaRegenPerSecond * deltaTime));
    }

    /// <summary>
    /// 경험치 획득
    /// </summary>
    public void GainExperience(int amount)
    {
        _currentExperience += amount;
        Debug.Log($"경험치 획득: +{amount} (현재: {_currentExperience}/{_experiencePerLevel})");

        // 레벨업 확인
        while (_currentExperience >= _experiencePerLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 레벨업
    /// </summary>
    private void LevelUp()
    {
        _currentExperience -= _experiencePerLevel;
        _currentLevel++;

        // 스탯 증가
        _attack += 5;
        _defense += 2;
        _maxMana += 20;
        _currentMana = _maxMana;

        Debug.Log($"레벨 업! 현재 레벨: {_currentLevel}");
        Debug.Log($"  공격력: +5 ({_attack})");
        Debug.Log($"  방어력: +2 ({_defense})");
        Debug.Log($"  최대 마나: +20 ({_maxMana})");
    }

    /// <summary>
    /// 장비로 인한 스탯 증가
    /// </summary>
    public void AddEquipmentBonus(int attackBonus, int defenseBonus)
    {
        _attack += attackBonus;
        _defense += defenseBonus;
    }

    // Getter 메서드들
    public int Attack => _attack;
    public int Defense => _defense;
    public int CurrentMana => _currentMana;
    public int MaxMana => _maxMana;
    public int CurrentLevel => _currentLevel;
    public int CurrentExperience => _currentExperience;
    public int ExperiencePerLevel => _experiencePerLevel;
}