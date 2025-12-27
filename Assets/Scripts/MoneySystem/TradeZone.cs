using UnityEngine;
using UnityEngine.UI;

public class MentorTradeZone : MonoBehaviour
{
    public GameObject sellButtonUI;

    private void Start()
    {
        if (sellButtonUI != null)
            sellButtonUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        sellButtonUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        sellButtonUI.SetActive(false);
    }

    public void SellAllItems()
    {
        int gained = InventoryManager.Instance.SellAll();
        GameState.Instance.money += gained;

        Debug.Log($"Sold all items. Gained {gained}, total money = {GameState.Instance.money}");

        StoryFlowController.Instance.CheckMoneyProgress();
    }
}
