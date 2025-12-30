using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Transform contentContainer;
    public GameObject itemRowPrefab;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            // 1. 订阅未来的变化
            InventoryManager.Instance.onInventoryChanged += UpdateUI;

            // 2. ★★★ 关键修复：立刻刷新一次，显示当前已有的东西 ★★★
            UpdateUI();
        }
        else
        {
            Debug.LogError("UI 报错：找不到 InventoryManager！请确保场景里有 GameManager 物体。");
        }
    }

    public void UpdateUI()
    {
        // 增加一行 Debug，帮你看看到底运行没运行
        Debug.Log("UI 正在刷新显示...");

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null) return;

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


