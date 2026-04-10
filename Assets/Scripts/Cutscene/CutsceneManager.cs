using Godot;

public partial class CutsceneManager : Node2D
{
    private const float W = 1152f;
    private const float H = 648f;

    // 8-directional alien sprites (shared by all aliens in cutscene)
    private static readonly string[] _dirNames =
    {
        "east", "north-east", "north", "north-west",
        "west", "south-west", "south", "south-east",
    };
    private readonly Texture2D[] _alienDirTex = new Texture2D[8];
    private readonly Texture2D[] _dogFrames   = new Texture2D[6];
    private float _bobTimer      = 0f;
    private float _animTimer     = 0f;
    private int   _animFrame     = 0;
    private float _dogAnimTimer  = 0f;
    private int   _dogAnimFrame  = 0;
    private const float AnimFPS    = 8f;
    private const float DogAnimFPS = 8f;

    private static readonly float[] StageDurations =
    {
        3.5f,  // 0  space — rocket launches
        3.0f,  // 1  alien planet — rocket crashes
        3.0f,  // 2  dogs walk from wreckage
        3.0f,  // 3  aliens emerge, follow dogs
        4.0f,  // 4  earth control room reaction
        3.0f,  // 5  "3 Years Later" card
        3.5f,  // 6  earth — saucer descends
        4.0f,  // 7  dogs walk out, scatter
        4.0f,  // 8  alien sneaks out
        2.0f,  // 9  fade to black
    };

    private int   _stage = 0;
    private float _timer = 0f;
    private float _fade  = 1f;  // start faded in

    // Characters
    private Vector2[] _dogPos    = new Vector2[3];
    private Vector2[] _dogTarget = new Vector2[3];
    private static readonly Color[] DogColors =
    {
        new Color(0.90f, 0.82f, 0.10f),  // yellow
        new Color(0.10f, 0.10f, 0.12f),  // black
        new Color(0.78f, 0.12f, 0.10f),  // red
    };

    private Vector2[] _alienPos    = new Vector2[3];
    private Vector2[] _alienTarget = new Vector2[3];

    private Vector2 _rocketPos;
    private Vector2 _shipPos;

    // Dialogue typewriter
    private string _fullText  = "";
    private string _dialogue  = "";
    private float  _typeTimer = 0f;
    private int    _typeIdx   = 0;

    public override void _Ready()
    {
        for (int i = 0; i < _dirNames.Length; i++)
        {
            string path = "res://Assets/Sprites/Aliens/" + _dirNames[i] + ".png";
            if (ResourceLoader.Exists(path))
                _alienDirTex[i] = ResourceLoader.Load<Texture2D>(path);
        }

        for (int i = 0; i < _dogFrames.Length; i++)
        {
            string path = "res://Assets/Sprites/Dogs/east/frame_00" + i + ".png";
            if (ResourceLoader.Exists(path))
                _dogFrames[i] = ResourceLoader.Load<Texture2D>(path);
        }
        EnsureInput();
        EnterStage(0);
    }

    private static void EnsureInput()
    {
        void Add(string name, Key key)
        {
            if (!InputMap.HasAction(name))
            {
                InputMap.AddAction(name);
                var ev = new InputEventKey { Keycode = key };
                InputMap.ActionAddEvent(name, ev);
            }
        }
        Add("move_left",  Key.A);
        Add("move_right", Key.D);
        Add("jump",       Key.Space);
    }

