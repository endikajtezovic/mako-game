using Godot;

public partial class SwingTestBuilder : Node2D
{
    private const float W = 1152f;
    private const float H = 648f;

    // { x, y, w, h } — all in world pixels, y = top edge
    // Ground platforms
    private static readonly float[] _start   = {    0, 480, 220, 168 };
    private static readonly float[] _plat2   = {  400, 390, 180, 258 };
    private static readonly float[] _plat3   = {  800, 280, 180, 368 };
    private static readonly float[] _end     = { 1080, 370,  72, 278 };
    // Hanging grapple anchor blocks
    private static readonly float[] _anc1    = {  275, 290,  80,  20 };
    private static readonly float[] _anc2    = {  620, 250,  80,  20 };
    private static readonly float[] _anc3    = { 1010, 240,  80,  20 };
    // Boundary walls (invisible, keep player on screen)
    private static readonly float[] _wallL   = {  -20,   0,  20, H   };
    private static readonly float[] _wallR   = { W,      0,  20, H   };

    public override void _Ready()
    {
        SpawnBackground();

        float[][] all = { _start, _plat2, _plat3, _end, _anc1, _anc2, _anc3, _wallL, _wallR };
        foreach (var p in all)
            SpawnPlatform(p[0], p[1], p[2], p[3]);

        SpawnKillZone();
        QueueRedraw();
    }

    private void SpawnBackground()
    {
        var tex = ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Backgrounds/summer1.png");
        if (tex == null) return;
        var bg = new Sprite2D { Texture = tex, Scale = new Vector2(0.5f, 0.5f),
                                Position = new Vector2(W / 2f, H / 2f), ZIndex = -10 };
        AddChild(bg);
    }

    private void SpawnPlatform(float x, float y, float w, float h)
    {
        var body  = new StaticBody2D();
        var shape = new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(w, h) } };
        body.Position = new Vector2(x + w / 2f, y + h / 2f);
        body.AddChild(shape);
        AddChild(body);
    }

    private void SpawnKillZone()
    {
        var area  = new Area2D();
        var shape = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(W + 200, 60) }
        };
        area.Position = new Vector2(W / 2f, H + 30f);
        area.AddChild(shape);
        area.BodyEntered += body => { if (body is Player p) p.CallDeferred("Respawn"); };
        AddChild(area);
    }

    public override void _Draw()
    {
        var woodTop  = new Color(0.62f, 0.44f, 0.24f);
        var woodBody = new Color(0.46f, 0.30f, 0.16f);
        var woodEdge = new Color(0.72f, 0.54f, 0.32f);

        DrawPlatformVisual(_start,  woodTop, woodBody, woodEdge);
        DrawPlatformVisual(_plat2,  woodTop, woodBody, woodEdge);
        DrawPlatformVisual(_plat3,  woodTop, woodBody, woodEdge);
        DrawPlatformVisual(_end,    woodTop, woodBody, woodEdge);

        var stoneTop  = new Color(0.52f, 0.52f, 0.58f);
        var stoneBody = new Color(0.36f, 0.36f, 0.40f);
        var stoneEdge = new Color(0.64f, 0.64f, 0.70f);

        DrawAnchorBlock(_anc1, stoneTop, stoneBody, stoneEdge);
        DrawAnchorBlock(_anc2, stoneTop, stoneBody, stoneEdge);
        DrawAnchorBlock(_anc3, stoneTop, stoneBody, stoneEdge);
    }

    private void DrawPlatformVisual(float[] p, Color top, Color body, Color edge)
    {
        float x = p[0], y = p[1], w = p[2], h = p[3];
        // Body fill
        DrawRect(new Rect2(x, y + 10, w, h - 10), body);
        // Surface
        DrawRect(new Rect2(x, y,      w, 10),     top);
        // Top highlight pixel line
        DrawRect(new Rect2(x, y,      w,  2),     edge);
        // Plank lines every 32px
        for (float px = x + 32; px < x + w - 4; px += 32)
            DrawLine(new Vector2(px, y), new Vector2(px, y + 10), new Color(0f, 0f, 0f, 0.20f), 1);
        // Left/right edges
        DrawRect(new Rect2(x,         y + 10, 4, h - 10), new Color(0f, 0f, 0f, 0.18f));
        DrawRect(new Rect2(x + w - 4, y + 10, 4, h - 10), new Color(0f, 0f, 0f, 0.18f));
    }

    private void DrawAnchorBlock(float[] p, Color top, Color body, Color edge)
    {
        float x = p[0], y = p[1], w = p[2], h = p[3];
        float cx = x + w / 2f;

        // Chain from screen top to block
        var chainCol = new Color(0.40f, 0.38f, 0.30f);
        for (float cy = 0; cy < y; cy += 10)
        {
            DrawRect(new Rect2(cx - 3, cy,     6, 5), chainCol);
            DrawRect(new Rect2(cx - 5, cy + 5, 10, 5), new Color(chainCol, chainCol.A * 0.7f));
        }

        // Block body
        DrawRect(new Rect2(x, y,      w, h), body);
        DrawRect(new Rect2(x, y,      w, 4), top);
        DrawRect(new Rect2(x, y,      w, 2), edge);

        // Grapple hint: pulsing yellow dot (static — just drawn bright)
        DrawCircle(new Vector2(cx, y + h / 2f), 7f, new Color(1f, 0.88f, 0.15f, 0.90f));
        DrawCircle(new Vector2(cx, y + h / 2f), 4f, new Color(1f, 1f,    0.60f, 1.00f));

        // Label
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(cx - 10, y + h + 14), "Q",
                   HorizontalAlignment.Left, -1, 14, new Color(1f, 0.88f, 0.15f, 0.85f));
    }
}
