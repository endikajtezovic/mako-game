using Godot;

public partial class LevelBuilder : Node
{
    private const int TileSize = 32;

    // (top-left tile pos, width in tiles, height in tiles)
    private static readonly (Vector2I pos, int w, int h)[] SolidAreas =
    {
        // Ground — full width, 3 tiles thick
        (new Vector2I( 0, 17), 80, 3),

        // Platforms
        (new Vector2I( 8, 14),  7, 1),
        (new Vector2I(18, 12),  7, 1),
        (new Vector2I(28, 10),  7, 1),
        (new Vector2I(38, 13),  6, 1),
        (new Vector2I(48, 11),  7, 1),
        (new Vector2I(58,  9),  6, 1),
        (new Vector2I(68, 12),  8, 1),
    };

    public override void _Ready()
    {
        // Visual layer — TileMap draws the tiles
        var tileSet = BuildVisualTileSet();
        var tileMap = new TileMap();
        tileMap.TileSet = tileSet;
        AddChild(tileMap);

        foreach (var (pos, w, h) in SolidAreas)
        {
            // Paint tiles
            for (int row = pos.Y; row < pos.Y + h; row++)
                for (int col = pos.X; col < pos.X + w; col++)
                    tileMap.SetCell(0, new Vector2I(col, row), 0, Vector2I.Zero);

            // Physics — one StaticBody2D per solid area (always works)
            var body  = new StaticBody2D();
            var shape = new CollisionShape2D();
            var rect  = new RectangleShape2D();

            rect.Size    = new Vector2(w * TileSize, h * TileSize);
            shape.Shape  = rect;
            body.Position = new Vector2(
                (pos.X + w / 2f) * TileSize,
                (pos.Y + h / 2f) * TileSize
            );

            body.AddChild(shape);
            AddChild(body);
        }
    }

    private static TileSet BuildVisualTileSet()
    {
        var src = new TileSetAtlasSource();
        src.Texture           = MakeTileTexture();
        src.TextureRegionSize = new Vector2I(TileSize, TileSize);
        src.CreateTile(Vector2I.Zero);

        var tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(TileSize, TileSize);
        tileSet.AddSource(src, 0);
        return tileSet;
    }

    private static ImageTexture MakeTileTexture()
    {
        var fill   = new Color(0.22f, 0.22f, 0.28f);
        var hi     = new Color(0.38f, 0.38f, 0.46f);
        var shadow = new Color(0.12f, 0.12f, 0.16f);
        var edge   = new Color(0.07f, 0.07f, 0.10f);

        var img = Image.Create(TileSize, TileSize, false, Image.Format.Rgba8);
        img.Fill(fill);

        for (int i = 0; i < TileSize; i++)
        {
            img.SetPixel(i, 0,          edge);
            img.SetPixel(i, TileSize-1, edge);
            img.SetPixel(0, i,          edge);
            img.SetPixel(TileSize-1, i, edge);

            if (i > 0 && i < TileSize-1)
            {
                img.SetPixel(i, 1,          hi);
                img.SetPixel(1, i,          hi);
                img.SetPixel(i, TileSize-2, shadow);
                img.SetPixel(TileSize-2, i, shadow);
            }
        }

        return ImageTexture.CreateFromImage(img);
    }
}
