using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{

    public string itemName;

    public int price; 


    [TextArea]
    public string description;

}