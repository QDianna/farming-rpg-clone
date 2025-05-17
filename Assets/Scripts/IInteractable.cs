using UnityEngine;

/// <summary>
/// Interface for all interactable objects in the game world.
/// Enables modular interaction logic by allowing any object to define its own Interact behavior
/// when triggered by the player (e.g., doors, chests, NPCs, signs).
/// </summary>

public interface IInteractable
{
    void OnTriggerEnter2D(Collider2D other);
    void OnTriggerExit2D(Collider2D other);
    
    void Interact(PlayerController player);
}
