using UnityEngine;
using UnityEngine.UI;

public class MentorDialoguePanel_Legacy : MonoBehaviour
{
    [Header("UI")]
    public Text dialogueText;
    public Button continueButton;

    [Header("Rewards")]
    public ItemData lockpickItem; // DRAG LOCKPICK ITEM DATA HERE

    [Header("Dialogue Lines")]
    [TextArea] public string[] normalLines;       // Default chat
    [TextArea] public string[] finalRewardLines;  // Ending chat (Stage 3)

    private string[] activeLines;
    private int index = 0;

    void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(Advance);

        CheckStoryState();
    }

    void CheckStoryState()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        // Check Stage
        if (gs.storyStage == 3) // Player returned with the Safe
        {
            activeLines = finalRewardLines;
        }
        else if (gs.storyStage == 4) // Already got the reward
        {
            activeLines = new string[] { "Go use your new tools." };
        }
        else
        {
            activeLines = normalLines;
        }

        // Start Dialogue
        index = 0;
        ShowLine(0);
    }

    public void Advance()
    {
        if (activeLines == null) return;

        index++;
        if (index >= activeLines.Length)
        {
            EndDialogue();
            return;
        }
        ShowLine(index);
    }

    void ShowLine(int i)
    {
        if (dialogueText != null)
            dialogueText.text = activeLines[i];
    }

    void EndDialogue()
    {
        // Give Reward ONLY if finishing Stage 3
        if (GameState.Instance != null && GameState.Instance.storyStage == 3)
        {
            GiveRewards();
        }

        if (dialogueText != null)
            dialogueText.text = "--- End ---";
    }

    void GiveRewards()
    {
        Debug.Log("Story: Giving Lockpicks...");

        if (lockpickItem != null && InventoryManager.Instance != null)
        {
            // Give 2 Lockpicks
            InventoryManager.Instance.AddItem(lockpickItem);
            InventoryManager.Instance.AddItem(lockpickItem);
        }

        // Set Stage to 4 so we don't give rewards again
        GameState.Instance.storyStage = 4;
    }
}