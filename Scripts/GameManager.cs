using Godot;
using System;

public partial class GameManager : Node
{
    // Синглтон для удобного доступа из любого скрипта: GameManager.Instance
    public static GameManager Instance { get; private set; }

    // События для UI (обновляем интерфейс только когда числа реально меняются)
    public event Action<long> OnBytesChanged;
    public event Action<int, int> OnRamChanged; // текущий, максимальный
    public event Action OnOutOfRam;

    // Ресурсы игрока
    public long Bytes { get; private set; } = 0;
    public int MaxRam { get; private set; } = 20;
    public int CurrentRam { get; private set; }

    // Характеристики (будем качать их между раундами)
    public int ClickDamage { get; set; } = 1;
    public int ClickRamCost { get; set; } = 1;

    // Базовый бонус к количеству выпадающих байтов (начинаем с 0, при прокачке увеличиваем на +1)
    public int BonusByteDrop { get; set; } = 0;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        StartNewDay();
    }

    public void StartNewDay()
    {
        CurrentRam = MaxRam;
        OnRamChanged?.Invoke(CurrentRam, MaxRam);
        OnBytesChanged?.Invoke(Bytes);
    }

    // Попытка потратить RAM на клик. Возвращает true, если энергии хватило
    public bool TryConsumeRam(int amount)
    {
        if (CurrentRam < amount)
            return false;

        CurrentRam -= amount;
        OnRamChanged?.Invoke(CurrentRam, MaxRam);

        if (CurrentRam <= 0)
        {
            GD.Print("RAM переполнена! Рабочий день окончен.");
            OnOutOfRam?.Invoke();
        }

        return true;
    }

    public void AddBytes(int amount)
    {
        Bytes += amount;
        OnBytesChanged?.Invoke(Bytes);
    }
}