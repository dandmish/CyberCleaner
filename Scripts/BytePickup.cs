using Godot;
using System;

public partial class BytePickup : Area2D
{
    [Export] public int Value = 1;
    [Export] public float MoveSpeed = 650.0f;

    private AudioStreamPlayer _pickupSound;
    private Sprite2D _sprite;
    private bool _isCollected = false;
    private bool _canBeCollected = false;

    public override void _Ready()
    {
        _pickupSound = GetNode<AudioStreamPlayer>("PickupSound");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        InputPickable = true; // Разрешаем клики мышкой по байту

        // Эффект "вылета" из взорвавшегося бага
        Vector2 randomOffset = new Vector2(
            (float)GD.RandRange(-60.0, 60.0),
            (float)GD.RandRange(-60.0, 60.0)
        );

        var popTween = CreateTween();
        popTween.TweenProperty(this, "global_position", GlobalPosition + randomOffset, 0.2f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        popTween.TweenCallback(Callable.From(() => _canBeCollected = true));
    }

    // Ручной подбор по клику ЛКМ (пока не прокачан авто-сбор при наведении)
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (!_canBeCollected || _isCollected) return;

        if (@event is InputEventMouseButton mb &&
            mb.Pressed &&
            mb.ButtonIndex == MouseButton.Left)
        {
            Collect();
        }
    }

    public override void _Process(double delta)
    {
        if (!_canBeCollected || _isCollected) return;

        // Если прокачан талант авто-сбора — притягиваем байт в радиусе PickupRadius
        if (GameManager.Instance.AutoCollectOnHover)
        {
            Vector2 mousePos = GetGlobalMousePosition();
            float distance = GlobalPosition.DistanceTo(mousePos);

            if (distance <= GameManager.Instance.PickupRadius)
            {
                GlobalPosition = GlobalPosition.MoveToward(mousePos, MoveSpeed * (float)delta);
                if (distance < 15.0f)
                    Collect();
            }
        }
    }

    private void Collect()
    {
        _isCollected = true;
        InputPickable = false;
        _sprite.Visible = false;

        GameManager.Instance.AddBytes(Value);

        _pickupSound.PitchScale = (float)GD.RandRange(0.95, 1.25);
        _pickupSound.Play();
        _pickupSound.Finished += QueueFree;
    }
}