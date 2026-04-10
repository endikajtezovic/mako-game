using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float MoveSpeed       = 220f;
    [Export] public float JumpForce       = -520f;
    [Export] public float Gravity         = 1400f;
    [Export] public float FallGravity     = 2200f;
    [Export] public float CoyoteTime      = 0.12f;
    [Export] public float JumpBufferTime  = 0.12f;

    [Export] public float WallSlideSpeed  = 80f;    // max fall speed while wall sliding
    [Export] public float WallJumpForceX  = 280f;   // horizontal kick off wall
    [Export] public float WallJumpForceY  = -500f;  // vertical kick off wall
    [Export] public float WallJumpLerp    = 8f;     // how quickly horizontal control returns

    [Export] public float DashSpeed       = 520f;
    [Export] public float DashDuration    = 0.18f;

    private Vector2 _velocity     = Vector2.Zero;
    private float   _coyoteTimer  = 0f;
    private float   _jumpBuffer   = 0f;
    private bool    _wasOnFloor   = false;
    private Vector2 _spawnPoint;

    // Wall
    private bool  _wallSliding  = false;
    private bool  _wallJumped   = false;
    private float _wallJumpTimer = 0f;   // counts up after a wall jump

    // Double jump
    [Export] public int ExtraJumps = 1;   // set to 2 in Inspector for triple jump, etc.
    private int _jumpsLeft = 0;

    // Dash
    private bool    _hasDashed  = false;
    private bool    _isDashing  = false;
    private float   _dashTimer  = 0f;
    private Vector2 _dashDir    = Vector2.Zero;

    public void Respawn()
    {
        Position   = _spawnPoint;
        _velocity  = Vector2.Zero;
        _isDashing = false;
        _dashTimer = 0f;
    }

    public override void _Ready()
    {
        _spawnPoint = Position;

        EnsureAction("move_left",  Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("move_up",    Key.W);
        EnsureAction("move_down",  Key.S);
        EnsureAction("jump",       Key.Space);
        EnsureAction("dash",       Key.X);

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
        bool  onWall  = IsOnWall();

        // ── Landing reset ────────────────────────────────────────────────────
        if (onFloor)
        {
            _hasDashed     = false;
            _wallJumped    = false;
            _wallJumpTimer = 0f;
            _jumpsLeft     = ExtraJumps;
        }

        // ── Dash ─────────────────────────────────────────────────────────────
        if (Input.IsActionJustPressed("dash") && !_hasDashed && !_isDashing)
        {
            float dx = Input.GetAxis("move_left", "move_right");
            float dy = Input.GetAxis("move_up",   "move_down");
            Vector2 dir = new Vector2(dx, -dy).Normalized();
            if (dir == Vector2.Zero) dir = new Vector2(_velocity.X >= 0 ? 1 : -1, 0);

            _dashDir   = dir;
            _isDashing = true;
            _hasDashed = true;
            _dashTimer = 0f;
        }

        if (_isDashing)
        {
            _dashTimer += dt;
            _velocity   = _dashDir * DashSpeed;

            if (_dashTimer >= DashDuration)
            {
                _isDashing  = false;
                // Preserve a bit of momentum coming out of dash
                _velocity  *= 0.25f;
            }

            Velocity = _velocity;
            MoveAndSlide();
            _velocity = Velocity;
            return; // skip normal physics while dashing
        }

        // ── Coyote time ──────────────────────────────────────────────────────
        if (_wasOnFloor && !onFloor && !_wallJumped)
            _coyoteTimer = CoyoteTime;
        else if (onFloor)
            _coyoteTimer = CoyoteTime;
        else
            _coyoteTimer -= dt;

        _wasOnFloor = onFloor;

        // ── Jump buffer ───────────────────────────────────────────────────────
        if (Input.IsActionJustPressed("jump"))
            _jumpBuffer = JumpBufferTime;
        else
            _jumpBuffer -= dt;

        // ── Wall slide ───────────────────────────────────────────────────────
        float dir_x = Input.GetAxis("move_left", "move_right");
        _wallSliding = false;

        if (onWall && !onFloor && _velocity.Y > 0)
        {
            // Only slide if pressing toward the wall
            Vector2 wallNormal = GetWallNormal();
            bool pressingIntoWall = (wallNormal.X > 0 && dir_x < 0) ||
                                    (wallNormal.X < 0 && dir_x > 0);
            if (pressingIntoWall)
            {
                _wallSliding = true;
                _velocity.Y  = Mathf.Min(_velocity.Y, WallSlideSpeed);
            }
        }

        // ── Wall jump ────────────────────────────────────────────────────────
        if (_jumpBuffer > 0f && _wallSliding)
        {
            Vector2 wallNormal = GetWallNormal();
            _velocity.X   = wallNormal.X * WallJumpForceX;
            _velocity.Y   = WallJumpForceY;
            _wallJumped   = true;
            _wallJumpTimer = 0f;
            _jumpBuffer   = 0f;
            _coyoteTimer  = 0f;
        }
        // ── Normal jump ──────────────────────────────────────────────────────
        else if (_jumpBuffer > 0f && _coyoteTimer > 0f)
        {
            _velocity.Y  = JumpForce;
            _coyoteTimer = 0f;
            _jumpBuffer  = 0f;
        }
        // ── Double jump ───────────────────────────────────────────────────────
        else if (_jumpBuffer > 0f && _jumpsLeft > 0 && !_wallSliding)
        {
            _velocity.Y = JumpForce;
            _jumpsLeft--;
            _jumpBuffer = 0f;
        }

        // ── Variable jump height (short hop) ─────────────────────────────────
        if (Input.IsActionJustReleased("jump") && _velocity.Y < 0f)
            _velocity.Y *= 0.45f;

        // ── Gravity ───────────────────────────────────────────────────────────
        if (!onFloor && !_wallSliding)
        {
            float grav = _velocity.Y > 0f ? FallGravity : Gravity;
            _velocity.Y += grav * dt;
        }
        else if (!onFloor && _wallSliding)
        {
            // Gentle gravity while sliding — speed is already capped above
            _velocity.Y += Gravity * 0.3f * dt;
        }

        // ── Horizontal ───────────────────────────────────────────────────────
        if (_wallJumped)
        {
            _wallJumpTimer += dt;
            // Lerp control back over ~0.3s after a wall jump
            _velocity.X = Mathf.Lerp(_velocity.X, dir_x * MoveSpeed, WallJumpLerp * dt);
            if (_wallJumpTimer > 0.3f)
                _wallJumped = false;
        }
        else
        {
            _velocity.X = dir_x * MoveSpeed;
        }

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;
    }
}
