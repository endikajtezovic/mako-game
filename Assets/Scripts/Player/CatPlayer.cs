using Godot;

// Simple platformer controller for the cat character.
public partial class CatPlayer : CharacterBody2D
{
    [Export] public float MoveSpeed  = 200f;
    [Export] public float JumpForce  = -480f;
    [Export] public float Gravity    = 1400f;
    [Export] public float FallGravity = 2000f;

    private Vector2 _velocity   = Vector2.Zero;
    private Vector2 _spawnPoint;

    public override void _Ready()
    {
        _spawnPoint = Position;

        EnsureAction("move_left",  Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("jump",       Key.Space);

        var cam = new Camera2D();
        cam.PositionSmoothingEnabled = true;
        cam.PositionSmoothingSpeed   = 6f;
        cam.LimitLeft                = 0;
        AddChild(cam);
    }

    private static void EnsureAction(string action, Key key)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
            var ev = new InputEventKey { Keycode = key };
            InputMap.ActionAddEvent(action, ev);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt      = (float)delta;
        bool  onFloor = IsOnFloor();

        float dir_x = Input.GetAxis("move_left", "move_right");
        _velocity.X = dir_x * MoveSpeed;

        if (onFloor && Input.IsActionJustPressed("jump"))
            _velocity.Y = JumpForce;

        // Short hop
        if (Input.IsActionJustReleased("jump") && _velocity.Y < 0f)
            _velocity.Y *= 0.5f;

        if (!onFloor)
            _velocity.Y += (_velocity.Y > 0f ? FallGravity : Gravity) * dt;
        else if (_velocity.Y > 0f)
            _velocity.Y = 0f;

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;
    }
}
