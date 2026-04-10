using Godot;

// Drives AnimatedSprite2D using Mako's hand-drawn PNG frames
public partial class MakoSprite : Node2D
{
    private AnimatedSprite2D _sprite;

    public enum AnimState { Idle, Walk, Jump, Fall, Land }
    private AnimState _state = AnimState.Idle;

    public override void _Ready()
    {
        _sprite = new AnimatedSprite2D();
        _sprite.Scale = new Vector2(3f, 3f);  // scale up from 32px to ~96px on screen

        var frames = new SpriteFrames();
        _sprite.SpriteFrames = frames;

        // ── Walk animation — all 4 hand-drawn frames ─────────────────────────
        frames.AddAnimation("walk");
        frames.SetAnimationSpeed("walk", 8f);
        frames.SetAnimationLoop("walk", true);
        frames.AddFrame("walk", Load("makosprite.png"),  0);
        frames.AddFrame("walk", Load("makosprite2.png"), 1);
        frames.AddFrame("walk", Load("makosprite3.png"), 2);
        frames.AddFrame("walk", Load("makosprite4.png"), 3);

        // ── Idle — just frame 0 held ──────────────────────────────────────────
        frames.AddAnimation("idle");
        frames.SetAnimationSpeed("idle", 2f);
        frames.SetAnimationLoop("idle", true);
        frames.AddFrame("idle", Load("makosprite.png"), 0);
        frames.AddFrame("idle", Load("makosprite2.png"), 1);

        // ── Jump / Fall / Land — use frame 0 as placeholder ──────────────────
        foreach (var anim in new[] { "jump", "fall", "land" })
        {
            frames.AddAnimation(anim);
            frames.SetAnimationSpeed(anim, 1f);
            frames.SetAnimationLoop(anim, false);
            frames.AddFrame(anim, Load("makosprite.png"), 0);
        }

        _sprite.Animation = "idle";
        _sprite.Play();
        AddChild(_sprite);
    }

    private static Texture2D Load(string filename)
        => ResourceLoader.Load<Texture2D>($"res://Assets/Sprites/Player/{filename}");

    public override void _Process(double delta)
    {
        var body = GetParent<CharacterBody2D>();
        if (body == null) return;

        float vx = body.Velocity.X;
        float vy = body.Velocity.Y;
        bool  onFloor = body.IsOnFloor();

        AnimState desired;
        if (!onFloor)
            desired = vy < 0f ? AnimState.Jump : AnimState.Fall;
        else
            desired = Mathf.Abs(vx) > 10f ? AnimState.Walk : AnimState.Idle;

        if (desired != _state)
        {
            _state = desired;
            string anim = _state switch
            {
                AnimState.Walk => "walk",
                AnimState.Jump => "jump",
                AnimState.Fall => "fall",
                AnimState.Land => "land",
                _              => "idle",
            };
            _sprite.Play(anim);
        }
    }
}
