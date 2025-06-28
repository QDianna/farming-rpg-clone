using System.Collections;
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
   [SerializeField] private InventoryItem healPotion;
   [SerializeField] private InventoryItem endurancePotion;
   [SerializeField] private InventoryItem speedPotion;
   
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
               StartFirstMeeting(player);
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
    public void StartFirstMeeting(PlayerController player)
    {
        StartCoroutine(FirstMeetingSequence(player));
    }

    private IEnumerator FirstMeetingSequence(PlayerController player)
    {
        if (QuestsSystem.Instance != null)
        {
            QuestsSystem.Instance.SetWitchMet();
        }

        // Queue up initial dialogues (already have delays handled internally)
        NotificationSystem.ShowDialogue("Witch: So... you have come. I know why you are here. " +
                                        "The illness that grips your spouse... it is no ordinary sickness.", 4f);
        NotificationSystem.ShowDialogue("Witch: It is the land itself that is ill. " +
                                        "A creeping corruption spreads through the soil, the air... and into us.", 4f);
        NotificationSystem.ShowDialogue("Witch: You must have seen it too, your plants falling ill, " +
                                        "stunted growth, and the weather twisting with unusual force...", 4f);
        NotificationSystem.ShowDialogue("Witch: The cold is more powerful than before, the land refuses to grow anything. " +
                                        "Without help, your crops will wither before they sprout.", 4f);
        NotificationSystem.ShowDialogue("Witch: Take this... It is a strength potion... " +
                                        "it will allow your seeds to grow during the cold.", 5f);

        // Wait for 21 seconds = total duration of the above
        yield return new WaitForSeconds(4f * 4 + 5f); // 21 seconds

        // Give potion
        InventorySystem.Instance.AddItem(strengthPotion, 3);
        strengthPotion.CollectItem(player);

        // Remaining dialogue (already internally delayed)
        NotificationSystem.ShowDialogue("Witch: But listen... this is only a gift. " +
                                        "You must learn how to craft more, or you will not survive the next winter!", 5f);
        NotificationSystem.ShowDialogue("Witch: I need you to bring me four potions: Power, Heal, Speed, and Endurance. " +
                                        "They will help us stand against what is poisoning this land.", 5f);

        // Wait another 10 seconds for the two above
        yield return new WaitForSeconds(10f);

        // Final unlocks
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
                                       "Power, Heal, Speed, and Endurance.", 4.8f);

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
              InventorySystem.Instance.HasItemByName("heal potion", 1) &&
              InventorySystem.Instance.HasItemByName("speed potion", 1) &&
              InventorySystem.Instance.HasItemByName("endurance potion", 1);
   }
   
   // Takes potions and gives the final flower
   private void CollectPotionsAndGiveReward(PlayerController player)
   {
       // Remove potions from inventory
       var powerPot = InventorySystem.Instance.FindItemByName("power potion");
       var healPot = InventorySystem.Instance.FindItemByName("heal potion");
       var endurancePot = InventorySystem.Instance.FindItemByName("endurance potion");
       var speedPot = InventorySystem.Instance.FindItemByName("speed potion");
       
       InventorySystem.Instance.RemoveItem(powerPot, 1);
       InventorySystem.Instance.RemoveItem(healPot, 1);
       InventorySystem.Instance.RemoveItem(endurancePot, 1);
       InventorySystem.Instance.RemoveItem(speedPot, 1);
       
       // Give final flower
       InventorySystem.Instance.AddItem(rareFlower, 1);
       rareFlower.CollectItem(player);
       
       StartCompletionDialogue(player);
   }
   
   public void StartCompletionDialogue(PlayerController player)
   {
       StartCoroutine(CompletionSequence(player));
   }

   private IEnumerator CompletionSequence(PlayerController player)
   {
       NotificationSystem.ShowDialogue("Witch: You've done it, good job!", 3f);
       NotificationSystem.ShowDialogue("Witch: With these, we can finally prepare the cure. " +
                                       "But it needs one last, secret ingredient... here, take this.", 4f);

       yield return new WaitForSeconds(3f + 4f); // Wait for first 2 lines to finish

       // Give rare flower
       InventorySystem.Instance.AddItem(rareFlower, 1);
       rareFlower.CollectItem(player);

       // Continue with the rest of the dialogue
       NotificationSystem.ShowDialogue("Witch: Add the Rare Flower to the potions I asked you for. " +
                                       "It will complete the final recipe!", 4f);
       NotificationSystem.ShowDialogue("Witch: Go now, your spouse awaits you!", 3f);
       
       currentState = WitchQuestState.QuestComplete;

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
       if (!InventorySystem.Instance.HasItemByName("heal potion", 1))
           missingPotions += "Heal, ";
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