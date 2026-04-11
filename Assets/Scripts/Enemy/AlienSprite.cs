using Godot;

// Reusable animated sprite controller for all alien types.
// Drop as a child of any Node2D/CharacterBody2D.
// Set SpritePath to the folder containing east.png, west.png, etc.
// Cycles through all 8 frames while moving (tentacle slosh), pauses when still.
public partial class AlienSprite : Node2D
{
    [Export] public string SpritePath  = "res://Assets/Sprites/Aliens/";
    [Export] public float  DisplaySize = 80f;    // height on screen in pixels
    [Export] public float  BobAmount   = 2.5f;
    [Export] public float  BobSpeed    = 3.0f;
    [Export] public float  AnimFPS     = 8f;     // frames per second while moving

    private Texture2D[] _textures = new Texture2D[8];
    private int         _frame    = 0;
    private float       _animTimer = 0f;
    private float       _bobTimer  = 0f;
    private Vector2     _velocity  = Vector2.Zero;

    private static readonly string[] Frames =
    {
        "east", "north-east", "north", "north-west",
        "west", "south-west", "south", "south-east",
    };

    public override void _Ready()
    {
        for (int i = 0; i < Frames.Length; i++)
        {
            string path = SpritePath + Frames[i] + ".png";
            if (ResourceLoader.Exists(path))
                _textures[i] = ResourceLoader.Load<Texture2D>(path);
        }
    }

    public void SetVelocity(Vector2 vel)
    {
        _velocity = vel;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _bobTimer += dt * BobSpeed;

        bool moving = _velocity.LengthSquared() > 4f;
        if (moving)
        {
            _animTimer += dt;
            float frameDuration = 1f / AnimFPS;
            if (_animTimer >= frameDuration)
            {
                _animTimer -= frameDuration;
                _frame = (_frame + 1) % _textures.Length;
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        var tex = _textures[_frame];
        if (tex == null) return;

        float bob = Mathf.Sin(_bobTimer) * BobAmount;
        float h   = DisplaySize;
        float w   = h * ((float)tex.GetWidth() / tex.GetHeight());

        DrawTextureRect(tex,
            new Rect2(-w / 2f, -h + bob, w, h),
            false);
    }
}
