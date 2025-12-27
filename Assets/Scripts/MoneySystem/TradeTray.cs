using UnityEngine;

public class TradeTray : MonoBehaviour
{
    public LootItem pendingLoot;

    public bool HasLoot => pendingLoot != null;

    void OnTriggerEnter(Collider other)
    {
        var loot = other.GetComponentInParent<LootItem>();
        if (loot != null)
        {
            pendingLoot = loot;
            Debug.Log("TradeTray: Loot detected: " + loot.itemName + " ($" + loot.value + ")");
        }
    }

    void OnTriggerExit(Collider other)
    {
        var loot = other.GetComponentInParent<LootItem>();
        if (loot != null && pendingLoot == loot)
        {
            pendingLoot = null;
            Debug.Log("TradeTray: Loot removed.");
        }
    }
}
