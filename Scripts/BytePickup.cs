using Godot;
using System;

public partial class BytePickup : Area2D
{
    [Export] public int Value = 2; // Сколько байтов дает один шарик
    [Export] public float MagnetRadius = 150.0f; // Радиус притяжения к курсору
    [Export] public float MoveSpeed = 600.0f;

    private AudioStreamPlayer _pickupSound;
    private Sprite2D _sprite;
    private bool _isCollected = false;
    private bool _canBeCollected = false;

    public override void _Ready()
    {
        _pickupSound = GetNode<AudioStreamPlayer>("PickupSound");
        _sprite = GetNode<Sprite2D>("Sprite2D");

        // Эффект "вылета" из взорвавшегося бага в случайную сторону
        Vector2 randomOffset = new Vector2(
            (float)GD.RandRange(-70.0, 70.0),
            (float)GD.RandRange(-70.0, 70.0)
        );

        var popTween = CreateTween();
        popTween.TweenProperty(this, "global_position", GlobalPosition + randomOffset, 0.25f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

        // Разрешаем подбор сразу после вылета
        popTween.TweenCallback(Callable.From(() => _canBeCollected = true));
    }

    public override void _Process(double delta)
    {
        if (!_canBeCollected || _isCollected)
            return;

        Vector2 mousePos = GetGlobalMousePosition();
        float distance = GlobalPosition.DistanceTo(mousePos);

        // Если курсор рядом — плавно летим к нему
        if (distance <= MagnetRadius)
        {
            GlobalPosition = GlobalPosition.MoveToward(mousePos, MoveSpeed * (float)delta);

            // Когда долетели до центра курсора — подбираем
            if (distance < 15.0f)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        _isCollected = true;
        _sprite.Visible = false; // Прячем картинку, но не удаляем узел, пока играет звук

        GameManager.Instance.AddBytes(Value);

        // Случайный тон звука, чтобы при сборе горсти байтов звучало мелодично
        _pickupSound.PitchScale = (float)GD.RandRange(0.95, 1.25);
        _pickupSound.Play();

        // Удаляем объект строго после того, как Pickup.wav доиграет до конца
        _pickupSound.Finished += QueueFree;
    }
}