using UnityEngine;
using System.Collections.Generic;
using System;

// ==========================================
// 第一部分：InventorySlot (保持不变)
// ==========================================
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int stackSize;

    public InventorySlot(ItemData item, int amount)
    {
        itemData = item;
        stackSize = amount;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }
}

// ==========================================
// 第二部分：InventoryManager (添加了开关逻辑)
// ==========================================
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // 1. 之前就有的：背包内容列表
    public List<InventorySlot> backpackContent = new List<InventorySlot>();

    // 2. 之前就有的：背包内容变化事件
    public event Action onInventoryChanged;

    // --- 新增：背包开关事件 ---
    // 这个事件专门用来通知 UI：“嘿，玩家想打开或关闭背包”
    public event Action onToggleBag;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        // 监听 M 键
        if (Input.GetKeyDown(KeyCode.M))
        {
            // 触发开关事件，UI 听到后会自动显示/隐藏
            Debug.Log("Manager 发送信号：切换背包显示状态");
            onToggleBag?.Invoke();
        }
    }

    // 添加物品逻辑 (保持不变)
    public void AddItem(ItemData item)
    {
        InventorySlot existingSlot = backpackContent.Find(slot => slot.itemData == item);

        if (existingSlot != null)
        {
            existingSlot.AddToStack(1);
        }
        else
        {
            InventorySlot newSlot = new InventorySlot(item, 1);
            backpackContent.Add(newSlot);
        }

        onInventoryChanged?.Invoke();
    }
}