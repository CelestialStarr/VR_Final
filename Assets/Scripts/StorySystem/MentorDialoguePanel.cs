using UnityEngine;
using UnityEngine.UI;

public class MentorDialoguePanel_Legacy : MonoBehaviour
{
    [Header("UI Main")]
    public GameObject uiPanel;

    [Header("UI Elements")]
    public Text dialogueText;
    public Button continueButton;

    [Header("Rewards")]
    public ItemData lockpickItem;

    [Header("Dialogue Lines")]
    [TextArea] public string[] stage0_IntroLines;      // Stage 0
    [TextArea] public string[] stage2_MoneyDoneLines; // Stage 2 
    [TextArea] public string[] stage3_RewardLines;    // Stage 3
    [TextArea] public string[] stage4_ReminderLines;  // Stage 4

    private string[] activeLines;
    private int index = 0;

    void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(Advance);

        CheckStoryState();
    }

    //  核心：根据 Stage 选择“哪一组 Inspector 配的台词”
    public void CheckStoryState()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        bool shouldShowPanel = false;

        switch (gs.storyStage)
        {
            case 0:
                activeLines = stage0_IntroLines;
                shouldShowPanel = true;
                break;

            case 2:
                activeLines = stage2_MoneyDoneLines;
                shouldShowPanel = true;
                break;

            case 3:
                activeLines = stage3_RewardLines;
                shouldShowPanel = true;
                break;

            case 4:
                activeLines = stage4_ReminderLines;
                shouldShowPanel = true;
                break;

            default:
                // Stage 1 / 其他阶段：不显示
                if (uiPanel != null) uiPanel.SetActive(false);
                return;
        }

        if (activeLines == null || activeLines.Length == 0)
        {
            Debug.LogWarning($"Stage {gs.storyStage} 没有配置任何台词！");
            return;
        }

        if (uiPanel != null && shouldShowPanel)
            uiPanel.SetActive(true);

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
        if (i < 0 || i >= activeLines.Length) return;

        dialogueText.text = activeLines[i];
    }

    void EndDialogue()
    {
        int currentStage = GameState.Instance != null ? GameState.Instance.storyStage : -1;

        // Stage 0 → Stage 1
        if (currentStage == 0)
        {
            GameState.Instance.storyStage = 1;
            Debug.Log("【剧情推进】进入 Stage 1（赚钱阶段）");
        }
        // Stage 3 → Stage 4
        else if (currentStage == 3)
        {
            GiveRewards();
        }

        if (uiPanel != null)
            uiPanel.SetActive(false);
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
        Debug.Log("Story Updated: Stage set to 4.");
    }
}
