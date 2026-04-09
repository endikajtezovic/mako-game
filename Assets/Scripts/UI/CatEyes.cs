using Godot;

public partial class CatEyes : Node2D
{
    private const int   PS         = 8;      // px per grid cell
    private const float EyeSpacing = 155f;

    // ── Left eye palette (blue — eerie, slightly glowing) ──
    private static readonly Color BL1 = new Color(0.72f, 0.91f, 1.00f);  // outer highlight
    private static readonly Color BL2 = new Color(0.38f, 0.74f, 0.97f);  // mid iris
    private static readonly Color BL3 = new Color(0.12f, 0.38f, 0.78f);  // inner iris
    private static readonly Color BL4 = new Color(0.04f, 0.14f, 0.48f);  // deep inner

    // ── Right eye palette (orange — warm, intense) ──
    private static readonly Color OR1 = new Color(1.00f, 0.88f, 0.30f);  // outer gold
    private static readonly Color OR2 = new Color(1.00f, 0.55f, 0.05f);  // mid orange
    private static readonly Color OR3 = new Color(0.78f, 0.22f, 0.02f);  // inner red-orange
    private static readonly Color OR4 = new Color(0.45f, 0.08f, 0.00f);  // deep inner

    private static readonly Color Pupil = new Color(0.04f, 0.04f, 0.04f);
    private static readonly Color Black = Colors.Black;
    private static readonly Color Shine = new Color(1f, 1f, 1f, 0.90f);

    // Color codes: 1=outer 2=mid 3=inner 4=deep-inner 5=pupil-slit
    // 20 wide × 8 tall — flat aggressive top, sharp angular corners, off-center bottom point
    private static readonly int[,] EyeOpen =
    {
        { 0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0 },  // flat top — wide, aggressive
        { 0,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,0,0 },  // just inside top
        { 1,2,2,3,3,3,5,5,5,5,5,3,3,3,2,2,2,1,1,0 },  // slit enters
        { 1,2,3,3,3,3,5,5,5,5,5,5,3,3,3,2,2,2,1,0 },  // widest — slit wide
        { 0,1,2,2,3,3,3,5,5,5,5,3,3,3,2,2,1,1,0,0 },  // narrowing down
        { 0,0,1,1,2,2,2,2,2,2,2,2,2,1,1,1,0,0,0,0 },  // closing fast
        { 0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 },  // near bottom tip
        { 0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0 },  // sharp asymmetric bottom point
    };

