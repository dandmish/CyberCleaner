using Godot;
using System;

public partial class Bug : Area2D
{
	[Export] public int MaxHealth = 3;
	[Export] public int ByteReward = 10;

	private int _currentHealth;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_sprite = GetNode<Sprite2D>("Sprite2D");
		InputPickable = true;
	}

	// Переопределяем метод вместо подписки на событие
	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mb &&
			mb.Pressed &&
			mb.ButtonIndex == MouseButton.Left)
		{
			TakeDamage(1);
		}
	}

	private void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		GD.Print($"Урон! HP: {_currentHealth}/{MaxHealth}");

		var tween = CreateTween();
		tween.TweenProperty(_sprite, "scale", _sprite.Scale * 0.8f, 0.05f);
		tween.TweenProperty(_sprite, "scale", _sprite.Scale, 0.05f);

		if (_currentHealth <= 0)
			Die();
	}

	private void Die()
	{
		GD.Print($"Баг уничтожен! +{ByteReward} байт");

		var tween = CreateTween();
		tween.TweenProperty(_sprite, "modulate:a", 0.0f, 0.2f);
		tween.TweenProperty(this, "scale", Vector2.Zero, 0.2f);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
