using Godot;

public partial class EyeTransition : Node
{
    private CatEyes _eyes;
    private float   _timer    = 0f;
    private int     _stage    = 0;

    // Stage timings (seconds)
    // 0: black screen pause before eyes open
    // 1: eyes opening
    // 2: eyes fully open — hold
    // 3: eyes closing
    // 4: fully closed — load game

    private float[] _stageDurations = { 0.4f, 0.8f, 0.6f, 0.8f, 0.3f };

    public override void _Ready()
    {
        Vector2I ws = new Vector2I(1152, 648);

        // Black background
        var bg = new ColorRect();
        bg.Color = Colors.Black;
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bg.Size = new Vector2(ws.X, ws.Y);
        AddChild(bg);

        // Cat eyes centered on screen
        _eyes = new CatEyes();
        _eyes.AutoBlink    = false;
        _eyes.TransitionSpeed = 0.18f;
        _eyes.State        = CatEyes.BlinkState.Closed;
        _eyes.Position     = new Vector2(ws.X / 2f, ws.Y / 2f);
        AddChild(_eyes);
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;

        if (_timer >= _stageDurations[_stage])
        {
            _timer = 0f;
            _stage++;

            switch (_stage)
            {
                case 1: // start opening
                    _eyes.State = CatEyes.BlinkState.Opening;
                    break;
                case 2: // fully open
                    _eyes.State = CatEyes.BlinkState.Open;
                    break;
                case 3: // start closing
                    _eyes.State = CatEyes.BlinkState.Closing;
                    break;
                case 4: // fully closed
                    _eyes.State = CatEyes.BlinkState.Closed;
                    break;
                case 5: // load game
                    GameManager.Instance.LoadScene("res://Assets/Scenes/Level1.tscn");
                    break;
            }
        }
    }
}
