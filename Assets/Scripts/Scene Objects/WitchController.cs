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
   
   [Header("Dialogue Settings")]
   [SerializeField] private float dialogueDelay = 4f;
   private float originalDelay;
   
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
       // Mark that player has met the witch
       if (QuestsSystem.Instance != null)
       {
           QuestsSystem.Instance.SetWitchMet();
       }
       
       // Store original delay and set dialogue delay
       originalDelay = NotificationSystem.Instance.displayDuration;
       NotificationSystem.Instance.displayDuration = dialogueDelay;
       
       // Show first meeting dialogue
       NotificationSystem.ShowNotification("Witch: Ah... I've been waiting for you. I know about your spouse... their illness... it's the same that has begun to wither me as well.");
       NotificationSystem.ShowNotification("Witch: This is no ordinary sickness. Something has spread across this land... corrupting the plants, the air, even us.");
       NotificationSystem.ShowNotification("Witch: I've spent my life gathering herbs and crafting potions... but now, I can no longer venture out. My body grows weaker each day.");
       NotificationSystem.ShowNotification("Witch: You must help me. There may still be hope, but I can no longer gather nor prepare the ingredients myself.");
       
       // Unlock research table for purchase
       UnlockResearchTable();
       
       // Wait for dialogue to finish, then give quest
       Invoke(nameof(GiveQuestDialogue), dialogueDelay * 3 + 1f);
   }
   
   // Shows the quest dialogue
   private void GiveQuestDialogue()
   {
       // Quest dialogue
       NotificationSystem.ShowNotification("Witch: First, you must learn to understand the plants around us. Study them well — only through knowledge can you hope to craft what we need.");
       NotificationSystem.ShowNotification("Witch: You will need to prepare four potions: one to heal, one to strengthen, one to hasten the body, and one to endure what is to come.");
       NotificationSystem.ShowNotification("Witch: Study, gather, craft... and bring them to me. Our lives may depend on it.");
       
       currentState = WitchQuestState.QuestGiven;
       
       // Wait for quest dialogue to finish, then restore delay and change state
       Invoke(nameof(CompleteDialogueSetup), dialogueDelay * 3 + 3f);
   }
   
   // Restores original delay and changes state
   private void CompleteDialogueSetup()
   {
       // Restore original delay
       NotificationSystem.Instance.displayDuration = originalDelay;
       currentState = WitchQuestState.WaitingForPotions;
       
       NotificationSystem.ShowNotification("New structures are now available in the market!");
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
       
       NotificationSystem.ShowNotification("Witch: Perfect... thanks to you, I can finally prepare the cure.");
       NotificationSystem.ShowNotification("Witch: I needed you to learn, as well — so that you can save your spouse.");
       NotificationSystem.ShowNotification("Witch: Take this rare flower. It is the final ingredient... " +
                                           "add it to your potions, and it will complete the cure.");
       
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
       
       NotificationSystem.ShowNotification($"Witch: You still need: {missingPotions}");
   }
   
   // Shows dialogue when quest is complete
   private void ShowQuestCompleteDialogue()
   {
       NotificationSystem.ShowNotification("Witch: Go now, craft the final potion and save your spouse!");
   }
}