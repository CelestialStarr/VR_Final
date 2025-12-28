using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Transform contentContainer; // ScrollView 的 Content
    public GameObject itemRowPrefab;   // Item Row 预制体

    private void Start()
    {
        // 只负责监听数据变化
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += UpdateUI;
        }

        // 不再控制 SetActive
        // UI 是否显示由手势 / 其他脚本控制
    }

    // 刷新 UI
    public void UpdateUI()
    {
        // 清空旧内容
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null) return;

        // 根据背包数据生成 UI
        foreach (var slot in InventoryManager.Instance.backpackContent)
        {
            GameObject newRow = Instantiate(itemRowPrefab, contentContainer);
            var rowScript = newRow.GetComponent<InventoryItemRow>();

            if (rowScript != null)
            {
                rowScript.Setup(
                    slot.itemData.itemName,
                    slot.stackSize,
                    slot.itemData.price
                );
            }
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= UpdateUI;
        }
    }
}