    // Lid halfway down — top rows gone, bottom half visible
    private static readonly int[,] EyeHalf =
    {
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 1,2,3,3,3,3,5,5,5,5,5,5,3,3,3,2,2,2,1,0 },  // widest row only
        { 0,1,2,2,3,3,3,5,5,5,5,3,3,3,2,2,1,1,0,0 },
        { 0,0,1,1,2,2,2,2,2,2,2,2,2,1,1,1,0,0,0,0 },
        { 0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0 },
    };

    // Fully closed — single angular line
    private static readonly int[,] EyeClosed =
    {
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0 },  // angular closed line
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
    };

    public enum BlinkState { Open, Closing, Closed, Opening }
    public BlinkState State       = BlinkState.Closed;
    private float _blinkTimer     = 10f;
    private float _stateTimer     = 0f;
    public float  TransitionSpeed = 0.13f;
    public bool   AutoBlink       = true;

    private Vector2 _leftPupilOffset  = Vector2.Zero;
    private Vector2 _rightPupilOffset = Vector2.Zero;

    private int[,] CurrentGrid =>
        State == BlinkState.Open    ? EyeOpen   :
        State == BlinkState.Closing ? EyeHalf   :
        State == BlinkState.Closed  ? EyeClosed :
                                      EyeHalf;

    public override void _Process(double delta)
    {
        Vector2 mouse = GetViewport().GetMousePosition();
        _leftPupilOffset  = SnapPupil(mouse, GlobalPosition + new Vector2(-EyeSpacing, 0));
        _rightPupilOffset = SnapPupil(mouse, GlobalPosition + new Vector2( EyeSpacing, 0));

        switch (State)
        {
            case BlinkState.Open:
                if (AutoBlink)
                {
                    _blinkTimer -= (float)delta;
                    if (_blinkTimer <= 0f) { State = BlinkState.Closing; _stateTimer = TransitionSpeed; }
                }
                break;
            case BlinkState.Closing:
                _stateTimer -= (float)delta;
                if (_stateTimer <= 0f) { State = BlinkState.Closed; _stateTimer = TransitionSpeed; }
                break;
            case BlinkState.Closed:
                _stateTimer -= (float)delta;
                if (_stateTimer <= 0f && AutoBlink) { State = BlinkState.Opening; _stateTimer = TransitionSpeed; }
                break;
            case BlinkState.Opening:
                _stateTimer -= (float)delta;
                if (_stateTimer <= 0f)
                {
                    State       = BlinkState.Open;
                    _blinkTimer = (float)GD.RandRange(8.0, 12.0);
                }
                break;
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawPixelEye(new Vector2(-EyeSpacing, 0), false, _leftPupilOffset);
        DrawPixelEye(new Vector2( EyeSpacing, 0), true,  _rightPupilOffset);
    }

    private void DrawPixelEye(Vector2 center, bool rightEye, Vector2 pupilOffset)
    {
        int[,] grid = CurrentGrid;
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        float ox = center.X - cols * PS / 2f;
        float oy = center.Y - rows * PS / 2f;

        int pdx = Mathf.RoundToInt(pupilOffset.X);
        int pdy = Mathf.RoundToInt(pupilOffset.Y);

        // Subtle pixel glow — faint iris colour 1 cell outside the outline
        Color glowColor = rightEye
            ? new Color(OR2.R, OR2.G, OR2.B, 0.18f)
            : new Color(BL2.R, BL2.G, BL2.B, 0.18f);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] != 0)
                    DrawRect(new Rect2(ox + c * PS - 3, oy + r * PS - 3, PS + 6, PS + 6), glowColor);

        // Thick black outline (2px border around each pixel)
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] != 0)
                    DrawRect(new Rect2(ox + c * PS - 2, oy + r * PS - 2, PS + 4, PS + 4), Black);

        // Iris + pupil fill
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = grid[r, c];
                if (cell == 0) continue;

                float px = ox + c * PS;
                float py = oy + r * PS;

                Color color;
                if (cell == 5)
                    color = Pupil;
                else if (rightEye)
                    color = cell == 1 ? OR1 : cell == 2 ? OR2 : cell == 3 ? OR3 : OR4;
                else
                    color = cell == 1 ? BL1 : cell == 2 ? BL2 : cell == 3 ? BL3 : BL4;

                float fx = cell == 5 ? px + pdx : px;
                float fy = cell == 5 ? py + pdy : py;
                DrawRect(new Rect2(fx, fy, PS, PS), color);
            }
        }

        // Shine — 2 bright pixels top-left of iris when open
        if (State == BlinkState.Open || State == BlinkState.Opening)
        {
            DrawRect(new Rect2(ox + 4 * PS, oy + 1 * PS, PS, PS), Shine);
            DrawRect(new Rect2(ox + 3 * PS, oy + 2 * PS, PS, PS), Shine);
        }
    }

    private Vector2 SnapPupil(Vector2 mouse, Vector2 eyeWorld)
    {
        Vector2 dir = mouse - eyeWorld;
        if (dir.Length() < 20f) return Vector2.Zero;
        dir = dir.Normalized() * Mathf.Min(PS, dir.Length() * 0.04f);
        return new Vector2(Mathf.Round(dir.X / PS) * PS, Mathf.Round(dir.Y / PS) * PS);
    }
}