    private void EnterStage(int s)
    {
        _stage = s;
        _timer = 0f;
        _dialogue = "";
        _fullText  = "";
        _typeIdx   = 0;

        float gnd = H * 0.70f;

        switch (s)
        {
            case 0:
                _rocketPos = new Vector2(W * 0.45f, H * 0.62f);
                _fade = 0f;
                break;

            case 1:
                _rocketPos = new Vector2(W * 0.48f, -80f);
                break;

            case 2:
                for (int i = 0; i < 3; i++)
                {
                    _dogPos[i]    = new Vector2(W * 0.32f + i * 28, gnd - 38);
                    _dogTarget[i] = new Vector2(W * 0.80f + i * 44, gnd - 38);
                }
                break;

            case 3:
                // Aliens start near crash, move after dogs
                for (int i = 0; i < 3; i++)
                {
                    _alienPos[i]    = new Vector2(W * 0.20f + i * 50, gnd - 38);
                    _alienTarget[i] = new Vector2(W * 1.15f + i * 60, gnd - 38);
                }
                break;

            case 4:
                SetDialogue("Signal lost. We've lost all contact...\nAll hope is gone.");
                break;

            case 5:
                break; // title card only

            case 6:
                _shipPos = new Vector2(W * 0.50f, -100f);
                break;

            case 7:
                for (int i = 0; i < 3; i++)
                {
                    _dogPos[i]    = new Vector2(W * 0.50f + i * 18, gnd - 38);
                    // yellow goes left, black forward-right, red far right
                    float[] tx = { W * 0.05f, W * 0.55f, W * 1.10f };
                    _dogTarget[i] = new Vector2(tx[i], gnd - 38);
                }
                break;

            case 8:
                // Secret alien sneaks out after dogs are gone
                _alienPos[0]    = new Vector2(W * 0.52f, gnd - 38);
                _alienTarget[0] = new Vector2(W * 0.05f, gnd - 38);  // follows yellow dog
                // Other dogs already off screen — freeze them
                for (int i = 1; i < 3; i++)
                {
                    _dogPos[i]    = _dogTarget[i];
                    _alienPos[i]  = new Vector2(-200, gnd - 38);
                    _alienTarget[i] = new Vector2(-200, gnd - 38);
                }
                break;

            case 9:
                break;
        }
    }

    private void SetDialogue(string text)
    {
        _fullText = text;
        _dialogue = "";
        _typeTimer = 0f;
        _typeIdx   = 0;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _timer    += dt;
        _bobTimer += dt * 3.0f;

        // Advance alien animation frame
        _animTimer += dt;
        if (_animTimer >= 1f / AnimFPS)
        {
            _animTimer -= 1f / AnimFPS;
            _animFrame = (_animFrame + 1) % _alienDirTex.Length;
        }

        // Advance dog animation frame
        _dogAnimTimer += dt;
        if (_dogAnimTimer >= 1f / DogAnimFPS)
        {
            _dogAnimTimer -= 1f / DogAnimFPS;
            _dogAnimFrame = (_dogAnimFrame + 1) % _dogFrames.Length;
        }

        // Typewriter
        if (_typeIdx < _fullText.Length)
        {
            _typeTimer += dt;
            if (_typeTimer >= 0.045f)
            {
                _typeTimer = 0f;
                _typeIdx   = Mathf.Min(_typeIdx + 1, _fullText.Length);
                _dialogue  = _fullText.Substring(0, _typeIdx);
            }
        }

        // Move dogs
        for (int i = 0; i < 3; i++)
        {
            _dogPos[i]   = _dogPos[i].MoveToward(_dogTarget[i],   55f * dt);
            _alienPos[i] = _alienPos[i].MoveToward(_alienTarget[i], 48f * dt);
        }

        // Rocket launch — rises in stage 0
        if (_stage == 0)
            _rocketPos.Y -= 90f * dt;

        // Rocket crash — falls in stage 1
        if (_stage == 1)
            _rocketPos.Y = Mathf.MoveToward(_rocketPos.Y, H * 0.63f, 220f * dt);

        // Ship descent
        if (_stage == 6)
            _shipPos.Y = Mathf.MoveToward(_shipPos.Y, H * 0.56f, 55f * dt);

        // Fade
        if (_stage == 9)
            _fade = Mathf.MoveToward(_fade, 1f, dt * 0.9f);
        else if (_stage == 0)
            _fade = Mathf.MoveToward(_fade, 0f, dt * 1.5f);

        // Skip
        if (Input.IsActionJustPressed("jump"))
            AdvanceStage();

        // Auto-advance
        if (_timer >= StageDurations[_stage])
            AdvanceStage();

        QueueRedraw();
    }

    private void AdvanceStage()
    {
        if (_stage >= StageDurations.Length - 1)
            GameManager.Instance.LoadScene("res://Assets/Scenes/Level1.tscn");
        else
            EnterStage(_stage + 1);
    }

    // ── DRAW ─────────────────────────────────────────────────────────────────

    public override void _Draw()
    {
        DrawBackground();
        DrawCharacters();
        DrawOverlays();

        if (_fade > 0.01f)
            DrawRect(new Rect2(0, 0, W, H), new Color(0, 0, 0, _fade));
    }

