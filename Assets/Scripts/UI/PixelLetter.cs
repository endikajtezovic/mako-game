using Godot;

public partial class PixelLetter : Node2D
{
    public Color FillColor  = new Color(1f, 0.92f, 0.1f);  // default yellow
    public Color ShadowColor = new Color(0.85f, 0.05f, 0.05f); // red 3D shadow
    public int   ShadowOffset = 4; // how many screen pixels the shadow is offset

    private int[,] _grid;
    private int _pixelSize;
    private int _outline;

    public void Setup(int[,] grid, int pixelSize = 6, int outlineThickness = 2)
    {
        _grid      = grid;
        _pixelSize = pixelSize;
        _outline   = outlineThickness;
        QueueRedraw();
    }

    public void SetColor(Color color)
    {
        FillColor = color;
        QueueRedraw();
    }

    public Vector2 GetLetterSize()
    {
        if (_grid == null) return Vector2.Zero;
        return new Vector2(_grid.GetLength(1) * _pixelSize, _grid.GetLength(0) * _pixelSize);
    }

    public override void _Draw()
    {
        if (_grid == null) return;

        int rows = _grid.GetLength(0);
        int cols = _grid.GetLength(1);
        int o    = _outline;
        int s    = ShadowOffset;

        // Pass 1 — red 3D shadow (offset down-right)
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (_grid[r, c] == 1)
                    DrawRect(new Rect2(c * _pixelSize + s, r * _pixelSize + s,
                                      _pixelSize, _pixelSize), ShadowColor);

        // Pass 2 — black outline (8-directional)
        int[] dx = { -o,  0, o, -o, o, -o, 0, o };
        int[] dy = { -o, -o, -o,  0, 0,  o, o, o };
        for (int d = 0; d < 8; d++)
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (_grid[r, c] == 1)
                        DrawRect(new Rect2(c * _pixelSize + dx[d], r * _pixelSize + dy[d],
                                          _pixelSize, _pixelSize), Colors.Black);

        // Pass 3 — yellow fill on top
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (_grid[r, c] == 1)
                    DrawRect(new Rect2(c * _pixelSize, r * _pixelSize,
                                      _pixelSize, _pixelSize), FillColor);
    }
}
