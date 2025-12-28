using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // STORY STAGES:
    // 0 = Tutorial / Start
    // 1 = Earning Money (Target: 50 Gold)
    // 2 = Phone Call Received / Mission: Steal Safe
    // 3 = Safe Stolen / Mission: Return to Cafe
    // 4 = Mission Complete / Lockpick Reward Received
    public int storyStage = 0;

    public int money = 0; // Syncs with InventoryManager

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}