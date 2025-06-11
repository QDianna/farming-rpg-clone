using UnityEngine;

/// <summary>
/// Witch NPC that manages the main quest storyline about the earth's illness.
/// Handles multi-stage dialogue and potion collection mission progression.
/// </summary>
public class WitchController : MonoBehaviour, IInteractable
{
   [Header("Quest Settings")]
   [SerializeField] private InventoryItem powerPotion;
   [SerializeField] private InventoryItem nourishPotion;
   [SerializeField] private InventoryItem speedPotion;
   [SerializeField] private InventoryItem endurancePotion;
   [SerializeField] private InventoryItem rareFlower;
   
   private enum WitchQuestState
   {
       FirstMeeting,
       QuestGiven,
       WaitingForPotions,
       QuestComplete
   }
   
   private WitchQuestState currentState = WitchQuestState.FirstMeeting;
   
   public void OnTriggerEnter2D(Collider2D other)
   {
       if (other.TryGetComponent<PlayerController>(out var _))
       {
           InteractionSystem.Instance.SetCurrentInteractable(this);
       }
   }

   public void OnTriggerExit2D(Collider2D other)
   {
       if (other.TryGetComponent<PlayerController>(out var _))
       {
           InteractionSystem.Instance.SetCurrentInteractable(null);
       }
   }

   public void Interact(PlayerController player)
   {
       switch (currentState)
       {
           case WitchQuestState.FirstMeeting:
               ShowFirstMeetingDialogue();
               break;
           case WitchQuestState.QuestGiven:
               ShowQuestReminderDialogue();
               break;
           case WitchQuestState.WaitingForPotions:
               CheckPotionCollection(player);
               break;
           case WitchQuestState.QuestComplete:
               ShowQuestCompleteDialogue();
               break;
       }
   }
   
   // Shows the initial story dialogue about the illness
   private void ShowFirstMeetingDialogue()
   {
       NotificationSystem.ShowNotification("Witch: A terrible illness has overcome our earth...");
       
       // You can expand this with a proper dialogue system
       // For now, using notifications in sequence
       Invoke(nameof(ContinueFirstDialogue), 3f);
   }
   
   // Continues the first meeting dialogue
   private void ContinueFirstDialogue()
   {
       NotificationSystem.ShowNotification("Witch: Your wife... she suffers from it, as do I...");
       Invoke(nameof(GiveQuest), 3f);
   }
   
   // Gives the potion collection quest
   private void GiveQuest()
   {
       NotificationSystem.ShowNotification("Witch: Bring me 4 potions: Power, Nourish, Speed, and Endurance.");
       NotificationSystem.ShowNotification("Witch: Only then can I give you the final ingredient...");
       
       currentState = WitchQuestState.QuestGiven;
       
       // Wait a moment then change to waiting state
       Invoke(nameof(StartWaitingForPotions), 2f);
   }
   
   // Changes state to waiting for potions
   private void StartWaitingForPotions()
   {
       currentState = WitchQuestState.WaitingForPotions;
   }
   
   // Shows quest reminder if player talks again
   private void ShowQuestReminderDialogue()
   {
       NotificationSystem.ShowNotification("Witch: Remember - bring me Power, Nourish, Speed, and Endurance potions.");
       currentState = WitchQuestState.WaitingForPotions;
   }
   
   // Checks if player has all required potions
   private void CheckPotionCollection(PlayerController player)
   {
       if (HasAllRequiredPotions())
       {
           CollectPotionsAndGiveReward(player);
       }
       else
       {
           ShowMissingPotionsMessage();
       }
   }
   
   // Checks if player has all 4 required potions
   private bool HasAllRequiredPotions()
   {
       return InventorySystem.Instance.HasItemByName("power potion", 1) &&
              InventorySystem.Instance.HasItemByName("nourish potion", 1) &&
              InventorySystem.Instance.HasItemByName("speed potion", 1) &&
              InventorySystem.Instance.HasItemByName("endurance potion", 1);
   }
   
   // Takes potions and gives the final flower
   private void CollectPotionsAndGiveReward(PlayerController player)
   {
       // Remove potions from inventory
       var powerPot = InventorySystem.Instance.FindItemByName("power potion");
       var nourishPot = InventorySystem.Instance.FindItemByName("nourish potion");
       var speedPot = InventorySystem.Instance.FindItemByName("speed potion");
       var endurancePot = InventorySystem.Instance.FindItemByName("endurance potion");
       
       InventorySystem.Instance.RemoveItem(powerPot, 1);
       InventorySystem.Instance.RemoveItem(nourishPot, 1);
       InventorySystem.Instance.RemoveItem(speedPot, 1);
       InventorySystem.Instance.RemoveItem(endurancePot, 1);
       
       // Give final flower
       InventorySystem.Instance.AddItem(rareFlower, 1);
       rareFlower.CollectItem(player);
       
       NotificationSystem.ShowNotification("Witch: Perfect! Here is the final ingredient - the Last Flower.");
       NotificationSystem.ShowNotification("Witch: Now you can craft the final potion to save us all!");
       
       currentState = WitchQuestState.QuestComplete;
   }
   
   // Shows what potions are still missing
   private void ShowMissingPotionsMessage()
   {
       string missingPotions = "";
       
       if (!InventorySystem.Instance.HasItemByName("power potion", 1))
           missingPotions += "Power, ";
       if (!InventorySystem.Instance.HasItemByName("nourish potion", 1))
           missingPotions += "Nourish, ";
       if (!InventorySystem.Instance.HasItemByName("speed potion", 1))
           missingPotions += "Speed, ";
       if (!InventorySystem.Instance.HasItemByName("endurance potion", 1))
           missingPotions += "Endurance, ";
       
       // Remove last comma and space
       if (missingPotions.Length > 2)
           missingPotions = missingPotions.Substring(0, missingPotions.Length - 2);
       
       NotificationSystem.ShowNotification($"Witch: You still need: {missingPotions}");
   }
   
   // Shows dialogue when quest is complete
   private void ShowQuestCompleteDialogue()
   {
       NotificationSystem.ShowNotification("Witch: Go now, craft the final potion and save your wife!");
   }
}