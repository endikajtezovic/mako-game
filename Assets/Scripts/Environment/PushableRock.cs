using Godot;

// A rock the player can grab (hold E) and push or pull.
// Uses CharacterBody2D so it sits on floors and can be jumped over.
[GlobalClass]
public partial class PushableRock : CharacterBody2D
{
    [Export] public float Gravity    = 1400f;
    [Export] public float RockWidth  = 48f;
    [Export] public float RockHeight = 56f;   // tall enough to be a small obstacle, jumpable

    private Vector2 _velocity   = Vector2.Zero;
    private bool    _grabbed    = false;
    private float   _grabVelX   = 0f;

    public override void _Ready()
    {
        AddToGroup("pushable");

        var shape = new CollisionShape2D();
        var rect  = new RectangleShape2D();
        rect.Size    = new Vector2(RockWidth, RockHeight);
        shape.Shape  = rect;
        AddChild(shape);
    }

    public void Grab(float velocityX)
    {
        _grabbed  = true;
        _grabVelX = velocityX;
    }

    public void Release()
    {
        _grabbed  = false;
        _grabVelX = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (!IsOnFloor())
            _velocity.Y += Gravity * dt;
        else
            _velocity.Y = 0f;

        _velocity.X = _grabbed ? _grabVelX : 0f;

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;
    }

    public override void _Draw()
    {
        float hw = RockWidth  / 2f;
        float hh = RockHeight / 2f;

        // Rock body
        DrawRect(new Rect2(-hw, -hh, RockWidth, RockHeight), new Color(0.42f, 0.38f, 0.34f));

        // Highlight top-left
        DrawRect(new Rect2(-hw, -hh, RockWidth, 4), new Color(0.58f, 0.54f, 0.50f));
        DrawRect(new Rect2(-hw, -hh, 4, RockHeight), new Color(0.55f, 0.51f, 0.47f));

        // Shadow bottom-right
        DrawRect(new Rect2(-hw, hh - 4, RockWidth, 4), new Color(0.22f, 0.18f, 0.16f));
        DrawRect(new Rect2(hw - 4, -hh, 4, RockHeight), new Color(0.22f, 0.18f, 0.16f));

        // Crack detail
        DrawLine(new Vector2(-6, -hh + 8), new Vector2(4, -4), new Color(0.28f, 0.24f, 0.22f), 2f);
        DrawLine(new Vector2(8, -2), new Vector2(hw - 6, hh - 10), new Color(0.28f, 0.24f, 0.22f), 2f);
    }
}
