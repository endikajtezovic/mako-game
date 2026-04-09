using Godot;

// Place this below the level floor. Any CharacterBody2D that touches it respawns.
public partial class KillZone : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
            player.Respawn();
    }
}
