using Godot;

public partial class CatEyes : Node2D
{
    private const int   PS         = 8;
    private const float EyeSpacing = 150f;

    // Right eye — warm golden amber, intense
    private static readonly Color OR1 = new Color(1.00f, 0.94f, 0.42f);  // bright warm gold
    private static readonly Color OR2 = new Color(1.00f, 0.70f, 0.14f);  // deep amber
    private static readonly Color OR3 = new Color(0.88f, 0.38f, 0.04f);  // burnt orange
    private static readonly Color OR4 = new Color(0.12f, 0.03f, 0.00f);  // near-black pupil

    // Left eye — pale icy blue, eerie glow
    private static readonly Color BL1 = new Color(0.82f, 0.97f, 1.00f);  // pale ice white-blue
    private static readonly Color BL2 = new Color(0.30f, 0.80f, 1.00f);  // vivid electric blue
    private static readonly Color BL3 = new Color(0.08f, 0.50f, 0.92f);  // deep vivid blue
    private static readonly Color BL4 = new Color(0.01f, 0.06f, 0.40f);  // near-black pupil

    private static readonly Color LidBlack = new Color(0.05f, 0.05f, 0.05f);
    private static readonly Color Shine    = new Color(1f, 1f, 1f, 0.95f);

    // Color codes:
    // 0 = empty  |  9 = black lid / outline
    // 1 = outer iris (brightest)  |  2 = mid iris  |  3 = inner iris  |  4 = slit pupil (darkest)

    // RIGHT eye — 20 wide x 14 tall
    // Outer corner spike = upper-right   |   inner sharp tip = lower-left
    private static readonly int[,] RightOpen =
    {
        { 0,0,0,0,0,0,0,0,0,0,0,0,9,9,9,9,0,0,0,0 },  // outer spike
        { 0,0,0,0,0,0,0,9,9,9,9,9,9,9,9,9,9,0,0,0 },  // lid sweeps in
        { 0,0,0,0,9,9,9,9,9,9,9,9,9,9,9,9,9,9,0,0 },  // heavy lid
        { 0,0,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,0,0 },  // heavier lid
        { 0,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,0,0,0,0 },  // maximum lid — eye almost shut
        { 9,9,1,2,3,4,4,4,4,4,4,3,2,1,9,9,0,0,0,0 },  // crack opens — slit (6 px wide)
        { 9,1,2,3,3,4,4,4,4,4,4,3,3,2,1,9,0,0,0,0 },  // widening
        { 9,1,2,3,4,4,4,4,4,4,4,4,3,2,1,9,0,0,0,0 },  // widest — slit (8 px wide)
        { 0,1,2,3,4,4,4,4,4,4,4,3,2,1,9,0,0,0,0,0 },  // narrowing
        { 0,0,1,2,3,4,4,4,4,4,3,2,1,9,0,0,0,0,0,0 },  // tapering
        { 0,0,0,1,2,3,4,4,4,3,2,1,9,0,0,0,0,0,0,0 },  // narrowing more
        { 0,0,0,0,1,2,2,3,2,2,1,9,0,0,0,0,0,0,0,0 },  // near inner corner
        { 0,0,0,0,0,1,1,1,1,9,0,0,0,0,0,0,0,0,0,0 },  // almost at point
        { 0,0,0,0,0,0,9,9,0,0,0,0,0,0,0,0,0,0,0,0 },  // sharp inner corner tip
    };

    // Half-closed — lid covers top, lower iris + tip visible
    private static readonly int[,] RightHalf =
    {
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,0,0,0,0 },  // lid edge
        { 9,1,2,3,4,4,4,4,4,4,4,4,3,2,1,9,0,0,0,0 },
        { 0,1,2,3,4,4,4,4,4,4,4,3,2,1,9,0,0,0,0,0 },
        { 0,0,1,2,3,4,4,4,4,4,3,2,1,9,0,0,0,0,0,0 },
        { 0,0,0,1,2,3,4,4,4,3,2,1,9,0,0,0,0,0,0,0 },
        { 0,0,0,0,1,2,2,3,2,2,1,9,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,1,1,1,1,9,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,9,9,0,0,0,0,0,0,0,0,0,0,0,0 },
    };

    private static readonly int[,] RightClosed =
    {
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,0,0,0,0 },  // thin closed line
        { 0,0,0,9,9,9,9,9,9,9,9,9,9,0,0,0,0,0,0,0 },  // slight lower curve
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
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

    private int[,] CurrentGrid =>
        State == BlinkState.Open    ? RightOpen   :
        State == BlinkState.Closing ? RightHalf   :
        State == BlinkState.Closed  ? RightClosed :
                                      RightHalf;

    public override void _Process(double delta)
    {
        switch (State)
        {
            case BlinkState.Open:
                if (AutoBlink) { _blinkTimer -= (float)delta; if (_blinkTimer <= 0f) { State = BlinkState.Closing; _stateTimer = TransitionSpeed; } }
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
                if (_stateTimer <= 0f) { State = BlinkState.Open; _blinkTimer = (float)GD.RandRange(8.0, 12.0); }
                break;
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        // Left eye = right eye grid mirrored, blue palette — slightly stronger glow for eerie effect
        DrawPixelEye(new Vector2(-EyeSpacing, 0), mirror: true,  rightEye: false);
        // Right eye = normal orientation, orange palette
        DrawPixelEye(new Vector2( EyeSpacing, 0), mirror: false, rightEye: true);
    }

    private void DrawPixelEye(Vector2 center, bool mirror, bool rightEye)
    {
        int[,] grid = CurrentGrid;
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        float ox = center.X - cols * PS / 2f;
        float oy = center.Y - rows * PS / 2f;

        // Outer glow — blue eye slightly stronger for eerie feel
        Color glow = rightEye
            ? new Color(OR2.R, OR2.G, OR2.B, 0.15f)
            : new Color(BL2.R, BL2.G, BL2.B, 0.22f);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int cell = GetCell(grid, r, c, cols, mirror);
                if (cell != 0)
                    DrawRect(new Rect2(ox + c * PS - 4, oy + r * PS - 4, PS + 8, PS + 8), glow);
            }

        // Black outline pass
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int cell = GetCell(grid, r, c, cols, mirror);
                if (cell != 0)
                    DrawRect(new Rect2(ox + c * PS - 1, oy + r * PS - 1, PS + 2, PS + 2), LidBlack);
            }

        // Color fill pass
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = GetCell(grid, r, c, cols, mirror);
                if (cell == 0) continue;

                Color color;
                if (cell == 9)
                    color = LidBlack;
                else if (rightEye)
                    color = cell == 1 ? OR1 : cell == 2 ? OR2 : cell == 3 ? OR3 : OR4;
                else
                    color = cell == 1 ? BL1 : cell == 2 ? BL2 : cell == 3 ? BL3 : BL4;

                DrawRect(new Rect2(ox + c * PS, oy + r * PS, PS, PS), color);
            }
        }

        // Shine — white glint in the outer iris, rows 5–6 (first open rows)
        if (State == BlinkState.Open || State == BlinkState.Opening)
        {
            int sc = mirror ? cols - 3 : 2;
            DrawRect(new Rect2(ox + sc * PS,       oy + 5 * PS, PS, PS), Shine);
            DrawRect(new Rect2(ox + (sc + 1) * PS, oy + 6 * PS, PS, PS), Shine);
        }
    }

    private int GetCell(int[,] grid, int r, int c, int cols, bool mirror)
    {
        int col = mirror ? (cols - 1 - c) : c;
        return grid[r, col];
    }
}
