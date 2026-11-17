using Godot;
using System;

public partial class Bug : CharacterBody2D
{
    [Export] public int MaxHealth = 3;
    [Export] public int BaseByteValue = 1;
    [Export] public int DropCount = 1;
    [Export] public float MoveSpeed = 60.0f; // Скорость дрейфа по экрану
    [Export] public PackedScene FloatingTextScene;
    [Export] public PackedScene BytePickupScene;

    public bool IsGolden { get; private set; } = false;

    private int _currentHealth;
    private bool _isDead = false;
    private Vector2 _moveDirection;
    private Sprite2D _sprite;
    private AudioStreamPlayer _hitSound;
    private Vector2 _initialScale;
    private Tween _hitTween;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _hitSound = GetNode<AudioStreamPlayer>("HitSound");
        _initialScale = _sprite.Scale;
        InputPickable = true;

        // Проверяем шанс стать Золотым багом
        if (GD.Randf() < GameManager.Instance.GoldenBugChance)
        {
            MakeGolden();
        }

        _currentHealth = MaxHealth;

        // Выбираем случайное направление дрейфа
        float randomAngle = (float)GD.RandRange(0, Mathf.Tau);
        _moveDirection = Vector2.FromAngle(randomAngle);
    }

    private void MakeGolden()
    {
        IsGolden = true;
        _sprite.Modulate = new Color(1.0f, 0.84f, 0.0f);
        MaxHealth = Mathf.RoundToInt(MaxHealth * 1.5f);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead) return;

        // Двигаем жука и проверяем физическое столкновение со стенами ArenaWalls
        Vector2 motion = _moveDirection * MoveSpeed * (float)delta;
        KinematicCollision2D collision = MoveAndCollide(motion);

        if (collision != null)
        {
            // Отскакиваем от стены под углом отражения
            _moveDirection = _moveDirection.Bounce(collision.GetNormal()).Normalized();
        }
    }

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (_isDead) return;

        if (@event is InputEventMouseButton mb &&
            mb.Pressed &&
            mb.ButtonIndex == MouseButton.Left)
        {
            TakeDamage(GameManager.Instance.TotalClickDamage);
        }
    }

    private void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        _hitSound.PitchScale = (float)GD.RandRange(0.9, 1.1);
        _hitSound.Play();

        if (FloatingTextScene != null)
        {
            var textInstance = FloatingTextScene.Instantiate<FloatingText>();
            GetTree().CurrentScene.AddChild(textInstance);
            Color dmgColor = IsGolden ? Colors.Gold : Colors.Yellow;
            textInstance.Setup($"-{damage}", dmgColor, GlobalPosition);
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
        _isDead = true;
        InputPickable = false;
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _hitTween?.Kill();

        int totalDrop = DropCount + GameManager.Instance.BonusByteDrop;
        int byteMultiplier = IsGolden ? GameManager.Instance.GoldenBugMultiplier : 1;

        if (BytePickupScene != null)
        {
            for (int i = 0; i < totalDrop; i++)
            {
                var pickup = BytePickupScene.Instantiate<BytePickup>();
                pickup.GlobalPosition = GlobalPosition;
                pickup.Value = BaseByteValue * byteMultiplier;

                if (IsGolden)
                    pickup.Modulate = new Color(1.0f, 0.85f, 0.1f);

                GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, pickup);
            }
        }

        // Примечание: если у вас есть отдельный узел звука смерти жука, запустите его здесь

        var deathTween = CreateTween();
        deathTween.SetParallel(true);
        deathTween.TweenProperty(_sprite, "modulate:a", 0.0f, 0.2f);
        deathTween.TweenProperty(this, "scale", Vector2.Zero, 0.2f);
        deathTween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}