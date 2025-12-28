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
        // 暂时注释掉这行，直到你在 InventoryManager 脚本里写了 public int SellAll() 方法
        // int gained = InventoryManager.Instance.SellAll(); 

        // 临时假装卖了东西赚了50块，防止报错
        int gained = 50;

        // 确保你有一个 GameState 脚本并且它是单例，否则这行也会报错
        // GameState.Instance.money += gained; 

        Debug.Log($"Sold all items (Test Mode). Gained {gained}");

        // 现在 StoryFlowController 有单例了，这行可以工作了
        StoryFlowController.Instance.CheckMoneyProgress();
    }
}
