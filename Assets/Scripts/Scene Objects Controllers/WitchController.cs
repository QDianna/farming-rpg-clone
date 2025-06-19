using UnityEngine;

/// <summary>
/// Witch NPC that manages the main quest storyline about the earth's illness.
/// Handles multi-stage dialogue and potion collection mission progression.
/// Also unlocks the research table for purchase after first interaction.
/// </summary>
public class WitchController : MonoBehaviour, IInteractable
{
   [Header("Quest Settings")]
   [SerializeField] private InventoryItem strengthPotion;
   [SerializeField] private InventoryItem rareFlower;
   
   [SerializeField] private InventoryItem powerPotion;
   [SerializeField] private InventoryItem nourishPotion;
   [SerializeField] private InventoryItem speedPotion;
   [SerializeField] private InventoryItem endurancePotion;
   
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
               ShowFirstMeetingDialogue(player);
               break;
           case WitchQuestState.WaitingForPotions:
               CheckPotionCollection(player);
               break;
           case WitchQuestState.QuestComplete:
               ShowQuestCompleteDialogue(player);
               break;
       }
   }

   // Shows the initial story dialogue about the illness
   private void ShowFirstMeetingDialogue(PlayerController player)
   {
       if (QuestsSystem.Instance != null)
       {
           QuestsSystem.Instance.SetWitchMet();
       }

       NotificationSystem.ShowDialogue("Witch: So... you have come. I know why you are here. " +
                                       "The illness that grips your spouse... it is no ordinary sickness.", 4f);
       NotificationSystem.ShowDialogue("Witch: It is the land itself that is ill. " +
                                       "A creeping corruption spreads through the soil, the air... and into us.", 4f);
       NotificationSystem.ShowDialogue("Witch: You must have seen it too, your plants falling ill, " +
                                       "stunted growth, and the weather twisting with unusual force...", 4f);
       NotificationSystem.ShowDialogue("Witch: The cold is more powerful than before, the land refuses to grow anything. " +
                                       "Without help, your crops will wither before they sprout.", 4f);
       NotificationSystem.ShowDialogue("Witch: Take this... It is a strength potion... " +
                                       "it will allow your seeds to grow during the cold.", 4.8f);

       InventorySystem.Instance.AddItem(strengthPotion, 3);
       strengthPotion.CollectItem(player);

       NotificationSystem.ShowDialogue("Witch: But listen... this is only a gift. " +
                                       "You must learn how to craft more, or you will not survive the next winter!", 4.8f);
       NotificationSystem.ShowDialogue("Witch: I need you to bring me four potions: Power, Nourish, Speed, and Endurance. " +
                                       "They will help us stand against what is poisoning this land.", 4.8f);

       UnlockResearchTable();
       NotificationSystem.ShowHelp("New structures are now available in the market!");
       
       currentState = WitchQuestState.WaitingForPotions;
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
       NotificationSystem.ShowDialogue("Witch: Have you brought the potions? " +
                                       "Power, Nourish, Speed, and Endurance.", 4.8f);

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
       
       ShowCompletionDialogue(player);
   }
   
   private void ShowCompletionDialogue(PlayerController player)
   {
       NotificationSystem.ShowDialogue("Witch: You've done it... Thank you.", 3.4f);
       NotificationSystem.ShowDialogue("Witch: I asked you to bring me those potions because... " +
                                       "I am sick too.", 3.4f);
       NotificationSystem.ShowDialogue("Witch: With these, we can finally prepare the cure. " +
                                       "But it needs one last, secret ingredient... here, take this.", 4.8f);
       
       InventorySystem.Instance.AddItem(rareFlower, 1);
       rareFlower.CollectItem(player);
       
       NotificationSystem.ShowDialogue("Witch: I've been too weak to search for ingredients. " +
                                       "But now, with your help, we can both craft the cure.", 4.8f);
       NotificationSystem.ShowDialogue("Witch: Add the Rare Flower to the potions I asked you for. " +
                                       "The your spouse awaits you.", 4.8f);
       
       currentState = WitchQuestState.QuestComplete;

       // Marchează quest-ul ca terminat
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
   private void ShowQuestCompleteDialogue(PlayerController player)
   {
       NotificationSystem.ShowDialogue("Witch: Go now, craft the final potion and save your spouse!", 4f);
   }
}