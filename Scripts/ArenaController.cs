using Godot;
using System;

public partial class ArenaController : Node2D
{
    [Export] public PackedScene BugScene;
    [Export] public float RespawnDelay = 1.0f;

    // Границы зеленого текстолита внутри вашей металлической рамки (подгоните под свой экран)
    [Export] public Rect2 PlayableArea = new Rect2(160, 140, 1600, 800);

    private float _respawnTimer = 0.0f;

    public override void _Ready()
    {
        // Мгновенно спавним жука, когда игрок покупает его в меню Улучшений
        GameManager.Instance.OnBugPurchased += SpawnBug;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBugPurchased -= SpawnBug;
    }

    public override void _Process(double delta)
    {
        if (BugScene == null) return;

        int aliveBugs = GetTree().GetNodesInGroup("bugs").Count;

        // Если купленных слотов больше, чем живых жуков на поле — возрождаем недостающего
        if (aliveBugs < GameManager.Instance.ActiveBugSlots)
        {
            _respawnTimer += (float)delta;
            if (_respawnTimer >= RespawnDelay)
            {
                _respawnTimer = 0.0f;
                SpawnBug();
            }
        }
        else
        {
            _respawnTimer = 0.0f;
        }
    }

    private void SpawnBug()
    {
        if (BugScene == null) return;

        var bug = BugScene.Instantiate<Node2D>();
        float randomX = (float)GD.RandRange(PlayableArea.Position.X, PlayableArea.End.X);
        float randomY = (float)GD.RandRange(PlayableArea.Position.Y, PlayableArea.End.Y);

        bug.GlobalPosition = new Vector2(randomX, randomY);
        bug.AddToGroup("bugs");
        GetTree().CurrentScene.AddChild(bug);
    }
}