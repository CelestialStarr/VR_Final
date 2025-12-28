using UnityEngine;
using TMPro; // 如果你使用的是 TextMeshPro
// using UnityEngine.UI; // 如果你使用的是旧版 Text，请取消这行注释并注释掉上一行

public class GoldDisplay : MonoBehaviour
{
    [Header("UI 组件")]
    // 这里使用 TextMeshProUGUI，如果你用旧版 Text，请改成 public Text goldText;
    public TextMeshProUGUI goldText;

    private void Start()
    {
        // 1. 游戏开始时，先更新一次，保证显示初始金额
        if (InventoryManager.Instance != null)
        {
            UpdateGoldText(InventoryManager.Instance.currentGold);

            // 2. 订阅事件：告诉背包管理器 "如果金币变了，请调用我的 UpdateGoldText 方法"
            InventoryManager.Instance.onGoldChanged += UpdateGoldText;
        }
    }

    private void OnDestroy()
    {
        // 3. 养成好习惯：物体销毁时取消订阅，防止报错
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onGoldChanged -= UpdateGoldText;
        }
    }

    // 这是一个回调函数，当金币变化时会自动执行
    private void UpdateGoldText(int currentGold)
    {
        // 这里你可以自定义格式，比如 "Gold: 100" 或 "100 G"
        goldText.text = currentGold.ToString();
    }
}