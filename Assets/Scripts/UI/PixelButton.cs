using Godot;

// A pixel-art styled button drawn entirely in code — no theme needed.
public partial class PixelButton : Button
{
    private string _label;
    private bool   _hovered = false;

    private static readonly Color NormalBg   = new Color(0.08f, 0.08f, 0.18f);
    private static readonly Color HoverBg    = new Color(0.18f, 0.08f, 0.08f);
    private static readonly Color BorderColor = new Color(1f, 1f, 1f);
    private static readonly Color HoverBorder = new Color(1f, 1f, 0.2f);
    private static readonly Color TextColor   = new Color(1f, 1f, 1f);
    private static readonly Color TextHover   = new Color(1f, 1f, 0.2f);

    private const int W = 200;
    private const int H = 48;
    private const int BorderThick = 3;

    public PixelButton(string label)
    {
        _label = label;
        CustomMinimumSize = new Vector2(W, H);
        // Hide the default Godot button visuals
        Flat = true;
        Text = "";
        MouseEntered += () => { _hovered = true;  QueueRedraw(); };
        MouseExited  += () => { _hovered = false; QueueRedraw(); };
    }

    public override void _Draw()
    {
        Color bg     = _hovered ? HoverBg     : NormalBg;
        Color border = _hovered ? HoverBorder : BorderColor;
        Color text   = _hovered ? TextHover   : TextColor;

        // Background
        DrawRect(new Rect2(0, 0, W, H), bg);

        // Pixel border — draw as solid rects for chunky 8-bit feel
        DrawRect(new Rect2(0, 0, W, BorderThick), border);              // top
        DrawRect(new Rect2(0, H - BorderThick, W, BorderThick), border); // bottom
        DrawRect(new Rect2(0, 0, BorderThick, H), border);              // left
        DrawRect(new Rect2(W - BorderThick, 0, BorderThick, H), border); // right

        // Corner notches — cut the corners for that pixel-art bevelled look
        DrawRect(new Rect2(0, 0, BorderThick * 2, BorderThick * 2), bg);
        DrawRect(new Rect2(W - BorderThick * 2, 0, BorderThick * 2, BorderThick * 2), bg);
        DrawRect(new Rect2(0, H - BorderThick * 2, BorderThick * 2, BorderThick * 2), bg);
        DrawRect(new Rect2(W - BorderThick * 2, H - BorderThick * 2, BorderThick * 2, BorderThick * 2), bg);

        // Text centered — use DrawString with a large font size
        var font = ThemeDB.FallbackFont;
        int fontSize = 20;
        Vector2 strSize = font.GetStringSize(_label, HorizontalAlignment.Left, -1, fontSize);
        Vector2 textPos = new Vector2((W - strSize.X) / 2f, (H + strSize.Y) / 2f - 4f);
        DrawString(font, textPos, _label, HorizontalAlignment.Left, -1, fontSize, text);
    }
}
