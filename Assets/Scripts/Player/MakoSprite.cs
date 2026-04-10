using Godot;

// Drives AnimatedSprite2D using Mako's sprite folder.
// Uses separate east/west frames for walk and jump — no flipping needed.
public partial class MakoSprite : Node2D
{
    private AnimatedSprite2D _sprite;

    private const string WalkBase = "res://Assets/Sprites/Player/mako-sprite/animations/Walking-c1cb9d4b/";
    private const string JumpBase = "res://Assets/Sprites/Player/mako-sprite/animations/Jumping-6cace7bf/";
    private const string RotBase  = "res://Assets/Sprites/Player/mako-sprite/rotations/";

    private const int WalkFrames = 6;
    private const int JumpFrames = 8;

    public enum AnimState { Idle, WalkEast, WalkWest, JumpEast, JumpWest }
    private AnimState _state       = AnimState.Idle;
    private bool      _facingWest  = false;

    public override void _Ready()
    {
        _sprite = new AnimatedSprite2D();
        _sprite.Scale = new Vector2(2f, 2f);

        var frames = new SpriteFrames();
        _sprite.SpriteFrames = frames;

        AddAnim(frames, "walk_east", WalkBase + "east/", WalkFrames, fps: 10f, loop: true);
        AddAnim(frames, "walk_west", WalkBase + "west/", WalkFrames, fps: 10f, loop: true);
        AddAnim(frames, "jump_east", JumpBase + "east/", JumpFrames, fps: 12f, loop: false);
        AddAnim(frames, "jump_west", JumpBase + "west/", JumpFrames, fps: 12f, loop: false);

        // Idle: front-facing south rotation
        frames.AddAnimation("idle");
        frames.SetAnimationSpeed("idle", 1f);
        frames.SetAnimationLoop("idle", false);
        frames.AddFrame("idle", Load(RotBase + "south.png"), 0);

        _sprite.Animation = "idle";
        _sprite.Play();
        AddChild(_sprite);
    }

    private static void AddAnim(SpriteFrames frames, string name, string dir, int count, float fps, bool loop)
    {
        frames.AddAnimation(name);
        frames.SetAnimationSpeed(name, fps);
        frames.SetAnimationLoop(name, loop);
        for (int i = 0; i < count; i++)
            frames.AddFrame(name, Load(dir + $"frame_00{i}.png"), i);
    }

    private static Texture2D Load(string path)
        => ResourceLoader.Load<Texture2D>(path);

    public override void _Process(double delta)
    {
        var body = GetParent<CharacterBody2D>();
        if (body == null) return;

        float vx      = body.Velocity.X;
        float vy      = body.Velocity.Y;
        bool  onFloor = body.IsOnFloor();

        // Track facing direction independently so idle holds the last direction
        if (Mathf.Abs(vx) > 10f)
            _facingWest = vx < 0f;

        AnimState desired;
        if (!onFloor)
            desired = _facingWest ? AnimState.JumpWest : AnimState.JumpEast;
        else if (Mathf.Abs(vx) > 10f)
            desired = _facingWest ? AnimState.WalkWest : AnimState.WalkEast;
        else
            desired = AnimState.Idle;

        if (desired != _state)
        {
            _state = desired;
            string anim = _state switch
            {
                AnimState.WalkEast => "walk_east",
                AnimState.WalkWest => "walk_west",
                AnimState.JumpEast => "jump_east",
                AnimState.JumpWest => "jump_west",
                _                  => "idle",
            };
            _sprite.Play(anim);
        }
    }
}
