using Godot;

// Animated sprite controller for the cat sidekick character.
public partial class CatSprite : Node2D
{
    private AnimatedSprite2D _sprite;

    private const string WalkBase = "res://Assets/Sprites/Cat/animations/animation-742400f8/";
    private const string IdleBase = "res://Assets/Sprites/Cat/animations/Seated_on_Belly_Idle-cafa95cf/";

    private const int WalkFrames = 8;
    private const int IdleFrames = 10;

    private bool _facingWest = false;

    public override void _Ready()
    {
        _sprite = new AnimatedSprite2D();
        _sprite.Scale = new Vector2(2f, 2f);

        var frames = new SpriteFrames();
        _sprite.SpriteFrames = frames;

        AddAnim(frames, "walk_east", WalkBase + "east/", WalkFrames, fps: 10f, loop: true);
        AddAnim(frames, "walk_west", WalkBase + "west/", WalkFrames, fps: 10f, loop: true);
        AddAnim(frames, "idle",      IdleBase + "south/", IdleFrames, fps: 8f, loop: true);

        _sprite.Animation = "idle";
        _sprite.Play();
        AddChild(_sprite);
    }

    private static void AddAnim(SpriteFrames f, string name, string dir, int count, float fps, bool loop)
    {
        f.AddAnimation(name);
        f.SetAnimationSpeed(name, fps);
        f.SetAnimationLoop(name, loop);
        for (int i = 0; i < count; i++)
            f.AddFrame(name, ResourceLoader.Load<Texture2D>(dir + $"frame_00{i}.png"), i);
    }

    public override void _Process(double delta)
    {
        var body = GetParent<CharacterBody2D>();
        if (body == null) return;

        float vx = body.Velocity.X;

        if (Mathf.Abs(vx) > 10f)
            _facingWest = vx < 0f;

        string desired = Mathf.Abs(vx) > 10f
            ? (_facingWest ? "walk_west" : "walk_east")
            : "idle";

        if (_sprite.Animation != desired)
            _sprite.Play(desired);
    }
}
