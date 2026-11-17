using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public event Action<long> OnBytesChanged;
    public event Action<long> OnPrestigePointsChanged;
    public event Action OnBugPurchased; // Событие для мгновенного спавна купленного бага

    // Валюты
    public long Bytes { get; private set; } = 1; // Начинаем с 1 байта, чтобы купить первого бага!
    public long PrestigePoints { get; private set; } = 0;

    // Купленные слоты багов в текущем цикле (изначально 0 — поле пустое)
    public int ActiveBugSlots { get; private set; } = 0;
    public long NextBugCost { get; private set; } = 1; // Первый баг стоит ровно 1 байт

    // Временные характеристики текущего цикла
    public int RunDamageBonus { get; private set; } = 0;
    public long DamageUpgradeCost { get; private set; } = 5;

    // --- ТАЛАНТЫ ПРЕСТИЖА (сохраняются навсегда) ---
    public int BaseClickDamage { get; set; } = 1;
    public int BonusByteDrop { get; set; } = 0;
    public int PrestigeFragmentMultiplier { get; set; } = 1;
    public int StartingCapitalLevel { get; set; } = 0;
    public float PickupRadius { get; set; } = 25.0f;
    public bool AutoCollectOnHover { get; set; } = false;
    public float GoldenBugChance { get; set; } = 0.0f;
    public int GoldenBugMultiplier { get; set; } = 3;

    public bool IsStoryCompleted { get; set; } = false;
    public bool IsOverclockingActive { get; private set; } = false;

    public int TotalClickDamage => BaseClickDamage + RunDamageBonus;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        // На старте игры у игрока 1 байт и 0 багов на поле
        Bytes = 1;
        ActiveBugSlots = 0;
        NextBugCost = 1;

        OnBytesChanged?.Invoke(Bytes);
        OnPrestigePointsChanged?.Invoke(PrestigePoints);
    }

    // Покупка слота бага в меню "Улучшения"
    public bool TryBuyBugSlot()
    {
        if (Bytes < NextBugCost) return false;

        Bytes -= NextBugCost;
        ActiveBugSlots++;
        // Цена следующего бага растет (1 -> 5 -> 15 -> 40...)
        NextBugCost = ActiveBugSlots == 1 ? 5 : (long)(NextBugCost * 2.5f);

        OnBytesChanged?.Invoke(Bytes);
        OnBugPurchased?.Invoke();
        return true;
    }

    // Временная прокачка урона клика за Байты
    public bool TryBuyRunDamage()
    {
        if (Bytes < DamageUpgradeCost) return false;

        Bytes -= DamageUpgradeCost;
        RunDamageBonus++;
        DamageUpgradeCost = (long)(DamageUpgradeCost * 1.8f);

        OnBytesChanged?.Invoke(Bytes);
        return true;
    }

    // Перерождение (Престиж)
    public void PerformPrestige()
    {
        long previousBytes = Bytes;

        if (StartingCapitalLevel <= 0)
        {
            Bytes = 1;
        }
        else
        {
            float keepPercent = Mathf.Clamp(StartingCapitalLevel * 0.25f, 0.25f, 1.0f);
            Bytes = Math.Max(1L, (long)(previousBytes * keepPercent));
        }

        // Сбрасываем временные покупки цикла (поле снова становится пустым до покупки первого бага)
        ActiveBugSlots = 0;
        NextBugCost = 1;
        RunDamageBonus = 0;
        DamageUpgradeCost = 5;

        OnBytesChanged?.Invoke(Bytes);
        OnPrestigePointsChanged?.Invoke(PrestigePoints);
    }

    public void AddBytes(int amount)
    {
        Bytes += amount;
        OnBytesChanged?.Invoke(Bytes);
    }

    public void AddPrestigeFromFragment(int baseFragmentValue)
    {
        long totalReward = baseFragmentValue * PrestigeFragmentMultiplier;
        PrestigePoints += totalReward;
        OnPrestigePointsChanged?.Invoke(PrestigePoints);
    }
}