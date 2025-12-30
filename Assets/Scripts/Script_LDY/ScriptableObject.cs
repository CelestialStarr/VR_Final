using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // === 所有的变量都必须在这一对大括号里面 ===

    public string itemName;

    public int price; // 这一行如果写在括号外面，Inspector就不会显示

   // public Sprite icon;

    [TextArea]
    public string description;

    // =========================================
}