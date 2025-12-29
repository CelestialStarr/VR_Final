using UnityEngine;

public class StoryMissionListener : MonoBehaviour
{
    [Header("调试")]
    public bool enableDebugLogs = true;

    [Header("任务配置")]
    public int targetGoldAmount = 50;

    void Start()
    {
        Invoke(nameof(RegisterEvents), 0.5f);
    }

    void RegisterEvents()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("找不到 InventoryManager！");
            return;
        }

        InventoryManager.Instance.onGoldChanged += CheckMoneyCondition;

        // 初始检查
        CheckMoneyCondition(InventoryManager.Instance.currentGold);
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onGoldChanged -= CheckMoneyCondition;
    }

    void CheckMoneyCondition(int currentGold)
    {
        if (GameState.Instance == null) return;

        if (enableDebugLogs)
        {
            Debug.Log($"[金币检测] 当前: {currentGold} / 目标: {targetGoldAmount} / Stage: {GameState.Instance.storyStage}");
        }

        // 核心逻辑：Stage 1 -> Stage 2
        if (GameState.Instance.storyStage == 1 && currentGold >= targetGoldAmount)
        {
            GameState.Instance.storyStage = 2;

            Debug.LogError("【剧情推进】金币足够，进入 Stage 2（回咖啡馆谈话）");
        }
    }
}
