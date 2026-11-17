using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private Label _prestigeLabel;
    private Label _bytesLabel;

    // Элементы меню Улучшений
    private Button _openUpgradesBtn;
    private PanelContainer _upgradesPanel;
    private Button _closeUpgradesBtn;
    private Button _buyBugBtn;
    private Button _upgradeDamageBtn;

    public override void _Ready()
    {
        // Верхний левый блок валют
        _bytesLabel = GetNode<Label>("RootControl/CurrencyPanel/VBoxContainer/BytesRow/BytesLabel");
        _prestigeLabel = GetNode<Label>("RootControl/CurrencyPanel/VBoxContainer/PrestigeLabel");

        // Кнопка открытия меню в правом нижнем углу (проверьте точное имя узла в вашем дереве!)
        _openUpgradesBtn = GetNode<Button>("RootControl/BottomRightMenu/UpgradesButton");

        // Сама панель улучшений и её кнопки
        _upgradesPanel = GetNode<PanelContainer>("RootControl/UpgradesPanel");
        var vbox = _upgradesPanel.GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        _closeUpgradesBtn = vbox.GetNode<Button>("HeaderRow/CloseButton");
        _buyBugBtn = vbox.GetNode<Button>("BuyBugButton");
        _upgradeDamageBtn = vbox.GetNode<Button>("UpgradeDamageButton");

        // Скрываем панель на старте
        _upgradesPanel.Visible = false;

        // Подключаем нажатия кнопок UI
        _openUpgradesBtn.Pressed += ToggleUpgradesPanel;
        _closeUpgradesBtn.Pressed += () => _upgradesPanel.Visible = false;
        _buyBugBtn.Pressed += OnBuyBugPressed;
        _upgradeDamageBtn.Pressed += OnUpgradeDamagePressed;

        // Подписываемся на события GameManager
        GameManager.Instance.OnBytesChanged += UpdateBytes;
        GameManager.Instance.OnPrestigePointsChanged += UpdatePrestige;

        // Инициализируем начальные значения на экране
        UpdateBytes(GameManager.Instance.Bytes);
        UpdatePrestige(GameManager.Instance.PrestigePoints);
        RefreshUpgradesUI();
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBytesChanged -= UpdateBytes;
            GameManager.Instance.OnPrestigePointsChanged -= UpdatePrestige;
        }
    }

    private void ToggleUpgradesPanel()
    {
        _upgradesPanel.Visible = !_upgradesPanel.Visible;
        if (_upgradesPanel.Visible)
        {
            RefreshUpgradesUI();
        }
    }

    private void UpdateBytes(long newBytes)
    {
        _bytesLabel.Text = NumberFormatter.Format(newBytes);

        var tween = CreateTween();
        _bytesLabel.PivotOffset = _bytesLabel.Size / 2;
        tween.TweenProperty(_bytesLabel, "scale", new Vector2(1.15f, 1.15f), 0.08f);
        tween.TweenProperty(_bytesLabel, "scale", Vector2.One, 0.08f);

        // При каждом изменении баланса обновляем доступность кнопок покупки
        RefreshUpgradesUI();
    }

    private void UpdatePrestige(long newPrestige)
    {
        _prestigeLabel.Text = $"Прошивка: {NumberFormatter.Format(newPrestige)}";
    }

    private void RefreshUpgradesUI()
    {
        if (_upgradesPanel == null || !_upgradesPanel.Visible) return;

        var gm = GameManager.Instance;

        // Обновляем текст и цену покупки бага
        string bugCostText = NumberFormatter.Format(gm.NextBugCost);
        _buyBugBtn.Text = $"Запустить баг [{gm.ActiveBugSlots}] — Цена: {bugCostText}";
        _buyBugBtn.Disabled = gm.Bytes < gm.NextBugCost;

        // Обновляем текст и цену прокачки урона
        string dmgCostText = NumberFormatter.Format(gm.DamageUpgradeCost);
        _upgradeDamageBtn.Text = $"Урон клика ({gm.TotalClickDamage} -> {gm.TotalClickDamage + 1}) — Цена: {dmgCostText}";
        _upgradeDamageBtn.Disabled = gm.Bytes < gm.DamageUpgradeCost;
    }

    private void OnBuyBugPressed()
    {
        if (GameManager.Instance.TryBuyBugSlot())
        {
            RefreshUpgradesUI();
        }
    }

    private void OnUpgradeDamagePressed()
    {
        if (GameManager.Instance.TryBuyRunDamage())
        {
            RefreshUpgradesUI();
        }
    }
}