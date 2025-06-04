using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player.
/// Implementers define their own interaction behavior and trigger detection.
/// </summary>
public interface IInteractable
{
    void OnTriggerEnter2D(Collider2D other);
    void OnTriggerExit2D(Collider2D other);
    void Interact(PlayerController player);
}