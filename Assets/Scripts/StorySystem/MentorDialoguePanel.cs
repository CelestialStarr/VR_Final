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
    [TextArea] public string[] stage0_IntroLines;
    [TextArea] public string[] stage2_MoneyDoneLines;
    [TextArea] public string[] stage3_RewardLines;
    [TextArea] public string[] stage4_ReminderLines;

    private string[] activeLines;
    private int index = 0;

    // ★ 用于 Stage2 控制，只播一次
    private bool stage2Played = false;

    void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(Advance);

        CheckStoryState();
    }

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
                if (stage2Played) // 如果已经播过，直接隐藏面板
                {
                    if (uiPanel != null) uiPanel.SetActive(false);
                    return;
                }
                activeLines = stage2_MoneyDoneLines;
                shouldShowPanel = true;
                stage2Played = true; // 播放一次后标记
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
                if (uiPanel != null) uiPanel.SetActive(false);
                return;
        }

        if (activeLines == null || activeLines.Length == 0)
        {
            Debug.LogWarning($"Stage {gs.storyStage} 没有配置台词！");
            if (uiPanel != null) uiPanel.SetActive(false);
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

        if (currentStage == 0)
        {
            GameState.Instance.storyStage = 1;
            Debug.Log("【剧情推进】进入 Stage 1（赚钱阶段）");
        }
        else if (currentStage == 3)
        {
            GiveRewards();
        }

        // Stage 2 不做任何推进，直接隐藏面板
        if (currentStage == 2)
        {
            if (uiPanel != null) uiPanel.SetActive(false);
            return;
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