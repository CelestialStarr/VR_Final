using UnityEngine;

public class StoryMissionListener : MonoBehaviour
{
    [Header("调试开关")]
    public bool enableDebugLogs = true;

    [Header("任务配置")]
    public ItemData safeItemData;
    public StreetPhoneSystem phoneSystem;
    public int targetGoldAmount = 30; // 你的目标金币

    void Start()
    {
        // 红色报错，强行显示，证明脚本活着
        Debug.LogError("【1】StoryMissionListener 脚本已启动！等待连接...");

        // 延迟 0.5秒 执行，防止 InventoryManager 还没初始化
        Invoke(nameof(RegisterEvents), 0.5f);
    }

    // --- 注册事件 (刚才报错的地方) ---
    void RegisterEvents()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("【严重错误】找不到 InventoryManager！脚本无法工作！");
            return;
        }

        Debug.LogError("【2】成功连接 InventoryManager！正在订阅事件...");

        try
        {
            // 订阅金币变化事件
            InventoryManager.Instance.onGoldChanged += CheckMoneyCondition;
            // 订阅背包变化事件
            InventoryManager.Instance.onInventoryChanged += CheckSafeCondition;

            Debug.LogError("【2.5】事件订阅成功！没有报错！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"【崩溃】订阅事件时出错了！原因: {e.Message}");
            return;
        }

        // 主动检查一次当前金币
        int currentGold = InventoryManager.Instance.currentGold;
        Debug.LogError($"【3】正在进行初始检查... 当前背包金币: {currentGold}");

        // 手动调用检查逻辑
        CheckMoneyCondition(currentGold);
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onGoldChanged -= CheckMoneyCondition;
            InventoryManager.Instance.onInventoryChanged -= CheckSafeCondition;
        }
    }

    // --- 核心逻辑 (刚才你缺失的部分) ---

    // 这个就是刚才报错说找不到的函数！必须要有它！
    void CheckMoneyCondition(int currentGold)
    {
        if (!enableDebugLogs) return;

        // 使用 LogError (红色) 强行让这行字在 Console 显示出来
        Debug.LogError($"【检查中】 目标: {targetGoldAmount} | 当前: {currentGold} | 剧情阶段: {(GameState.Instance != null ? GameState.Instance.storyStage.ToString() : "空")}");

        if (GameState.Instance == null)
        {
            Debug.LogError("【致命错误】GameState 是空的！无法读取剧情阶段！");
            return;
        }

        // 逻辑判断
        if (GameState.Instance.storyStage == 1)
        {
            if (currentGold >= targetGoldAmount)
            {
                Debug.LogError("【任务完成】金币够了！准备显示电话 UI！");

                if (phoneSystem != null)
                {
                    // 修改剧情阶段，防止重复触发
                    // GameState.Instance.storyStage = 2;
                    // 显示电话
                    phoneSystem.ShowPhoneCall();
                  //  Debug.LogError("【成功】已调用 phoneSystem.ShowPhoneCall()。请看屏幕！");
                }
                else
                {
                   // Debug.LogError("【配置错误】Phone System 没拖进 Inspector！电话无法显示！");
                }
            }
            else
            {
              //  Debug.LogError($"【等待】钱还不够。还需要赚 {targetGoldAmount - currentGold} 金币。");
            }
        }
        else
        {
            //Debug.LogError($"【跳过】剧情阶段不对。当前是 {GameState.Instance.storyStage}，但必须是 1 才能触发。");
        }
    }

    // 监听背包变化 (当 InventoryManager 触发 onInventoryChanged 时调用)
    // 监听背包变化 (当 InventoryManager 触发 onInventoryChanged 时调用)
    void CheckSafeCondition()
    {
        // 1. 只有在剧情阶段 2 (已接电话) 时才检查
        // 如果你现在是 1 或者 3，代码会在这里停下
        if (GameState.Instance == null || GameState.Instance.storyStage != 2)
        {
            // 如果你想调试，可以把下面这行注释打开，看看是不是阶段不对
            // Debug.Log($"【等待】当前阶段是 {GameState.Instance.storyStage}，不是 2，不检查保险箱。");
            return;
        }

        // 2. 检查必须配置了目标数据
        if (safeItemData == null)
        {
           // Debug.LogError("【配置错误】Safe Item Data 没拖进 StoryMissionListener！");
            return;
        }

        // 3. 遍历背包，看看有没有这个物品
        bool hasSafe = false;
        if (InventoryManager.Instance.backpackContent != null)
        {
            foreach (var slot in InventoryManager.Instance.backpackContent)
            {
                // 如果找到了保险箱
                if (slot.itemData == safeItemData)
                {
                    hasSafe = true;
                    break; // 找到了，退出循环
                }
            }
        }

        // 4. 判定结果
        if (hasSafe)
        {
            //Debug.LogError("【任务完成】拿到保险箱了！进入 Stage 3！");

            // ★★★ 核心动作：推进剧情到阶段 3 (Return to Cafe) ★★★
            GameState.Instance.storyStage = 3;

            // 弹出一个提示电话，告诉玩家该撤了
            if (phoneSystem != null)
            {
                phoneSystem.mentorMessage = "Boss: You got it? \nExcellent. \nBring it back to the Cafe. NOW.";
                phoneSystem.ShowPhoneCall();
            }
        }
    }
}