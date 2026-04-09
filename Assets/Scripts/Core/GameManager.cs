using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void LoadScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }
}
