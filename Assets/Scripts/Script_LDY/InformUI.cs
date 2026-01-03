using UnityEngine;
using System.Collections;

public class InventoryWarning : MonoBehaviour
{
    [Header("Bag Full UI")]
    public GameObject bagFullPanel;

    [Header("Item Limit UI")]
    public GameObject itemLimitPanel;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onBagFull += ShowBagFullWarning;
            InventoryManager.Instance.onItemLimitReached += ShowItemLimitWarning;
        }

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

    private void ShowBagFullWarning()
    {
        if (bagFullPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHide(bagFullPanel));
        }
    }

    private void ShowItemLimitWarning(string message)
    {
        if (itemLimitPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHide(itemLimitPanel));
        }
    }

    IEnumerator ShowAndHide(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        panel.SetActive(false);
    }
}