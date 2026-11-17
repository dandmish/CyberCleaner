using Godot;
using System;

public partial class FloatingText : Label
{
    public void Setup(string text, Color color, Vector2 startPosition)
    {
        Text = text;
        Modulate = color;
        GlobalPosition = startPosition;

        // Небольшой случайный разброс по горизонтали, чтобы цифры не слипались
        float randomX = (float)GD.RandRange(-30.0, 30.0);
        Vector2 targetPos = startPosition + new Vector2(randomX, -60f);

        var tween = CreateTween();
        tween.SetParallel(true);
        // Текст плавно улетает вверх с замедлением (EaseOut)
        tween.TweenProperty(this, "global_position", targetPos, 0.5f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);
        // И растворяется в конце полета
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f)
             .SetDelay(0.2f);

        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}