    private void DrawBackground()
    {
        switch (_stage)
        {
            case 0: DrawSpace();               break;
            case 1: DrawPlanet(crashing:true); break;
            case 2:
            case 3: DrawPlanet(crashing:false); break;
            case 4: DrawControlRoom();          break;
            case 5: DrawRect(new Rect2(0,0,W,H), new Color(0.04f,0.04f,0.06f)); break;
            case 6:
            case 7:
            case 8:
            case 9: DrawEarth();               break;
        }
    }

    private void DrawSpace()
    {
        // Deep space
        DrawRect(new Rect2(0, 0, W, H), new Color(0.02f, 0.02f, 0.06f));

        // Stars (seeded random so they don't flicker)
        var rng = new RandomNumberGenerator();
        rng.Seed = 12345;
        for (int i = 0; i < 90; i++)
        {
            float sx = rng.RandfRange(0, W);
            float sy = rng.RandfRange(0, H * 0.72f);
            float ss = rng.RandfRange(1.5f, 3.5f);
            float br = rng.RandfRange(0.6f, 1f);
            DrawRect(new Rect2(sx, sy, ss, ss), new Color(br, br, br));
        }

        // Earth horizon glow at the very bottom
        DrawRect(new Rect2(0, H * 0.78f, W, H * 0.22f), new Color(0.10f, 0.18f, 0.10f));
        DrawRect(new Rect2(0, H * 0.76f, W, 12), new Color(0.20f, 0.50f, 0.20f, 0.5f));

        // Launch pad
        DrawRect(new Rect2(W*0.38f, H*0.78f, 90, 14), new Color(0.35f, 0.35f, 0.38f));
        DrawRect(new Rect2(W*0.40f, H*0.72f, 8, (H*0.78f - H*0.72f)), new Color(0.28f,0.28f,0.30f)); // support strut L
        DrawRect(new Rect2(W*0.46f+8, H*0.72f, 8, (H*0.78f - H*0.72f)), new Color(0.28f,0.28f,0.30f)); // support strut R

        DrawRocket(_rocketPos);

        // Exhaust plume
        if (_rocketPos.Y > H * 0.30f)
        {
            float ex = _rocketPos.X;
            float ey = _rocketPos.Y + 60;
            DrawCircle(new Vector2(ex,      ey),      22, new Color(0.9f, 0.6f, 0.2f, 0.7f));
            DrawCircle(new Vector2(ex - 16, ey + 14), 14, new Color(0.8f, 0.8f, 0.8f, 0.5f));
            DrawCircle(new Vector2(ex + 16, ey + 14), 14, new Color(0.8f, 0.8f, 0.8f, 0.5f));
        }
    }

    private void DrawPlanet(bool crashing)
    {
        // Eerie alien sky
        DrawRect(new Rect2(0, 0,       W, H * 0.50f), new Color(0.12f, 0.05f, 0.18f));
        DrawRect(new Rect2(0, H*0.50f, W, H * 0.18f), new Color(0.30f, 0.10f, 0.06f));

        // Alien planet in sky
        DrawCircle(new Vector2(W*0.18f, H*0.18f), 38, new Color(0.55f, 0.38f, 0.08f));
        DrawCircle(new Vector2(W*0.18f, H*0.18f), 42, new Color(0.50f, 0.32f, 0.06f, 0.3f));
        DrawCircle(new Vector2(W*0.78f, H*0.10f), 14, new Color(0.28f, 0.55f, 0.42f));

        // Stars
        var rng = new RandomNumberGenerator();
        rng.Seed = 99999;
        for (int i = 0; i < 50; i++)
            DrawRect(new Rect2(rng.RandfRange(0,W), rng.RandfRange(0,H*0.48f), 2, 2), new Color(0.9f,0.9f,0.8f));

        // Rocky ground
        DrawRect(new Rect2(0, H*0.68f, W, H*0.32f), new Color(0.18f, 0.11f, 0.08f));
        // Rock shapes
        int[] rx = {0, 160, 320, 500, 680, 860, 1040};
        int[] rw = {100, 80, 140, 90, 120, 80, 110};
        for (int i = 0; i < rx.Length; i++)
            DrawRect(new Rect2(rx[i], H*0.65f, rw[i], 28), new Color(0.12f, 0.07f, 0.05f));

        if (crashing)
        {
            DrawRocket(_rocketPos);
            // Impact dust when near ground
            if (_rocketPos.Y > H * 0.38f)
            {
                float t = (_rocketPos.Y - H * 0.38f) / (H * 0.25f);
                float r = 30 + t * 60;
                DrawCircle(new Vector2(_rocketPos.X, H*0.67f), r, new Color(0.45f,0.30f,0.18f, 0.65f));
                DrawCircle(new Vector2(_rocketPos.X - 40, H*0.64f), r*0.6f, new Color(0.40f,0.25f,0.14f, 0.5f));
                DrawCircle(new Vector2(_rocketPos.X + 40, H*0.64f), r*0.6f, new Color(0.40f,0.25f,0.14f, 0.5f));
            }
        }
        else
        {
            DrawCrashedRocket(new Vector2(W * 0.36f, H * 0.65f));
        }
    }

