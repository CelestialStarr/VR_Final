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
    [TextArea] public string[] normalLines;
    [TextArea] public string[] finalRewardLines;

    private string[] activeLines;
    private int index = 0;

    void Start()
    {
        // ★ 修改 1: Start 里只需要绑定按钮，剩下的交给 CheckStoryState
        // 也就是“进游戏先检查一遍剧情”
        if (continueButton != null)
            continueButton.onClick.AddListener(Advance);

        CheckStoryState();
    }

    // ★ 修改 2: 改成 public，这样如果你从外面（比如别的脚本）修改了 Stage，
    // 可以喊一句这个函数，让面板重新根据新 Stage 弹出来！
    public void CheckStoryState()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        Debug.Log("Checking Story State... Current Stage: " + gs.storyStage);

        // --- 修正后的逻辑 ---

        // 情况 A：你是带着保险箱回来的 (Stage 3)
        // 这里的数字必须是 3！不能是 4！
        if (gs.storyStage == 3)
        {
            Debug.Log("检测到 Stage 3 -> 加载奖励对话");
            activeLines = finalRewardLines;
        }
        // 情况 B：奖励已经领完了 (Stage 4)
        // 必须加这个判断，否则你领完奖励再点师父，他又会说第一句话
        else if (gs.storyStage == 4)
        {
            Debug.Log("检测到 Stage 4 -> 任务已完成");
            // 这里可以写死一句话，或者你在 Inspector 里加一个 completedLines 数组
            activeLines = new string[] { "Don't just stand there. Go try the lockpicks." };
        }
        // 情况 C：其他情况 (Stage 0, 1, 2)
        else
        {
            Debug.Log("检测到其他 Stage -> 加载默认对话");
            activeLines = normalLines;
        }

        // --- 下面保持不变 ---

        // Safety Check
        if (activeLines == null || activeLines.Length == 0)
        {
            if (dialogueText != null) dialogueText.text = "...";
            return;
        }

        // 只要有话要说，就强制弹窗
        if (uiPanel != null)
            uiPanel.SetActive(true);

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

        // 2. Hide the UI
        Debug.Log("Dialogue Finished. Closing UI.");

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
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

        // ★ 修改 4: 给完东西立刻把 Stage 设为 4
        // 这样下次对话就会变成 "Go use your new tools"，不会重复给奖励
        if (GameState.Instance != null)
        {
            GameState.Instance.storyStage = 4;
            Debug.Log("Story Updated: Stage set to 4.");
        }
    }
}