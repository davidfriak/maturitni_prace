using System;
using UnityEngine;

/// <summary>
/// Data hráče (životy, XP, peníze, šípy, zbraň). Přežívá změny scén (DontDestroyOnLoad).
/// Umísti tento skript na prázdný GameObject "PlayerStats" v PRVNÍ scéně hry (kořenový objekt ve scéně).
/// Další kopie, které by vznikly při návratu do scény, se samy zničí.
/// Pohybové skripty a UI s ním komunikují přes PlayerStats.Instance a události.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public enum Weapon { Sword, Bow }
    public enum UpgradeType { MaxHealth, Damage, Speed }

    public static PlayerStats Instance { get; private set; }

    [Header("Výchozí hodnoty")]
    [SerializeField] private int startMaxHealth = 100;
    [SerializeField] private int startArrows = 10;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Levelování")]
    [SerializeField] private int baseXpToLevel = 100;
    [SerializeField] private int xpIncreasePerLevel = 50;

    // ---------- Stav ----------
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    public int Money { get; private set; }
    public int Arrows { get; private set; }
    public int Xp { get; private set; }
    public int Level { get; private set; } = 1;
    public int PendingUpgrades { get; private set; }
    public Weapon EquippedWeapon { get; private set; } = Weapon.Sword;

    // ---------- Vylepšení ----------
    public float DamageMultiplier { get; private set; } = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;

    public int XpToNextLevel => baseXpToLevel + (Level - 1) * xpIncreasePerLevel;
    public bool IsDead => Health <= 0;

    // ---------- Události (pro UI) ----------
    public event Action<int, int> OnHealthChanged;   // (aktuální, maximum)
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnArrowsChanged;
    public event Action<int, int> OnXpChanged;       // (aktuální XP, XP do dalšího levelu)
    public event Action<int> OnLevelUp;              // nový level -> otevři menu vylepšení
    public event Action<Weapon> OnWeaponChanged;
    public event Action OnDied;

    private float lastDamageTime = -999f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetForNewGame();
    }

    /// <summary>Nová hra / restart po smrti.</summary>
    public void ResetForNewGame()
    {
        MaxHealth = startMaxHealth;
        Health = MaxHealth;
        Money = 0;
        Arrows = startArrows;
        Xp = 0;
        Level = 1;
        PendingUpgrades = 0;
        DamageMultiplier = 1f;
        SpeedMultiplier = 1f;
        EquippedWeapon = Weapon.Sword;

        // UI si po načtení scény vyžádá aktuální hodnoty přes RefreshAll()
        RefreshAll();
    }

    /// <summary>Zavolej z UI ve Start(), aby se zobrazily aktuální hodnoty po změně scény.</summary>
    public void RefreshAll()
    {
        OnHealthChanged?.Invoke(Health, MaxHealth);
        OnMoneyChanged?.Invoke(Money);
        OnArrowsChanged?.Invoke(Arrows);
        OnXpChanged?.Invoke(Xp, XpToNextLevel);
        OnWeaponChanged?.Invoke(EquippedWeapon);
    }

    // ---------- Životy ----------
    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        if (Time.time - lastDamageTime < invulnerabilityTime) return;   // krátká nezranitelnost po zásahu

        lastDamageTime = Time.time;
        Health = Mathf.Max(0, Health - amount);
        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health == 0)
            OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        Health = Mathf.Min(MaxHealth, Health + amount);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    // ---------- Peníze ----------
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    /// <summary>Vrátí true, když hráč měl dost peněz a nákup proběhl.</summary>
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || Money < amount) return false;
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        return true;
    }

    // ---------- Šípy ----------
    public void AddArrows(int amount)
    {
        if (amount <= 0) return;
        Arrows += amount;
        OnArrowsChanged?.Invoke(Arrows);
    }

    /// <summary>Spotřebuje jeden šíp. Vrátí false, pokud žádný nemá.</summary>
    public bool TryUseArrow()
    {
        if (Arrows <= 0) return false;
        Arrows--;
        OnArrowsChanged?.Invoke(Arrows);
        return true;
    }

    // ---------- Zbraň ----------
    public void SetWeapon(Weapon weapon)
    {
        if (EquippedWeapon == weapon) return;
        EquippedWeapon = weapon;
        OnWeaponChanged?.Invoke(EquippedWeapon);
    }

    public void ToggleWeapon()
    {
        SetWeapon(EquippedWeapon == Weapon.Sword ? Weapon.Bow : Weapon.Sword);
    }

    // ---------- XP a level ----------
    public void AddXp(int amount)
    {
        if (amount <= 0) return;
        Xp += amount;

        while (Xp >= XpToNextLevel)
        {
            Xp -= XpToNextLevel;
            Level++;
            PendingUpgrades++;
            OnLevelUp?.Invoke(Level);
        }

        OnXpChanged?.Invoke(Xp, XpToNextLevel);
    }

    /// <summary>Použije jeden bod vylepšení (volá menu po levelu).</summary>
    public bool TryApplyUpgrade(UpgradeType type)
    {
        if (PendingUpgrades <= 0) return false;
        PendingUpgrades--;

        switch (type)
        {
            case UpgradeType.MaxHealth:
                MaxHealth += 20;
                Health = Mathf.Min(MaxHealth, Health + 20);
                OnHealthChanged?.Invoke(Health, MaxHealth);
                break;
            case UpgradeType.Damage:
                DamageMultiplier += 0.15f;
                break;
            case UpgradeType.Speed:
                SpeedMultiplier += 0.1f;
                break;
        }
        return true;
    }
}