    private void DrawEarth()
    {
        // Sky gradient (two rects)
        DrawRect(new Rect2(0, 0,       W, H * 0.45f), new Color(0.35f, 0.58f, 0.80f));
        DrawRect(new Rect2(0, H*0.45f, W, H * 0.25f), new Color(0.48f, 0.68f, 0.55f));

        // Clouds
        void Cloud(float cx, float cy, float cw)
        {
            DrawRect(new Rect2(cx,       cy,      cw,     20), new Color(1f,1f,1f,0.85f));
            DrawRect(new Rect2(cx + 14,  cy - 10, cw-28,  16), new Color(1f,1f,1f,0.85f));
        }
        Cloud(W*0.08f, H*0.12f, 130);
        Cloud(W*0.52f, H*0.19f, 110);
        Cloud(W*0.78f, H*0.09f, 90);

        // Ground
        DrawRect(new Rect2(0, H*0.70f, W, H*0.30f), new Color(0.20f, 0.38f, 0.16f));
        // Darker dirt strip
        DrawRect(new Rect2(0, H*0.70f, W, 8), new Color(0.28f, 0.20f, 0.10f));

        // Distant tree line silhouette
        for (int i = 0; i < 14; i++)
        {
            float tx = i * 85f + 10;
            DrawRect(new Rect2(tx + 20, H*0.60f, 12, 44), new Color(0.10f,0.22f,0.10f));   // trunk
            DrawRect(new Rect2(tx,      H*0.50f, 55, 32), new Color(0.12f,0.24f,0.12f));  // canopy
        }

        DrawShip(_shipPos);
    }

    private void DrawControlRoom()
    {
        DrawRect(new Rect2(0, 0, W, H), new Color(0.06f, 0.06f, 0.08f));

        // Main monitor
        DrawRect(new Rect2(W*0.15f, H*0.06f, W*0.70f, H*0.50f), new Color(0.04f, 0.12f, 0.04f));
        DrawRect(new Rect2(W*0.17f, H*0.08f, W*0.66f, H*0.46f), new Color(0.06f, 0.18f, 0.06f));
        // Static noise lines
        for (int i = 0; i < 12; i++)
            DrawRect(new Rect2(W*0.17f, H*0.08f + i * 24, W*0.66f, 2), new Color(0f,0f,0f,0.18f));

        // "NO SIGNAL" text
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(W*0.42f, H*0.34f), "NO SIGNAL", HorizontalAlignment.Left, -1, 32, new Color(0.25f, 0.90f, 0.25f));
        DrawString(font, new Vector2(W*0.37f, H*0.42f), "SIGNAL LOST — KSSP-1", HorizontalAlignment.Left, -1, 18, new Color(0.18f, 0.70f, 0.18f, 0.7f));

        // Console desk
        DrawRect(new Rect2(W*0.02f, H*0.62f, W*0.96f, 22), new Color(0.22f, 0.18f, 0.12f));
        // Side monitors
        DrawRect(new Rect2(W*0.02f, H*0.10f, W*0.10f, H*0.46f), new Color(0.05f, 0.14f, 0.05f));
        DrawRect(new Rect2(W*0.88f, H*0.10f, W*0.10f, H*0.46f), new Color(0.05f, 0.14f, 0.05f));

