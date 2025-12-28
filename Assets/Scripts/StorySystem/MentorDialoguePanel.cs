using UnityEngine;
using UnityEngine.UI;

public class MentorDialoguePanel_Legacy : MonoBehaviour
{
    [Header("UI Main")]
    // бя NEW: Drag your entire UI Panel (the parent object) here
    public GameObject uiPanel;

    [Header("UI Elements")]
    public Text dialogueText;
    public Button continueButton;

    [Header("Rewards")]
    public ItemData lockpickItem;

    [Header("Dialogue Lines")]
    [TextArea] public string[] normalLines;
    [TextArea] public string[] finalRewardLines;

    private string[] activeLines;
    private int index = 0;

    void Start()
    {
        // бя Ensure the panel is visible when game starts
        if (uiPanel != null)
            uiPanel.SetActive(true);

        if (continueButton != null)
            continueButton.onClick.AddListener(Advance);

        CheckStoryState();
    }

    void CheckStoryState()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        // Check Stage
        if (gs.storyStage == 3)
        {
            activeLines = finalRewardLines;
        }
        else if (gs.storyStage == 4)
        {
            activeLines = new string[] { "Go use your new tools." };
        }
        else
        {
            activeLines = normalLines;
        }

        // Safety Check (Prevention for IndexOutOfRange)
        if (activeLines == null || activeLines.Length == 0)
        {
            if (dialogueText != null) dialogueText.text = "...";
            return;
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
        if (dialogueText == null) return;

        // Safety check
        if (i < 0 || i >= activeLines.Length) return;

        dialogueText.text = activeLines[i];
    }

    void EndDialogue()
    {
        // 1. Give Rewards logic
        if (GameState.Instance != null && GameState.Instance.storyStage == 3)
        {
            GiveRewards();
        }

        // 2. бя Hide the UI
        Debug.Log("Dialogue Finished. Closing UI.");

        if (uiPanel != null)
        {
            // Option A: Hide the whole panel (Recommended)
            uiPanel.SetActive(false);
        }
        else
        {
            // Option B: Fallback - Hide individual elements if panel is not assigned
            if (dialogueText != null) dialogueText.gameObject.SetActive(false);
            if (continueButton != null) continueButton.gameObject.SetActive(false);

            // Option C: Hide this entire object (if the script is on the panel itself)
            // gameObject.SetActive(false);
        }
    }

    void GiveRewards()
    {
        Debug.Log("Story: Giving Lockpicks...");

        if (lockpickItem != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(lockpickItem);
            InventoryManager.Instance.AddItem(lockpickItem);
        }

        GameState.Instance.storyStage = 4;
    }
}