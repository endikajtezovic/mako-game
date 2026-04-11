using Godot;

// Patrol enemy — walks back and forth, turns at walls or after PatrolRange pixels.
// Add AlienSprite as a child (SpritePath set to the alien's folder).
public partial class AlienEnemy : CharacterBody2D
{
    [Export] public float MoveSpeed   = 80f;
    [Export] public float Gravity     = 1400f;
    [Export] public float PatrolRange = 200f;   // pixels before turning around
    [Export] public int   MaxHealth   = 3;

    private int     _health;
    private float   _dir        = 1f;           // +1 right, -1 left
    private float   _walked     = 0f;           // distance since last turn
    private Vector2 _velocity   = Vector2.Zero;
    private AlienSprite _sprite;

    public override void _Ready()
    {
        _health = MaxHealth;

        // Wire up the AlienSprite child if present
        _sprite = GetNodeOrNull<AlienSprite>("AlienSprite");

        // Collision shape — capsule sized to ~80px sprite
        var body  = new CollisionShape2D();
        var caps  = new CapsuleShape2D();
        caps.Radius = 18f;
        caps.Height = 60f;
        body.Shape    = caps;
        body.Position = new Vector2(0, -30);
        AddChild(body);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Gravity
        if (!IsOnFloor())
            _velocity.Y += Gravity * dt;
        else
            _velocity.Y = 0f;

        _velocity.X = _dir * MoveSpeed;

        Velocity = _velocity;
        MoveAndSlide();

        // Turn on wall collision
        if (IsOnWall())
            Turn();

        // Turn after PatrolRange pixels
        _walked += Mathf.Abs(_velocity.X) * dt;
        if (_walked >= PatrolRange)
            Turn();

        // Let the sprite know which way we're moving
        _sprite?.SetVelocity(_velocity);
    }

    private void Turn()
    {
        _dir    = -_dir;
        _walked = 0f;
    }

    // Call this when the player stomps or attacks the enemy
    public void TakeDamage(int amount = 1)
    {
        _health -= amount;
        if (_health <= 0)
            Die();
    }

    private void Die()
    {
        QueueFree();
    }
}
