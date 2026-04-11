using Godot;

public partial class LevelBuilder : Node
{
    private const int   TileSize    = 32;
    private const float LevelWidth  = 1152f;  // matches Summer1 scaled to screen
    private const float LevelHeight = 648f;
    private const float GroundY     = 544f;   // pixel Y of ground surface

    public override void _Ready()
    {
        SpawnBackground();
        SpawnGround();
        SpawnLevelEnd();
    }

    private void SpawnBackground()
    {
        var tex = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Backgrounds/summer1.png");
        if (tex == null) return;

        var bg = new Sprite2D();
        bg.Texture  = tex;
        bg.Scale    = new Vector2(0.5f, 0.5f);  // 2304x1296 → 1152x648
        bg.Position = new Vector2(LevelWidth / 2f, LevelHeight / 2f);
        bg.ZIndex   = -10;
        AddChild(bg);
    }

    private void SpawnGround()
    {
        // Invisible collision-only ground across the full level width
        var body  = new StaticBody2D();
        var shape = new CollisionShape2D();
        var rect  = new RectangleShape2D();

        rect.Size     = new Vector2(LevelWidth, TileSize * 3);
        shape.Shape   = rect;
        body.Position = new Vector2(LevelWidth / 2f, GroundY + TileSize * 1.5f);

        body.AddChild(shape);
        AddChild(body);
    }

    private void SpawnLevelEnd()
    {
        var area  = new Area2D();
        var shape = new CollisionShape2D();
        var rect  = new RectangleShape2D();

        rect.Size     = new Vector2(TileSize * 2, TileSize * 8);
        shape.Shape   = rect;
        area.Position = new Vector2(LevelWidth - TileSize, GroundY - TileSize * 3);
        area.AddChild(shape);

        area.BodyEntered += (body) =>
        {
            if (body is Player)
                GameManager.Instance.CallDeferred("LoadScene", "res://Assets/Scenes/Level2.tscn");
        };

        AddChild(area);
    }
}
