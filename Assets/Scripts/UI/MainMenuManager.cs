using Godot;
using System.Collections.Generic;

public partial class MainMenuManager : CanvasLayer
{
    [Export] private AudioStream titleMusic;
    [Export] private float bounceSpeed  = 1.4f;
    [Export] private float bounceHeight = 5f;

    private const float ArcDepth         = 24f;
    private const float PerspectiveScale  = 0.16f;
    private const int   PixelSize         = 6;
    private const int   OutlineThickness  = 2;
    private const float LetterGap         = -2f;
    private const float TitleY            = 80f;

    private readonly int[][,] letterGrids = {
        PixelFont.M, PixelFont.A, PixelFont.K, PixelFont.O, PixelFont.Exclaim
    };

    private List<PixelLetter> letterNodes = new();
    private List<float>       letterXBase = new();
    private float elapsed = 0f;
    private bool  _built  = false;

    private Color[] palette = {
        new Color(1f,   0.12f, 0.12f),
        new Color(1f,   0.62f, 0.05f),
        new Color(1f,   1f,   0.12f),
        new Color(0.1f, 1f,   0.3f),
        new Color(0.1f, 0.45f, 1f),
        new Color(0.7f, 0.1f,  1f),
    };

    public override void _Ready()
    {
        if (AudioManager.Instance != null && titleMusic != null)
            AudioManager.Instance.PlayMusic(titleMusic);
    }

    private void BuildAll()
    {
        Vector2 screen = new Vector2(1152, 648);
        BuildTitle(screen);
        BuildButtons(screen);
        _built = true;
    }

    private void BuildTitle(Vector2 screen)
    {
        float totalWidth = 0f;
        var widths = new float[letterGrids.Length];
        for (int i = 0; i < letterGrids.Length; i++)
        {
            widths[i] = letterGrids[i].GetLength(1) * PixelSize * 0.68f;
            totalWidth += widths[i];
            if (i < letterGrids.Length - 1) totalWidth += LetterGap;
        }

        float cursorX = (screen.X - totalWidth) / 2f;

        for (int i = 0; i < letterGrids.Length; i++)
        {
            float t = letterGrids.Length > 1
                ? (i / (float)(letterGrids.Length - 1)) * 2f - 1f : 0f;

            float arcY     = ArcDepth * (1f - t * t);
            float scale    = 1f + PerspectiveScale * (1f - t * t);
            float rotation = Mathf.Atan2(-ArcDepth * 2f * t, widths[i] + LetterGap) * 0.4f;

            var node = new PixelLetter();
            node.Setup(letterGrids[i], PixelSize, OutlineThickness);
            node.Position = new Vector2(cursorX, TitleY + arcY);
            node.Scale    = new Vector2(0.68f * scale, 1.32f * scale);
            node.Rotation = rotation;

            AddChild(node);
            letterNodes.Add(node);
            letterXBase.Add(cursorX);

            cursorX += widths[i] + LetterGap;
        }
    }

    private void BuildEyes(Vector2 screen)
    {
        var eyes = new CatEyes();
        eyes.AutoBlink = true;
        eyes.State     = CatEyes.BlinkState.Open;
        eyes.Position  = new Vector2(screen.X / 2f, screen.Y * 0.36f);
        AddChild(eyes);
    }

    private void BuildButtons(Vector2 screen)
    {
        float centerX = screen.X / 2f - 60f;
        float startY  = screen.Y * 0.44f;
        float gap     = 46f;

        var newGameBtn = new PixelTextButton("NEW GAME");
        newGameBtn.Position = new Vector2(centerX, startY);
        newGameBtn.Pressed += OnNewGamePressed;
        AddChild(newGameBtn);

        var loadBtn = new PixelTextButton("LOAD");
        loadBtn.Position = new Vector2(centerX, startY + gap);
        loadBtn.Pressed += OnLoadPressed;
        AddChild(loadBtn);

        var diffBtn = new PixelTextButton("DIFFICULTY");
        diffBtn.Position = new Vector2(centerX, startY + gap * 2f);
        diffBtn.Pressed += OnDifficultyPressed;
        AddChild(diffBtn);
    }

    public override void _Process(double delta)
    {
        if (!_built) { BuildAll(); return; }

        elapsed += (float)delta;

        for (int i = 0; i < letterNodes.Count; i++)
        {
            float t = letterNodes.Count > 1
                ? (i / (float)(letterNodes.Count - 1)) * 2f - 1f : 0f;

            float arcY     = ArcDepth * (1f - t * t);
            float wave     = Mathf.Sin(elapsed * bounceSpeed + i * 0.4f) * bounceHeight;
            float scale    = 1f + PerspectiveScale * (1f - t * t);
            float rotation = Mathf.Atan2(-ArcDepth * 2f * t, 50f) * 0.4f;

            letterNodes[i].Position = new Vector2(letterXBase[i], TitleY + arcY + wave);
            letterNodes[i].Scale    = new Vector2(0.68f * scale, 1.32f * scale);
            letterNodes[i].Rotation = rotation;
            letterNodes[i].SetColor(new Color(1f, 0.92f, 0.1f));
        }
    }

    private void OnNewGamePressed()    => GameManager.Instance.LoadScene("res://Assets/Scenes/EyeTransition.tscn");
    private void OnLoadPressed()       => GD.Print("Load — not yet implemented");
    private void OnDifficultyPressed() => GD.Print("Difficulty — not yet implemented");
}
