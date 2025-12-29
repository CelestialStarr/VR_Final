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

        // 默认不显示面板，只有特定情况才打开
        bool shouldShowPanel = false;

        // 情况 A：你是带着保险箱回来的 (Stage 3) -> 显示奖励对话
        if (gs.storyStage == 3)
        {
            activeLines = finalRewardLines;
            shouldShowPanel = true;
        }
        // 情况 B：任务彻底完成 (Stage 4) -> 提醒你去用工具
        else if (gs.storyStage == 4)
        {
            activeLines = new string[] { "Don't just stand there. Go try the lockpicks." };
            shouldShowPanel = true;
        }
        // 情况 C：刚开始 (Stage 0) -> 可能你想显示个介绍？如果不想显示，这里也可以关掉
        else if (gs.storyStage == 0)
        {
            activeLines = normalLines;
            shouldShowPanel = true;
        }
        // 情况 D：赚钱中 (Stage 1) 或 刚接任务 (Stage 2) -> 师父不想理你
        else
        {
            // ★★★ 关键修改：在这里直接关闭面板，不显示任何东西 ★★★
            Debug.Log("当前阶段师父没有话要说，关闭面板。");
            if (uiPanel != null) uiPanel.SetActive(false);
            return; // 直接退出函数
        }

        // --- 下面只处理需要显示的情况 ---

        if (activeLines == null || activeLines.Length == 0) return;

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
        // 获取当前阶段
        int currentStage = (GameState.Instance != null) ? GameState.Instance.storyStage : -1;

        // 逻辑 A：如果是 Stage 3 (交任务)，给奖励并进阶到 4
        if (currentStage == 3)
        {
            GiveRewards();
            // GiveRewards 里已经写了 storyStage = 4，所以这里不用重复写
        }

        // ★★★ 逻辑 B (新增)：如果是 Stage 0 (刚开始)，进阶到 1 (赚钱阶段) ★★★
        else if (currentStage == 0)
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.storyStage = 1;
                Debug.Log("【剧情推进】新手对话结束，进入 Stage 1 (赚钱阶段)");
            }
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