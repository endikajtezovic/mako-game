public static class PixelFont
{
    // All uppercase, thick 2px strokes, serrated/jagged bottoms, angular cuts
    // 12 rows tall for a towering feel

    // M — wide double pillars, deep V-notch, spiked top corners
    public static readonly int[,] M =
    {
        { 1,1,0,0,0,0,1,1 },
        { 1,1,1,0,0,1,1,1 },
        { 1,1,1,1,1,1,1,1 },
        { 1,1,0,1,1,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,1,0,0,0,0,1,1 },
        { 1,0,0,0,0,0,0,1 },  // jagged bottom
        { 0,1,0,0,0,0,1,0 },  // serrated spike
    };

    // A — angular cap, thick strokes, crossbar, serrated feet
    public static readonly int[,] A =
    {
        { 0,0,1,1,1,0,0 },
        { 0,1,1,1,1,1,0 },
        { 1,1,1,0,1,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,1,1,1,1,1,1 },  // crossbar
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,0,0,0,0,0,1 },  // jagged
        { 0,1,0,0,0,1,0 },  // serrated
    };

    // K — diagonal arms, sharp pivot, bottom-right spike
    public static readonly int[,] K =
    {
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,1,1,0 },
        { 1,1,0,1,1,0,0 },
        { 1,1,1,1,0,0,0 },
        { 1,1,1,1,1,0,0 },  // thick pivot
        { 1,1,1,1,0,0,0 },
        { 1,1,0,1,1,0,0 },
        { 1,1,0,0,1,1,0 },
        { 1,1,0,0,0,1,1 },
        { 1,1,0,0,0,1,1 },
        { 1,0,0,0,0,0,1 },  // jagged
        { 0,1,0,0,0,1,0 },  // serrated
    };

    // O — octagonal flat top/bottom, no soft curves, jagged base
    public static readonly int[,] O =
    {
        { 0,1,1,1,1,0 },
        { 1,1,1,1,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,0,0,1,1 },
        { 1,1,1,1,1,1 },
        { 0,1,1,1,1,0 },
        { 0,0,1,1,0,0 },  // jagged
        { 0,0,0,0,0,0 },
    };

    // ! — thick double column, gap, bold dot with spike
    public static readonly int[,] Exclaim =
    {
        { 1,1,1 },
        { 1,1,1 },
        { 1,1,1 },
        { 1,1,1 },
        { 1,1,1 },
        { 1,1,1 },
        { 1,1,1 },
        { 0,0,0 },  // gap
        { 1,1,1 },  // dot
        { 1,1,1 },
        { 0,1,0 },  // spike
        { 0,0,0 },
    };
}
