using UnityEngine;

/// <summary>
/// Witch NPC that manages the main quest storyline about the earth's illness.
/// Handles multi-stage dialogue and potion collection mission progression.
/// Also unlocks the research table for purchase after first interaction.
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
       // Mark that player has met the witch
       if (QuestsSystem.Instance != null)
       {
           QuestsSystem.Instance.SetWitchMet();
       }

       // Show dialogue sequence
       NotificationSystem.ShowDialogue("Witch: So... you’ve come. I know why you’re here. The illness that grips your spouse... it is no ordinary sickness.", 4f);
       NotificationSystem.ShowDialogue("Witch: It is the land itself that is ill. A creeping corruption spreads through the soil, the air... and into us.", 4f);
       NotificationSystem.ShowDialogue("Witch: I’ve seen it before. It is why I grow weaker each day. Alone, I can no longer fight it.", 4f);
       NotificationSystem.ShowDialogue("Witch: If we are to stop this, we must study the plants — understand how to craft potions to resist the corruption.", 4f);
       NotificationSystem.ShowDialogue("Witch: Bring me four potions: Power, Nourish, Speed, and Endurance. They will help us stand against what is poisoning this land.", 4f);

       // Unlock research table and crafting bench
       UnlockResearchTable();

       // Change state to waiting for potions
       currentState = WitchQuestState.WaitingForPotions;

       // Notify player
       NotificationSystem.ShowHelp("New structures are now available in the market!");
   }
   
   // Unlocks the research table for purchase in the market
   private void UnlockResearchTable()
   {
       if (MarketSystem.Instance != null)
       {
           MarketSystem.Instance.UnlockResearchTable();
           MarketSystem.Instance.UnlockCraftingBench();
       }
   }

   // Checks if player has all required potions
   private void CheckPotionCollection(PlayerController player)
   {
       NotificationSystem.ShowDialogue("Witch: Have you brought the potions? Power, Nourish, Speed, and Endurance.", 4f);

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
       
       ShowCompletionDialogue();
   }
   
   private void ShowCompletionDialogue()
   {
       NotificationSystem.ShowDialogue("Witch: Perfect... thanks to you, I can finally prepare the cure.", 4f);
       Invoke(nameof(ShowFinalDialogue), 5f);
   }
   
   private void ShowFinalDialogue()
   {
       NotificationSystem.ShowDialogue("Witch: Take this rare flower. Add it to your potions to complete the cure.", 4f);
       
       currentState = WitchQuestState.QuestComplete;
       
       // Mark quest as completed
       if (QuestsSystem.Instance != null)
       {
           QuestsSystem.Instance.SetWitchQuestCompleted();
       }
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
       
       NotificationSystem.ShowDialogue($"Witch: You still need: {missingPotions}", 4f);
   }
   
   // Shows dialogue when quest is complete
   private void ShowQuestCompleteDialogue()
   {
       NotificationSystem.ShowDialogue("Witch: Go now, craft the final potion and save your spouse!", 4f);
   }
}