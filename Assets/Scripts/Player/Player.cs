using Godot;

public partial class Player : CharacterBody2D
{
    // ── Movement exports ─────────────────────────────────────────────────────
    [Export] public float MoveSpeed      = 220f;
    [Export] public float GrabMoveSpeed  = 110f;
    [Export] public float JumpForce      = -520f;
    [Export] public float Gravity        = 1400f;
    [Export] public float FallGravity    = 2200f;
    [Export] public float CoyoteTime     = 0.12f;
    [Export] public float JumpBufferTime = 0.12f;

    [Export] public float WallSlideSpeed = 80f;
    [Export] public float WallJumpForceX = 280f;
    [Export] public float WallJumpForceY = -500f;
    [Export] public float WallJumpLerp   = 8f;

    [Export] public int   ExtraJumps     = 1;
    [Export] public float DashSpeed      = 520f;
    [Export] public float DashDuration   = 0.18f;
    [Export] public float GrabRange      = 72f;

    // ── Sprite paths ─────────────────────────────────────────────────────────
    private const string SpriteBase = "res://Assets/Sprites/Player/mako-sprite/";
    private const string WalkBase   = SpriteBase + "animations/Walking-c1cb9d4b/";
    private const string JumpBase   = SpriteBase + "animations/Jumping-6cace7bf/";
    private const string PushBase   = SpriteBase + "animations/Push_Object-c3b3fa26/";
    private const string PullBase   = SpriteBase + "animations/Pull_Object-0d671f3d/";
    private const string RotBase    = SpriteBase + "rotations/";

    // ── Physics state ────────────────────────────────────────────────────────
    private Vector2 _velocity      = Vector2.Zero;
    private float   _coyoteTimer   = 0f;
    private float   _jumpBuffer    = 0f;
    private bool    _wasOnFloor    = false;
    private Vector2 _spawnPoint;

    private bool  _wallSliding   = false;
    private bool  _wallJumped    = false;
    private float _wallJumpTimer = 0f;
    private int   _jumpsLeft     = 0;

    private bool    _hasDashed = false;
    private bool    _isDashing = false;
    private float   _dashTimer = 0f;
    private Vector2 _dashDir   = Vector2.Zero;

    // ── Grab state ───────────────────────────────────────────────────────────
    private PushableRock _heldRock  = null;
    private bool         _facingWest = false;

    // ── Sprite ───────────────────────────────────────────────────────────────
    private AnimatedSprite2D _sprite;

    private enum AnimState
    {
        Idle, WalkEast, WalkWest, JumpEast, JumpWest,
        PushEast, PushWest, PullEast, PullWest
    }
    private AnimState _animState = AnimState.Idle;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        _spawnPoint = Position;

        EnsureAction("move_left",  Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("move_up",    Key.W);
        EnsureAction("move_down",  Key.S);
        EnsureAction("jump",       Key.Space);
        EnsureAction("dash",       Key.X);
        EnsureAction("grab",       Key.E);

        BuildSprite();

        var cam = new Camera2D();
        cam.PositionSmoothingEnabled = true;
        cam.PositionSmoothingSpeed   = 6f;
        cam.LimitLeft                = 0;
        cam.LimitRight               = 1152;
        cam.LimitBottom              = 648;
        AddChild(cam);
    }

