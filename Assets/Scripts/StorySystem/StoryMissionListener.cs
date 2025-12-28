using UnityEngine;

public class StoryMissionListener : MonoBehaviour
{
    [Header("Mission Configuration")]
    public ItemData safeItemData; // DRAG THE SAFE ITEM DATA HERE
    public StreetPhoneSystem phoneSystem; // DRAG YOUR PHONE UI SCRIPT HERE

    void Start()
    {
        // Wait a bit to ensure InventoryManager is initialized
        Invoke(nameof(RegisterEvents), 0.1f);
    }

    void RegisterEvents()
    {
        if (InventoryManager.Instance != null)
        {
            // Subscribe to the events from your classmate's script
            InventoryManager.Instance.onGoldChanged += CheckMoneyCondition;
            InventoryManager.Instance.onInventoryChanged += CheckSafeCondition;

            Debug.Log("StoryMissionListener: Connected to InventoryManager.");
        }
    }

    void OnDestroy()
    {
        // Always unsubscribe when destroyed to prevent errors
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onGoldChanged -= CheckMoneyCondition;
            InventoryManager.Instance.onInventoryChanged -= CheckSafeCondition;
        }
    }

    // --- Logic ---

    // 1. Listen for Money Changes
    void CheckMoneyCondition(int currentGold)
    {
        // Sync gold to GameState
        if (GameState.Instance != null)
            GameState.Instance.money = currentGold;

        // Check: Is Player in Stage 1 AND has >= 50 Gold?
        if (GameState.Instance != null && GameState.Instance.storyStage == 1)
        {
            if (currentGold >= 50)
            {
                Debug.Log("Mission: Money target reached! Triggering Phone Call.");

                // Advance Stage to prevent repeating the call
                GameState.Instance.storyStage = 2;

                // Show the Phone UI
                if (phoneSystem != null)
                    phoneSystem.ShowPhoneCall();
            }
        }
    }

    // 2. Listen for Backpack Item Changes
    void CheckSafeCondition()
    {
        // Check: Is Player in Stage 2 (Post-Phone Call)?
        if (GameState.Instance != null && GameState.Instance.storyStage == 2)
        {
            // Loop through classmate's backpack list
            foreach (var slot in InventoryManager.Instance.backpackContent)
            {
                // Check if we have the Safe
                if (slot.itemData == safeItemData)
                {
                    Debug.Log("Mission: Safe acquired! Return to Cafe.");

                    // Advance Stage
                    GameState.Instance.storyStage = 3;

                    // Optional: Show a "Mission Updated" popup here
                    break;
                }
            }
        }
    }
}