using UnityEngine;
using TMPro; // 这一行非常重要，如果没有它，下面的 TextMeshProUGUI 会报错

public class InventoryItemRow : MonoBehaviour
{
    // 使用 [SerializeField] 强制在 Inspector 中显示私有变量
    [Header("UI 组件绑定")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;

    // 公开方法供外部调用，内部使用私有变量
    public void Setup(string name, int amount, int price)
    {
        // 增加判空，防止忘记拖拽导致报错
        if (nameText != null) nameText.text = name;
        if (amountText != null) amountText.text = $"x{amount}";
        if (priceText != null) priceText.text = $"{price}$";
    }
}