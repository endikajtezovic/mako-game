using Godot;

// A button that looks like plain glowing pixel text — no box, no border
public partial class PixelTextButton : Button
{
    private string _label;
    private bool   _hovered = false;

    private static readonly Color NormalColor = new Color(1f, 1f, 1f);
    private static readonly Color HoverColor  = new Color(1f, 0.92f, 0.1f); // yellow on hover

    public PixelTextButton(string label)
    {
        _label = label;
        Flat   = true;
        Text   = "";
        FocusMode = FocusModeEnum.None;
        CustomMinimumSize = new Vector2(200, 36);

        // Strip all theme styles so nothing draws behind it
        AddThemeStyleboxOverride("normal",   new StyleBoxEmpty());
        AddThemeStyleboxOverride("hover",    new StyleBoxEmpty());
        AddThemeStyleboxOverride("pressed",  new StyleBoxEmpty());
        AddThemeStyleboxOverride("focus",    new StyleBoxEmpty());
        AddThemeStyleboxOverride("disabled", new StyleBoxEmpty());

        MouseEntered += () => { _hovered = true;  QueueRedraw(); };
        MouseExited  += () => { _hovered = false; QueueRedraw(); };
    }

    public override void _Draw()
    {
        Color color = _hovered ? HoverColor : NormalColor;
        var font = ThemeDB.FallbackFont;
        int fontSize = 22;

        // Black shadow for pixel depth
        DrawString(font, new Vector2(2, fontSize + 2), _label,
                   HorizontalAlignment.Left, -1, fontSize, Colors.Black);
        // Main text
        DrawString(font, new Vector2(0, fontSize), _label,
                   HorizontalAlignment.Left, -1, fontSize, color);
    }
}
