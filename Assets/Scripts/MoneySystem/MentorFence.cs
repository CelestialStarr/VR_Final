using UnityEngine;

public class MentorFence : MonoBehaviour
{
    public TradeTray tray;

    public void Sell()
    {
        if (tray == null || tray.pendingLoot == null)
        {
            Debug.Log("MentorFence: No loot on the tray.");
            return;
        }

        var gs = GameState.Instance;
        if (gs == null) return;

        LootItem loot = tray.pendingLoot;

        gs.money += loot.value;

        Debug.Log("Sold: " + loot.itemName + " for $" + loot.value + ". Total money = $" + gs.money);

        // remove the sold item
        Destroy(loot.gameObject);
        tray.pendingLoot = null;

        // reward gate
        if (!gs.rewardUnlocked && gs.money >= gs.moneyTargetForReward)
        {
            gs.rewardUnlocked = true;
            gs.mentorStage = Mathf.Max(gs.mentorStage, 2); // stage 2 = reward dialogue/animation
            Debug.Log("Money target reached. Reward unlocked!");
        }
    }
}
