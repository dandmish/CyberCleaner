using Godot;
using System;

public partial class Bug : Area2D
{
	[Export] public int MaxHealth = 3;
	[Export] public int ByteReward = 10;
	[Export] public PackedScene FloatingTextScene;
	[Export] public PackedScene BytePickupScene;
	[Export] public int DropCount = 5;

	private int _currentHealth;
	private bool _isDead = false;
	private Sprite2D _sprite;
	private AudioStreamPlayer _hitSound; // Ссылка на плеер звука
	private Vector2 _initialScale;
	private Tween _hitTween;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_hitSound = GetNode<AudioStreamPlayer>("HitSound"); // Получаем узел
		_initialScale = _sprite.Scale;
		InputPickable = true;
	}

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (_isDead) return;

		if (@event is InputEventMouseButton mb &&
			mb.Pressed &&
			mb.ButtonIndex == MouseButton.Left)
		{
			if (GameManager.Instance.TryConsumeRam(GameManager.Instance.ClickRamCost))
			{
				TakeDamage(GameManager.Instance.ClickDamage);
			}
		}
	}

	private void TakeDamage(int damage)
	{
		_currentHealth -= damage;

		// Проигрываем звук удара со случайной высотой тона (от 0.9 до 1.1)
		_hitSound.PitchScale = (float)GD.RandRange(0.9, 1.1);
		_hitSound.Play();

		if (FloatingTextScene != null)
		{
			var textInstance = FloatingTextScene.Instantiate<FloatingText>();
			GetTree().CurrentScene.AddChild(textInstance);
			textInstance.Setup($"-{damage}", Colors.Yellow, GlobalPosition);
		}

		_hitTween?.Kill();
		_hitTween = CreateTween();
		_hitTween.TweenProperty(_sprite, "scale", _initialScale * 0.8f, 0.05f);
		_hitTween.TweenProperty(_sprite, "scale", _initialScale, 0.05f);

		if (_currentHealth <= 0)
			Die();
	}

	private void Die()
	{
        int totalDrop = DropCount + GameManager.Instance.BonusByteDrop;

        if (BytePickupScene != null)
        {
            for (int i = 0; i < totalDrop; i++)
            {
                var pickup = BytePickupScene.Instantiate<BytePickup>();
                pickup.GlobalPosition = GlobalPosition;
                GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, pickup);
            }
        }
        
		_isDead = true;
		InputPickable = false;
		_hitTween?.Kill();

		if (BytePickupScene != null)
		{
			for (int i = 0; i < DropCount; i++)
			{
				var pickup = BytePickupScene.Instantiate<BytePickup>();
				pickup.GlobalPosition = GlobalPosition;
				// Используем CallDeferred, чтобы безопасно добавить узлы во время анимации смерти
				GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, pickup);
			}
		}

		var deathTween = CreateTween();
		deathTween.SetParallel(true);
		deathTween.TweenProperty(_sprite, "modulate:a", 0.0f, 0.2f);
		deathTween.TweenProperty(this, "scale", Vector2.Zero, 0.2f);
		deathTween.Chain().TweenCallback(Callable.From(QueueFree));
	}
}