        // Scientist silhouettes
        float[] sxs = { W*0.12f, W*0.28f, W*0.58f, W*0.74f };
        foreach (float sx in sxs)
        {
            DrawRect(new Rect2(sx - 14, H*0.44f, 28, 44), new Color(0.07f, 0.07f, 0.09f));  // body
            DrawCircle(new Vector2(sx, H*0.42f), 13, new Color(0.07f, 0.07f, 0.09f));        // head
        }
    }

    private void DrawCharacters()
    {
        switch (_stage)
        {
            case 2:
                for (int i = 0; i < 3; i++)
                    DrawDog(_dogPos[i], DogColors[i], facingRight: true);
                break;

            case 3:
                for (int i = 0; i < 3; i++)
                {
                    DrawDog(_dogPos[i], DogColors[i], facingRight: true);
                    DrawAlien(_alienPos[i], sneaky: false);
                }
                break;

            case 7:
                for (int i = 0; i < 3; i++)
                    DrawDog(_dogPos[i], DogColors[i], facingRight: i != 0, glowEyes: true);
                break;

            case 8:
                // Dog 0 (yellow) is off-left, hero alien follows
                DrawDog(_dogPos[0], DogColors[0], facingRight: false, glowEyes: true);
                DrawAlien(_alienPos[0], sneaky: true);
                break;
        }
    }

    private void DrawOverlays()
    {
        var font = ThemeDB.FallbackFont;

        // Dialogue box
        if (_dialogue.Length > 0)
        {
            DrawRect(new Rect2(W*0.08f, H*0.80f, W*0.84f, 90), new Color(0,0,0,0.88f));
            DrawRect(new Rect2(W*0.08f, H*0.80f, W*0.84f, 3), new Color(0.80f, 0.80f, 0.18f));
            DrawString(font, new Vector2(W*0.11f, H*0.80f + 44), _dialogue,
                       HorizontalAlignment.Left, -1, 26, Colors.White);
        }

        // "3 YEARS LATER" card
        if (_stage == 5)
        {
            float a = Mathf.Min(1f, _timer * 1.4f);
            DrawString(font, new Vector2(W*0.50f - 220, H*0.42f),
                       "3 YEARS LATER", HorizontalAlignment.Left, -1, 60, new Color(1f, 1f, 1f, a));
            DrawString(font, new Vector2(W*0.50f - 80, H*0.56f),
                       "Earth, 1960", HorizontalAlignment.Left, -1, 28, new Color(0.70f, 0.70f, 0.70f, a * 0.8f));
        }

        // Skip hint (pulsing)
        float pulse = Mathf.Sin(_timer * 3.5f) * 0.35f + 0.35f;
        DrawString(font, new Vector2(W - 210, H - 18), "SPACE — skip",
                   HorizontalAlignment.Left, -1, 16, new Color(1f, 1f, 1f, pulse));
    }

    // ── SPRITE HELPERS ────────────────────────────────────────────────────────

    private void DrawRocket(Vector2 pos)
    {
        // Fins
        DrawColoredPolygon(new Vector2[]
        {
            new Vector2(pos.X - 14, pos.Y),
            new Vector2(pos.X - 26, pos.Y + 22),
            new Vector2(pos.X - 14, pos.Y + 22),
        }, new Color(0.60f, 0.60f, 0.65f));
        DrawColoredPolygon(new Vector2[]
        {
            new Vector2(pos.X + 14, pos.Y),
            new Vector2(pos.X + 26, pos.Y + 22),
            new Vector2(pos.X + 14, pos.Y + 22),
        }, new Color(0.60f, 0.60f, 0.65f));
        // Body
        DrawRect(new Rect2(pos.X - 14, pos.Y - 60, 28, 80), new Color(0.76f, 0.76f, 0.80f));
        // Soviet red band
        DrawRect(new Rect2(pos.X - 14, pos.Y - 20, 28, 10), new Color(0.82f, 0.12f, 0.12f));
        // Nose cone
        DrawColoredPolygon(new Vector2[]
        {
            new Vector2(pos.X,      pos.Y - 88),
            new Vector2(pos.X - 14, pos.Y - 60),
            new Vector2(pos.X + 14, pos.Y - 60),
        }, new Color(0.82f, 0.12f, 0.12f));
        // Window
        DrawCircle(new Vector2(pos.X, pos.Y - 42), 9, new Color(0.55f, 0.75f, 0.92f));
        DrawCircle(new Vector2(pos.X, pos.Y - 42), 6, new Color(0.70f, 0.88f, 1.00f));
    }

    private void DrawCrashedRocket(Vector2 pos)
    {
        // Sideways body
        DrawRect(new Rect2(pos.X - 60, pos.Y - 14, 90,  28), new Color(0.62f, 0.62f, 0.66f));
        DrawRect(new Rect2(pos.X - 60, pos.Y -  6, 90,  10), new Color(0.80f, 0.12f, 0.12f));
        DrawRect(new Rect2(pos.X + 28, pos.Y - 12, 28,  24), new Color(0.80f, 0.12f, 0.12f)); // nose
        // Smoke plumes
        DrawCircle(new Vector2(pos.X - 50, pos.Y - 36), 24, new Color(0.50f, 0.50f, 0.52f, 0.65f));
        DrawCircle(new Vector2(pos.X - 66, pos.Y - 58), 16, new Color(0.40f, 0.40f, 0.42f, 0.45f));
        DrawCircle(new Vector2(pos.X - 36, pos.Y - 56), 14, new Color(0.45f, 0.45f, 0.46f, 0.40f));
    }

    private void DrawShip(Vector2 pos)
    {
        // Glow underneath
        DrawRect(new Rect2(pos.X - 75, pos.Y + 14, 150, 10), new Color(0.25f, 0.92f, 0.45f, 0.35f));
        // Saucer layers
        DrawRect(new Rect2(pos.X - 90, pos.Y - 8,  180, 22), new Color(0.48f, 0.52f, 0.58f));
        DrawRect(new Rect2(pos.X - 56, pos.Y - 26, 112, 20), new Color(0.58f, 0.62f, 0.68f));
        DrawRect(new Rect2(pos.X - 28, pos.Y - 40, 56,  16), new Color(0.66f, 0.70f, 0.76f));
        DrawRect(new Rect2(pos.X - 12, pos.Y - 50, 24,  12), new Color(0.72f, 0.76f, 0.82f));
        // Light ring
        for (int i = -4; i <= 4; i++)
            DrawCircle(new Vector2(pos.X + i * 20, pos.Y + 4), 4, new Color(0.75f, 1f, 0.40f, 0.92f));
        // Outline
        DrawRect(new Rect2(pos.X - 91, pos.Y - 9,  182, 2), new Color(0.25f, 0.28f, 0.32f));
        DrawRect(new Rect2(pos.X - 91, pos.Y + 14, 182, 2), new Color(0.25f, 0.28f, 0.32f));
    }

    private void DrawDog(Vector2 pos, Color col, bool facingRight, bool glowEyes = false)
    {
        var tex = _dogFrames[_dogAnimFrame];
        if (tex == null) return;

        float size = 80f;
        float aspect = (float)tex.GetWidth() / tex.GetHeight();
        float dw = size * aspect;
        float dh = size;

        // Flip horizontally for left-facing dogs
        var dest = new Rect2(pos.X - dw / 2f, pos.Y - dh, dw, dh);
        if (!facingRight)
        {
            // Mirror by shifting rect to the right and using negative width
            dest = new Rect2(pos.X + dw / 2f, pos.Y - dh, -dw, dh);
        }

        // Tint the sprite with the dog's colour (yellow/black/red)
        DrawTextureRect(tex, dest, false, col);

        // Glowing eyes overlay — drawn on top at approximate eye position
        if (glowEyes)
        {
            float ex = facingRight ? pos.X + dw * 0.28f : pos.X - dw * 0.28f;
            float ey = pos.Y - dh * 0.72f;
            DrawCircle(new Vector2(ex, ey), 5, new Color(0.95f, 0.08f, 0.08f));
            DrawCircle(new Vector2(ex, ey), 8, new Color(1f, 0.12f, 0.12f, 0.35f));
        }
    }

    private void DrawAlien(Vector2 pos, bool sneaky)
    {
        var tex = _alienDirTex[_animFrame];
        if (tex == null) return;

        float a    = sneaky ? 0.75f : 1.0f;
        float size = sneaky ? 72f   : 90f;
        float bob  = Mathf.Sin(_bobTimer) * 2.5f;

        float aspect = (float)tex.GetWidth() / tex.GetHeight();
        float dw = size * aspect;
        float dh = size;

        var dest = new Rect2(pos.X - dw / 2f, pos.Y + bob - dh, dw, dh);
        DrawTextureRect(tex, dest, false, new Color(1f, 1f, 1f, a));
    }
}