    private void BuildSprite()
    {
        _sprite = new AnimatedSprite2D();
        _sprite.Scale    = new Vector2(2f, 2f);
        _sprite.Position = new Vector2(0, -23);

        var frames = new SpriteFrames();
        _sprite.SpriteFrames = frames;

        AddAnim(frames, "walk_east",  WalkBase + "east/",  6,  fps: 10f, loop: true);
        AddAnim(frames, "walk_west",  WalkBase + "west/",  6,  fps: 10f, loop: true);
        AddAnim(frames, "jump_east",  JumpBase + "east/",  8,  fps: 12f, loop: false);
        AddAnim(frames, "jump_west",  JumpBase + "west/",  8,  fps: 12f, loop: false);
        AddAnim(frames, "push_east",  PushBase + "east/",  6,  fps: 8f,  loop: true);
        AddAnim(frames, "push_west",  PushBase + "west/",  6,  fps: 8f,  loop: true);
        AddAnim(frames, "pull_east",  PullBase + "east/",  6,  fps: 8f,  loop: true);
        AddAnim(frames, "pull_west",  PullBase + "west/",  6,  fps: 8f,  loop: true);

        frames.AddAnimation("idle");
        frames.SetAnimationSpeed("idle", 1f);
        frames.SetAnimationLoop("idle", false);
        frames.AddFrame("idle", ResourceLoader.Load<Texture2D>(RotBase + "south.png"), 0);

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

    public void Respawn()
    {
        Position   = _spawnPoint;
        _velocity  = Vector2.Zero;
        _isDashing = false;
        _dashTimer = 0f;
        ReleaseRock();
    }

    // ── Physics ──────────────────────────────────────────────────────────────

    public override void _PhysicsProcess(double delta)
    {
        float dt      = (float)delta;
        bool  onFloor = IsOnFloor();
        bool  onWall  = IsOnWall();

        if (onFloor)
        {
            _hasDashed     = false;
            _wallJumped    = false;
            _wallJumpTimer = 0f;
            _jumpsLeft     = ExtraJumps;
        }

        // Grab
        bool grabPressed = Input.IsActionPressed("grab");
        if (grabPressed && _heldRock == null && onFloor)
        {
            foreach (Node node in GetTree().GetNodesInGroup("pushable"))
            {
                if (node is PushableRock rock && Position.DistanceTo(rock.Position) < GrabRange)
                {
                    _heldRock = rock;
                    break;
                }
            }
        }
        else if (!grabPressed && _heldRock != null)
        {
            ReleaseRock();
        }

        bool isGrabbing = _heldRock != null;

        // Dash (blocked while grabbing)
        if (!isGrabbing && Input.IsActionJustPressed("dash") && !_hasDashed && !_isDashing)
        {
            float dx      = Input.GetAxis("move_left", "move_right");
            float dy      = Input.GetAxis("move_up",   "move_down");
            Vector2 dir   = new Vector2(dx, -dy).Normalized();
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
                _isDashing = false;
                _velocity *= 0.25f;
            }
            Velocity = _velocity;
            MoveAndSlide();
            _velocity = Velocity;
            UpdateAnim(isGrabbing: false, onFloor);
            return;
        }

        // Coyote
        if (_wasOnFloor && !onFloor && !_wallJumped) _coyoteTimer = CoyoteTime;
        else if (onFloor)                             _coyoteTimer = CoyoteTime;
        else                                          _coyoteTimer -= dt;
        _wasOnFloor = onFloor;

        // Jump buffer
        if (Input.IsActionJustPressed("jump")) _jumpBuffer = JumpBufferTime;
        else                                   _jumpBuffer -= dt;

        // Wall slide
        float dir_x  = Input.GetAxis("move_left", "move_right");
        _wallSliding = false;
        if (!isGrabbing && onWall && !onFloor && _velocity.Y > 0)
        {
            Vector2 wn = GetWallNormal();
            if ((wn.X > 0 && dir_x < 0) || (wn.X < 0 && dir_x > 0))
            {
                _wallSliding = true;
                _velocity.Y  = Mathf.Min(_velocity.Y, WallSlideSpeed);
            }
        }

        // Jumps
        if (_jumpBuffer > 0f && _wallSliding)
        {
            Vector2 wn = GetWallNormal();
            _velocity.X    = wn.X * WallJumpForceX;
            _velocity.Y    = WallJumpForceY;
            _wallJumped    = true;
            _wallJumpTimer = 0f;
            _jumpBuffer    = 0f;
            _coyoteTimer   = 0f;
        }
        else if (_jumpBuffer > 0f && _coyoteTimer > 0f && !isGrabbing)
        {
            _velocity.Y  = JumpForce;
            _coyoteTimer = 0f;
            _jumpBuffer  = 0f;
        }
        else if (_jumpBuffer > 0f && _jumpsLeft > 0 && !_wallSliding && !isGrabbing)
        {
            _velocity.Y = JumpForce;
            _jumpsLeft--;
            _jumpBuffer = 0f;
        }

        if (Input.IsActionJustReleased("jump") && _velocity.Y < 0f)
            _velocity.Y *= 0.45f;

        // Gravity
        if (!onFloor && !_wallSliding)
            _velocity.Y += (_velocity.Y > 0f ? FallGravity : Gravity) * dt;
        else if (!onFloor && _wallSliding)
            _velocity.Y += Gravity * 0.3f * dt;

        // Horizontal
        float speed = isGrabbing ? GrabMoveSpeed : MoveSpeed;
        if (_wallJumped)
        {
            _wallJumpTimer += dt;
            _velocity.X = Mathf.Lerp(_velocity.X, dir_x * speed, WallJumpLerp * dt);
            if (_wallJumpTimer > 0.3f) _wallJumped = false;
        }
        else
        {
            _velocity.X = dir_x * speed;
        }

        if (_heldRock != null) _heldRock.Grab(_velocity.X);

        Velocity = _velocity;
        MoveAndSlide();
        _velocity = Velocity;

        UpdateAnim(isGrabbing, onFloor);
    }

    // ── Animation ────────────────────────────────────────────────────────────

    private void UpdateAnim(bool isGrabbing, bool onFloor)
    {
        if (_sprite == null) return;

        float vx = _velocity.X;
        float vy = _velocity.Y;

        if (Mathf.Abs(vx) > 10f)
            _facingWest = vx < 0f;

        AnimState desired;

        if (isGrabbing && onFloor)
        {
            bool moving = Mathf.Abs(vx) > 10f;
            if (!moving)
            {
                desired = AnimState.Idle;
            }
            else
            {
                float rockOffsetX = _heldRock != null ? _heldRock.Position.X - Position.X : 1f;
                bool isPush = (vx > 0.1f && rockOffsetX > 0) || (vx < -0.1f && rockOffsetX < 0);
                bool east   = vx > 0f;
                desired = (isPush, east) switch
                {
                    (true,  true)  => AnimState.PushEast,
                    (true,  false) => AnimState.PushWest,
                    // Pull: facing opposite to movement (toward the rock)
                    (false, true)  => AnimState.PullWest,
                    _              => AnimState.PullEast,
                };
            }
        }
        else if (!onFloor)
        {
            desired = _facingWest ? AnimState.JumpWest : AnimState.JumpEast;
        }
        else if (Mathf.Abs(vx) > 10f)
        {
            desired = _facingWest ? AnimState.WalkWest : AnimState.WalkEast;
        }
        else
        {
            desired = AnimState.Idle;
        }

        if (desired == _animState) return;
        _animState = desired;

        _sprite.Play(_animState switch
        {
            AnimState.WalkEast => "walk_east",
            AnimState.WalkWest => "walk_west",
            AnimState.JumpEast => "jump_east",
            AnimState.JumpWest => "jump_west",
            AnimState.PushEast => "push_east",
            AnimState.PushWest => "push_west",
            AnimState.PullEast => "pull_east",
            AnimState.PullWest => "pull_west",
            _                  => "idle",
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ReleaseRock()
    {
        _heldRock?.Release();
        _heldRock = null;
    }

    private static void EnsureAction(string action, Key key)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
            InputMap.ActionAddEvent(action, new InputEventKey { Keycode = key });
        }
    }
}
