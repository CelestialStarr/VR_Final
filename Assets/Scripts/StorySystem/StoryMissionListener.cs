using UnityEngine;
using UnityEngine.SceneManagement; // ★★★ 必须加这个头文件！ ★★★

public class StoryMissionListener : MonoBehaviour
{
    [Header("调试开关")]
    public bool enableDebugLogs = true;

    [Header("任务配置")]
    public ItemData safeItemData;
    // 这里依然可以拖拽引用，但跨场景时它会断，代码会自动处理
    public StreetPhoneSystem phoneSystem;
    public int targetGoldAmount = 30;

    void Start()
    {
        Debug.Log("【1】StoryMissionListener 启动");
        // 延迟注册，等待 Inventory 初始化
        Invoke(nameof(RegisterEvents), 0.5f);
    }

    // ★★★ 新增：监听场景加载事件 ★★★
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 每次切换场景完成时，Unity 会自动调这个函数
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (enableDebugLogs) Debug.Log($"【切换场景】进入了新场景：{scene.name}，正在重新检查任务条件...");

        // 到了新地方，先主动去找一下有没有电话（万一是在 Scene 2 呢）
        FindPhoneInScene();

        // 然后检查一下金币是否达标
        if (InventoryManager.Instance != null)
        {
            CheckMoneyCondition(InventoryManager.Instance.currentGold);
        }
    }

    void RegisterEvents()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("【错误】找不到 InventoryManager");
            return;
        }

        InventoryManager.Instance.onGoldChanged += CheckMoneyCondition;
        InventoryManager.Instance.onInventoryChanged += CheckSafeCondition;

        Debug.Log("【2】事件订阅成功");

        // 初始检查
        CheckMoneyCondition(InventoryManager.Instance.currentGold);
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onGoldChanged -= CheckMoneyCondition;
            InventoryManager.Instance.onInventoryChanged -= CheckSafeCondition;
        }
    }

    // --- 辅助函数：专门负责找电话 ---
    void FindPhoneInScene()
    {
        // 如果手里已经有活的引用，就不用找了
        if (phoneSystem != null && phoneSystem.gameObject != null) return;

        // 尝试在当前场景找一个（哪怕它是隐藏的）
        var foundPhone = FindObjectOfType<StreetPhoneSystem>(true);
        if (foundPhone != null)
        {
            phoneSystem = foundPhone;
            if (enableDebugLogs) Debug.Log($"【连接】在新场景找到了电话 UI：{foundPhone.name}");
        }
    }

    // --- 核心逻辑 ---
    void CheckMoneyCondition(int currentGold)
    {
        // 过滤阶段
        if (GameState.Instance == null || GameState.Instance.storyStage != 1) return;

        if (currentGold >= targetGoldAmount)
        {
            if (enableDebugLogs) Debug.Log("【达标】金币够了，尝试呼叫电话...");

            // 1. 确保手里有电话引用
            FindPhoneInScene();

            // 2. 只有找到了电话才显示，找不到就拉倒（静默等待）
            if (phoneSystem != null)
            {
                Debug.Log("【执行】电话在当前场景存在，显示通话！");
                phoneSystem.ShowPhoneCall();
            }
            else
            {
                // ★★★ 这里不再报错了！ ★★★
                // 在 Scene 1 时，这里会执行，但这是符合预期的。
                if (enableDebugLogs) Debug.LogWarning("【等待】金币已达标，但当前场景没有电话 UI。等待玩家进入 Scene 2...");
            }
        }
    }

    // ... CheckSafeCondition 保持不变 ...
    void CheckSafeCondition()
    {
        // (保持你原来的代码逻辑)
        if (GameState.Instance == null || GameState.Instance.storyStage != 2) return;
        if (safeItemData == null) return;

        bool hasSafe = false;
        if (InventoryManager.Instance.backpackContent != null)
        {
            foreach (var slot in InventoryManager.Instance.backpackContent)
            {
                if (slot.itemData == safeItemData)
                {
                    hasSafe = true;
                    break;
                }
            }
        }

        if (hasSafe)
        {
            GameState.Instance.storyStage = 3;
            // 拿到保险箱后，也是同样的逻辑：先找电话，有就弹，没有就等
            FindPhoneInScene();
            if (phoneSystem != null)
            {
                phoneSystem.mentorMessage = "Boss: You got it? \nExcellent. \nBring it back to the Cafe. NOW.";
                phoneSystem.ShowPhoneCall();
            }
        }
    }
}