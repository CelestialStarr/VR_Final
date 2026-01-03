using UnityEngine;
using TMPro;

public class InventoryItemRow : MonoBehaviour
{
    [Header("UI 组件绑定")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;


    private int _myPrice;
    private int _myAmount;

    public void Setup(string name, int amount, int price)
    {

        _myAmount = amount;
        _myPrice = price;


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

    public int GetTotalValue()
    {

        if (_myPrice < 0)
        {
            return 0;
        }


        return _myPrice * _myAmount;
    }

    public void SellThisItem()
    {
        int value = GetTotalValue();
        if (value <= 0) return;

        MoneyManager.Instance.AddMoney(value);

    }



}

