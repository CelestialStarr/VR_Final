using UnityEngine;
using TMPro;

public class InventoryItemRow : MonoBehaviour
{
    [Header("UI 组件绑定")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;

    // --- 新增：私有变量，用来“记住”数据 ---
    private int _myPrice;
    private int _myAmount;

    public void Setup(string name, int amount, int price)
    {
        // 1. 先把数据存起来，方便以后计算用
        _myAmount = amount;
        _myPrice = price;

        // 2. 更新 UI 显示 (和之前一样)
        if (nameText != null) nameText.text = name;
        if (amountText != null) amountText.text = $"x{amount}";

        if (priceText != null)
        {
            if (price < 0)
            {
                priceText.text = "?";
            }
            else
            {
                priceText.text = $"{price}$";
            }
        }
    }

    // --- 新增：专门给外部调用计算总价的方法 ---
    // 返回值：这行物品的总价值 (单价 * 数量)
    public int GetTotalValue()
    {
        // 关键逻辑：如果是 -1 (未知/未鉴定)，就当做 0 价值
        if (_myPrice < 0)
        {
            return 0;
        }

        // 否则返回：单价 * 数量
        return _myPrice * _myAmount;
    }

    public void SellThisItem()
    {
        int value = GetTotalValue();
        if (value <= 0) return;

        MoneyManager.Instance.AddMoney(value);

        // TODO: 通知 InventoryManager 减少或移除该物品
    }



}