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
    public ItemData unsellableItem; // 你的 Parcle 数据
    public int maxLimitedItemCount = 5;

    [Header("背包设置")]
    public int maxTotalCapacity = 30;
    public List<InventorySlot> backpackContent = new List<InventorySlot>();

    // 事件定义
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
    // ★ 新增：允许外部脚本触发 UI 事件的公共通道
    // ============================================
    public void TriggerBagFullEvent()
    {
        // 这样外部脚本就能通过 Manager 间接触发 UI 提示了
        onBagFull?.Invoke();
    }

    public void TriggerItemLimitEvent(string message)
    {
        onItemLimitReached?.Invoke(message);
    }
    // ============================================

    // 一键出售逻辑 (保持不变)
    public void SellAllItems()
    {
        Debug.Log("正在尝试出售物品...");
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
            onGoldChanged?.Invoke(currentGold);
            onInventoryChanged?.Invoke();
            Debug.Log($"出售成功！获得: {totalEarnings}");
        }
    }

    // 开盲盒消耗逻辑 (保持不变)
    public bool TryConsumeItem(ItemData itemToConsume)
    {
        InventorySlot slot = backpackContent.Find(x => x.itemData == itemToConsume);
        if (slot != null)
        {
            slot.stackSize--;
            if (slot.stackSize <= 0) backpackContent.Remove(slot);
            onInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    // 添加物品逻辑 (保持不变)
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

        onInventoryChanged?.Invoke();
        return true;
    }
}