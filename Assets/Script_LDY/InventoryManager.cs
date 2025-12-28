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

    // [核心修改 1] 静态变量保险箱：保存金币和背包内容
    private static int _savedGold = -1;
    private static List<InventorySlot> _savedBackpack = null;

    [Header("金钱系统")]
    public int currentGold = 0;
    public event Action<int> onGoldChanged;

    [Header("限制/不可售卖设置")]
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

        // [核心修改 2] 读取数据逻辑
        // 如果 _savedGold 大于等于0，说明不是第一次运行，有存档
        if (_savedGold >= 0)
        {
            currentGold = _savedGold;

            // 恢复背包列表 (如果保险箱里有列表，就直接拿来用)
            if (_savedBackpack != null)
                backpackContent = _savedBackpack;

            Debug.Log($"<color=green>已从跨场景存档加载背包：金币 {currentGold}, 物品数 {backpackContent.Count}</color>");
        }
        else
        {
            // 第一次运行，初始化保险箱
            _savedGold = currentGold;
            _savedBackpack = backpackContent;
        }
    }

    // [新增] 加上 Start 方法，确保进入新场景时 UI 能刷新
    private void Start()
    {
        // 强制通知 UI 刷新一次，否则刚进场景 UI 显示的是 0
        onGoldChanged?.Invoke(currentGold);
        onInventoryChanged?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) onToggleBag?.Invoke();

        // [核心修改 3] 每一帧都把数据同步到保险箱
        _savedGold = currentGold;
        // 列表是引用类型，只要把引用指过去就行
        _savedBackpack = backpackContent;
    }

    // [新增] 如果你有“开始新游戏”按钮，必须调用这个！
    public void ResetInventoryData()
    {
        _savedGold = 0;
        _savedBackpack = new List<InventorySlot>();

        currentGold = 0;
        backpackContent = new List<InventorySlot>();

        onGoldChanged?.Invoke(0);
        onInventoryChanged?.Invoke();

        Debug.Log("背包数据已重置");
    }

    // --- 以下是你原有的逻辑，保持不变 ---

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
        }
    }

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

    public bool TryConsumeItem(ItemData itemToConsume)
    {
        InventorySlot slot = backpackContent.Find(x => x.itemData == itemToConsume);

        if (slot != null)
        {
            slot.stackSize--;
            if (slot.stackSize <= 0)
            {
                backpackContent.Remove(slot);
            }
            onInventoryChanged?.Invoke();
            Debug.Log($"成功消耗物品: {itemToConsume.itemName}");
            return true;
        }

        Debug.Log("背包里没有这个物品，无法开包！");
        return false;
    }
}