using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private ProgressBar _ramBar;
    private Label _ramLabel;
    private Label _bytesLabel;

    public override void _Ready()
    {
        // Получаем ссылки на узлы (убедитесь, что пути совпадают с вашей иерархией)
        _ramBar = GetNode<ProgressBar>("MarginContainer/HBoxContainer/RamBar");
        _ramLabel = _ramBar.GetNode<Label>("RamLabel");
        _bytesLabel = GetNode<Label>("MarginContainer/HBoxContainer/BytesLabel");

        // Подписываемся на события GameManager
        GameManager.Instance.OnBytesChanged += UpdateBytes;
        GameManager.Instance.OnRamChanged += UpdateRam;

        // Принудительно обновляем UI на старте, чтобы показать начальные значения
        UpdateBytes(GameManager.Instance.Bytes);
        UpdateRam(GameManager.Instance.CurrentRam, GameManager.Instance.MaxRam);
    }

    // Обязательно отписываемся от C#-событий при удалении узла, чтобы избежать утечек памяти
    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBytesChanged -= UpdateBytes;
            GameManager.Instance.OnRamChanged -= UpdateRam;
        }
    }

    private void UpdateBytes(long newBytes)
    {
        _bytesLabel.Text = $"Bytes: {newBytes} B";

        // Небольшой "прыжок" текста при получении байтов
        var tween = CreateTween();
        _bytesLabel.PivotOffset = _bytesLabel.Size / 2;
        tween.TweenProperty(_bytesLabel, "scale", new Vector2(1.2f, 1.2f), 0.08f);
        tween.TweenProperty(_bytesLabel, "scale", Vector2.One, 0.08f);
    }

    private void UpdateRam(int current, int max)
    {
        _ramBar.MaxValue = max;
        _ramBar.Value = current;
        _ramLabel.Text = $"RAM: {current} / {max} MB";
    }
}