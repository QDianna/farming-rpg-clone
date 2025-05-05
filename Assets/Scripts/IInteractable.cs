/// <summary>
/// Interface for all interactable objects in the game world.
/// Enables modular interaction logic by allowing any object to define its own Interact behavior
/// when triggered by the player (e.g., doors, chests, NPCs, signs).
/// </summary>

public interface IInteractable
{
    void Interact(PlayerController player);
}
