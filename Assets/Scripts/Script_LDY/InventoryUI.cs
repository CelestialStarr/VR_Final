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
            InventoryManager.Instance.onInventoryChanged += UpdateUI;


            UpdateUI();
        }
        else
        {
            Debug.LogError("UI 报错：找不到 InventoryManager！请确保场景里有 GameManager 物体。");
        }
    }

    public void UpdateUI()
    {

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


