using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Min(1)]
    public int value = 20;

    [Tooltip("Optional: display name for UI")]
    public string itemName = "Loot";
}
