using UnityEngine;
using UnityEngine.UI; // 或者是 TMPro 如果你用了
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Transform contentContainer; // ScrollView 的 Content
    public GameObject itemRowPrefab;   // 你的预制体

    // --- 关键点：我们要控制这个面板的显示/隐藏 ---
    public GameObject mainPanel;       // 把 Canvas 下的 MainPanel 拖进去

    private void Start()
    {
        // 1. 订阅数据变化 (刷新列表)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += UpdateUI;

            // 2. 订阅开关事件 (显示/隐藏)
            InventoryManager.Instance.onToggleBag += ToggleVisuals;
        }

        // 游戏一开始，先关闭背包界面
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }

    // 当按下 M 键时，Manager 会调用这个方法
    private void ToggleVisuals()
    {
        if (mainPanel != null)
        {
            bool isActive = mainPanel.activeSelf;
            mainPanel.SetActive(!isActive); // 如果开着就关，如果关着就开

            // 如果刚刚打开了背包，顺便刷新一下数据，确保显示最新内容
            if (!isActive)
            {
                UpdateUI();
            }
        }
    }

    // 刷新 UI 的逻辑 (保持你之前的逻辑)
    public void UpdateUI()
    {
        // 清空旧数据
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null) return;

        // 生成新数据
        foreach (var slot in InventoryManager.Instance.backpackContent)
        {
            GameObject newRow = Instantiate(itemRowPrefab, contentContainer);
            var rowScript = newRow.GetComponent<InventoryItemRow>();
            if (rowScript != null)
            {
                rowScript.Setup(slot.itemData.itemName, slot.stackSize, slot.itemData.price);
            }
        }
    }

    // 记得销毁时取消订阅，防止报错
    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= UpdateUI;
            InventoryManager.Instance.onToggleBag -= ToggleVisuals;
        }
    }
}