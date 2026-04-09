using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float MoveSpeed       = 220f;
    [Export] public float JumpForce       = -500f;
    [Export] public float Gravity         = 1400f;
    [Export] public float FallGravity     = 2000f;  // faster fall, snappier arc
    [Export] public float CoyoteTime      = 0.12f;  // seconds you can still jump after leaving a ledge
    [Export] public float JumpBufferTime  = 0.12f;  // seconds a jump press is remembered before landing

    private Vector2 _velocity     = Vector2.Zero;
    private float   _coyoteTimer  = 0f;
    private float   _jumpBuffer   = 0f;
    private bool    _wasOnFloor   = false;

    public override void _Ready()
    {
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
            var ev = new InputEventKey();
            ev.Keycode = key;
            InputMap.ActionAddEvent(action, ev);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt      = (float)delta;
        bool  onFloor = IsOnFloor();

        // -- Coyote time --
        // Give a grace window to jump after walking off a ledge
        if (_wasOnFloor && !onFloor)
            _coyoteTimer = CoyoteTime;
        else if (onFloor)
            _coyoteTimer = CoyoteTime;
        else
            _coyoteTimer -= dt;

        _wasOnFloor = onFloor;

        // -- Jump buffer --
        // Remember a jump press for a moment so hitting it slightly early still works
        if (Input.IsActionJustPressed("jump"))
            _jumpBuffer = JumpBufferTime;
        else
            _jumpBuffer -= dt;

        // -- Jump --
        bool canJump = _coyoteTimer > 0f;
        if (_jumpBuffer > 0f && canJump)
        {
            _velocity.Y  = JumpForce;
            _coyoteTimer = 0f;
            _jumpBuffer  = 0f;
        }

        // -- Variable jump height --
        // Release jump early = shorter hop
        if (Input.IsActionJustReleased("jump") && _velocity.Y < 0f)
            _velocity.Y *= 0.45f;

        // -- Gravity --
        // Fall faster than you rise for a snappier, less floaty arc
        float grav = _velocity.Y > 0f ? FallGravity : Gravity;
        if (!onFloor)
            _velocity.Y += grav * dt;

        // -- Horizontal --
        float dir = Input.GetAxis("move_left", "move_right");
        _velocity.X = dir * MoveSpeed;

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;
    }
}
