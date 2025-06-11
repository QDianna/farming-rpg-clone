using UnityEngine;

/// <summary>
/// Interface for player-interactable objects with trigger detection and custom behavior.
/// Implementers handle their own proximity detection and define specific interaction actions.
/// </summary>
public interface IInteractable
{
    void OnTriggerEnter2D(Collider2D other);
    void OnTriggerExit2D(Collider2D other);
    void Interact(PlayerController player);
}