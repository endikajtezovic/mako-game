using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private AudioStreamPlayer musicPlayer;
    private AudioStreamPlayer sfxPlayer;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;

        musicPlayer = new AudioStreamPlayer();
        sfxPlayer = new AudioStreamPlayer();
        AddChild(musicPlayer);
        AddChild(sfxPlayer);
    }

    public void PlayMusic(AudioStream stream, bool loop = true)
    {
        if (musicPlayer.Stream == stream) return;

        musicPlayer.Stream = stream;
        musicPlayer.Play();
    }

    public void PlaySFX(AudioStream stream)
    {
        sfxPlayer.Stream = stream;
        sfxPlayer.Play();
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }
}
