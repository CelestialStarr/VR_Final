using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int stackSize;
    public InventorySlot(ItemData item, int amount) { itemData = item; stackSize = amount; }
    public void AddToStack(int amount) { stackSize += amount; }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // ====================================================
    // ★★★ 核心修改：静态变量 (Static) 作为“永久保险箱” ★★★
    // 这些数据存在内存里，切场景绝对不会消失！
    // ====================================================
    private static List<InventorySlot> globalBackpackData = new List<InventorySlot>();
    private static int globalGold = 0;
    private static bool isDataInitialized = false; // 标记是否是第一次运行
    // ====================================================

    [Header("金钱系统")]
    public int currentGold = 0; // 这个变量只用于 Inspector 显示
    public event Action<int> onGoldChanged;

    [Header("限制/不可售卖设置")]
    public ItemData unsellableItem;
    public int maxLimitedItemCount = 5;

    [Header("背包设置")]
    public int maxTotalCapacity = 30;

    // 这里显示当前场景的背包，方便你在 Inspector 里看
    public List<InventorySlot> backpackContent = new List<InventorySlot>();

    // 事件定义
    public event Action onInventoryChanged;
    public event Action onToggleBag;
    public event Action onBagFull;
    public event Action<string> onItemLimitReached;

    private void Awake()
    {
        // 标准单例写法 (切场景时，新的Manager会覆盖旧的Instance引用，这没问题)
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // ★★★ 关键步骤 1：同步数据 (从静态保险箱 -> 到当前物体) ★★★
        // 如果我们已经在之前玩过（存过数据），就把数据取出来
        if (isDataInitialized)
        {
            // 把静态数据复制给当前的 backpackContent，这样你在 Inspector 就能看到之前的装备了
            backpackContent = new List<InventorySlot>(globalBackpackData);
            currentGold = globalGold;
        }
        else
        {
            // 如果是游戏第一次启动，标记一下
            isDataInitialized = true;
            // 确保静态数据是空的或者初始状态
            globalBackpackData = new List<InventorySlot>();
            globalGold = 0;
        }
    }

    private void Start()
    {
        // 游戏开始，通知 UI 刷新一下
        onInventoryChanged?.Invoke();
        onGoldChanged?.Invoke(currentGold);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) onToggleBag?.Invoke();
    }

    // ============================================
    // ★★★ 辅助函数：每次改动后，把数据存回静态保险箱 ★★★
    // ============================================
    private void SaveToStatic()
    {
        // 1. 保存背包列表
        globalBackpackData = new List<InventorySlot>(backpackContent);
        // 2. 保存金币
        globalGold = currentGold;
    }

    // ============================================
    //  以下逻辑修改：每次改完数据，都要调用 SaveToStatic()
    // ============================================

    public void TriggerBagFullEvent() => onBagFull?.Invoke();
    public void TriggerItemLimitEvent(string message) => onItemLimitReached?.Invoke(message);

    // 一键出售逻辑
    public void SellAllItems()
    {
        int totalEarnings = 0;
        for (int i = backpackContent.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = backpackContent[i];
            if (unsellableItem != null && slot.itemData == unsellableItem) continue;

            totalEarnings += slot.stackSize * slot.itemData.price;
            backpackContent.RemoveAt(i);
        }

        if (totalEarnings > 0)
        {
            currentGold += totalEarnings;

            // ★ 保存数据
            SaveToStatic();

            onGoldChanged?.Invoke(currentGold);
            onInventoryChanged?.Invoke();
        }
    }

    // 消耗物品逻辑
    public bool TryConsumeItem(ItemData itemToConsume)
    {
        InventorySlot slot = backpackContent.Find(x => x.itemData == itemToConsume);
        if (slot != null)
        {
            slot.stackSize--;
            if (slot.stackSize <= 0) backpackContent.Remove(slot);

            // ★ 保存数据
            SaveToStatic();

            onInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    // 添加物品逻辑
    public bool AddItem(ItemData item)
    {
        int total = 0;
        foreach (var slot in backpackContent) total += slot.stackSize;

        if (total >= maxTotalCapacity)
        {
            onBagFull?.Invoke();
            return false;
        }

        if (unsellableItem != null && item == unsellableItem)
        {
            InventorySlot specificSlot = backpackContent.Find(s => s.itemData == item);
            int count = (specificSlot != null) ? specificSlot.stackSize : 0;
            if (count >= maxLimitedItemCount)
            {
                onItemLimitReached?.Invoke($"最多只能获取{maxLimitedItemCount}个包裹");
                return false;
            }
        }

        InventorySlot existingSlot = backpackContent.Find(slot => slot.itemData == item);
        if (existingSlot != null) existingSlot.AddToStack(1);
        else backpackContent.Add(new InventorySlot(item, 1));

        // ★ 保存数据 (这一步最重要！一定要在添加后保存)
        SaveToStatic();

        onInventoryChanged?.Invoke();
        return true;
    }
}

