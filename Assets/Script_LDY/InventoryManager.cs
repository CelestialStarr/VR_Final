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

    [Header("金钱系统")]
    public int currentGold = 0;
    public event Action<int> onGoldChanged;

    [Header("限制/不可售卖设置")]
    // ★ 修改点1：这里改成 ItemData 类型，你可以直接把 Parcle 的数据文件拖进来
    public ItemData unsellableItem;
    public int maxLimitedItemCount = 5;

    [Header("背包设置")]
    public int maxTotalCapacity = 30;
    public List<InventorySlot> backpackContent = new List<InventorySlot>();

    public event Action onInventoryChanged;
    public event Action onToggleBag;
    public event Action onBagFull;
    public event Action<string> onItemLimitReached;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) onToggleBag?.Invoke();
    }

    // ============================================
    // ★ 一键出售逻辑
    // ============================================
    public void SellAllItems()
    {
        // ★ 调试第一步：只要点击按钮，Console里必须出现这句话。如果没有，说明按钮绑定坏了。
        Debug.Log("正在尝试出售物品...");

        int totalEarnings = 0;
        int soldCount = 0; // 记录卖了多少个格子

        for (int i = backpackContent.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = backpackContent[i];

            // ★ 修改点2：直接对比数据引用，比对比名字更安全
            // 如果你没在Inspector里拖拽 unsellableItem，这里可能会报错，记得要拖进去！
            if (unsellableItem != null && slot.itemData == unsellableItem)
            {
                Debug.Log($"跳过不可售卖物品: {slot.itemData.itemName}");
                continue;
            }

            // 计算价格
            int value = slot.stackSize * slot.itemData.price;
            totalEarnings += value;
            soldCount++;

            // 移除
            backpackContent.RemoveAt(i);
        }

        if (totalEarnings > 0)
        {
            currentGold += totalEarnings;
            onGoldChanged?.Invoke(currentGold);
            onInventoryChanged?.Invoke();

            Debug.Log($"出售成功！卖出格子数: {soldCount}, 获得金币: {totalEarnings}");
        }
        else
        {
            Debug.Log("出售结束：没有可出售的物品，或者只剩下了不可卖的物品。");
        }
    }

    // ============================================
    // 添加物品逻辑 (也同步修改了判定逻辑)
    // ============================================
    public bool AddItem(ItemData item)
    {
        int total = 0;
        foreach (var slot in backpackContent) total += slot.stackSize;

        if (total >= maxTotalCapacity)
        {
            onBagFull?.Invoke();
            return false;
        }

        // ★ 修改点3：这里的限制逻辑也同步改成用 item 对比
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

        onInventoryChanged?.Invoke();
        return true;
    }
}