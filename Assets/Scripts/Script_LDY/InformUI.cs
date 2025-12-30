using UnityEngine;
using System.Collections;

public class InventoryWarning : MonoBehaviour
{
    [Header("情况1：背包已满 UI")]
    public GameObject bagFullPanel;       // 拖入“背包已满”的 Panel

    [Header("情况2：特殊物品限制 UI")]
    public GameObject itemLimitPanel;     // 拖入“限购警告”的 Panel

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            // 监听信号
            InventoryManager.Instance.onBagFull += ShowBagFullWarning;
            InventoryManager.Instance.onItemLimitReached += ShowItemLimitWarning;
        }

        // 初始隐藏
        if (bagFullPanel != null) bagFullPanel.SetActive(false);
        if (itemLimitPanel != null) itemLimitPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onBagFull -= ShowBagFullWarning;
            InventoryManager.Instance.onItemLimitReached -= ShowItemLimitWarning;
        }
    }

    // 处理背包满
    private void ShowBagFullWarning()
    {
        if (bagFullPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHide(bagFullPanel));
        }
    }

    // 处理限购 (代码自动忽略传入的文字，只弹窗)
    private void ShowItemLimitWarning(string message)
    {
        if (itemLimitPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHide(itemLimitPanel));
        }
    }

    // 通用显示3秒协程
    IEnumerator ShowAndHide(GameObject panel)
    {
        panel.SetActive(true);        // 显示
        yield return new WaitForSeconds(3.0f); // 等3秒
        panel.SetActive(false);       // 隐藏
    }
}